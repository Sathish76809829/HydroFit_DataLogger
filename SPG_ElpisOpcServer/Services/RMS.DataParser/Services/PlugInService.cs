using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RMS.DataParser.Configurations;
using RMS.DataParser.PlugIns;
using RMS.Service.Abstractions;
using RMS.Service.Abstractions.Database;
using RMS.Service.Abstractions.PlugIns;
using RMS.Service.Abstractions.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace RMS.DataParser.Services
{
    /// <summary>
    /// Starts the topic subscriptions for different providers using <code>provider.json</code> file
    /// </summary>
    public class PlugInService
    {
        /// <summary>
        /// Contains list of topics and topic process
        /// </summary>
        internal readonly ConcurrentDictionary<string, IDataServiceContext> DataProcess;

        private readonly PlugInContainer container;

        private readonly PlugInAppContext context;

        private ProviderTopicConfig topicConfig;

        private readonly DataParserOptions topicOptions;

        private readonly ILogger logger;

        public PlugInService(IServiceProvider services, IOptions<DataParserOptions> options)
        {
            topicOptions = options.Value;
            logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("RMS DataProcess");
            DataProcess = new ConcurrentDictionary<string, IDataServiceContext>();
            container = services.GetRequiredService<PlugInContainer>();
            context = new PlugInAppContext(services);
        }

        public async Task LoadProviders()
        {
            var file = new FileInfo(topicOptions.ProviderConfigFile);
            if (file.Exists)
            {
                using (var stream = file.OpenRead())
                {
                    topicConfig = await JsonSerializer.DeserializeAsync<ProviderTopicConfig>(stream);
                }
            }
            else
            {
                topicConfig = new ProviderTopicConfig()
                {
                    IncommingTopics = new Dictionary<string, List<Guid>>()
                };
            }
        }

        public async Task StartAsync()
        {
            if (topicConfig == null)
                return;
            foreach (var topic in topicConfig.IncommingTopics)
            {
                List<IPlugIn> plugIns = new List<IPlugIn>();
                foreach (var id in topic.Value)
                {
                    var plugInStart = container.GetPlugInById(id);
                    if (plugInStart == null)
                    {
                        continue;
                    }

                    IPlugIn plugIn = await CreatePlugIn(plugInStart);
                    if (plugIn != null)
                        plugIns.Add(plugIn);
                }
                var process = CreateService(topic.Key);
                foreach (var plugIn in plugIns)
                {
                    await process.TryAddSync(plugIn);
                }
                DataProcess.TryAdd(topic.Key, process);
                process.Start();
            }
        }

        private IDataServiceContext CreateService(string topic)
        {
            switch (topic)
            {
                case "opc":
                    return new Opc.OpcDataProcessContext(context.AppServices, topic);
                default:
                    return new DataServiceContext(context.AppServices, topic);
            }
        }

        public async Task StopAsync()
        {
            foreach (var process in DataProcess)
            {
                await process.Value.DisposeAsync();
            }
            DataProcess.Clear();
        }

        public async ValueTask<bool> UsePlugInAsync(string topic, Guid id)
        {
            var plugInStart = container.GetPlugInById(id);
            if (plugInStart == null)
            {
                return false;
            }
            IPlugIn plugIn = await CreatePlugIn(plugInStart);
            if (plugIn == null)
                return false;
            if (!topicConfig.IncommingTopics.TryGetValue(topic, out var ids))
            {
                ids = new List<Guid>();
                topicConfig.IncommingTopics.Add(topic, ids);
            }
            ids.Add(id);
            if (!DataProcess.TryGetValue(topic, out var process))
            {
                process = CreateService(topic);
                DataProcess[topic] = process;
            }
            if (await process.TryAddSync(plugIn))
            {
                process.Start();
                await UpdateConfig();
                return true;
            }
            return false;
        }

        public async ValueTask<bool> RemovePlugInAsync(string topic, Guid id)
        {
            if (DataProcess.TryGetValue(topic, out var process)
                && await process.TryRemoveAsync(id))
            {
                if(topicConfig.IncommingTopics.TryGetValue(topic, out var plugIns))
                {
                    plugIns.Remove(id);
                }
                await UpdateConfig();
                return true;
            }
            return false;
        }

        async Task<IPlugIn> CreatePlugIn(PlugInStart plugInStart)
        {
            var builder = PlugIn.CreateBuilder(plugInStart.Info).UseContentRoot(plugInStart.Info.PlugInDir);
            builder.ConfigureServices((ctx, services) =>
            {
                services.AddSingleton(context.AppServices.GetRequiredService<ILoggerFactory>())
                        .AddSingleton(context.AppServices.GetService<IDbFactory>())
                        .AddSingleton(typeof(ILogger<>), typeof(Logger<>))
                        .AddSingleton(context.AppServices.GetRequiredService<IServiceFactory>())
                        .AddSingleton(services => services.GetService<IServiceFactory>()?.CreateScript())
                        .AddSingleton(context.AppServices.GetService<IEventMonitor>());
            });
            try
            {
                plugInStart.Host.ConfigurePlugIn(context, builder);
                var plugIn = builder.Build();
                var initialtor = plugIn.Services.GetService<IPlugInInitalizer>();
                if (initialtor != null)
                {
                    await initialtor.ConfigureAsync();
                }
                return plugIn;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return null;
            }

        }

        public async Task UpdateConfig()
        {
            using (var stream = new FileStream(path: topicOptions.ProviderConfigFile, FileMode.Create, FileAccess.Write))
            {
                await JsonSerializer.SerializeAsync(stream, topicConfig, new JsonSerializerOptions { WriteIndented = true });
            }
        }
    }
}

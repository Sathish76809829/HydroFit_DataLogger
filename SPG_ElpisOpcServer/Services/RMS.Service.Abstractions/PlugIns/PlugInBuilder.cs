using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RMS.Service.Abstractions.Providers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RMS.Service.Abstractions.PlugIns
{
    /// <summary>
    /// PlugIn builder instance which will pass to <see cref="IPlugInHost.ConfigurePlugIn(PlugInAppContext, IPlugInBuilder)"/>
    /// </summary>
    public class PlugInBuilder : IPlugInBuilder
    {
        private readonly IDictionary<string, object> properties;

        public IDictionary<string, object> Properties => properties;

        public IServiceCollection Services => services;

        private readonly List<Action<IConfigurationBuilder>> _configActions;

        private readonly List<Action<PlugInContext, IServiceCollection>> _configServiceActions;



        private readonly IServiceCollection services;

        internal PlugInBuilder(PlugInInfo info)
        {
            services = new ServiceCollection();
            properties = info.Properties;
            _configActions = new List<Action<IConfigurationBuilder>>();
            _configServiceActions = new List<Action<PlugInContext, IServiceCollection>>();
        }

        public IPlugIn Build()
        {
            var context = new PlugInContext(properties)
            {
                PlugInPath = PlugInDir
            };
            services.AddSingleton(context)
                .AddSingleton<IPlugIn, PlugIn>();
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(PlugInDir);
            foreach (var configurationAction in _configActions)
            {
                configurationAction(configurationBuilder);
            }
            IConfiguration configuration = configurationBuilder.Build();
            services.AddSingleton(configuration);
            context.Configuration = configuration;
            foreach (var configServiceAction in _configServiceActions)
            {
                configServiceAction(context, services);
            }
            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetService<IPlugIn>();
        }

        public string PlugInDir
        {
            get
            {
                if (properties.TryGetValue(PlugIn.BasePathKey, out var value))
                {
                    return value?.ToString();
                }
                return Environment.CurrentDirectory;
            }
        }

        public IPlugInBuilder UseContentRoot(string root)
        {
            properties[PlugIn.BasePathKey] = root;
            return this;
        }

        public IPlugInBuilder UseProvider<TProvider>(Func<IServiceProvider, TProvider> implement = null) where TProvider : class, IDeviceProvider
        {
            ServiceDescriptor provider;
            if (implement == null)
            {
                provider = ServiceDescriptor.Transient<IDeviceProvider, TProvider>();
            }
            else
            {
                provider = ServiceDescriptor.Transient<IDeviceProvider>((services) => implement(services));
            }
            services.Add(provider);
            return this;
        }


        public IPlugInBuilder Configure(Action<IPlugIn> configure)
        {
            services.AddSingleton<IPlugInInitalizer>((services) =>
            {
                return new PlugInInitializer(services, configure);
            });
            return this;
        }

        public IPlugInBuilder Configure(Func<IPlugIn, Task> configure)
        {
            services.AddSingleton<IPlugInInitalizer>((services) =>
            {
                return new PlugInInitializer(services, configure);
            });
            return this;
        }

        public IPlugInBuilder ConfigureConfiguration(Action<IConfigurationBuilder> configureDelegate)
        {
            if (configureDelegate == null)
                throw new ArgumentNullException(nameof(configureDelegate));
            _configActions.Add(configureDelegate);
            return this;
        }

        public IPlugInBuilder ConfigureServices(Action<PlugInContext, IServiceCollection> configureDelegate)
        {
            if (configureDelegate == null)
                throw new ArgumentNullException(nameof(configureDelegate));
            _configServiceActions.Add(configureDelegate);
            return this;
        }
    }
}

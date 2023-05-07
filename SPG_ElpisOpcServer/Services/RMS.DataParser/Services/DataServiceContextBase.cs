using Microsoft.Extensions.DependencyInjection;
using RMS.Service.Abstractions.PlugIns;
using RMS.Service.Abstractions.Providers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RMS.DataParser.Services
{
    public abstract class DataServiceContextBase<TProvider> : IDataServiceContext where TProvider:IDeviceProvider
    {
        internal readonly ConcurrentDictionary<Guid, PlugInContext> Contexts;

        internal readonly string Topic;

        private readonly Dictionary<int, TProvider> providers;

        protected readonly IServiceProvider Services;

        private Task task;

        protected IDataProcessService Service;

        private CancellationTokenSource cts;

        public IDictionary<int, TProvider> Providers => providers;


        internal class PlugInContext
        {
            public PlugInInfo Info;

            public IPlugIn PlugIn;

            public IServiceScope Scope;
        }

        public DataServiceContextBase(IServiceProvider services, string topic)
        {
            Topic = topic;
            this.Services = services;
            providers = new Dictionary<int, TProvider>();
            Contexts = new ConcurrentDictionary<Guid, PlugInContext>();
        }

        public async ValueTask DisposeAsync()
        {
            if (task != null)
            {
                cts.Cancel();
                await Task.WhenAll(task, Task.Delay(100));
                task.Dispose();
                task = null;
            }
            foreach (var scope in Contexts.Values)
            {
                await scope.PlugIn.DisposeAsync();
                scope.Scope.Dispose();
            }
            Contexts.Clear();
            if (Service != null)
            {
                Service.Dispose();
                Service = null;
            }
        }

        public IList<PlugInInfo> GetPlugIns()
        {
            ICollection<PlugInContext> values = Contexts.Values;
            var res = new List<PlugInInfo>(values.Count);
            foreach (var item in values)
            {
                res.Add(item.Info);
            }
            return res;
        }

        public void Start()
        {
            if (task != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();
                task.Wait();
                task = null;
            }
            cts = new CancellationTokenSource();
            task = Task.Factory.StartNew(() => StartProcess(cts.Token), creationOptions: TaskCreationOptions.LongRunning);
        }

        public async  Task<bool> TryAddSync(IPlugIn plugIn)
        {
            PlugInInfo info = plugIn.Info;
            if (Contexts.ContainsKey(info.Id))
            {
                return false;
            }

            IServiceScope scope = plugIn.Services.CreateScope();
            Contexts[info.Id] = new PlugInContext
            {
                Info = info,
                PlugIn = plugIn,
                Scope = scope
            };
            var provider = (TProvider)scope.ServiceProvider.GetService<IDeviceProvider>();
            if (provider != null)
            {
                await provider.InitalizeAsync();
                return providers.TryAdd(info.TypeId, provider);
            }
            return false;
        }

        public async ValueTask<bool> TryRemoveAsync(Guid id)
        {
            if (Contexts.TryRemove(id, out var ctx))
            {
                var provider = ctx.Scope.ServiceProvider.GetService<IDeviceProvider>();
                if (provider != null && providers.Remove(ctx.Info.TypeId))
                {
                    await ctx.PlugIn.DisposeAsync();
                }
                ctx.Scope.Dispose();
                return true;
            }
            return false;
        }

        public abstract void StartProcess(CancellationToken cancellationToken);
    }
}
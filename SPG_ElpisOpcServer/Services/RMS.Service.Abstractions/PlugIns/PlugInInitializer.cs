using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace RMS.Service.Abstractions.PlugIns
{
    public interface IPlugInInitalizer
    {
        Task ConfigureAsync();
    }

    /// <summary>
    /// PlugIn Initlaizer service used in <see cref="IPlugInBuilder.Configure(Func{IPlugIn, Task})"/>
    /// </summary>
    public class PlugInInitializer : IPlugInInitalizer
    {
        private readonly IServiceProvider services;

        private readonly Func<IPlugIn, Task> onConfig;

        public PlugInInitializer(IServiceProvider services, Func<IPlugIn, Task> onConfig)
        {
            this.services = services;
            this.onConfig = onConfig;
        }

        internal PlugInInitializer(IServiceProvider services, Action<IPlugIn> onConfig)
        {
            this.services = services;
            this.onConfig = (plugIn) =>
            {
                onConfig(plugIn);
                return Task.CompletedTask;
            };
        }

        public Task ConfigureAsync()
        {
            return onConfig(services.GetService<IPlugIn>());
        }
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RMS.Service.Abstractions.Providers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RMS.Service.Abstractions.PlugIns
{
    /// <summary>
    /// PlugIn builder instance for RMS
    /// </summary>
    public interface IPlugInBuilder
    {
        IDictionary<string, object> Properties { get; }
        IPlugIn Build();
        IPlugInBuilder UseContentRoot(string root);
        string PlugInDir { get; }
        IPlugInBuilder ConfigureConfiguration(Action<IConfigurationBuilder> configureDelegate);
        IPlugInBuilder ConfigureServices(Action<PlugInContext, IServiceCollection> configureDelegate);
        IPlugInBuilder Configure(Action<IPlugIn> configure);
        IPlugInBuilder Configure(Func<IPlugIn, Task> configure);
        IPlugInBuilder UseProvider<TProvider>(Func<IServiceProvider, TProvider> implement = null) where TProvider : class, IDeviceProvider;
    }
}

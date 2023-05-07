using Microsoft.Extensions.DependencyInjection;

namespace RMS.Service.Abstractions.PlugIns
{
    /// <summary>
    /// PlugInHost info for RMS
    /// </summary>
    public interface IPlugInHost
    {
        /// <summary>
        /// Type Id of RMS Device
        /// </summary>
        int TypeId { get; }

        /// <summary>
        /// Configure ASP.NET service
        /// </summary>
        /// <param name="services"></param>
        void Configure(IServiceCollection services);

        /// <summary>
        /// Configure Plugin service and configuration
        /// </summary>
        /// <param name="context">PlugIn App context which includes ASP.NET service and properties</param>
        /// <param name="builder">PlugIn builder used for configuring service</param>
        void ConfigurePlugIn(PlugInAppContext context, IPlugInBuilder builder);
    }
}

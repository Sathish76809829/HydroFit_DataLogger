using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RMS.Service.Abstractions.Configurations;
using RMS.Service.Abstractions.Database;
using RMS.Service.Abstractions.PlugIns;
using RMS.Service.Abstractions.Services;
using System;

namespace Daq
{
    /// <summary>
    /// Daq Entry to register services
    /// </summary>
    [HostInfo("mDaq", "1E0D30E3-7959-4246-BAF6-C005449E7D0E")]
    public class DaqHost : IPlugInHost
    {
        public int TypeId => 2;

        public void Configure(IServiceCollection services)
        {
        }

        public void ConfigurePlugIn(PlugInAppContext context, IPlugInBuilder builder)
        {
            builder.ConfigureServices((ctx, services) =>
            {
                services.Configure<CacheOptions>(options =>
                {
                    options.FormulaCacheInterval = TimeSpan.FromHours(2);
                });
                services.AddTransient<IFormulaRepository, DefaultFormulaRepository>();
                services.AddDbContext<IDbContext, DaqDbContext>((services, op) =>
                {
                    services.GetService<IDbFactory>()
                            .SqlServer<DaqDbContext>(ctx.Configuration.GetConnectionString("MSSql"), op);
                }, ServiceLifetime.Transient);
            });
            builder.UseProvider(provider =>
            {
                return new DaqDeviceProvider(provider);
            }).ConfigureConfiguration(config => config.AddJsonFile("config.json"));
        }
    }
}

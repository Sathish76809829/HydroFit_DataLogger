using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RMS.Service.Abstractions.Database;
using RMS.Service.Abstractions.PlugIns;
using RMS.Service.Abstractions.Providers;

namespace CngBooster
{
    /// <summary>
    /// Cng Device Entry point which will configure Services
    /// </summary>
    [HostInfo("Cng", "005D7180-019B-4AD8-902D-28C47FCC3D7D")]
    public class CngHost : IPlugInHost
    {
        public int TypeId => 3;

        public void Configure(IServiceCollection services)
        {
        }

        public void ConfigurePlugIn(PlugInAppContext context, IPlugInBuilder builder)
        {
            builder.UseProvider<CngDeviceProvider>()
                .ConfigureServices((ctx, services) =>
                {
                    services.AddTransient<SignalBitRepository>()
                    .AddTransient<SignalMemicRepository>()
                    .AddTransient<ADCCalculationRepository>()
                    .AddDbContext<IDbContext, CngDbContext>((services, op) => services.GetService<IDbFactory>().SqlServer<CngDbContext>(ctx.Configuration.GetConnectionString("MSSql"), op), ServiceLifetime.Transient);
                })
                .ConfigureConfiguration(config => config.AddJsonFile("config.json"));
        }
    }
}

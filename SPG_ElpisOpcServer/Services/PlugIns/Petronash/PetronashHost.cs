using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Petronash.Configurations;
using Petronash.IntegrationEvents.EventHandling;
using RMS.EventBus.Abstrations;
using RMS.Service.Abstractions.Database;
using RMS.Service.Abstractions.Events;
using RMS.Service.Abstractions.PlugIns;
using RMS.Service.Abstractions.Services;

namespace Petronash
{
    /// <summary>
    /// Petronash Entry which register services
    /// </summary>
    [HostInfo("Petronash", "4EE54C4C-9DF5-4668-9020-8EB02A4DCA24")]
    public class PetronashHost : IPlugInHost
    {
        public int TypeId => 5;

        public void ConfigurePlugIn(PlugInAppContext context, IPlugInBuilder builder)
        {
            var eventBus = context.AppServices.GetRequiredService<IEventBus>();
            builder.ConfigureServices((ctx, services) =>
            {
                services.AddDbContext<IDbContext, PetronashDbContext>((services, op) =>
                 {
                     services.GetRequiredService<IDbFactory>()
                             .SqlServer<PetronashDbContext>(ctx.Configuration.GetConnectionString("MSSql"), op.EnableSensitiveDataLogging(false));
                 }, ServiceLifetime.Scoped)
                .ConfigureOptions<PetronashCacheConfiguration>()
                .Configure<PetronashSettings>((options) => ctx.Configuration.Bind(options))
                .AddTransient<IFormulaRepository, DefaultFormulaRepository>()
                .AddTransient<PetronashSignalRepository>()
                .AddTransient<TestDetailsService>();
            }).UseProvider<PetronashDeviceProvider>()
            .ConfigureConfiguration(configuration => configuration.AddJsonFile("config.json", optional: true, reloadOnChange: true));
            eventBus.Subscribe<InputChangeEvent, InputChangeEventHandler>("PumpState");
        }

        public void Configure(IServiceCollection services)
        {
            services.AddSingleton<InputChangeEventHandler>();
        }
    }
}

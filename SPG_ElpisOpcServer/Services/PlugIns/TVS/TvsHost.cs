using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RMS.Service.Abstractions.Configurations;
using RMS.Service.Abstractions.Database;
using RMS.Service.Abstractions.PlugIns;
using RMS.Service.Abstractions.Services;
using System;

namespace TVS
{
    [HostInfo("TVS", "159F9CE5-1980-4229-902F-A0AF7A0F05B2")]
    public class TvsHost : IPlugInHost
    {
        public int TypeId => 6;

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
                services.AddDbContext<IDbContext, TvsDbContext>((services, op) =>
                {
                    services.GetService<IDbFactory>()
                            .SqlServer<TvsDbContext>(ctx.Configuration.GetConnectionString("MSSql"), op);
                }, ServiceLifetime.Transient);
            });
            builder.UseProvider(provider =>
            {
                return new TvsProvider();
            }).ConfigureConfiguration(config => config.AddJsonFile("config.json"));
        }
    }
}

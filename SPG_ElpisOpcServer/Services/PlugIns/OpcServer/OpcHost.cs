using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RMS.Service.Abstractions.Configurations;
using RMS.Service.Abstractions.Database;
using RMS.Service.Abstractions.PlugIns;
using RMS.Service.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpcServer
{
    [HostInfo("opc", "3DB3851C-47F1-4EAC-8BED-17EE02D99DB0")]
    class OpcHost : IPlugInHost
    {
        public int TypeId => 4;

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
                services.AddDbContext<IDbContext, OpcDbContext>((services, op) =>
                {
                    services.GetService<IDbFactory>()
                            .SqlServer<OpcDbContext>(ctx.Configuration.GetConnectionString("MSSql"), op);
                }, ServiceLifetime.Transient);

                services.AddTransient<OpcSignalRepository>();
            });
            builder.UseProvider(provider =>
            {
                
                return new OpcDeviceProvider(provider);
            }).ConfigureConfiguration(config => config.AddJsonFile("config.json"));
        }
    }
}

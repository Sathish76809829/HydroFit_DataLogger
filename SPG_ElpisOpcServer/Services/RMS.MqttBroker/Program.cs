using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using RMS.Broker.Extensions;

namespace RMS.Broker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls("https://*:5006");
            })
            .ConfigureLogging((ctx, builder) =>
            {
                builder.AddFile(ctx.Configuration);
            });
        }
    }
}

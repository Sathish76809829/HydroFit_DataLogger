#region Namespaces
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RMS.DataParser.Configurations;
using RMS.DataParser.PlugIns;
using RMS.DataParser.Services;
using RMS.EventBusKafka;
using RMS.Service.Abstractions;
using RMS.Service.Abstractions.Database;
using RMS.Service.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace RMS.DataParser
{
    /// <summary>
    /// Entry for ASP.NET application
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Environment setup and server register
        /// </summary>
        /// <param name="args">Application arguments</param>
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json")
                   .AddEnvironmentVariables("DOTNET_")
                   .AddEnvironmentVariables()
                   .AddCommandLine(args)
                   .Build();
            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseKestrel()
                .UseUrls("http://*:5003")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .Configure(app =>
                {
                    #region Routing
                    app.UseRouting();
                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.UseEndpoints(e =>
                    {
                        e.MapGet("/", async c =>
                        {
                            c.Response.ContentType = "text/plain";
                            c.Response.StatusCode = StatusCodes.Status200OK;
                            await c.Response.WriteAsync("Data Parser v1.00");
                        });
                        e.MapGet("/api/v1/plugins", GetPlugIns).RequireAuthorization();
                        e.MapGet("/api/v1/topics", GetTopics).RequireAuthorization();
                        e.MapPost("/api/v1/plugins/add", UsePlugIn).RequireAuthorization();
                        e.MapPost("/api/v1/plugins/remove", DeletePlugIn).RequireAuthorization();
                    }); 
                    #endregion
                })
                .ConfigureServices(new Action<WebHostBuilderContext, IServiceCollection>((context, services) =>
                {
                    #region Configure Services
                    var mssqlStr = context.Configuration.GetConnectionString("MSSql");
                    services
                    .Configure<DataParserOptions>(context.Configuration.GetSection("Topics"))
                    .AddKafka(context.Configuration)
                    .AddDbContext<IDbContext, RMSDbContext>(op => op.UseSqlServer(mssqlStr), ServiceLifetime.Scoped)
                    .AddTransient<LiveDataRepository>()
                    .AddTransient<IServiceFactory, ServiceFactory>()
                    .AddSingleton<IEventMonitor, EventMonitor>()
                    .AddSingleton<IDbFactory, DbFactory>()
                    .AddSingleton<PlugInService>()
                    .AddRouting()
                    .AddMemoryCache()
                    .AddPlugIns(Path.Combine(AppContext.BaseDirectory, "plugins"));
                    // Basic Authentication
                    services.AddAuthorization()
                            .AddAuthentication("BasicAuthentication")
                            .AddScheme<AuthenticationSchemeOptions, Services.AuthenticationService>("BasicAuthentication", null);
                    services.AddHostedService((provider) =>
                    {
                        var service = provider.GetRequiredService<PlugInService>();
                        return new Worker(service);
                    });

                    #endregion
                }))
                .ConfigureLogging((ctx, providers) =>
                {
                    #region Default Logging
                    providers.AddConfiguration(ctx.Configuration.GetSection("Logging"))
                                         .AddConsole(); 
                    #endregion
                });
            host.Build().Run();
        }

        #region EndPoints
        /// <summary>
        /// Http EndPoint for getting customer topics
        /// </summary>
        /// <param name="context">Http request information</param>
        /// <returns>Task for request</returns>
        async static Task GetTopics(HttpContext context)
        {
            var service = context.RequestServices.GetRequiredService<PlugInService>();
            context.Response.ContentType = "application/json";
            var res = new Dictionary<string, IList<Service.Abstractions.PlugIns.PlugInInfo>>();
            foreach (var item in service.DataProcess)
            {
                res.Add(item.Key, item.Value.GetPlugIns());
            }
            await context.Response.WriteAsync(JsonSerializer.Serialize(res));
        }

        /// <summary>
        /// Http EndPoint for delete topic for customer device (plugIn)
        /// </summary>
        /// <param name="context">Http request information</param>
        /// <returns>Task for request</returns>
        async static Task DeletePlugIn(HttpContext context)
        {
            var service = context.RequestServices.GetRequiredService<PlugInService>();
            // Check the request type
            if (context.Request.ContentType == "application/json")
            {
                context.Response.ContentType = "application/json";
                var details = await GetPlugInDetails(context);
                if (details.PlugInId == Guid.Empty
                    || string.IsNullOrEmpty(details.Topic)
                    || !await service.RemovePlugInAsync(details.Topic, details.PlugInId))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }
                context.Response.StatusCode = StatusCodes.Status200OK;
            }
        }

        /// <summary>
        /// Http endpoint for using customer topic for device
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        async static Task UsePlugIn(HttpContext context)
        {
            var service = context.RequestServices.GetRequiredService<PlugInService>();
            if (context.Request.ContentType == "application/json")
            {
                context.Response.ContentType = "application/json";
                var details = await GetPlugInDetails(context);
                if (details.PlugInId == Guid.Empty
                    || string.IsNullOrEmpty(details.Topic)
                    || !await service.UsePlugInAsync(details.Topic, details.PlugInId))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }
                context.Response.StatusCode = StatusCodes.Status200OK;
            }
        }

        async static Task<Models.PlugInTopicModel> GetPlugInDetails(HttpContext context)
        {
            Models.PlugInTopicModel plugInTopic = new Models.PlugInTopicModel();
            var document = await JsonDocument.ParseAsync(context.Request.Body);
            try
            {
                if (document.RootElement.ValueKind == JsonValueKind.Object)
                {
                    var topicElement = document.RootElement.GetProperty("topic");
                    if (topicElement.ValueKind == JsonValueKind.String)
                    {
                        plugInTopic.Topic = topicElement.GetString();
                    }
                    var idElement = document.RootElement.GetProperty("pluginId");
                    if (idElement.ValueKind == JsonValueKind.String)
                    {
                        plugInTopic.PlugInId = idElement.GetGuid();
                    }
                }
            }
            catch (JsonException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
            finally
            {
                document.Dispose();
            }
            return plugInTopic;
        }

        async static Task GetPlugIns(HttpContext context)
        {
            var container = context.RequestServices.GetRequiredService<PlugInContainer>();
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(container.PlugInInfos));
        }
        #endregion
    }
}

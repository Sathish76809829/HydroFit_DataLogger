using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RMS.Broker.Configuration;
using RMS.Broker.Mqtt;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace RMS.Broker
{
    /// <summary>
    /// ASP .NET Start up configuration service
    /// </summary>
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var mqttSettings = new MqttSettingsModel();
            Configuration.Bind("MQTT", mqttSettings);
            services.AddSingleton(mqttSettings);
            var kafkaSettings = new KafkaConsumerSettings();

            Configuration.Bind("Kafka:Consumer", kafkaSettings);

            services.AddSingleton(kafkaSettings);

            services.AddSingleton<MqttServerService>();
            services.AddSingleton<Services.ClientService>();
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new Utils.JsonDictionaryConverter());
                });
            services.AddHostedService<Services.KafkaService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "RMS Broker API (Linux)",
                    Version = "v1.04",
                    Description = "The public API for the RMS broker .",
                    License = new OpenApiLicense
                    {
                        Name = "MIT",
                        Url = new Uri("https://rms.data.source.elpisitsolutions.com")
                    },
                    Contact = new OpenApiContact
                    {
                        Name = "RMS Broker",
                        Email = string.Empty,
                        Url = new Uri("https://rms.data.source.elpisitsolutions.com")
                    },
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IServiceProvider services)
        {
            var env = services.GetService<IWebHostEnvironment>();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            var mqttService = services.GetService<MqttServerService>();
            ConfigureWebSocketEndpoint(app, mqttService, mqttService.Settings);
            mqttService.Configure();
            app.UseSwagger(o => o.RouteTemplate = "/api/{documentName}/swagger.json");

            app.UseSwaggerUI(o =>
            {
                o.RoutePrefix = "api";
                o.DocumentTitle = "RMS Broker API";
                o.SwaggerEndpoint("/api/v1/swagger.json", "RMS Broker API v1");
                o.DisplayRequestDuration();
                o.DocExpansion(DocExpansion.List);
                o.DefaultModelRendering(ModelRendering.Model);
            });
        }

        static void ConfigureWebSocketEndpoint(
           IApplicationBuilder application,
           MqttServerService mqttServerService,
           MqttSettingsModel mqttSettings)
        {
            if (mqttSettings?.WebSocketEndPoint?.Enabled != true)
            {
                return;
            }


            string socketPath = mqttSettings.WebSocketEndPoint.Path;
            if (string.IsNullOrEmpty(socketPath))
            {
                return;
            }

            var webSocketOptions = new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(mqttSettings.WebSocketEndPoint.KeepAliveInterval)
            };

            List<string> allowedOrigins = mqttSettings.WebSocketEndPoint.AllowedOrigins;
            if (allowedOrigins?.Any() == true)
            {
                allowedOrigins.ForEach(webSocketOptions.AllowedOrigins.Add);
            }

            application.UseWebSockets(webSocketOptions);
            application.Use(async (context, next) =>
            {
                if (context.Request.Path == socketPath)
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        string subProtocol = null;
                        if (context.Request.Headers.TryGetValue("Sec-WebSocket-Protocol", out var requestedSubProtocolValues))
                        {
                            subProtocol = SelectSubProtocol(requestedSubProtocolValues);
                        }

                        using var webSocket = await context.WebSockets.AcceptWebSocketAsync(subProtocol).ConfigureAwait(false);
                        await mqttServerService.RunWebSocketConnectionAsync(webSocket, context).ConfigureAwait(false);
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    }
                }
                else
                {
                    await next().ConfigureAwait(false);
                }
            });
        }

        public static string SelectSubProtocol(IList<string> requestedSubProtocolValues)
        {
            if (requestedSubProtocolValues == null)
                throw new ArgumentNullException(nameof(requestedSubProtocolValues));

            // Order the protocols to also match "mqtt", "mqttv-3.1", "mqttv-3.11" etc.
            return requestedSubProtocolValues
                .OrderByDescending(p => p.Length)
                .FirstOrDefault(p => p.ToLower().StartsWith("mqtt"));
        }
    }
}

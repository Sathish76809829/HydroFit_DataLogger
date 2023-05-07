using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RMS.SocketServer.Services;
using System;

namespace RMS.SocketServer
{
    /// <summary>
    /// Start up class which will register dependency service 
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
            var socketSettings = new Configurations.SocketConfiguration();
            Configuration.Bind("Socket", socketSettings);
            services.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions, Services.AuthenticationService>("BasicAuthentication", null);
            services.AddSingleton(socketSettings)
                .AddMemoryCache()
                .AddSingleton<Net.SocketServer>()
                .AddHostedService<SocketService>()
                .AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Net.SocketServer server)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            var socketOptions = server.SocketConfiguration.WebSocketEndPoint;
            string socketPath = socketOptions.Path;
            if (string.IsNullOrEmpty(socketPath))
            {
                return;
            }

            var webSocketOptions = new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(socketOptions.KeepAliveInterval)
            };

            var allowedOrigins = socketOptions.AllowedOrigins;
            if (allowedOrigins?.Count > 0)
            {
                allowedOrigins.ForEach(webSocketOptions.AllowedOrigins.Add);
            }

            app.UseWebSockets(webSocketOptions);
            app.Use(async (context, next) =>
            {
                if (context.Request.Path == socketPath)
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        string subProtocol = null;
                        if (context.Request.Headers.TryGetValue("Sec-WebSocket-Protocol", out var requestedSubProtocolValues))
                        {
                            subProtocol = "transport";
                        }

                        using (var webSocket = await context.WebSockets.AcceptWebSocketAsync(subProtocol).ConfigureAwait(false))
                        {
                            await server.UseSocket(webSocket, context).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    }
                }
                else
                {
                    await next().ConfigureAwait(false);
                }
            });
        }
    }
}

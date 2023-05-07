using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace RMS.SocketServer.Services
{
    /// <summary>
    /// Socket service which will create socket server 
    /// </summary>
    public class SocketService : BackgroundService
    {
        private readonly Net.SocketServer server;

        public SocketService(Net.SocketServer server)
        {
            this.server = server;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            server.Start();
            return Task.CompletedTask;
        }

        public async override Task StopAsync(CancellationToken cancellationToken)
        {
            await server.Stop(cancellationToken);
        }

        public override void Dispose()
        {
            server.Dispose();
            base.Dispose();
        }
    }
}

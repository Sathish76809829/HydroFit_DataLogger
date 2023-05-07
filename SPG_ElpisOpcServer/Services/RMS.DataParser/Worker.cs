using Microsoft.Extensions.Hosting;
using RMS.DataParser.Services;
using System.Threading;
using System.Threading.Tasks;

namespace RMS.DataParser
{
    /// <summary>
    /// RMS Woker Hosted service which will create Data Parser process <see cref="DataProcessService"/>
    /// </summary>
    public class Worker : IHostedService
    {
        private readonly PlugInService _service;

        public Worker(PlugInService service)
        {
            _service = service;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _service.LoadProviders();
            await _service.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _service.StopAsync();
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using RMS.Service.Abstractions.Providers;
using System;
using System.Threading;

namespace RMS.DataParser.Services.Opc
{
    public class OpcDataProcessContext : DataServiceContextBase<IOpcDeviceProvider>
    {
        public OpcDataProcessContext(IServiceProvider services, string topic) : base(services, topic)
        {
        }

        public async override void StartProcess(CancellationToken cancellationToken)
        {
            using IServiceScope scope = Services.CreateScope();
            Service = new OpcDataProcessService(scope.ServiceProvider, Providers);
            await Service.DoWork(Topic, cancellationToken);
        }
    }
}

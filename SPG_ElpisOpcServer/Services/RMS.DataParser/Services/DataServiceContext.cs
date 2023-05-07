using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RMS.DataParser.Configurations;
using RMS.EventBusKafka;
using RMS.Service.Abstractions.PlugIns;
using RMS.Service.Abstractions.Providers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RMS.DataParser.Services
{
    #region Context
    /// <summary>
    /// Includes information such as providers to use 
    /// </summary>
    public class DataServiceContext : DataServiceContextBase<IRmsDeviceProvider>
    {
        public DataServiceContext(IServiceProvider services, string topic) : base(services, topic)
        {
        }

        public async override void StartProcess(CancellationToken cancellationToken)
        {
            using IServiceScope scope = Services.CreateScope();
            Service = new DataProcessService(scope.ServiceProvider, Providers);
            await Service.DoWork(Topic, cancellationToken);
        }
        
    }
    #endregion
}

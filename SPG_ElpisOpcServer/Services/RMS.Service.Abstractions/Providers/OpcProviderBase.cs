using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RMS.Service.Abstractions.Providers
{
    public abstract class OpcProviderBase : IOpcDeviceProvider
    {
        public virtual ValueTask InitalizeAsync()
        {
#if NET5_0
            return ValueTask.CompletedTask;
#else
            return default(ValueTask);
#endif
        }

        public abstract Task<IParsedItems> ProcessDataAsync(Models.OpcDataModel deviceData);
    }
}

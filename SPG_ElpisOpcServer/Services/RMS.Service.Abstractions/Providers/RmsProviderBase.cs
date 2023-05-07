using RMS.Service.Models;
using System.Threading.Tasks;

namespace RMS.Service.Abstractions.Providers
{
    /// <summary>
    /// <see cref="IDeviceProvider"/> abstract implementation
    /// </summary>
    public abstract class RmsProviderBase : IRmsDeviceProvider
    {
        public virtual ValueTask InitalizeAsync()
        {
#if NET5_0
            return ValueTask.CompletedTask;
#else
            return default(ValueTask);
#endif
        }

        public abstract Task<IParsedItems> ProcessDataAsync(DeviceDataModel deviceData);

    }
}

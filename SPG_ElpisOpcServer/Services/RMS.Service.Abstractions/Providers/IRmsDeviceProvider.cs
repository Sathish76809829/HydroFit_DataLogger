using RMS.Service.Models;
using System.Threading.Tasks;

namespace RMS.Service.Abstractions.Providers
{
    public interface IRmsDeviceProvider : IDeviceProvider
    {
        /// <summary>
        /// Process the Device data from customer
        /// </summary>
        /// <param name="deviceData">Device data from customer</param>
        /// <returns>List of parsed data</returns>
        Task<IParsedItems> ProcessDataAsync(DeviceDataModel deviceData);
    }
}

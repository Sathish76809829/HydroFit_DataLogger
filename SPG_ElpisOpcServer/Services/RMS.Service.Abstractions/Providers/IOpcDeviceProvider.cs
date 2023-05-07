using System.Threading.Tasks;

namespace RMS.Service.Abstractions.Providers
{
    /// <summary>
    /// Device Provider for Opc
    /// </summary>
    public interface IOpcDeviceProvider : IDeviceProvider
    {
        /// <summary>
        /// Opc data process
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        Task<IParsedItems> ProcessDataAsync(Models.OpcDataModel dataModel);
        
    }
}

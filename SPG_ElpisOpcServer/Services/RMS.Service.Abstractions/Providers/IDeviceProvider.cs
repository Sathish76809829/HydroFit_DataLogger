using RMS.Service.Abstractions.Models;
using RMS.Service.Models;
using System.Threading.Tasks;

namespace RMS.Service.Abstractions.Providers
{
    /// <summary>
    /// Device provider interface which will be called from RMS Data Parser
    /// </summary>
    public interface IDeviceProvider
    {
        /// <summary>
        /// Initializer the provider
        /// </summary>
        /// <returns></returns>
        ValueTask InitalizeAsync();
    }
}

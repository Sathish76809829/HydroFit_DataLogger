using RMS.Service.Abstractions;
using RMS.Service.Abstractions.Services;

namespace RMS.DataParser.Services
{
    /// <summary>
    /// Service factory implementation for Service 
    /// </summary>
    public class ServiceFactory : IServiceFactory
    {
        public IScriptService CreateScript()
        {
            return new ScriptService();
        }
    }
}

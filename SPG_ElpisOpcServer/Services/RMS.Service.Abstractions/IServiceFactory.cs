namespace RMS.Service.Abstractions
{
    /// <summary>
    /// Factory class for creating RMS Services
    /// </summary>
    public interface IServiceFactory
    {
        Services.IScriptService CreateScript();
    }
}

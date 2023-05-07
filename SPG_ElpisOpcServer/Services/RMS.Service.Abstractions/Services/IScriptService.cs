using RMS.Service.Models;
using System.Collections.Generic;

namespace RMS.Service.Abstractions.Services
{
    /// <summary>
    /// Script service which will execute basic formulas
    /// </summary>
    public interface IScriptService
    {
        IDictionary<string, object> Values { get; }

        IScriptScope Begin(SignalSet<DataSendModel> values);

        object Invoke(string script);
    }
}

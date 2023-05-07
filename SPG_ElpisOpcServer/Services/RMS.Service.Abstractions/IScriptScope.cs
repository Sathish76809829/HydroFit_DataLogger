using System;
using System.Collections.Generic;

namespace RMS.Service.Abstractions
{
    /// <summary>
    /// Script scope for executing script
    /// </summary>
    public interface IScriptScope : IDisposable
    {
        IDictionary<string, object> Variables { get; }
    }
}

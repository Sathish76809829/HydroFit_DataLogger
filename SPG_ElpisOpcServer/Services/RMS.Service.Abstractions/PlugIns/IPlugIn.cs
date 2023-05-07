using System;

namespace RMS.Service.Abstractions.PlugIns
{
    /// <summary>
    /// PlugIn interface for RMS
    /// </summary>
    public interface IPlugIn : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Context for the plugIn with includes configuration and info
        /// </summary>
        PlugInContext Context { get; }

        /// <summary>
        /// Dependency service for PlugIn
        /// </summary>
        IServiceProvider Services { get; }

        /// <summary>
        /// PlugIn info for the instance
        /// </summary>
        PlugInInfo Info { get; }
    }
}
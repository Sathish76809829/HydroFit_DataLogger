using RMS.Service.Abstractions.PlugIns;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RMS.Service.Abstractions.Providers
{
    /// <summary>
    /// PlugIn manager interface which includes list of plugIns
    /// </summary>
    public interface IPlugInManager
    {
        IReadOnlyList<IPlugInHost> PlugIns { get; }

        TPlugIn Find<TPlugIn>(Func<TPlugIn, bool> filter = null) where TPlugIn : IPlugInHost;
    }

    /// <summary>
    /// Default implementation of <see cref="IPlugInManager"/>
    /// </summary>
    public class DefaultPlugInManager : IPlugInManager
    {
        private readonly IPlugInHost[] _plugIns;

        public DefaultPlugInManager(IEnumerable<IPlugInHost> plugIns)
        {
            var items = plugIns as IPlugInHost[];
            if (items != null)
            {
                _plugIns = items;
            }
            else
            {
                _plugIns = plugIns.ToArray();
            }
        }

        public IReadOnlyList<IPlugInHost> PlugIns => _plugIns;

        public TPlugInHost Find<TPlugInHost>(Func<TPlugInHost, bool> filter) where TPlugInHost : IPlugInHost
        {
            if (filter == null)
                filter = (f) => true;
            return (TPlugInHost)Array.Find(_plugIns, p => p is TPlugInHost && filter((TPlugInHost)p));
        }
    }
}

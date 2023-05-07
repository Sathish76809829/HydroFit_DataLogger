using System;
using System.Collections.Generic;

namespace RMS.Service.Abstractions.PlugIns
{
    /// <summary>
    /// Initial PlugIn App Context
    /// </summary>
    public class PlugInAppContext
    {
        private readonly Dictionary<string, object> properies;

        private readonly IServiceProvider appServices;

        public PlugInAppContext(IServiceProvider appServices)
        {
            this.appServices = appServices;
            properies = new Dictionary<string, object>();
        }

        public IDictionary<string, object> Properies => properies;

        /// <summary>
        /// ASP.NET dependency services
        /// </summary>
        public IServiceProvider AppServices => appServices;
    }
}

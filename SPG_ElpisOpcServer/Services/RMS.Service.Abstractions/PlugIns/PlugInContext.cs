using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace RMS.Service.Abstractions.PlugIns
{
    /// <summary>
    /// PlugIn context instance for <see cref="IPlugInHost.ConfigurePlugIn(PlugInAppContext, IPlugInBuilder)"/>
    /// </summary>
    public class PlugInContext
    {
        public PlugInContext(IDictionary<string, object> properties)
        {
            Properties = properties;
        }

        public string PlugInPath { get; set; }

        public IDictionary<string, object> Properties { get; set; }

        public IConfiguration Configuration { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace RMS.Service.Abstractions.PlugIns
{
    /// <summary>
    /// PlugIn info which includes plug in directory info, type and Guid
    /// </summary>
    public class PlugInInfo
    {
        internal readonly IDictionary<string, object> Properties;

        internal PlugInInfo(IDictionary<string, object> properties)
        {
            Properties = properties;
        }

        public PlugInInfo(string name, Guid id)
        {
            Properties = new Dictionary<string, object>()
            {
                [PlugIn.PlugInNameKey] = name,
                [PlugIn.PlugInIdKey] = id
            };
        }

        public object this[string key]
        {
            get => Properties[key];
            set => Properties[key] = value;
        }

        public string PlugInDir { get => (string)Properties[PlugIn.PlugInDirKey]; }

        public Guid Id { get => (Guid)Properties[PlugIn.PlugInIdKey]; }

        public int TypeId { get => (int)Properties[PlugIn.PlugInTypeIdKey]; }

        public string Name { get => (string)Properties[PlugIn.PlugInNameKey]; }
    }
}

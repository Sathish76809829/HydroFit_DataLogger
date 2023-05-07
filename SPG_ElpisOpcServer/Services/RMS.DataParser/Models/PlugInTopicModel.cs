using System;

namespace RMS.DataParser.Models
{
    /// <summary>
    /// PlugIn Topic to use for Data Parsing
    /// </summary>
    public class PlugInTopicModel
    {
        public string Topic { get; set; }

        public Guid PlugInId { get; set; }
    }
}

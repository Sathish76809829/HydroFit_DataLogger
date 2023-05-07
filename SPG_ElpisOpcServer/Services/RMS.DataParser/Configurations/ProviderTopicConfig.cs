using System;
using System.Collections.Generic;

namespace RMS.DataParser.Configurations
{
    /// <summary>
    /// List of customer topic which will create a device data process
    /// </summary>
    public class ProviderTopicConfig
    {
        /// <summary>
        /// Topic for device data process
        /// </summary>
        public Dictionary<string, List<Guid>> IncommingTopics { get; set; }
    }
}

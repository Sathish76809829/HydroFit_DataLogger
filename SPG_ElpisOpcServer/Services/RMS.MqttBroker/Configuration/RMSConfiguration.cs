using System;

namespace RMS.Broker.Configuration
{
    /// <summary>
    /// RMS Endpoint configuration
    /// </summary>
    public class RMSConfiguration
    {
        public Uri BaseUrl { get; set; }

        public string AuthEndPoint { get; set; }

        public string PreferenceEndPoint { get; set; }

        //public string ADCComputationEndPoint { get; set; }
    }
}

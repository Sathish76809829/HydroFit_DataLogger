using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OpcServer.Models
{
    public class SignalInfo
    {
        [JsonPropertyName("deviceId")]
        public string deviceId { get; set; }

        [JsonPropertyName("signalName")]
        public string signalName { get; set; }

        [JsonPropertyName("signalId")]
        public /*int*/string signalId { get; set; }
    }

   
}

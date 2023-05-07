using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RMS.Broker.Models
{
    public class ADCInputModel
    {
        [JsonPropertyName("signalId")]
        public string SignalId { get; set; }

        [JsonPropertyName("mValue")]
        public double MValue { get; set; }

        [JsonPropertyName("cValue")]
        public double CValue { get; set; }
    }
}

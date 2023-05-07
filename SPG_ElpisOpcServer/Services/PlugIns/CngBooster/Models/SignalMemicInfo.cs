using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CngBooster.Models
{
    public class SignalMemicInfo
    {
        [JsonPropertyName("signalId")]
        public string SignalId { get; set; }

        [JsonPropertyName("dataValue")]
        public object DataValue { get; set; }

        [JsonPropertyName("comparator")]
        public string Comparator { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

}

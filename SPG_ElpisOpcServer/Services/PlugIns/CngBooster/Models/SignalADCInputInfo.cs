using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CngBooster.Models
{
    public class SignalADCInputInfo
    {
        [JsonPropertyName("signalId")]
        public string SignalId { get; set; }

        [JsonPropertyName("mValue")]
        public double MValue { get; set; }

        [JsonPropertyName("cValue")]
        public double CValue { get; set; }
    }
}

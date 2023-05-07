using System.Text.Json.Serialization;

namespace RMS.Broker.Models
{
    /// <summary>
    /// Signal model for RMS which includes signalId & deviceId
    /// </summary>
    public class SignalModel
    {
        [JsonPropertyName("signalId")]
        public /*int*/string SignalId { get; set; }

        [JsonPropertyName("deviceId")]
        public /*int*/string DeviceId { get; set; }
    }
}

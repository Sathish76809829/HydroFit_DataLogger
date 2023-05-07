using System.Text.Json.Serialization;

namespace RMS.Broker.Models
{
    /// <summary>
    /// User Signal Model that will sent to mqtt
    /// </summary>
    public class UserSignalModel : IJsonWritter
    {
        [JsonPropertyName("deviceId")]
        public /*int*/string DeviceId { get; set; }

        [JsonPropertyName("signalId")]
        public /*int*/string SignalId { get; set; }

        [JsonPropertyName("dataValue")]
        public System.Text.Json.JsonElement DataValue { get; set; }

        [JsonPropertyName("timeReceived")]
        public string TimeRecieved { get; set; }

        public void Write(System.Text.Json.Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            //writer.WriteNumber("deviceId", DeviceId);
            //writer.WriteNumber("signalId", SignalId);
            writer.WriteString("deviceId", DeviceId);
            writer.WriteString("signalId", SignalId);
            writer.WritePropertyName("dataValue");
            DataValue.WriteTo(writer);
            writer.WriteString("timeReceived", TimeRecieved);
            writer.WriteEndObject();
        }
    }
}

using RMS.Broker.Utils;
using System;
using System.Text.Json.Serialization;

namespace RMS.Broker.Models
{
    /// <summary>
    /// User signal publish model for <see cref="Controllers.MessagesController.PublishMessage(long, UserSignalPublishModel[])"/>
    /// </summary>
    public class UserSignalPublishModel : IJsonWritter
    {
        [JsonPropertyName("deviceId")]
        public /*int*/string DeviceId { get; set; }

        [JsonPropertyName("signalId")]
        public /*int*/string SignalId { get; set; }

        [JsonPropertyName("dataValue")]
        public System.Text.Json.JsonElement DataValue { get; set; }

        [JsonPropertyName("timeReceived")]
        public DateTime TimeRecieved { get; set; }

        public void Write(System.Text.Json.Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            //writer.WriteNumber(JsonPropertyKeys.DeviceId, DeviceId);
            //writer.WriteNumber(JsonPropertyKeys.SignalId, SignalId);
            writer.WriteString("deviceId", DeviceId);
            writer.WriteString("signalId", SignalId);
            writer.WritePropertyName(JsonPropertyKeys.DataValue);
            DataValue.WriteTo(writer);
            writer.WriteString(JsonPropertyKeys.TimeReceived, TimeRecieved);
            writer.WriteEndObject();
        }
    }
}

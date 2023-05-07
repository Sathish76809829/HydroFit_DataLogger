using RMS.EventBus.Events;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RMS.Service.Abstractions.Events
{
    /// <summary>
    /// when Input from dashboard send API call to DeviceAPI
    /// </summary>
    public class InputChangeEvent : IntegrationEvent
    {
        /// <summary>
        /// Json Value of Signal
        /// </summary>
        [JsonPropertyName("signal")]
        public JsonElement Signal { get; set; }
    }
}

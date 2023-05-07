using System.Text.Json.Serialization;

namespace RMS.Broker.Models
{
    /// <summary>
    /// Signal Group Model for RMS
    /// </summary>
    public class SignalGroupModel
    {
        [JsonPropertyName("signalGroupId")]
        public int SignalGroupId { get; set; }
    }
}

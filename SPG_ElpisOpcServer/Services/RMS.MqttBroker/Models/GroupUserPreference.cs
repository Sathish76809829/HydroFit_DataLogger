using System.Text.Json.Serialization;

namespace RMS.Broker.Models
{
    /// <summary>
    /// RMS User group preference
    /// </summary>
    public class GroupUserPreference : UserPreference
    {
        [JsonPropertyName("signalGroupModel")]
        public SignalGroupModel SignalGroupModel { get; set; }
    }
}

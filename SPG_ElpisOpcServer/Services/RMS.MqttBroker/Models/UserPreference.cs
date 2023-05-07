using System.Text.Json.Serialization;

namespace RMS.Broker.Models
{
    /// <summary>
    /// User Preference model for RMS
    /// </summary>
    public class UserPreference
    {
        [JsonPropertyName("signalModel")]
        public SignalModel SignalModel { get; set; }

    }
}

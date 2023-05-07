using System.Text.Json.Serialization;

namespace CngBooster.Models
{
    /// <summary>
    /// Signal bit info for analog values
    /// </summary>
    public class SignalBitInfo
    {
        [JsonPropertyName("signalBitId")]
        public int SignalBitId { get; set; }
        [JsonPropertyName("signalId")]
        public /*int*/string SignalId { get; set; }
        [JsonPropertyName("bitId")]
        public int BitId { get; set; }
    }
}

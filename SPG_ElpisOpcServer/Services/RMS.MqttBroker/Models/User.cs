using System.Text.Json.Serialization;

namespace RMS.Broker.Models
{
    /// <summary>
    /// User account class for RMS
    /// </summary>
    public class User
    {
        [JsonPropertyName("userAccountId")]
        public long UserAcountId { get; set; }

        [JsonPropertyName("customerId")]
        public long CustomerId { get; set; }

        [JsonPropertyName("userRoleId")]
        public int UserRoleId { get; set; }

        [JsonPropertyName("userPreference")]
        public string UserPreference { get; set; }

        [JsonPropertyName("groupUserPreference")]
        public string GroupUserPreference { get; set; }
    }
}

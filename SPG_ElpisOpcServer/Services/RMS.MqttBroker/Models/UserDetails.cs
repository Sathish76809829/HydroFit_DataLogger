using System.Text.Json.Serialization;

namespace RMS.Broker.Models
{
    public class UserDetails
    {
        [JsonPropertyName("userAccountModel")]
        public User User
        {
            get;
            set;
        }
    }
}

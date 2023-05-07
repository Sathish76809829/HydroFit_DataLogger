using System.Text.Json.Serialization;

namespace RMS.SocketServer.Models
{
    /// <summary>
    /// Info about client Id and IP address
    /// </summary>
    public class ClientInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }
    }
}

using System.Text.Json;

namespace RMS.Broker.Models
{
    /// <summary>
    /// Abstract interface to serialize as json
    /// </summary>
    public interface IJsonWritter
    {
        void Write(Utf8JsonWriter writter);
    }
}
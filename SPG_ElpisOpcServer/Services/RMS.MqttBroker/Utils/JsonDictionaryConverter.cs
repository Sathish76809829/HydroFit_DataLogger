using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RMS.Broker.Utils
{
    /// <summary>
    /// <see cref="IDictionary&lt;object, object&gt;"/> to json converter
    /// </summary>
    public class JsonDictionaryConverter : JsonConverter<IDictionary<object, object>>
    {
        public override IDictionary<object, object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, IDictionary<object, object> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var item in value)
            {
                writer.WriteStartObject();
                writer.WriteString("key", item.Key.ToString());
                writer.WritePropertyName("value");
                if (item.Value is Models.User user)
                {
                    writer.WriteStartObject();
                    writer.WriteNumber("userAcountId", user.UserAcountId);
                    writer.WriteNumber("customerId", user.CustomerId);
                    writer.WriteEndObject();
                }
                else
                {
                    writer.WriteStringValue(item.Value?.ToString());
                }

                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
    }
}

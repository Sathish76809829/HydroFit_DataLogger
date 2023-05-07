using Confluent.Kafka;
using RMS.Service.Abstractions.Parser;
using System;

namespace RMS.DataParser.Utils
{
    /// <summary>
    /// Json Deserializer for Kafka <see cref="IDeserializer{T}"/>
    /// </summary>
    public class JsonSerializer : IDeserializer<JsonSource>
    {
        public JsonSource Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            return new JsonSource(data.ToArray());
        }
    }
}

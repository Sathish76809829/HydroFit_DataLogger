using RMS.Broker.Models;
using System;
using System.Threading.Tasks;

namespace RMS.Broker.Utils
{
    /// <summary>
    /// Json writter for kafka data
    /// </summary>
    public class JsonBufferWriter : IDisposable, IAsyncDisposable
    {
        private readonly System.Buffers.ArrayBufferWriter<byte> buffer;

        private readonly System.Text.Json.Utf8JsonWriter writer;

        public JsonBufferWriter()
        {
            buffer = new System.Buffers.ArrayBufferWriter<byte>();
            writer = new System.Text.Json.Utf8JsonWriter(buffer);
        }

        public async Task<byte[]> GetBytesAsync(System.Collections.Generic.IReadOnlyList<IJsonWritter> items)
        {
            writer.Reset();
            writer.WriteStartArray();
            foreach (var item in items)
            {
                item.Write(writer);
            }
            writer.WriteEndArray();
            await writer.FlushAsync();
            return buffer.WrittenMemory.ToArray();
        }

        public void Dispose()
        {
            writer.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            return writer.DisposeAsync();
        }
    }
}

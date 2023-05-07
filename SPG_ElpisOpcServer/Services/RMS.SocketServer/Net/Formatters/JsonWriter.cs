using RMS.SocketServer.Net.Messages;

namespace RMS.SocketServer.Net.Formatters
{
    /// <summary>
    /// Json writer for sending json message to web socket clients
    /// </summary>
    public class JsonWriter 
    {
        private readonly System.Buffers.ArrayBufferWriter<byte> buffer;
        private readonly System.Text.Json.Utf8JsonWriter jsonWriter;

        public JsonWriter()
        {
            buffer = new System.Buffers.ArrayBufferWriter<byte>();
            jsonWriter = new System.Text.Json.Utf8JsonWriter(buffer);
        }

        public byte[] CreateHeader()
        {
            throw new System.NotImplementedException();
        }

        public void Reset()
        {
            buffer.Clear();
            jsonWriter.Reset();
        }

        public byte[] Encode(IMessageResponse response)
        {
            buffer.Clear();
            jsonWriter.Reset();
            jsonWriter.WriteStartObject();
            jsonWriter.WriteNumber(JsonPropertyKeys.Type, (int)response.ResponseType);
            jsonWriter.WriteString(JsonPropertyKeys.Message, response.Message.Value);
            jsonWriter.WriteEndObject();
            jsonWriter.Flush();
            return buffer.WrittenMemory.ToArray();
        }

        public MessageHeader ReadHeader(byte[] header)
        {
            throw new System.NotImplementedException("Not Supported");
        }
    }
}

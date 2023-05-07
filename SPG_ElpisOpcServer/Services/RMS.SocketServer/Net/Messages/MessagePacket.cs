namespace RMS.SocketServer.Net.Messages
{
    /// <summary>
    /// Json message from web socket client <see cref="WebSocketConnection"/>
    /// </summary>
    public readonly struct MessagePacket : IMessage
    {
        public readonly byte[] Buffer;
        public readonly int Count;

        public MessagePacket(byte[] buffer, int count)
        {
            Buffer = buffer;
            Count = count;
        }

        public IUserMessage GetMessage()
        {
            var reader = new System.Text.Json.Utf8JsonReader(new System.Buffers.ReadOnlySequence<byte>(Buffer, 0, Count));
            UserMessage message = default;
            while (reader.Read())
            {
                if (reader.TokenType == System.Text.Json.JsonTokenType.PropertyName)
                {
                    if (reader.ValueTextEquals(JsonPropertyKeys.Type))
                    {
                        reader.Read();
                        message.Type = (MessageType)reader.GetInt32();
                        continue;
                    }
                    if (reader.ValueTextEquals(JsonPropertyKeys.Message))
                    {
                        reader.Read();
                        message.Value = reader.ValueSpan.ToArray();
                        continue;
                    }
                    if (reader.ValueTextEquals(JsonPropertyKeys.Retain))
                    {
                        reader.Read();
                        message.Retain = reader.GetBoolean();
                        continue;
                    }
                    if (reader.ValueTextEquals(JsonPropertyKeys.Quality))
                    {
                        reader.Read();
                        message.Quality = (DeliveryQuality)reader.GetInt32();
                        continue;
                    }
                    if (reader.ValueTextEquals(JsonPropertyKeys.Id))
                    {
                        reader.Read();
                        message.Id = reader.GetUInt16();
                        continue;
                    }
                }
            }
            return message;
        }
    }
}
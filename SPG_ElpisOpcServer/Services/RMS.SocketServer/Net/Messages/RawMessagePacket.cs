namespace RMS.SocketServer.Net.Messages
{
    public readonly struct RawMessagePacket : IMessage, IUserMessage
    {
        public readonly MessageHeader Header;
        public readonly byte[] Buffer;

        public RawMessagePacket(MessageHeader header, byte[] buffer)
        {
            Header = header;
            Buffer = buffer;
        }

        public MessageType Type => Header.Type;

        public byte[] Value => Buffer;

        public bool Retain => Header.Retain;

        public DeliveryQuality Quality => Header.Quality;

        public ushort Id => Header.Id;

        public IUserMessage GetMessage()
        {
            return this;
        }
    }
}

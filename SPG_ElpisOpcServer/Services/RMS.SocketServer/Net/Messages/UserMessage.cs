namespace RMS.SocketServer.Net.Messages
{
    /// <summary>
    /// User message from client
    /// </summary>
    public struct UserMessage : IUserMessage
    {
        public MessageType Type { get; set; }

        public byte[] Value { get; set; }

        public bool Retain { get; set; }

        public DeliveryQuality Quality { get; set; }

        public ushort Id { get; set; }
    }
}

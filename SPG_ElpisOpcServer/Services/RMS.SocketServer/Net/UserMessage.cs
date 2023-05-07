namespace RMS.SocketServer.Net
{
    /// <summary>
    /// Used message from socket client
    /// </summary>
    public interface IUserMessage
    {
        public MessageType Type { get; }
        bool Retain { get; }
        DeliveryQuality Quality { get; } 
        ushort Id { get; }
        public byte[] Value { get; }
    }
}
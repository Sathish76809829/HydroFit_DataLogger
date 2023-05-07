namespace RMS.SocketServer.Net
{
    /// <summary>
    /// Message type enumeration for identifying message
    /// </summary>
    public enum MessageType : byte
    {
        None = 0,
        FromClient = 1,
        FromDevice = 2,
        ToDevice = 4,
        ToClient = 8,
        Ping = 16,
        DeviceReply = 32,
        DeviceData=44,
        External = 255
    }

    /// <summary>
    /// Delivery quality for sending message to TCP Client 
    /// </summary>
    public enum DeliveryQuality
    {
        None = 0,
        AtLeaseOnce = 1,
        ExactlyOnce = 2,
        Synchronous = 4
    }
}
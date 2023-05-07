namespace ElpisOpcServer.SocketService.Net
{
    public struct MessageHeader
    {
        /// <summary>
        /// Length of the payload
        /// </summary>
        public int Length;
        /// <summary>
        /// Type of message
        /// </summary>
        public MessageType Type;
        /// <summary>
        /// Shoult retain message for some interval
        /// </summary>
        public bool Retain;
        /// <summary>
        /// Id for message
        /// </summary>
        public ushort Id;
        /// <summary>
        /// Delivery quality for message
        /// </summary>
        public DeliveryQuality Quality;
    }
}
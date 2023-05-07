namespace RMS.SocketServer.Net.Messages
{
    /// <summary>
    /// Delivery response for socket client
    /// </summary>
    public readonly struct DeliveryResponse : IUserMessage, IMessageResponse
    {
        public MessageType ResponseType => MessageType.DeviceReply;

        private readonly IUserMessage _request;
        private readonly string _clientId;

        public DeliveryResponse(string clientId, IUserMessage message)
        {
            _clientId = clientId;
            _request = message;
        }

        public string ClientId => _clientId;

        public MessageType RequestType => _request.Type;

        public IUserMessage Message => this;

        public MessageType Type => MessageType.DeviceReply;

        public bool Retain => false;

        public DeliveryQuality Quality => DeliveryQuality.None;

        public ushort Id => _request.Id;

        public byte[] Value => _request.Value;

        public void SetResult(bool status)
        {
        }
    }
}

namespace RMS.SocketServer.Net.Messages
{
    /// <summary>
    /// Device message from socket client
    /// </summary>
    public readonly struct DeviceResponse : IMessageResponse
    {
        private readonly IUserMessage _message;

        private readonly string _clientId;

        public DeviceResponse(string clientId, IUserMessage message)
        {
            _clientId = clientId;
            _message = message;
        }

        public MessageType RequestType => _message.Type;

        public MessageType ResponseType => MessageType.ToDevice;

        public string ClientId => _clientId;

        public IUserMessage Message => _message;

        public void SetResult(bool status)
        {
        }
    }
}

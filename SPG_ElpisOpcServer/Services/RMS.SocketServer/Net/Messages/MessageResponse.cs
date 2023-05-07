namespace RMS.SocketServer.Net.Messages
{
    public readonly struct MessageResponse : IMessageResponse
    {
        private readonly IUserMessage _message;

        private readonly string _clientId;

        private readonly MessageType _responseType;

        public MessageResponse(IUserMessage message, MessageType responseType, string clientId)
        {
            _message = message;
            _responseType = responseType;
            _clientId = clientId;
        }

        public MessageType RequestType => _message.Type;

        public MessageType ResponseType => _responseType;

        public string ClientId => _clientId;

        public IUserMessage Message => _message;

        public void SetResult(bool status)
        {
        }
    }
}

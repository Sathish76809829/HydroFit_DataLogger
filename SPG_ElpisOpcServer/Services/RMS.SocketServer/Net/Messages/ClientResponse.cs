namespace RMS.SocketServer.Net.Messages
{
    /// <summary>
    /// Response message from client socket
    /// </summary>
    public readonly struct ClientResponse : IMessageResponse
    {
        private readonly string _clientId;

        private readonly IUserMessage _message;

        public ClientResponse(string clientId, IUserMessage message)
        {
            _clientId = clientId;
            _message = message;
        }

        public MessageType RequestType => _message.Type;

        public MessageType ResponseType => MessageType.ToClient;

        public string ClientId => _clientId;

        public IUserMessage Message => _message;

        public void SetResult(bool status)
        {
        }
    }
}

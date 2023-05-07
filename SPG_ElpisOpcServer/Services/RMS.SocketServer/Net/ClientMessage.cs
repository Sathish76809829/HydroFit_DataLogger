using RMS.SocketServer.Net.Messages;

namespace RMS.SocketServer.Net
{
    /// <summary>
    /// Client message instance passed to <see cref="SocketConnection.Recieved"/> event
    /// </summary>
    public class ClientMessage
    {
        public ClientMessage(string clientId, IMessage value)
        {
            ClientId = clientId;
            Value = value;
        }

        public string ClientId { get; }

        public IMessage Value { get; }
    }
}

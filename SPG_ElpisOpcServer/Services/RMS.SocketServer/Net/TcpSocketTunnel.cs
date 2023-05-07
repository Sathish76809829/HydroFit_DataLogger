using Microsoft.Extensions.Logging;
using RMS.SocketServer.Net.Messages;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RMS.SocketServer.Net
{
    public class TcpSocketTunnel : TcpSocketConnection
    {
        public TcpSocketTunnel(Socket socket, ILogger logger) : base(socket, logger)
        {
        }

        protected override void ReceivedMessage(IMessage message)
        {
            var original = (RawMessagePacket)message;
            // ignore external message
            switch (original.Header.Type)
            {
                case MessageType.External:
                case MessageType.None:
                    return;
                default:
                    base.ReceivedMessage(new RedirectMessage(original));
                    return;
            }

        }

        protected override Task SendAsync(IMessageResponse res)
        {
            if (res.RequestType != res.ResponseType
                && res.RequestType != MessageType.External)
            {
                var message = res.Message.Value;
                Socket.Send(Writter.WriteByte((byte)MessageType.External)
                    .WriteUInt16(message.Length)
                    .WriteByte((byte)res.ResponseType)
                    .Write(message).GetBytes(), SocketFlags.None);
            }
            return Task.CompletedTask;
        }
    }
}

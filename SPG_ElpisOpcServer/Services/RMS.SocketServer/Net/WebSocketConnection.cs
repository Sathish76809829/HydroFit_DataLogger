using Microsoft.AspNetCore.Http;
using RMS.SocketServer.Configurations;
using RMS.SocketServer.Models;
using RMS.SocketServer.Net.Formatters;
using RMS.SocketServer.Net.Messages;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace RMS.SocketServer.Net
{
    public class WebSocketConnection : SocketConnection
    {
        private readonly WebSocket socket;

        private readonly AsyncLock _lock;

        private readonly WebSocketEndPointConfiguration configuration;

        private readonly ConnectionInfo _info;

        public WebSocketConnection(ConnectionInfo info, WebSocket socket, WebSocketEndPointConfiguration configuration) : base(info.Id)
        {
            _info = info;
            this.socket = socket;
            this.configuration = configuration;
            _lock = new AsyncLock();
            Writer = new JsonWriter();
        }

        public virtual JsonWriter Writer { get; }

        public async Task RunAsync()
        {
            var token = CancellationTokenSource.Token;
            int receiveBufferSize = configuration.ReceiveBufferSize;
            for (; ; )
            {
                if (token.IsCancellationRequested)
                    return;
                var buffer = new byte[receiveBufferSize];
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                if (result.CloseStatus.HasValue)
                    return;
                if (result.EndOfMessage)
                {
                    ReceivedMessage(new MessagePacket(buffer, result.Count));
                    continue;
                }
                await ReadRemaining(buffer, result.Count, token);
            }
        }

        async Task ReadRemaining(byte[] received, int count, CancellationToken token)
        {
            for (; ; )
            {
                var buffer = new byte[1024];
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), token);
                Array.Resize(ref received, received.Length + buffer.Length);
                Array.Copy(buffer, 0, received, count, result.Count);
                count += result.Count;
                if (result.EndOfMessage)
                {
                    ReceivedMessage(new MessagePacket(received, count));
                    return;
                }
            }
        }

        protected override async Task SendAsync(IMessageResponse res)
        {
            var token = CancellationTokenSource.Token;
            using (await _lock.WaitAsync(token).ConfigureAwait(false))
            {
                await socket.SendAsync(Writer.Encode(res), WebSocketMessageType.Text, true, token)
                    .ConfigureAwait(false);
                res.SetResult(true);
            }
        }

        public override ClientInfo ClientInfo
        {
            get
            {
                return new ClientInfo
                {
                    Id = ClientId,
                    Address = $"{_info.RemoteIpAddress}:{_info.RemotePort}"
                };
            }
        }

        public override void Stop()
        {
            CancellationTokenSource.Cancel();
        }

        protected override void OnDispose()
        {
            _lock.Dispose();
        }
    }
}
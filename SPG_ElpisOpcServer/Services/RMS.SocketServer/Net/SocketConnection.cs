using RMS.SocketServer.Net.Messages;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RMS.SocketServer.Net
{
    /// <summary>
    /// abstract class for socket connection. Provides sending and reciveing message from socket
    /// </summary>
    public abstract class SocketConnection : IDisposable
    {
        public readonly string ClientId;

        public event Action<ClientMessage> Recieved;

        protected CancellationTokenSource CancellationTokenSource;

        protected readonly SemaphoreSlim MessageHandle;

        public event Action<SocketConnection> Disconnected;

        private readonly ConcurrentQueue<IMessageResponse> responses;

        private bool disposed;

        protected SocketConnection(string clientId)
        {
            ClientId = clientId;
            CancellationTokenSource = new CancellationTokenSource();
            MessageHandle = new SemaphoreSlim(0);
            responses = new ConcurrentQueue<IMessageResponse>();
        }

        public void Start(CancellationToken cancellationToken)
        {
            CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            TaskFx.Start(BegineDispatch, CancellationTokenSource.Token);
            OnStart(cancellationToken);
        }

        protected virtual void OnStart(CancellationToken cancellationToken)
        {
        }

        async void BegineDispatch()
        {
            var token = CancellationTokenSource.Token;
            try
            {
                for (; ; )
                {
                    if (token.IsCancellationRequested)
                        return;
                    await MessageHandle.WaitAsync(token);
                    if (responses.TryDequeue(out var res))
                    {
                        await SendAsync(res);
                    }
                }
            }
            catch (SocketException ex)
            {
                OnDisconnected();
                System.Diagnostics.Trace.TraceError(ClientId + " Socket Client Error :" + ex.Message);
                System.Diagnostics.Trace.TraceError(ex.StackTrace);
            }
            catch (OperationCanceledException)
            {
                //Operation Called
            }
            catch (ObjectDisposedException)
            {
                OnDisconnected();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ClientId + " Client Error :" + ex.Message);
                System.Diagnostics.Trace.TraceError(ex.StackTrace);
            }
        }

        protected virtual void ReceivedMessage(IMessage message)
        {
            Recieved?.Invoke(new ClientMessage(ClientId, message));
        }

        protected abstract Task SendAsync(IMessageResponse res);

        public void Enqueue(IMessageResponse message)
        {
            responses.Enqueue(message);
            MessageHandle.Release(1);
        }

        protected void OnDisconnected()
        {
            Disconnected?.Invoke(this);
        }

        protected virtual void OnDispose() { }

        public abstract Models.ClientInfo ClientInfo { get; }

        public abstract void Stop();

        public void Dispose()
        {
            if (!disposed)
            {
                CancellationTokenSource.Cancel();
                CancellationTokenSource.Dispose();
                MessageHandle.Dispose();
                OnDispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}

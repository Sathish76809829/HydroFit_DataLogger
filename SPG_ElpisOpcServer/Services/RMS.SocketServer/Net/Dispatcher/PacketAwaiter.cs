using System.Threading;
using System.Threading.Tasks;

namespace RMS.SocketServer.Net.Dispatcher
{
    /// <summary>
    /// Packet response awaiter to wait for client delivery success.
    /// </summary>
    public class PacketAwaiter : System.IDisposable
    {
        private readonly TaskCompletionSource<bool> _taskCompletionSource;

        public readonly ushort MessageId;

        private readonly PacketDispatcher _dispatcher;

        public PacketAwaiter(ushort messageId, PacketDispatcher dispatcher)
        {
            MessageId = messageId;
            _dispatcher = dispatcher;
            _taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        /// <summary>
        /// Wait for client reply
        /// </summary>
        /// <param name="timeout">timeout in millisec</param>
        /// <returns></returns>
        public async Task<bool> WaitOneAsync(System.TimeSpan timeout)
        {
            using (var timeoutToken = new CancellationTokenSource(timeout))
            {
                using (timeoutToken.Token.Register(() => Fail()))
                {
                    return await _taskCompletionSource.Task.ConfigureAwait(false);
                }
            }
        }

        public void Complete()
        {
            _taskCompletionSource.TrySetResult(true);
        }

        public void Cancel()
        {
            _taskCompletionSource.TrySetCanceled();
        }

        void Fail()
        {
            _taskCompletionSource.TrySetResult(false);
        }

        public void Dispose()
        {
            _dispatcher.RemoveAwaiter(this);
        }
    }
}

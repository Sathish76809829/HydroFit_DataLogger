using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ElpisOpcServer.SocketService.Dispatcher
{
    public class PacketAwaiter : IDisposable
    {
        private readonly TaskCompletionSource<bool> _taskCompletionSource;
        public readonly ushort MessageId;
        private readonly PacketDispatecher _dispatcher;
        public PacketAwaiter(ushort messageId,PacketDispatecher dispatcher)
        {
            MessageId = messageId;
            _dispatcher = dispatcher;
            _taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.LongRunning);
        }

        public async Task<bool> WaitOneAsync(TimeSpan timeout)
        {
            using(var timeoutToken=new CancellationTokenSource(timeout))
            {
                using(timeoutToken.Token.Register(()=>fail()))
                {
                    return await _taskCompletionSource.Task.ConfigureAwait(false);
                }
            }
        }

        private void fail()
        {
            _taskCompletionSource.TrySetCanceled();
        }

        public void Dispose()
        {
            _dispatcher.RemoveAwaiter(this);
        }
        public void Cancel()
        {
            _taskCompletionSource.TrySetCanceled();
        }
        public void Complete()
        {
            _taskCompletionSource.TrySetResult(true);
        }
    }
}

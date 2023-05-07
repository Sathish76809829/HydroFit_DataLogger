using RMS.SocketServer.Net.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace RMS.SocketServer.Net.Dispatcher
{
    /// <summary>
    /// Message waiter to wait for message to deliver
    /// </summary>
    public class MessageResponseAwaiter : IMessageResponse
    {
        public readonly SemaphoreSlim _semaphore;

        public readonly IMessageResponse Response;

        public volatile int _state;

        public MessageResponseAwaiter(IMessageResponse response, SemaphoreSlim semaphore)
        {
            Response = response;
            _semaphore = semaphore;
        }

        public MessageType ResponseType => Response.ResponseType;

        public string ClientId => Response.ClientId;

        public MessageType RequestType => Response.RequestType;

        public IUserMessage Message => Response.Message;

        /// <summary>
        /// Wait for this message
        /// </summary>
        /// <param name="timeout">Timout in millisec</param>
        /// <param name="cancellationToken">Cancellation token for cancel the task</param>
        /// <returns></returns>
        public async Task<bool> WaitOneAsync(int timeout, CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(timeout, cancellationToken);
            return _state == 1;
        }

        /// <summary>
        /// Set the message as delivered
        /// </summary>
        /// <param name="status">result of delivery</param>
        public void SetResult(bool status)
        {
            Interlocked.Exchange(ref _state, status ? 1 : 0);
            _semaphore.Release(1);
        }
    }
}

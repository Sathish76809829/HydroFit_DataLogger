using Microsoft.Extensions.Caching.Memory;
using RMS.SocketServer.Net.Messages;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RMS.SocketServer.Net
{
    /// <summary>
    /// Support message retain for 5 min
    /// </summary>
    public class RetainedMessages
    {
        private readonly IMemoryCache unDeliveredMessages;
        private readonly HashSet<string> clients;
        private readonly ReaderWriterLockSlim _lock;

        public RetainedMessages(IMemoryCache cache)
        {
            unDeliveredMessages = cache;
            clients = new HashSet<string>();
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        }

        /// <summary>
        /// Add to client retain message 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="response"></param>
        public void AddMessage(string clientId, IMessageResponse response)
        {
            _lock.EnterWriteLock();
            try
            {
                clients.Add(clientId);
                unDeliveredMessages.Set(clientId, response, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(30)).RegisterPostEvictionCallback(OnEvicted));
            }
            finally
            {
                if (_lock.IsWriteLockHeld)
                    _lock.ExitWriteLock();
            }
        }

        public void RemoveMessage(string clientId)
        {
            _lock.EnterWriteLock();
            try
            {
                unDeliveredMessages.Remove(clientId);
            }
            finally
            {
                if (_lock.IsWriteLockHeld)
                    _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Get the reatined messages and clear the cache
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public bool TryGetRetainedMessages(out IReadOnlyList<IMessageResponse> messages)
        {
            _lock.EnterReadLock();
            try
            {
                int count = this.clients.Count;
                if (count == 0)
                {
                    messages = null;
                    return false;
                }
                string[] clients = new string[count];
                this.clients.CopyTo(clients);
                List<IMessageResponse> res = new List<IMessageResponse>(count);

                for (int i = 0; i < count; i++)
                {
                    string item = clients[i];
                    if (unDeliveredMessages.TryGetValue(item, out var message))
                    {
                        res.Add((IMessageResponse)message);
                    }
                }
                messages = res;
                return res.Count > 0;
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// After 5 min elpase or remove of cache remove client key
        /// </summary>
        void OnEvicted(object key, object value, EvictionReason reason, object state)
        {
            if (reason != EvictionReason.Expired && reason != EvictionReason.Removed)
                return;
            _lock.EnterWriteLock();
            try
            {
                clients.Remove((string)key);
            }
            finally
            {
                if (_lock.IsWriteLockHeld)
                    _lock.ExitWriteLock();
            }
        }

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_lock != null)
                    _lock.Dispose();
            }
        }

        ~RetainedMessages()
        {
            Dispose(false);
        }

        #endregion
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElpisOpcServer.SocketService.Dispatcher
{
    public  class PacketDispatecher
    {
        readonly List<PacketAwaiter> _awaiters = new List<PacketAwaiter>();
        public void CancelAll()
        {
            lock(_awaiters)
            {
                foreach (var entry in _awaiters)
                {
                    entry.Cancel();
                }
                _awaiters.Clear();
            }
        }
        public  void Dispatch(ushort messageId)
        {
            lock(_awaiters)
            {
                for (var i = _awaiters.Count-1; i >= 0; i--)
                {
                    var entry = _awaiters[i];
                    if(entry.MessageId !=messageId)
                    {
                        continue;

                    }
                    entry.Complete();
                    _awaiters.RemoveAt(i);
                }
            }
        }
        public PacketAwaiter AddAwaiter(ushort messageId)
        {
            var awaiter = new PacketAwaiter(messageId, this);
            lock(_awaiters)
            {
                _awaiters.Add(awaiter);
            }
            return awaiter;
        }
        public void RemoveAwaiter(PacketAwaiter awaiter)
        {
            if (awaiter == null)
                throw new ArgumentNullException(nameof(awaiter));
            lock(_awaiters)
            {
                _awaiters.Remove(awaiter);
            }
        }
    }
}

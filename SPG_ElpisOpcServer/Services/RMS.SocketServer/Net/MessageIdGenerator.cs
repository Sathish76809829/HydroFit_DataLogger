namespace RMS.SocketServer.Net
{
    /// <summary>
    /// <see cref="ushort"/> Id Generator for sending message 
    /// </summary>
    public class MessageIdGenerator
    {
        readonly object _syncRoot = new object();

        ushort _value;

        public void Reset()
        {
            lock (_syncRoot)
            {
                _value = 0;
            }
        }

        public ushort GetNextPacketIdentifier()
        {
            lock (_syncRoot)
            {
                _value++;

                if (_value == 0)
                {
                    //id should never be 0.
                    _value = 1;
                }

                return _value;
            }
        }
    }
}

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RMS.SocketServer.Extensions
{
    /// <summary>
    /// Socket extension for connect to endpoint asynchrously
    /// </summary>
    public static class SocketExtensions
    {
        public static Task<bool> ConnectAsync(this Socket self, EndPoint endPoint, int timeout)
        {
            return Task.Run(() =>
            {
                var clientDone = new System.Threading.ManualResetEvent(false);
                var arg = new SocketAsyncEventArgs
                {
                    RemoteEndPoint = endPoint
                };
                var connected = false;
                arg.Completed += (s, e) =>
                {
                    connected = e.SocketError == SocketError.Success;
                    clientDone.Set();
                };
                self.ConnectAsync(arg);
                clientDone.Reset();
                clientDone.WaitOne(timeout);
                return connected;
            });
        }

        public static string ReceiveCommands(this Socket socket, byte[] header)
        {
            byte[] buffer = new byte[16];
            Array.Copy(header, 0, buffer, 0, header.Length);
            int idx = header.Length;
            while (idx < buffer.Length)
            {
                // fixed scenario with socket closed gracefully by peer/broker and
                // Read return 0. Avoid infinite loop.
                int read = socket.Receive(buffer, idx, 1, SocketFlags.None);
                if (read == 0)
                    break;
                if (buffer[idx] == '\r')
                    break;
                idx += read;
                if (buffer.Length < idx)
                {
                    int newCapacity = buffer.Length * 2;
                    // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
                    // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
                    if ((uint)newCapacity > int.MaxValue)
                        newCapacity = int.MaxValue;
                    if (newCapacity < idx)
                        newCapacity = idx;
                    byte[] newItems = new byte[newCapacity];
                    if (idx > 0)
                    {
                        Array.Copy(buffer, 0, newItems, 0, idx);
                    }
                }
            }
            return System.Text.Encoding.ASCII.GetString(buffer, 0, idx);
        }
    }
}

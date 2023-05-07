using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace ElpisOpcServer.SocketService
{
    public static class SocketExtension
    {
        public static string Receivecommands(this Socket socket,byte[] header)
        {
            byte[] buffer = new byte[16];
            Array.Copy(header, 0, buffer, 0, header.Length);
            int idx = header.Length;
            while(idx<buffer.Length)
            {
                int read = socket.Receive(buffer, idx, 1, SocketFlags.None);
                if (read == 0)
                    break;
                if (buffer[idx] == '\r')
                    break;
                idx += read;
                if(buffer.Length<idx)
                {
                    int newCapacity = buffer.Length * 2;
                    if ((uint)newCapacity > int.MaxValue)
                        newCapacity = int.MaxValue;
                    if (newCapacity < idx)
                        newCapacity = idx;
                    byte[] newItems = new byte[newCapacity];
                    if(idx>0)
                    {
                        Array.Copy(buffer, 0, newItems, 0, idx);
                    }
                }

            }
            return System.Text.Encoding.ASCII.GetString(buffer, 0, idx);
        }
    }
}

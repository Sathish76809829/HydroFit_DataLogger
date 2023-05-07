using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElpisOpcServer.SocketService.Net.Formatters
{
    public class CharReader : IPacketReader
    {
        public static int FromHexValue(byte value)
        {
            if (value < 87)
            {
                return value - 48;
            }
            return value - 87;
        }

        public MessageHeader ReadHeader(byte[] header)
        {
            MessageHeader res = default;
            res.Type = (MessageType)ReadByte(header, 0);
            res.Length = ReadUInt16(header, 2);
            res.Id = ReadUInt16(header, 8);
            return res;
        }

        public byte[] CreateHeader()
        {
            return new byte[12];
        }

        public ushort ReadUInt16(byte[] value, int index)
        {
            return (byte)(FromHexValue(value[index]) * 4096 + FromHexValue(value[index + 1]) * 256 + FromHexValue(value[index + 2]) * 16 + FromHexValue(value[index + 3]));
        }

        public byte ReadByte(byte[] value, int index)
        {
            return (byte)(FromHexValue(value[index]) * 16 + FromHexValue(value[index + 1]));
        }
    }
}

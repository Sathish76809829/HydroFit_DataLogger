using RMS.SocketServer.Net.Messages;

namespace RMS.SocketServer.Net.Formatters
{
    /// <summary>
    /// 2 bytes char reader implementation for reading ascii message from client
    /// </summary>
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
            var rawData = System.Text.Encoding.UTF8.GetString(header);
            MessageHeader res = default;
            res.Type = (MessageType)ReadByte(header,0);
            res.Length = ReadUInt16(header, 2);
            var extra = ReadByte(header, 6);
            res.Quality = (DeliveryQuality)(extra & 0xF);
            res.Retain = ((extra & 0x10) >> 4) == 1;
            res.Id = ReadUInt16(header, 8);
            return res;
        }

        public byte[] CreateHeader()
        {
            return new byte[12];
        }

        public ushort ReadUInt16(byte[] value, int index)
        {
            return (ushort)(FromHexValue(value[index]) * 4096 + FromHexValue(value[index + 1]) * 256 + FromHexValue(value[index + 2]) * 16 + FromHexValue(value[index + 3]));
        }

        public byte ReadByte(byte[] value, int index)
        {
            return (byte)(FromHexValue(value[index]) * 16 + FromHexValue(value[index + 1]));
        }
    }
}

using RMS.SocketServer.Net.Messages;

namespace RMS.SocketServer.Net.Formatters
{
    /// <summary>
    /// Byte reader which takes <see cref="CreateHeader"/> as header 
    /// </summary>
    public class ByteReader : IPacketReader
    {
        public MessageHeader ReadHeader(byte[] header)
        {
            MessageHeader res = default;
            res.Type = (MessageType)header[0];
            res.Length = (header[1]) | ((header[2]) << 8);
            res.Quality = (DeliveryQuality)(header[3] & 0x0F);
            res.Retain = (header[3] & 0x08) >> 4 == 1;
            res.Id = (ushort)(header[5] | ((header[6]) << 8));
            return res;
        }
        
        public byte[] CreateHeader()
        {
            return new byte[6];
        }

        public ushort ReadUInt16(byte[] value, int index)
        {
            return (ushort)((value[index]) | ((value[index + 1]) << 8));
        }
    }
}

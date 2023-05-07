using RMS.SocketServer.Net.Messages;

namespace RMS.SocketServer.Net.Formatters
{
    /// <summary>
    /// Byte reader for incomming message bytes
    /// </summary>
    public interface IPacketReader
    {
        byte[] CreateHeader();
        MessageHeader ReadHeader(byte[] header);
        ushort ReadUInt16(byte[] value, int index);
    }
}
namespace ElpisOpcServer.SocketService.Net.Formatters
{
    public interface IPacketReader
    {
        byte[] CreateHeader();
        MessageHeader ReadHeader(byte[] header);
        ushort ReadUInt16(byte[] value, int index);
    }
}
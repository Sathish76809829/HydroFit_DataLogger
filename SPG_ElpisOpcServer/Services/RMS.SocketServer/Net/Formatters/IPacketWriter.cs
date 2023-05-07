namespace RMS.SocketServer.Net.Formatters
{
    /// <summary>
    /// Byte writer for creating byte array for sending messages
    /// </summary>
    public interface IPacketWriter
    {
        void Reset();
        byte[] GetBytes();
        IPacketWriter WriteUInt16(int value);
        IPacketWriter WriteByte(int value);
        /// <summary>
        /// Write char in ASCII 
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="System.NotSupportedException">For non ascii char </exception>
        /// <returns>Call chain</returns>
        IPacketWriter WriteChar(char value);
        /// <summary>
        /// Write Id in ASCII char format
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        IPacketWriter WriteIdentifier(ushort value);
        IPacketWriter WriteRawByte(byte value);
        IPacketWriter Write(byte[] value);

    }
}
using System;

namespace RMS.SocketServer.Net.Formatters
{
    /// <summary>
    /// Byte writer for writing <see cref="Messages.IMessageResponse"/> to bytes
    /// </summary>
    public class ByteWriter : ByteWriterBase, IPacketWriter
    {
        public ByteWriter WriteInt16(int value)
        {
            var span = Memory.Span;
            GrowIfNeeded(2);
            span[BytesPending++] = (byte)(value & 0xFF);
            span[BytesPending++] = (byte)(value >> 8);
            return this;
        }

        public IPacketWriter WriteRawByte(byte value)
        {
            var span = Memory.Span;
            GrowIfNeeded(1);
            span[BytesPending++] = value;
            return this;
        }

        public IPacketWriter Write(byte[] value)
        {
            GrowIfNeeded(value.Length);
            value.AsMemory().CopyTo(Memory.Slice(BytesPending));
            BytesPending += value.Length;
            return this;
        }

        public IPacketWriter WriteUInt16(int value)
        {
            return WriteInt16(value);
        }

        public IPacketWriter WriteByte(int value)
        {
            return WriteRawByte((byte)value);
        }

        public IPacketWriter WriteChar(char value)
        {
            var span = Memory.Span;
            GrowIfNeeded(1);
            if (value > 256)
            {
                throw new NotSupportedException("Only ASCII write() supported");
            }
            span[BytesPending++] = (byte)value;
            return this;
        }

        public IPacketWriter WriteIdentifier(ushort value)
        {
            var span = Memory.Span;
            int count = (int)Math.Floor(Math.Log10(value) + 1);
            GrowIfNeeded(count);
            BytesPending += count;
            for (int i = count; i > -1; i--, value /= 10)
            {
                span[BytesPending - i] = (byte)((value % 10) + 48);
            }
            return this;
        }
    }
}

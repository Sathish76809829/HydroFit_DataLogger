using System;

namespace RMS.SocketServer.Net.Formatters
{
    /// <summary>
    /// 2 bytes char writer implementation for writing ascii value to byte array
    /// </summary>
    public class CharWriter : ByteWriterBase, IPacketWriter
    {
        static byte GetHexValue(int i)
        {
            return i < 10 ? (byte)(i + 48) : (byte)(i + 87);
        }

        public IPacketWriter Write(byte[] value)
        {
            while (Memory.Length < BytesPending+ value.Length)
            {
                GrowIfNeeded(value.Length);
            }
            value.AsMemory().CopyTo(Memory.Slice(BytesPending));
            BytesPending += value.Length;
            return this;
        }

        public IPacketWriter WriteIdentifier(ushort value)
        {
            var span = Memory.Span;
            int count = (int)Math.Floor(Math.Log10(value) + 1);
            GrowIfNeeded(count);
            BytesPending += count;
            for (int i = 1; i <= count; i++, value /= 10)
            {
                span[BytesPending - i] = (byte)((value % 10) + 48);
            }
            return this;
        }

        public IPacketWriter WriteRawByte(byte value)
        {
            var span = Memory.Span;
            GrowIfNeeded(1);
            span[BytesPending++] = value;
            return this;
        }

        public CharWriter WriteASCII(int value)
        {
            var span = Memory.Span;
            GrowIfNeeded(2);
            if (value < 10)
            {
                span[BytesPending++] = 48;
                span[BytesPending++] = (byte)(value + 48);
                return this;
            }
            span[BytesPending++] = GetHexValue(value / 16);
            span[BytesPending++] = GetHexValue(value % 16);
            return this;
        }

        public IPacketWriter WriteUInt16(int value)
        {
            var span = Memory.Span;
            GrowIfNeeded(4);
            if (value < 10)
            {
                span[BytesPending++] = 48;
                span[BytesPending++] = 48;
                span[BytesPending++] = 48;
                span[BytesPending++] = (byte)(value + 48);
                return this;
            }
            if (value < 256)
            {
                span[BytesPending++] = 48;
                span[BytesPending++] = 48;
                span[BytesPending++] = GetHexValue(value / 16);
                span[BytesPending++] = GetHexValue(value % 16);
                return this;
            }
            span[BytesPending++] = GetHexValue(value / 4096);
            span[BytesPending++] = GetHexValue(value / 256 % 16);
            span[BytesPending++] = GetHexValue(value / 16 % 16);
            span[BytesPending++] = GetHexValue(value % 16);
            return this;
        }

        public IPacketWriter WriteByte(int value)
        {
            return WriteASCII(value);
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
    }
}

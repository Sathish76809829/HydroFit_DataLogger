using System;

namespace RMS.SocketServer.Net.Formatters
{
    /// <summary>
    /// Base class which contains logic for writing bytes into array
    /// </summary>
    public class ByteWriterBase 
    {
        protected readonly System.Buffers.ArrayBufferWriter<byte> Writer;

        protected Memory<byte> Memory;

        protected int BytesPending;

        public ByteWriterBase()
        {
            Writer = new System.Buffers.ArrayBufferWriter<byte>();
        }

        public int Length => BytesPending;

        public byte[] GetBytes()
        {
            Writer.Advance(BytesPending);
            BytesPending = 0;
            return Writer.WrittenMemory.ToArray();
        }

        public void Reset(int size)
        {
            BytesPending = 0;
            Writer.Clear();
            Memory = Writer.GetMemory(size);
        }

        public void Reset()
        {
            BytesPending = 0;
            Writer.Clear();
            Memory = Writer.GetMemory(1);
        }

        protected void GrowIfNeeded(int length)
        {
            if (Memory.Length - BytesPending < length)
            {
                var newMemory = Writer.GetMemory(Memory.Length * 2);
                Memory.CopyTo(newMemory);
                Memory = newMemory;
            }
        }
    }
}
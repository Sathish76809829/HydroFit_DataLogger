using System.Text;

namespace RMS.Service.Abstractions.Parser
{
    /// <summary>
    /// Json source from string
    /// </summary>
    public class JsonSource
    {
        public const int MinimumBufferSize = 16;

        private readonly char[] m_chars;

        private char[] buffers;

        private int bufferSize;

        private readonly int size;

        int pos;

        public JsonSource(byte[] bytes)
        {
            m_chars = Encoding.UTF8.GetChars(bytes, 0, bytes.Length);
            size = m_chars.Length;
            buffers = new char[MinimumBufferSize];
        }

        public JsonSource(string text)
        {
            m_chars = text.ToCharArray();
            size = text.Length;
            buffers = new char[MinimumBufferSize];
        }


        public bool CanAdvance => pos < size;

        public int StoreSize => bufferSize;

        public char ReadChar()
        {
            if (pos >= size)
                return char.MinValue;
            return m_chars[pos++];
        }

        public char PeekChar()
        {
            if (pos < size)
                return m_chars[pos];
            return char.MinValue;
        }

        public JsonSource AdvanceChar()
        {
            if (pos >= size)
                return this;
            pos++;
            return this;
        }

        public JsonSource Store(char value)
        {
            if (bufferSize == buffers.Length)
            {
                EnsureCapcity(size + 1);
            }
            buffers[bufferSize++] = value;
            return this;
        }

        public string ReturnStore()
        {
            string result = new string(buffers, 0, bufferSize);
            bufferSize = 0;
            return result;
        }

        private unsafe void EnsureCapcity(int min)
        {
            int newCapacity = buffers.Length * 2;
            // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            if (newCapacity > int.MaxValue)
                newCapacity = int.MaxValue;
            if (newCapacity < min)
                newCapacity = min;
            char[] newItems = new char[newCapacity];
            // srcPtr and destPtr are IntPtr's pointing to valid memory locations
            // size is the number of long (normally 4 bytes) to copy
            fixed (char* src = buffers, dest = newItems)
                for (int i = 0; i < size; i++)
                    dest[i] = src[i];
            buffers = newItems;
        }


        public void Reset()
        {
            pos = 0;
            bufferSize = 0;
        }

        public override string ToString()
        {
            return new string(m_chars);
        }
    }
}

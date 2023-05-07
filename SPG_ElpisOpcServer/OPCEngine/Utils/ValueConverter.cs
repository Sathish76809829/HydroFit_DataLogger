using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elpis.Windows.OPC.Server
{
    public class ValueConverter
    {
        /// <summary>
        ///     Converts four UInt16 values into a IEEE 64 floating point format.
        /// </summary>
        /// <param name="b3">Highest-order ushort value.</param>
        /// <param name="b2">Second-to-highest-order ushort value.</param>
        /// <param name="b1">Second-to-lowest-order ushort value.</param>
        /// <param name="b0">Lowest-order ushort value.</param>
        /// <returns>IEEE 64 floating point value.</returns>
        public static double GetDouble(ushort b3, ushort b2, ushort b1, ushort b0)
        {
            byte[] value = BitConverter.GetBytes(b0)
                .Concat(BitConverter.GetBytes(b1))
                .Concat(BitConverter.GetBytes(b2))
                .Concat(BitConverter.GetBytes(b3))
                .ToArray();

            return BitConverter.ToDouble(value, 0);
        }

        /// <summary>
        ///     Converts two UInt16 values into a IEEE 32 floating point format.
        /// </summary>
        /// <param name="highOrderValue">High order ushort value.</param>
        /// <param name="lowOrderValue">Low order ushort value.</param>
        /// <returns>IEEE 32 floating point value.</returns>
        public static float GetSingle(ushort highOrderValue, ushort lowOrderValue)
        {
            byte[] value = BitConverter.GetBytes(lowOrderValue)
                .Concat(BitConverter.GetBytes(highOrderValue))
                .ToArray();

            return BitConverter.ToSingle(value, 0);
        }

        /// <summary>
        ///     Converts two UInt16 values into a UInt32.
        /// </summary>
        public static uint GetUInt32(ushort highOrderValue, ushort lowOrderValue)
        {
            byte[] value = BitConverter.GetBytes(lowOrderValue)
                .Concat(BitConverter.GetBytes(highOrderValue))
                .ToArray();

            return BitConverter.ToUInt32(value, 0);
        }

        /// <summary>
        ///     Converts two UInt16 values into a Int32.
        /// </summary>
        public static int GetInt32(ushort highOrderValue, ushort lowOrderValue)
        {
            byte[] value = BitConverter.GetBytes(lowOrderValue)
                .Concat(BitConverter.GetBytes(highOrderValue))
                .ToArray();
            return BitConverter.ToInt32(value, 0);
        }

        /// <summary>
        /// Converts the byte array into ushort values. Each ushort value is generated from two byte value.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>array of Ushort values</returns>
        public static ushort[] ConvertTOUShort(byte[] bytes)
        {
            ushort[] result = new ushort[bytes.Length / 2];
            for (int i = 0; i < bytes.Length; i = i + 2)
            {
                result[i / 2] = BitConverter.ToUInt16(bytes, i);
            }
            return result;
        }

    }
}

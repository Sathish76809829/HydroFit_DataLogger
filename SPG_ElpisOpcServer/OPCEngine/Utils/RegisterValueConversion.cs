using Elpis.Windows.OPC.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPCEngine.Utils
{
    public class RegisterValueConversion
    {
        #region ConvertAnalogArray
        internal static object ConvertAnalogArray(dynamic data, EnumDataType EDataType) //TODO: --Done Move to Utils 
        {
            byte[] bData = new byte[data.Length * 2];
            int iCur = 0;
            foreach (Int16 i16 in data)
            {
                BitConverter.GetBytes(i16).CopyTo(bData, iCur);
                iCur += 2;
            }
            System.Collections.BitArray B = new System.Collections.BitArray(bData);
            bool[] bitdata = new bool[B.Count];
            B.CopyTo(bitdata, 0);
            return ConvertBoolArray(data, EDataType);
        }
        #endregion


        #region Convert Bool Array
        internal static object ConvertBoolArray(dynamic data, EnumDataType EDataType) //TODO: --Done Move to Utils 
        {
            System.Collections.BitArray bits = new System.Collections.BitArray(data);
            byte[] value = new byte[bits.Count / 8];
            bits.CopyTo(value, 0);
            object ConvertedValue = null;
            try
            {
                switch (EDataType)
                {
                    case EnumDataType.Bool:
                        ConvertedValue = data[0];
                        break;
                    case EnumDataType.Byte:
                        ConvertedValue = value[0];
                        break;
                    case EnumDataType.SByte:
                        ConvertedValue = unchecked((sbyte)value[0]);
                        break;
                    case EnumDataType.Int16:
                        ConvertedValue = BitConverter.ToInt16(value, 0);
                        break;
                    case EnumDataType.UInt16:
                        ConvertedValue = BitConverter.ToUInt16(value, 0);
                        break;
                    case EnumDataType.Int32:
                        ConvertedValue = BitConverter.ToInt32(value, 0);
                        break;
                    case EnumDataType.UInt32:
                        ConvertedValue = BitConverter.ToUInt32(value, 0);
                        break;
                    case EnumDataType.Int64:
                        ConvertedValue = BitConverter.ToInt64(value, 0);
                        break;
                    case EnumDataType.UInt64:
                        ConvertedValue = BitConverter.ToUInt64(value, 0);
                        break;
                    case EnumDataType.Float32:
                        ConvertedValue = BitConverter.ToSingle(value, 0);
                        break;
                    case EnumDataType.Float64:
                        ConvertedValue = BitConverter.ToDouble(value, 0);
                        break;
                    case EnumDataType.String:
                        ConvertedValue = System.Text.Encoding.UTF8.GetString(value);
                        break;
                }
            }
            catch (Exception)
            {

            }
            return ConvertedValue;
        }
        #endregion Convert Bool Array
    }
}

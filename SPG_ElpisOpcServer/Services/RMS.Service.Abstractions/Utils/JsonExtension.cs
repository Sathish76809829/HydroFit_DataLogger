using System;

namespace RMS.Service.Utils
{
    /// <summary>
    /// Json writer extensions
    /// </summary>
    public static class JsonExtension
    {
        /// <summary>
        /// Write <see cref="IConvertible"/> value to <see cref="System.Text.Json.Utf8JsonWriter"/>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="c"></param>
        public static void WriteValue(this System.Text.Json.Utf8JsonWriter self, IConvertible c)
        {
            switch (c.GetTypeCode())
            {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    self.WriteStringValue(string.Empty);
                    return;
                case TypeCode.Object:
                    self.WriteStringValue(c.ToString());
                    return;
                case TypeCode.Boolean:
                    self.WriteBooleanValue(c.ToBoolean(null));
                    return;
                case TypeCode.String:
                case TypeCode.Char:
                    self.WriteStringValue(c.ToString(null));
                    return;
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                    self.WriteNumberValue(c.ToInt32(null));
                    return;
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                    self.WriteNumberValue(c.ToUInt32(null));
                    return;
                case TypeCode.Int64:
                    self.WriteNumberValue(c.ToInt64(null));
                    return;
                case TypeCode.UInt64:
                    self.WriteNumberValue(c.ToUInt32(null));
                    return;
                case TypeCode.Single:
                    self.WriteNumberValue(c.ToSingle(null));
                    return;
                case TypeCode.Double:
                    self.WriteNumberValue(c.ToDouble(null));
                    return;
                case TypeCode.Decimal:
                    self.WriteNumberValue(c.ToDecimal(null));
                    return;
                case TypeCode.DateTime:
                    self.WriteStringValue(c.ToDateTime(null));
                    return;
            }
        }
    }
}

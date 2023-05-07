namespace RMS.Service.Abstractions.Utils
{
    /// <summary>
    /// Signal Value converters
    /// </summary>
    public static class ValueConverters
    {
        /// <summary>
        /// Converts a value to <paramref name="dataType"/> to 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="dataType"></param>
        /// <returns>Converted value</returns>
        public static object Convert(object value, Models.SignalDataType dataType)
        {
            switch (dataType)
            {
                case Models.SignalDataType.Bool:
                    return System.Convert.ToBoolean(value);
                case Models.SignalDataType.Int32:
                    return System.Convert.ToInt32(value);
                case Models.SignalDataType.Single:
                    return System.Convert.ToSingle(value);
                case Models.SignalDataType.Double:
                    return System.Convert.ToDouble(value);
                case Models.SignalDataType.String:
                    return value?.ToString();
                default:
                    return value;
            }
        }
    }
}

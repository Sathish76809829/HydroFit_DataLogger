using RMS.Service.Abstractions.Models;

namespace Petronash.Models
{
    /// <summary>
    /// Device input model for RMS petronash
    /// </summary>
    public class DeviceInputs
    {
        public int InputId { get; set; }

        public string Id { get; set; }

        public /*int*/string DeviceId { get; set; }

        public SignalDataType DataType { get; set; }

        public string Value { get; set; }

        public bool IsActive { get; set; }

        public object ConvertValue()
        {
            switch (DataType)
            {
                case SignalDataType.Bool:
                    return System.Convert.ToBoolean(Value);
                case SignalDataType.Int32:
                    return System.Convert.ToInt32(Value);
                case SignalDataType.Single:
                    return System.Convert.ToSingle(Value);
                case SignalDataType.Double:
                    return System.Convert.ToDouble(Value);
                default:
                case SignalDataType.String:
                    return Value;
            }
        }
    }
}

using RMS.Service.Abstractions;
using RMS.Service.Utils;
using System.Text.Json;

namespace RMS.Service.Models
{
    /// <summary>
    /// Data Model sent to RMS 
    /// </summary>
    public struct DataSendModel : Abstractions.Models.ISignalModel
    {
        /// <summary>
        /// Device id for signal
        /// </summary>
        public /*int*/string DeviceId
        {
            readonly get;
            set;
        }

        /// <summary>
        /// Signal id for signal
        /// </summary>
        public /*int*/string SignalId
        {
            readonly get;
            set;
        }

        /// <summary>
        /// Value of the signal
        /// </summary>
        public object DataValue
        {
            readonly get;
            set;
        }

        /// <summary>
        /// Received time of the signal
        /// </summary>
        public string TimeReceived
        {
            readonly get;
            set;
        }

        /// <summary>
        /// Data type for the signal
        /// </summary>
        public int DataType
        {
            readonly get;
            set;
        }

        /// <summary>
        /// Write the instace to Json
        /// </summary>
        /// <param name="writer"></param>
        public void WriteTo(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteString(JsonPropertyKeys.DeviceId, DeviceId);
            writer.WriteString(JsonPropertyKeys.SignalId, SignalId);
            //writer.WriteNumber(JsonPropertyKeys.DeviceId, DeviceId);
            //writer.WriteNumber(JsonPropertyKeys.SignalId, SignalId);
            switch (DataValue)
            {
                case int i:
                    writer.WriteNumber(JsonPropertyKeys.DataValue, i);
                    break;
                case double d:
                    writer.WriteNumber(JsonPropertyKeys.DataValue, d);
                    break;
                case long l:
                    writer.WriteNumber(JsonPropertyKeys.DataValue, l);
                    break;
                case string s:
                    writer.WriteString(JsonPropertyKeys.DataValue, s);
                    break;
                case System.IConvertible c:
                    writer.WritePropertyName(JsonPropertyKeys.DataValue);
                    writer.WriteValue(c);
                    break;
            }
            writer.WriteString(JsonPropertyKeys.TimeReceived, TimeReceived);
            writer.WriteEndObject();
        }

        public override string ToString()
        {
            return $"{SignalId}:{DataValue}";
        }
    }
}

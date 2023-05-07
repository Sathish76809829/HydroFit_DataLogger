using RMS.Service.Abstractions.Parser;

namespace RMS.Service.Models
{
    /// <summary>
    /// Device data from RMS customer
    /// </summary>
    public class DeviceDataModel
    {
        /// <summary>
        /// Device id for the data
        /// </summary>
        public object DeviceId { get; set; }

        /// <summary>
        /// Type of the provider
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Value in between deviceId and type ex: <code>{deviceId}&gt;{value}&lt;{type}</code> 
        /// </summary>
        public JsonContent Value { get; set; }

        /// <summary>
        /// list of signal data in Json Array
        /// </summary>
        public JsonArray SignalList { get; set; }

        public static bool TryParse(JsonObject value, out DeviceDataModel deviceData)
        {
            if (value.FirstNode is JsonProperty first
                && first.Name == "device"
                && first.Content is JsonContent device
                && value.LastNode is JsonProperty last
                && last.Name.Equals("SignalDataList", System.StringComparison.OrdinalIgnoreCase)
                && last.Content is JsonArray items)
            {
                deviceData = new DeviceDataModel
                {
                    DeviceId = /*(int)*/device.FirstValue.Content,
                    Value = device,
                    Type = (int)device.LastValue.Content,
                    SignalList = items
                };
                return true;
            }
            deviceData = null;
            return false;
        }
    }
}

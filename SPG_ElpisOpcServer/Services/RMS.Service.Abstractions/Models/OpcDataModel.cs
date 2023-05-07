namespace RMS.Service.Abstractions.Models
{
    public class OpcDataModel
    {
        /// <summary>
        /// Device id for the data
        /// </summary>
        public object DeviceId { get; set; }



        /// <summary>
        /// list of signal data in Json Array
        /// </summary>
        public string SignalData { get; set; }

        public static bool TryParse(string value, /*int*/string deviceId, out OpcDataModel opcDeviceData)
        {
            string[] splitString = value.Split(',');
            if (splitString.Length >= 5 && splitString.Length <= 6 && deviceId != null)
            {
                opcDeviceData = new OpcDataModel
                {
                    DeviceId = /*(int)*/deviceId,
                    SignalData = value
                };
                return true;
            }
            opcDeviceData = null;
            return false;
        }
    }
}

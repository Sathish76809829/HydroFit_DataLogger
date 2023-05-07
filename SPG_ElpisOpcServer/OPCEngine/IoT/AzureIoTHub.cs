using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elpis.Windows.OPC.Server
{
    [Serializable, Description("Azure IoT Settings")]
    public class AzureIoTHub
    {
        [Description("Specify the host hame of the Azure IoTHub"),DisplayName("Host Name")]
        public string HostName { get; set; }

        [Description("Specify the device id Of the Azure IoT Hub"),DisplayName("Device Id")]
        public string DeviceId { get; set; }

        [Description("Specify the shared access key"), PasswordPropertyText(true) ,DisplayName("Shared Access Key")]
        public string SharedAccessKey { get; set; }



        public void Load(AzureIoTHub savedObj)
        {
            HostName = savedObj.HostName;
            DeviceId = savedObj.DeviceId;
            SharedAccessKey = savedObj.SharedAccessKey;
        }
        
        public void SaveAzureIoTSettings(dynamic ObjToSave)
        {
            ElpisServer.AzureIoTHubObj = ObjToSave;
        }
    }
}
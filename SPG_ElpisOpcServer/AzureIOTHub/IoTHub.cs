#region Namespaces
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualBasic;
using System.Windows.Forms;
#endregion Namespaces

#region AzureIOTHub NameSpace
namespace AzureIOTHub
{

    #region IoTHub class
    public class IoTHub
    {
        #region Properties
        private string DeviceConnectionString { get; set; }
        public DeviceClient deviceClient { get; set; }
        #endregion Properties
        
        #region Constructor
        /// <summary>
        /// Constructor interact with OPC Engine DLL to use the configuration object
        /// </summary>
        /// <param name="AzureIoTConfigurationObj"></param>
        public IoTHub(dynamic AzureIoTConfigurationObj)
        {
            if (AzureIoTConfigurationObj.HostName != null && AzureIoTConfigurationObj.DeviceId != null && AzureIoTConfigurationObj.SharedAccessKey != null)
            {
                DeviceConnectionString = "HostName=" + AzureIoTConfigurationObj.HostName + ";" + "DeviceId=" + AzureIoTConfigurationObj.DeviceId + ";" + "SharedAccessKey=" + AzureIoTConfigurationObj.SharedAccessKey;
                
                try
                {
                    deviceClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString, TransportType.Http1);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error in sample: {0}", ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Looks like your Azure Credentials entered wrong or not entered yet .Check this: Internet Of Things-> AzureIoT Configuration Settings");
            }
        }

        #endregion Constructor
        
        #region SendEvent Function
        /// <summary>
        /// SendEvent function sends the current server data to the Azure IoT Cloud storage
        /// </summary>
        /// <param name="TagObj"></param>
        /// <param name="DataType"></param>
        /// <returns></returns>
        public async Task SendEvent(dynamic TagObj, string DataType)
        {
            if (deviceClient != null)
            {
                object CurrentValueFromDevice = null;
                if (DataType == "8")
                {
                    //CurrentValueFromDevice = new
                    //{
                    //    DeviceID = "OPCServer",
                    //    PartitionKey = "PKPLC",
                    //    CurrentDateTime = DateTime.Now.ToString("MMMM dd,yyyy, H:mm:ss"),
                    //    RowKey = (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString(),
                    //    Value = TagObj,
                    //    ValueBool = 0,
                    //    Quality = 1
                    //};

                    CurrentValueFromDevice = new
                    {
                        DeviceID = "Pi3",
                        PartitionKey = "PartitionKey",
                        CurrentDateTime = DateTime.Now.ToString("MMMM dd,yyyy, H:mm:ss"),
                        RowKey = (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString(),
                        Temperature = TagObj.ToString(),
                        Altitude =TagObj.ToString(),
                        Pressure =TagObj.ToString(),
                        Humidity = TagObj.ToString()
                    };
                }
                else
                {
                    CurrentValueFromDevice = new
                    {
                        DeviceID = "OPCServer",
                        PartitionKey = "PKPLC",
                        CurrentDateTime = DateTime.Now.ToString("MMMM dd,yyyy, H:mm:ss"),
                        RowKey = (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString(),
                        Value = 0,
                        ValueBool = TagObj,
                        Quality = 1
                    };
                }

                if (CurrentValueFromDevice == null) return;
                var message = JsonConvert.SerializeObject(CurrentValueFromDevice);
                Microsoft.Azure.Devices.Client.Message eventMessage = new Microsoft.Azure.Devices.Client.Message(Encoding.UTF8.GetBytes(message));
                await deviceClient.SendEventAsync(eventMessage);
            }
        }
        #endregion SendEvent Function
    }

    #endregion IoTHub class
}
#endregion AzureIOTHub NameSpace
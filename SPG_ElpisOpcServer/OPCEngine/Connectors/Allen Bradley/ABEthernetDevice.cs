using Elpis.Windows.OPC.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace OPCEngine.Connectors.Allen_Bradley
{
    [DisplayName("Micrologix Ethernet Device")]
    [Serializable]
    public class ABMicrologixEthernetDevice : DeviceBase
    {
        #region Constructor
        /// <summary>
        /// Serialization Constructor
        /// </summary>
        public ABMicrologixEthernetDevice() : base()
        {

        }
        
        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public ABMicrologixEthernetDevice(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
        #endregion Constructor

        #region Properties

        private string ipAddress;

        [Description("Specify the IP address of the object. The valid IP address is something between integers like 0-255.0-255.0-255.0-255"), DisplayName("IP Address *"), PropertyOrder(3)]

        public string IPAddress
        {
            get
            {
                return ipAddress;
            }
            set
            {
                ipAddress = value;
                OnPropertyChanged("IPAddress");
            }
        }

        private ushort port = 44818;

        [Description("Specify the port number of the device.The valid range is 0 to 65,535."), DisplayName("Port Number *"), PropertyOrder(4)]

        public ushort Port
        {
            get
            {
                return port;
            }
            set
            {
                try
                {
                    if (value >= ushort.MinValue && value <= ushort.MaxValue)
                    {
                        port = value;
                    }
                    else
                    {
                        MessageBox.Show("Check the port number");
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Check the port number");
                }
                OnPropertyChanged("Port");
            }
        }



        private AllenBbadleyModel deviceModelType = AllenBbadleyModel.MicroLogix;

        [Description("Select the specific type of device associated in communication."), DisplayName("Device Model Type *"), PropertyOrder(5)]

        public AllenBbadleyModel DeviceModelType
        {
            get
            {
                return deviceModelType;
            }
            set
            {
                deviceModelType = value;
                OnPropertyChanged("DeviceCPUType");
            }
        }
        #endregion Properties

    }
}

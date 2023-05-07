using Elpis.Windows.OPC.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Elpis.Windows.OPC.Server
{
    [DisplayName("TcpSocket Device")]
    [Serializable()]
    public class TcpSocketDevice : DeviceBase
    {
        #region Constructor
        /// <summary>
        /// Serialization constructor
        /// </summary> 
        /// 
       // private readonly TcpListener listener;
        public TcpSocketDevice() : base()
        {
            
        }
        //public TcpSocketDevice(string ipAddress, ushort port)
        //{
        //    this.port = port;
        //    listener = new TcpListener(ipAddress, port);
        //}

        public TcpSocketDevice(SerializationInfo info, StreamingContext context) : base(info, context)
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
        //private IPAddress ipAddress;

        //[Description("Specify the IP address of the object. The valid IP address is something between integers like 0-255.0-255.0-255.0-255"), DisplayName("IP Address *"), PropertyOrder(3)]

        //public IPAddress IPAddress
        //{
        //    get
        //    {
        //        return ipAddress;
        //    }
        //    set
        //    {
        //        ipAddress = value;
        //        OnPropertyChanged("IPAddress");
        //    }
        //}

        private ushort port;

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

        private string deviceId;
        [DisplayName("Device Id *"), Description("Specify the DeviceId"),PropertyOrder(3)]
        public string DeviceId
        {
            get
            {
                return deviceId;
            }
            set
            {
                deviceId = value;
                OnPropertyChanged("DeviceId");
            }
        }
       
        #endregion Properties
    }
}

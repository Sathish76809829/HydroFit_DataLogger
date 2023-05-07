#region Usings
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
#endregion Usings

#region OPCEngine Namespace
namespace Elpis.Windows.OPC.Server
{
    /// <summary>
    /// Modbus Ethernet Device Class
    /// </summary>
    [Serializable(), DisplayName("Modbus Ethernet Device")]
    public class ModbusEthernetDevice : DeviceBase
    {
        #region Constructor
        /// <summary>
        /// Serialization Constructor
        /// </summary>
        public ModbusEthernetDevice():base()
        {

        }
        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public ModbusEthernetDevice(SerializationInfo info, StreamingContext context):base(info,context)
        {

        }
        #endregion Constructor


        #region Properties

        private string ipAddress;

        [Description("Specify the IP address of the object. The valid IP address is something between integers like 0-255.0-255.0-255.0-255"), DisplayName("IP Address *"),PropertyOrder(4)]

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
        //public string IPAddress
        //{
        //    get { return (string)GetValue(IPAddressProperty); }
        //    set
        //    {
        //        bool flag = Util.ValidateIPAddress(value);
        //        if (flag)
        //        {
        //            SetValue(IPAddressProperty, value);
        //        }
        //        else
        //        {
        //            MessageBox.Show("Check The IP Address");
        //        }
        //    }
        //}

        //// Using a DependencyProperty as the backing store for ID.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty IPAddressProperty =
        //    DependencyProperty.Register("IPAddress", typeof(string), typeof(DeviceBase), new PropertyMetadata(null));

        //[Category("Device Properties")]

        private ushort port;

        [Description("Specify the port number of the device.The valid range is 0 to 65,535."), DisplayName("Port Number *"),PropertyOrder(5)]

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

        //public ushort Port
        //{
        //    get
        //    {
        //        return (ushort)GetValue(PortProperty);
        //    }
        //    set
        //    {
        //        try
        //        {
        //            if (value >= ushort.MinValue && value <= ushort.MaxValue)
        //            {
        //                SetValue(PortProperty, value);
        //            }
        //            else
        //            {
        //                MessageBox.Show("Check the port number");
        //            }
        //        }
        //        catch (Exception)
        //        {
        //            MessageBox.Show("Check the port number");
        //        }
        //    }
        //}

        //// Using a DependencyProperty as the backing store for Port.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty PortProperty =
        //    DependencyProperty.Register("Port", typeof(ushort), typeof(DeviceBase), new PropertyMetadata(null));

        #endregion Properties

        //[Browsable(false)]
        //string IDevice.Name { get; set; }        
    }

}
#endregion OPCEngine Namespace

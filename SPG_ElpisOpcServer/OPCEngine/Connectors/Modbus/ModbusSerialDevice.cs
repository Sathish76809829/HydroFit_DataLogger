using Modbus.Device;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO.Ports;
using System.Runtime.Serialization;
using System.Windows;

namespace Elpis.Windows.OPC.Server
{
    [Serializable]
   public class ModbusSerialDevice: SerialDeviceBase
    {
        #region Constructor
        /// <summary>
        /// Serialization Constructor
        /// </summary>
        public ModbusSerialDevice() : base()
        {
            
        }       

       

        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public ModbusSerialDevice(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
        #endregion Constructor


        #region Properties
        [DisplayAttribute(Name = "Baud Rate", Description = "Specifies Baud rate of communication "), RequiredAttribute(AllowEmptyStrings = false, ErrorMessage = "COM Port required")]
        public int BaudRate
        {
            get { return (int)GetValue(BaudRateProperty); }
            set { SetValue(BaudRateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BaudRate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BaudRateProperty =
            DependencyProperty.Register("BaudRate", typeof(int), typeof(ModbusSerialDevice), new PropertyMetadata(null));


        [DisplayAttribute(Name = "COM Port", Description = "On which COM Port server has connect to device"), RequiredAttribute(AllowEmptyStrings = false, ErrorMessage = "COM Port required")]
        public string COMPort
        {
            get { return (string)GetValue(COMPortProperty); }
            set { SetValue(COMPortProperty, value); }
        }

        // Using a DependencyProperty as the backing store for COMPort.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty COMPortProperty =
            DependencyProperty.Register("COMPort", typeof(string), typeof(ModbusSerialDevice), new PropertyMetadata(null));


        [DisplayAttribute(Name = "Data Bits", Description = "specifies the transmission of data between ports."), RequiredAttribute(AllowEmptyStrings = false, ErrorMessage = "COM Port required")]
        public int DataBits
        {
            get { return (int)GetValue(DataBitsProperty); }
            set { SetValue(DataBitsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DataBits.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataBitsProperty =
            DependencyProperty.Register("DataBits", typeof(int), typeof(ModbusSerialDevice), new PropertyMetadata(null));


        [DisplayAttribute(Name = "Parity Bit", Description = "Parity bit used for error checking."), RequiredAttribute(AllowEmptyStrings = false, ErrorMessage = "COM Port required")]
        public Parity ConnectorParityBit
        {
            get { return (Parity)GetValue(ConnectorParityBitProperty); }
            set { SetValue(ConnectorParityBitProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConnectorParityBitProperty =
            DependencyProperty.Register("ConnectorParityBit", typeof(Parity), typeof(ModbusSerialDevice), new PropertyMetadata(null));



        [DisplayAttribute(Name = "Stop Bits", Description = "Specify how to send last bit to serial port"), RequiredAttribute(AllowEmptyStrings = false, ErrorMessage = "COM Port required")]
        public StopBits ConnectorStopBits
        {
            get { return (StopBits)GetValue(ConnectorStopBitsProperty); }
            set { SetValue(ConnectorStopBitsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ConnectorStopBits.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConnectorStopBitsProperty =
            DependencyProperty.Register("ConnectorStopBits",typeof(StopBits), typeof(ModbusSerialDevice), new PropertyMetadata(null));


        //[Browsable(false)]
        //[NonSerialized]
        //private SerialPort port;
        //public SerialPort Port
        //{
        //    get
        //    {
        //        return port;
        //    }

        //    set
        //    {
        //        port = value;
        //    }
        //}


        private SerialPort port;
        
        [Browsable(false)]
        
        public SerialPort Port
        {
            get
            {
                return port;
            }
            set
            {
                port = value;
                OnPropertyChanged("Port");
            }
        }

       
        //public SerialPort Port
        //{
        //    get { return (SerialPort)GetValue(PortProperty); }
        //    set { SetValue(PortProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for Port.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty PortProperty =
        //    DependencyProperty.Register("Port", typeof(SerialPort), typeof(ModbusSerialDevice), new PropertyMetadata(null));

        //private ModbusSerialMaster master;
        //[Browsable(false)]

        //public ModbusSerialMaster Master
        //{
        //    get
        //    {
        //        return master;
        //    }
        //    set
        //    {
        //        master = value;
        //        OnPropertyChanged("Master");
        //    }
        //}
        //public ModbusSerialMaster Master
        //{
        //    get { return (ModbusSerialMaster)GetValue(MasterProperty); }
        //    set { SetValue(MasterProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty MasterProperty =
        //    DependencyProperty.Register("Master", typeof(ModbusSerialMaster), typeof(ModbusSerialDevice), new PropertyMetadata(null));

        private byte slaveId;
       // [Browsable(false)]
        public byte SlaveId
        {
            get
            {
                return slaveId;
            }
            set
            {
                slaveId = value;
            }
        }
        //public byte SlaveId
        //{
        //    get { return (byte)GetValue(slaveIdProperty); }
        //    set { SetValue(slaveIdProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for slaveId.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty slaveIdProperty =
        //    DependencyProperty.Register("slaveId", typeof(byte), typeof(ModbusSerialDevice), new PropertyMetadata(null));



        #endregion Properties
    }
}

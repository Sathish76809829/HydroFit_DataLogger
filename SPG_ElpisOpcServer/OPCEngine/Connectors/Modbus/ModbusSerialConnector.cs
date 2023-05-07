#region Usings
using Modbus.Device;
using NDI.SLIKDA.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
#endregion Usings

namespace Elpis.Windows.OPC.Server
{
    /// <summary>
    /// Modbus Serial Connector Class.
    /// </summary>
    #region ModbusSerial class
    //[Serializable, DisplayName("Modbus Serial Connector")]
    [DisplayName("Modbus Serial Connector")]
    [Serializable]
    public class ModbusSerialConnector : ConnectorBase, IConnector
    {
        #region Constructor
        /// <summary>
        /// Serialization Constructor
        /// </summary>
        public ModbusSerialConnector() : base()
        {

        }
        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public ModbusSerialConnector(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Master = new Dictionary<string, ModbusSerialMaster>();
        }
        #endregion Constructor


        #region Properties        

        string IConnector.Name { get; set; }


        [NonSerialized]
        private Dictionary<string, ModbusSerialMaster> master;
        [NonSerialized]
        private bool isWritingTag;
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Dictionary<string, ModbusSerialMaster> Master
        {
            get
            {
                return master;
            }

            set
            {
                master = value;
            }
        }

        [Browsable(false)]
        public bool IsWritingTag
        {
            get
            {
                return isWritingTag;
            }
            set
            {
                isWritingTag = value;
                OnPropertyChanged("IsWritingTag");
            }
        }

        [Browsable(false)]
        [NonSerialized]
        private Object thisLock = new Object();

        #endregion Properties

        #region Read
        public void Read(ISLIKTag currentItem, DeviceBase device, Tag tag, ushort noOfAddressToRead = 1)
        {
            try
            {
                ModbusSerialDevice serialDevice = device as ModbusSerialDevice;
               // string key = string.Format("{0}.{1}.{2}", tag.ScanRate, device.ConnectorAssignment, device.DeviceName);
                string key = string.Format("{0}.{1}", tag.ScanRate, device.DeviceName);
                if (tag.DataType == DataType.Boolean)
                    ReadCoils(currentItem, tag, serialDevice, noOfAddressToRead, key);
                else if (tag.DataType == DataType.Integer)
                    ReadHoldingRegistersInt(currentItem, tag, serialDevice, 2, key);
                else if (tag.DataType == DataType.Short)
                    ReadHoldingRegistersShort(currentItem, tag, serialDevice, 1, key);
                else if (tag.DataType == DataType.Double)
                    ReadHoldingRegistersDouble(currentItem, tag, serialDevice, 4, key);
                else if (tag.DataType == DataType.String)
                    ReadHoldingRegisterString(currentItem, tag, serialDevice, noOfAddressToRead, key);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        #endregion Read

        #region Subscribe
        public void Subscribe(ISLIKTag currentItem, DeviceBase device, Tag tag, ushort noOfAddresstoRead = 1)
        {

            string key = string.Format("{0}.{1}", tag.ScanRate, device.DeviceName);
            //string key = string.Format("{0}.{1}.{2}", tag.ScanRate, device.ConnectorAssignment, device.DeviceName);
            //CreateModbusConnection(device);
            try
            {
                ModbusSerialDevice serialDevice = device as ModbusSerialDevice;
                //using (SerialPort port = new SerialPort(serialDevice.COMPort.ToUpper()))
                //{
                //    // configure serial port
                //    port.BaudRate = serialDevice.BaudRate;
                //    port.DataBits = serialDevice.DataBits;
                //    port.Parity = serialDevice.ConnectorParityBit;
                //    port.StopBits = serialDevice.ConnectorStopBits;
                //    port.Open();
                //    var adapter = new SerialPortAdapter(port);
                //    // create modbus serial master
                //    ModbusSerialMaster master = ModbusSerialMaster.CreateRtu(adapter);

                //Changed on 04-Apr-2018
                if (Master[key] !=null) // serialDevice.Master != null)
                {
                    switch (currentItem.DataType)
                    {
                        case (short)DataType.Boolean:
                            ReadCoils(currentItem, tag, serialDevice, noOfAddresstoRead, key);
                            break;

                        case (short)DataType.Integer:
                            ReadHoldingRegistersInt(currentItem, tag, serialDevice, 2, key);
                            break;

                        case (short)DataType.Short:
                            ReadHoldingRegistersShort(currentItem, tag, serialDevice, noOfAddresstoRead, key);
                            break;

                        case (short)DataType.Double:
                            ReadHoldingRegistersDouble(currentItem, tag, serialDevice, 4, key);
                            break;

                        case (short)DataType.String:
                            ReadHoldingRegisterString(currentItem, tag, serialDevice, noOfAddresstoRead, key);
                            break;
                    }
                    return;
                }
                else
                {
                    if (currentItem.Quality == (short)QualityStatusEnum.sdaGood)
                        currentItem.SetVQT(null, (short)QualityStatusEnum.sdaBadNotConnected, DateTime.Now);

                }
                //master.Dispose();
                //port.Close();
                //  }

            }
            catch (Exception exception)
            {
                throw exception;
            }

        }

        #endregion Subscribe

        #region Write
        public void Write(ISLIKTag currentSlikdaTag, dynamic currentValue, DeviceBase device, Tag tag, ushort noOfAddressToRead = 1)
        {
            if (tag == null)
            {
                return;
            }
            ModbusSerialDevice serialDevice = device as ModbusSerialDevice;
            if (tag.DataAccessRights == DataAccess.Write || tag.DataAccessRights == DataAccess.ReadWrite)
            {
                if (serialDevice.Port != null && !serialDevice.Port.IsOpen)
                {
                    serialDevice.Port.Open();
                    if (!serialDevice.Port.IsOpen) return;
                }
                //string key = string.Format("{0}.{1}.{2}", tag.ScanRate, device.ConnectorAssignment, device.DeviceName);
                string key = string.Format("{0}.{1}", tag.ScanRate,  device.DeviceName);
                switch (tag.DataType)
                {
                    case DataType.Boolean:
                        try
                        {
                            ushort registerAddress = (ushort)(int.Parse(tag.Address) % 100001);
                            bool[] writeValues = new bool[] { currentValue };
                            Master[key].WriteMultipleCoils(serialDevice.SlaveId, registerAddress, writeValues);
                            currentSlikdaTag.SetVQT(currentValue, 192, DateTime.Now);
                            tag.PrevoiusBooleanTagValue = currentValue;
                            //ElpisServer.Addlogs("Information", @"Server\Data Write", string.Format("value at Address is:{0}-->{1}", tag.Address, currentValue), LogStatus.Information);
                        }
                        catch (Exception e)
                        {
                            IsWritingTag = false;
                            throw e;
                        }
                        break;

                    case DataType.Integer:
                        try
                        {
                            ushort registerAddress = (ushort)(int.Parse(tag.Address) % 400001);
                            byte[] bytes = BitConverter.GetBytes(currentValue);
                            ushort[] results = ValueConverter.ConvertTOUShort(bytes);
                            //master[key].WriteSingleRegister(registerAddress, Convert.ToInt32(currentValue));
                            Master[key].WriteMultipleRegisters(serialDevice.SlaveId, registerAddress, results);
                            currentSlikdaTag.SetVQT(currentValue, 192, DateTime.Now);
                            tag.PrevoiusIntegerTagValue = currentValue;
                            //ElpisServer.Addlogs("Information", @"Server\Data Write", string.Format("Old:value at Address is:{0}-->{1}\nNew:value at Address is:{0}-->{2}", tag.Address, tag.PrevoiusIntegerTagValue, currentValue), LogStatus.Information);
                        }
                        catch (Exception e)
                        {
                            IsWritingTag = false;
                            throw e;
                        }
                        break;

                    case DataType.Short:
                        try
                        {
                            //ConvertAnalogArray(currentValue, EnumDataType.Int16);
                            ushort registerAddress = (ushort)(int.Parse(tag.Address) % 400001);
                            byte[] bytes = BitConverter.GetBytes(currentValue);
                            ushort result = BitConverter.ToUInt16(bytes, 0);
                            ushort[] array1 = new ushort[2];
                            //array1[0] = result;
                            Master[key].WriteSingleRegister(serialDevice.SlaveId, registerAddress, result);
                            currentSlikdaTag.SetVQT(currentValue, 192, DateTime.Now);
                            tag.PrevoiusShortTagValue = currentValue;
                            //ElpisServer.Addlogs("Information", @"Server\Data Write", string.Format("value at Address is:{0}-->{1}", tag.Address, currentValue), LogStatus.Information);


                        }
                        catch (Exception e)
                        {
                            IsWritingTag = false;
                            throw e;
                        }
                        break;

                    case DataType.Double:
                        try
                        {
                            ushort registerAddress = (ushort)(int.Parse(tag.Address) % 400001);
                            //master[key].WriteSingleRegister(registerAddress, (ushort)currentValue);
                            byte[] bytes = BitConverter.GetBytes(currentValue);
                            ushort[] results = ValueConverter.ConvertTOUShort(bytes);
                            //master[key].WriteSingleRegister(registerAddress, results);
                            Master[key].WriteMultipleRegisters(serialDevice.SlaveId, registerAddress, results);
                            currentSlikdaTag.SetVQT(currentValue, 192, DateTime.Now);
                            tag.PrevoiusDoubleTagValue = currentValue;
                            // ElpisServer.Addlogs("Information", @"Server\Data Write", string.Format("value at Address is:{0}-->{1}", TagObjesct.Address, currentValue), LogStatus.Information);
                        }
                        catch (Exception e)
                        {
                            IsWritingTag = false;
                            throw e;
                        }
                        break;

                    case DataType.String:
                        try
                        {
                            byte[] array = Encoding.ASCII.GetBytes(currentValue);
                            var mydata = OPCEngine.Utils.RegisterValueConversion.ConvertAnalogArray(array, EnumDataType.String);
                            ushort registerAddress = (ushort)(int.Parse(tag.Address) % 400001);
                            Byte[] byte1 = WriteString(registerAddress, serialDevice.SlaveId, mydata.ToString(), Master[key], DataType.String);//WriteSingleRegister(registerAddress, mydata.ToString(), Master[key], DataType.String);
                            currentSlikdaTag.SetVQT(int.Parse(byte1[0].ToString()), 192, DateTime.Now);
                            tag.PrevoiusStringTagValue = currentValue;
                            // ElpisServer.Addlogs("Information", @"Server\Data Write", string.Format("value at Address is:{0}-->{1}", tag.Address, byte1[0]), LogStatus.Information);
                        }
                        catch (Exception e)
                        {
                            IsWritingTag = false;
                            throw e;
                        }
                        break;
                }
            }
        }

        #endregion Write


        #region Read Holding Registers Functions

        /// <summary>
        /// Read the Holding Register value on modbus device from the requested registers. Convert the value into equivalent Integer value. 
        /// </summary>
        /// <param name="currentItem"></param>
        /// <param name="tag"></param>
        /// <param name="noOfAddressToRead"></param>
        private void ReadHoldingRegistersInt(ISLIKTag currentItem, Tag tag, ModbusSerialDevice device, ushort noOfAddressToRead, string key)
        {
            if (!IsWritingTag)
            {
                //byte SlaveId = 1;
                try
                {
                    if (device.Port != null)
                    {
                        if (!device.Port.IsOpen)
                        {
                            device.Port.Open();
                            if (!device.Port.IsOpen) return;
                        }
                        ushort registerAddress = (ushort)(int.Parse(tag.Address) % 400001);
                        ushort[] returnReadHoldingRegisters = Master[key].ReadHoldingRegisters(device.SlaveId, registerAddress, noOfAddressToRead);
                        int result = ValueConverter.GetInt32(returnReadHoldingRegisters[1], returnReadHoldingRegisters[0]);

                        if (tag.PrevoiusIntegerTagValue != result && TaskHelper.isTaskRunning)
                        {
                            currentItem.SetVQT(result, (short)QualityStatusEnum.sdaGood, DateTime.Now);
                            tag.PrevoiusIntegerTagValue = result;
                        }
                        else if (!TaskHelper.isTaskRunning)
                        {
                            if (currentItem.Quality != (short)QualityStatusEnum.sdaBadCommFailure)
                            {
                                currentItem.SetVQT(tag.PrevoiusIntegerTagValue, (short)QualityStatusEnum.sdaBadCommFailure, DateTime.Now);
                            }
                        }
                        else if (currentItem.Quality != (short)QualityStatusEnum.sdaGood)
                        {
                            currentItem.SetVQT(result, (short)QualityStatusEnum.sdaGood, DateTime.Now);
                        }

                    }
                }
                catch (TimeoutException te)
                {
                    //if (currentItem.Quality != (short)QualityStatusEnum.sdaBadDeviceFailure)
                    //    currentItem.SetVQT(tag.PrevoiusIntegerTagValue, (short)QualityStatusEnum.sdaBadDeviceFailure, DateTime.Now);
                    //device.RetryCounter--;
                    throw te;

                }
                catch (Exception)
                {
                    ElpisServer.Addlogs("Communication", @"Elpis OPC Server/Serial Communication", "Port is Closed/Disconnected", LogStatus.Error);
                    //throw exception;
                }
            }
        }

        /// <summary>
        /// Read the Coil value on modbus device from the requested registers.
        /// </summary>
        /// <param name="currentItem"></param>
        /// <param name="tag"></param>
        /// <param name="noOfAddressToRead"></param>
        private void ReadCoils(ISLIKTag currentItem, Tag tag, ModbusSerialDevice device, ushort noOfAddressToRead, string key)
        {
            if (!IsWritingTag)
            {
                bool[] returnReadCoils;
                //byte SlaveId = 1;

                try
                {
                    if (device.Port != null)
                    {
                        if (!device.Port.IsOpen)
                        {
                            device.Port.Open();
                            if (!device.Port.IsOpen) return;
                        }
                        returnReadCoils = Master[key].ReadCoils(device.SlaveId, ushort.Parse(tag.Address), noOfAddressToRead);
                        //for (int index = 0; index < returnReadCoils.Length; index++)
                        //{
                        if (tag.PrevoiusBooleanTagValue != returnReadCoils[0] && TaskHelper.isTaskRunning)
                        {
                            currentItem.SetVQT((Boolean)returnReadCoils[0], (short)QualityStatusEnum.sdaGood, DateTime.Now);
                            tag.PrevoiusBooleanTagValue = returnReadCoils[0];
                        }
                        else if (!TaskHelper.isTaskRunning)
                        {
                            if (currentItem.Quality != (short)QualityStatusEnum.sdaBadCommFailure)
                            {
                                currentItem.SetVQT(tag.PrevoiusBooleanTagValue, (short)QualityStatusEnum.sdaBadCommFailure, DateTime.Now);
                            }
                        }
                        else if (currentItem.Quality != (short)QualityStatusEnum.sdaGood)
                        {
                            currentItem.SetVQT(returnReadCoils[0], (short)QualityStatusEnum.sdaGood, DateTime.Now);
                        }
                    }
                }

                catch (TimeoutException te)
                {
                    //if (currentItem.Quality != (short)QualityStatusEnum.sdaBadDeviceFailure)
                    //    currentItem.SetVQT(tag.PrevoiusBooleanTagValue, (short)QualityStatusEnum.sdaBadDeviceFailure, DateTime.Now);
                    //device.RetryCounter--;
                    throw te;

                }
                catch (Exception exception)
                {
                    if (currentItem.Quality != (short)QualityStatusEnum.sdaBad)
                        currentItem.SetVQT(tag.PrevoiusBooleanTagValue, (short)QualityStatusEnum.sdaBadDeviceFailure, DateTime.Now);
                    ElpisServer.Addlogs("Communication", @"Elpis OPC Server/Serial Communication", "Port is Closed/Disconnected" + exception.Message, LogStatus.Error);
                    //throw exception;
                }
            }
        }

        /// <summary>
        /// Read the Holding Register value on modbus device from the requested registers. Convert the value into equivalent String value. 
        /// </summary>
        /// <param name="currentItem"></param>
        /// <param name="tag"></param>
        /// <param name="noOfAddresstoRead"></param>
        private void ReadHoldingRegisterString(ISLIKTag currentItem, Tag tag, ModbusSerialDevice device, ushort noOfAddresstoRead, string key)
        {
            if (!IsWritingTag)
            {
                //byte SlaveId = 1;
                try
                {
                    if (device.Port != null)
                    {
                        if (!device.Port.IsOpen)
                        {
                            device.Port.Open();
                            if (!device.Port.IsOpen) return;
                        }

                        ushort registerAddress = (ushort)(int.Parse(tag.Address) % 400001);
                        ushort[] returnReadHoldingRegister = Master[key].ReadHoldingRegisters(device.SlaveId, registerAddress, noOfAddresstoRead);
                        byte[] asciiBytes = Modbus.Utility.ModbusUtility.GetAsciiBytes(returnReadHoldingRegister);
                        string result = Encoding.ASCII.GetString(asciiBytes);
                        if (tag.PrevoiusStringTagValue.ToString() != result.ToString() && TaskHelper.isTaskRunning)
                        {
                            tag.PrevoiusStringTagValue = result.ToString();
                            currentItem.SetVQT(result, (short)QualityStatusEnum.sdaGood, DateTime.Now);
                            //ElpisServer.Addlogs("Information", @"Server\Data Read", string.Format("value at Address is:{0}-->{1}", TagObjesct.Address, inputs), LogStatus.Information);
                        }
                        else if (!TaskHelper.isTaskRunning)
                        {
                            if (currentItem.Quality != (short)QualityStatusEnum.sdaBadCommFailure)
                            {
                                currentItem.SetVQT(tag.PrevoiusStringTagValue, (short)QualityStatusEnum.sdaBadCommFailure, DateTime.Now);
                            }
                        }
                        else if (currentItem.Quality != (short)QualityStatusEnum.sdaGood)
                        {
                            currentItem.SetVQT(result, (short)QualityStatusEnum.sdaGood, DateTime.Now);
                        }
                    }
                }
                catch (TimeoutException te)
                {
                    //if (currentItem.Quality != (short)QualityStatusEnum.sdaBadDeviceFailure)
                    //    currentItem.SetVQT(tag.PrevoiusStringTagValue, (short)QualityStatusEnum.sdaBadDeviceFailure, DateTime.Now);
                    //device.RetryCounter--;'

                    throw te;

                }
                catch (Exception)
                {
                    ElpisServer.Addlogs("Communication", @"Elpis OPC Server/Serial Communication", "Port is Closed/Disconnected", LogStatus.Error);
                    //throw exception;
                }
            }
        }

        /// <summary>
        /// Read the Holding Register value on modbus device from the requested registers. Convert the value into equivalent Double value. 
        /// </summary>
        /// <param name="currentItem"></param>
        /// <param name="tag"></param>
        /// <param name="noOfAddressToRead"></param>
        private void ReadHoldingRegistersDouble(ISLIKTag currentItem, Tag tag, ModbusSerialDevice device, ushort noOfAddressToRead, string key)
        {
            if (!IsWritingTag)
            {
                // byte SlaveId = 1;
                try
                {
                    if (device.Port != null)
                    {
                        if (!device.Port.IsOpen)
                        {
                            device.Port.Open();
                            if (!device.Port.IsOpen) return;
                        }

                        ushort registerAddress = (ushort)(int.Parse(tag.Address) % (Int32)ModbusAddressType.HoldingRegister);
                        ushort[] returnReadHoldingRegister = Master[key].ReadHoldingRegisters(device.SlaveId, registerAddress, noOfAddressToRead);
                        if (returnReadHoldingRegister.Count() == 4)
                        {
                            double doubleResult = ValueConverter.GetDouble(returnReadHoldingRegister[3], returnReadHoldingRegister[2], returnReadHoldingRegister[1], returnReadHoldingRegister[0]);
                            if (tag.PrevoiusDoubleTagValue != doubleResult && TaskHelper.isTaskRunning)
                            {
                                tag.PrevoiusDoubleTagValue = doubleResult;
                                currentItem.SetVQT(doubleResult, (short)QualityStatusEnum.sdaGood, DateTime.Now);
                                //ElpisServer.Addlogs("Information", @"Server\Data Read", string.Format("value at Address is:{0}-->{1}", TagObjesct.Address, mydata), LogStatus.Information);
                            }
                            else if (!TaskHelper.isTaskRunning)
                            {
                                if (currentItem.Quality != (short)QualityStatusEnum.sdaBadCommFailure)
                                {
                                    currentItem.SetVQT(tag.PrevoiusDoubleTagValue, (short)QualityStatusEnum.sdaBadCommFailure, DateTime.Now);
                                }
                            }
                            else if (currentItem.Quality != (short)QualityStatusEnum.sdaGood)
                            {
                                currentItem.SetVQT(doubleResult, (short)QualityStatusEnum.sdaGood, DateTime.Now);
                            }
                        }
                    }
                }
                catch (TimeoutException te)
                {
                    //if (currentItem.Quality != (short)QualityStatusEnum.sdaBadDeviceFailure)
                    //    currentItem.SetVQT(tag.PrevoiusDoubleTagValue, (short)QualityStatusEnum.sdaBadDeviceFailure, DateTime.Now);
                    //device.RetryCounter--;
                    throw te;

                }
                catch (Exception)
                {
                    ElpisServer.Addlogs("Communication", @"Elpis OPC Server/Serial Communication", "Port is Closed/Disconnected", LogStatus.Error);
                    //throw exception;
                }
            }
        }

        /// <summary>
        /// Read the Holding Register value on modbus device from the requested registers. Convert the value into equivalent Short value. 
        /// </summary>
        /// <param name="currentTag"></param>
        /// <param name="tag"></param>
        /// <param name="noOfAddresstoRead"></param>
        private void ReadHoldingRegistersShort(ISLIKTag currentTag, Tag tag, ModbusSerialDevice device, ushort noOfAddresstoRead, string key)
        {
            //lock (thisLock)
            //{
            if (!IsWritingTag)
            {
                Debug.WriteLine(device.SlaveId.ToString());
                try
                {
                    #region comment
                    //device = device as ModbusSerialDevice;
                    //string[] list = key.Split('.');
                    //ModbusSerialConnector connector = ServerConnectorCollection.FirstOrDefault(c => c.Name == list[1]) as ModbusSerialConnector;

                    //ModbusSerialDevice serialDevice = device as ModbusSerialDevice;
                    //using (SerialPort port = new SerialPort(serialDevice.COMPort.ToUpper()))
                    //{
                    //    // configure serial port
                    //    port.BaudRate = serialDevice.BaudRate;
                    //    port.DataBits = serialDevice.DataBits;
                    //    port.Parity = serialDevice.ConnectorParityBit;
                    //    port.StopBits = serialDevice.ConnectorStopBits;
                    //    port.Open();
                    //    var adapter = new SerialPortAdapter(port);
                    //    // create modbus serial master
                    //    ModbusSerialMaster master = ModbusSerialMaster.CreateRtu(adapter);
                    #endregion

                    if (device.Port != null)
                    {
                        if (!device.Port.IsOpen)
                        {
                            device.Port.Open();
                            if (!device.Port.IsOpen) return;
                        }
                        // device.Port.ReadTimeout = 800;
                        ushort registerAddress = (ushort)(int.Parse(tag.Address) % 400001);
                        ushort[] readResult = Master[key].ReadHoldingRegisters(device.SlaveId, registerAddress, noOfAddresstoRead);
                        byte[] value = BitConverter.GetBytes(readResult[0]).ToArray();
                        short result = BitConverter.ToInt16(value, 0);
                        if (tag.PrevoiusShortTagValue != result && TaskHelper.isTaskRunning)
                        {
                            tag.PrevoiusShortTagValue = result;
                            currentTag.SetVQT(result, 192, DateTime.Now);
                        }
                        else if (!TaskHelper.isTaskRunning)
                        {
                            if (currentTag.Quality != (short)QualityStatusEnum.sdaBadCommFailure)
                            {
                                currentTag.SetVQT(tag.PrevoiusShortTagValue, (short)QualityStatusEnum.sdaBadCommFailure, DateTime.Now);
                            }
                        }
                        else if (currentTag.Quality != (short)QualityStatusEnum.sdaGood)
                        {
                            currentTag.SetVQT(result, (short)QualityStatusEnum.sdaGood, DateTime.Now);
                        }
                        //device.Port.Close();
                    }

                }
                catch (TimeoutException te)
                {
                //    if (currentTag.Quality != (short)QualityStatusEnum.sdaBadDeviceFailure)
                //        currentTag.SetVQT(tag.PrevoiusShortTagValue, (short)QualityStatusEnum.sdaBadDeviceFailure, DateTime.Now);
                //    device.RetryCounter--;
                    throw te;
                }
                catch (Exception)
                {
                    ElpisServer.Addlogs("Communication", @"Elpis OPC Server/Serial Communication", "Port is Closed/Disconnected", LogStatus.Error);
                    //throw exception;
                }
            }
            // }
        }
        #endregion Read Holding Registers Functions


        public byte[] WriteString(ushort registerAddress, byte SlaveId, string value, ModbusSerialMaster master, DataType dataType)
        {
            byte[] asciiBytes = Encoding.ASCII.GetBytes(value);
            //byte[] asciiBytes = Encoding.Unicode.GetBytes(value);
            int result = asciiBytes.Max();
            try
            {
                master.WriteSingleRegister(SlaveId, registerAddress, Convert.ToUInt16(value));
            }
            catch (Exception ex)
            {
                //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                    ElpisServer.Addlogs("Configuration", @"Elpis/Communication", ex.Message, LogStatus.Error);
                //}), DispatcherPriority.Normal, null);
            }
            return (BitConverter.GetBytes(result));
        }


        #region new Subscribe implemented on 05-Mar-2018

        internal void Subscribe(List<ISLIKTag> slikdaTagList, DeviceBase deviceBaseObject, ObservableCollection<Tag> tagsCollections, ObservableCollection<TagGroup> groupsCollection, ushort noOfAddresstoRead = 1)
        {
            //lock (thisLock)
            //{
            //string key = string.Format("{0}.{1}.{2}", tagsCollections[0].ScanRate, deviceBaseObject.ConnectorAssignment, deviceBaseObject.DeviceName);
            string key = string.Format("{0}.{1}", tagsCollections[0].ScanRate, deviceBaseObject.DeviceName);
            try
            {
                ModbusSerialDevice modbusSerialDevice = deviceBaseObject as ModbusSerialDevice;


                ReadHoldingRegistersShort(slikdaTagList[0], tagsCollections[0], modbusSerialDevice, noOfAddresstoRead, key);
                ReadHoldingRegistersShort(slikdaTagList[1], tagsCollections[1], modbusSerialDevice, noOfAddresstoRead, key);
                ReadHoldingRegistersShort(slikdaTagList[2], tagsCollections[2], modbusSerialDevice, noOfAddresstoRead, key);
                ReadHoldingRegistersShort(slikdaTagList[3], tagsCollections[3], modbusSerialDevice, noOfAddresstoRead, key);
                ReadHoldingRegistersShort(slikdaTagList[4], tagsCollections[4], modbusSerialDevice, noOfAddresstoRead, key);
                ReadHoldingRegistersShort(slikdaTagList[5], tagsCollections[5], modbusSerialDevice, noOfAddresstoRead, key);

                //Parallel.ForEach(slikdaTagList, item =>
                // {
                //     Debug.WriteLine(item.Name);
                //     //foreach (ISLIKTag item in slikdaTagList)
                //     //{
                //     if (item.Active)
                //    {
                //        //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //        //{
                //        if (modbusSerialDevice.Master != null)
                //        {
                //            Tag tag = null;
                //            string[] tagDescription = item.Name.Split('.');
                //            if (tagDescription.Count() == 4)
                //            {
                //                tag = tagsCollections.FirstOrDefault(t => t.TagName == tagDescription[3]);
                //            }
                //            else
                //            {
                //                TagGroup tagGroup = groupsCollection.FirstOrDefault(g => g.GroupName == tagDescription[3]);
                //                tag = tagGroup.TagsCollection.FirstOrDefault(t => t.TagName == tagDescription[4]);
                //            }
                //            if (tag != null)
                //            {
                //                switch (item.DataType)
                //                {
                //                    case (short)DataType.Boolean:
                //                        ReadCoils(item, tag, modbusSerialDevice, noOfAddresstoRead, key);
                //                        break;

                //                    case (short)DataType.Integer:
                //                        ReadHoldingRegistersInt(item, tag, modbusSerialDevice, 2, key);
                //                        break;

                //                    case (short)DataType.Short:
                //                        ReadHoldingRegistersShort(item, tag, modbusSerialDevice, noOfAddresstoRead, key);
                //                        break;

                //                    case (short)DataType.Double:
                //                        ReadHoldingRegistersDouble(item, tag, modbusSerialDevice, 4, key);
                //                        break;

                //                    case (short)DataType.String:
                //                        ReadHoldingRegisterString(item, tag, modbusSerialDevice, noOfAddresstoRead, key);
                //                        break;
                //                }
                //            }                           
                //        }

                //        else
                //        {
                //            if (item.Quality == (short)QualityStatusEnum.sdaGood)
                //                item.SetVQT(null, (short)QualityStatusEnum.sdaBadNotConnected, DateTime.Now);

                //        }
                //        //}), DispatcherPriority.Background, null);

                //    }
                //     // }
                // });
                //master.Dispose();
                //port.Close();
                //  }

            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            //}
        }

        internal void Subscribe(Dictionary<ISLIKTag, Tag> mappedList, DeviceBase deviceBaseObject,int scanRate, ushort noOfAddresstoRead = 1)
        {
            string key = string.Format("{0}.{1}", scanRate, deviceBaseObject.DeviceName);
            //string key = string.Format("{0}.{1}.{2}", scanRate, deviceBaseObject.ConnectorAssignment, deviceBaseObject.DeviceName);
            try
            {
                ModbusSerialDevice modbusSerialDevice = deviceBaseObject as ModbusSerialDevice;
                Parallel.ForEach(mappedList, (item,state) =>

                //foreach (var item in mappedList)
                {
                    try
                    {
                        switch (item.Key.DataType)
                    {
                    
                        case (short)DataType.Boolean:
                            ReadCoils(item.Key, item.Value, modbusSerialDevice, noOfAddresstoRead, key);
                        break;

                        case (short)DataType.Integer:
                            ReadHoldingRegistersInt(item.Key, item.Value, modbusSerialDevice, 2, key);
                        break;

                        case (short)DataType.Short:
                            ReadHoldingRegistersShort(item.Key, item.Value, modbusSerialDevice, noOfAddresstoRead, key);
                        break;

                        case (short)DataType.Double:
                            ReadHoldingRegistersDouble(item.Key, item.Value, modbusSerialDevice, 4, key);
                        break;

                        case (short)DataType.String:
                            ReadHoldingRegisterString(item.Key, item.Value, modbusSerialDevice, noOfAddresstoRead, key);
                        break;
                    }
                    
                }
                    catch(TimeoutException te)
                    {
                        state.Break();
                        throw te;
                    }
                    catch (Exception ex)
                    {
                        state.Break();                        
                        throw ex;
                    }
                }
               );
            }
            catch (TimeoutException)
            {
                
               // throw te;
            }
            catch (Exception)
            {
                //throw exception;
            }
        }

        #endregion

    }

    #endregion ModbusSerial class
}

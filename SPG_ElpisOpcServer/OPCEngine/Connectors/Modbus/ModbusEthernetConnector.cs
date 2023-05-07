#region Usings
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Modbus.Device;
using NDI.SLIKDA.Interop;
using System.Runtime.Serialization;
using System.Configuration;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using System.Collections.ObjectModel;
#endregion Usings

#region OPCEngine Namespace
namespace Elpis.Windows.OPC.Server
{
    #region ModbusEthernetConnector class
    /// <summary>
    /// Modbus Ethernet Connector Class.
    /// </summary>
    [DisplayName("Modbus Ethernet Connector")]
    [Serializable]
    public class ModbusEthernetConnector : ConnectorBase, IConnector
    {
        #region Constructor
        /// <summary>
        /// Serialization Constructor
        /// </summary>
        public ModbusEthernetConnector() : base()
        {
            // master = new Dictionary<string, ModbusIpMaster>();
        }
        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public ModbusEthernetConnector(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
        #endregion Constructor

        #region Properties

        [Description("Specify the name of the Connector"), DisplayName("Connector Name"), Browsable(false)]
        string IConnector.Name { get; set; }

        [NonSerialized]
        private Dictionary<string, ModbusIpMaster> master;
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Dictionary<string, ModbusIpMaster> Master
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

        [NonSerialized]
        private bool isWritingTag;
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
        #endregion Properties

        #region Read
        /// <summary>
        /// This Method Read the register value form the device based on the tag address. noOfAddressToRead default value is 1
        /// 
        /// </summary>
        /// <param name="currentItem"></param>
        /// <param name="device"></param>
        /// <param name="tag"></param>
        /// <param name="noOfAddressToRead">Default Parameter, default is 1</param>
        public void Read(ISLIKTag currentItem, DeviceBase device, Tag tag, ushort noOfAddressToRead = 1)
        {
            if (tag.DataAccessRights == DataAccess.Read || tag.DataAccessRights == DataAccess.ReadWrite)
            {
                try
                {
                    if (currentItem == null)
                    {
                        Debug.Assert(false);
                        //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        //{
                            ElpisServer.Addlogs("Configuration", @"Elpis/Communication/OnRead", "SlikTag is Null", LogStatus.Error);
                        //}), DispatcherPriority.Normal, null);
                        return;
                    }
                    if (device == null)
                    {
                        Debug.Assert(false);
                        //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        //{
                            ElpisServer.Addlogs("Configuration", @"Elpis/Communication/OnRead", "Device is Null", LogStatus.Error);
                        //}), DispatcherPriority.Normal, null);
                        return;
                    }
                    if (tag == null)
                    {
                        Debug.Assert(false);
                        //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        //{
                            ElpisServer.Addlogs("Configuration", @"Elpis/Communication/OnRead", "Tag is Null", LogStatus.Error);
                        //}), DispatcherPriority.Normal, null);
                        return;
                    }

                    string key = string.Format("{0}.{1}.{ 2}", tag.ScanRate, device.ConnectorAssignment, device.DeviceName);
                    if (tag.DataType == DataType.Boolean)
                        ReadCoils(currentItem, tag, noOfAddressToRead, key);
                    else if (tag.DataType == DataType.Integer)
                        ReadHoldingRegistersInt(currentItem, tag, 2, key);
                    else if (tag.DataType == DataType.Short)
                        ReadHoldingRegistersShort(currentItem, tag, 1, key);
                    else if (tag.DataType == DataType.Double)
                        ReadHoldingRegistersDouble(currentItem, tag, 4, key);
                    else if (tag.DataType == DataType.String)
                        ReadHoldingRegisterString(currentItem, tag, noOfAddressToRead, key);

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }


        #endregion Read


        #region Subscribe
        /// <summary>
        /// This method Set the tag value and  quality with time stamp on OPC Client. noOfAddresstoRead default value is 1.
        /// </summary>
        /// <param name="currentItem"></param>
        /// <param name="device"></param>
        /// <param name="tag"></param>
        /// <param name="noOfAddresstoRead"></param>
        public void Subscribe(ISLIKTag currentItem, DeviceBase device, Tag tag, ushort noOfAddresstoRead = 1)
        {
            string key = string.Format("{0}.{1}.{2}", tag.ScanRate, device.ConnectorAssignment, device.DeviceName);
            //CreateModbusConnection(device);
            try
            {
                if (Master != null)
                {
                    switch (currentItem.DataType)
                    {
                        case (short)DataType.Boolean:
                            ReadCoils(currentItem, tag, noOfAddresstoRead, key);
                            break;

                        case (short)DataType.Integer:
                            ReadHoldingRegistersInt(currentItem, tag, 2, key);
                            break;

                        case (short)DataType.Short:
                            ReadHoldingRegistersShort(currentItem, tag, noOfAddresstoRead, key);
                            break;

                        case (short)DataType.Double:
                            ReadHoldingRegistersDouble(currentItem, tag, 4, key);
                            break;

                        case (short)DataType.String:
                            ReadHoldingRegisterString(currentItem, tag, noOfAddresstoRead, key);
                            break;
                    }
                }
                else
                {
                    currentItem.SetVQT(null, (short)QualityStatusEnum.sdaBadNotConnected, DateTime.Now);
                    //CreateModbusConnection(device);
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            //DeleteModbusConnection(master);
        }
        #endregion Subscribe


        #region Write
        /// <summary>
        /// This Method Update/Overwrite the current value on device. noOfAddressToRead default value is 1.
        /// </summary>
        /// <param name="currentItem"></param>
        /// <param name="currentValue"></param>
        /// <param name="device"></param>
        /// <param name="tag"></param>
        /// <param name="noOfAddressToRead"></param>
        public void Write(ISLIKTag currentItem, dynamic currentValue, DeviceBase device, Tag tag, ushort noOfAddressToRead = 1)
        {
            if (tag == null)
            {
                return;
            }
            if(device==null)
            {
                return;
            }

            if (tag.DataAccessRights == DataAccess.Write || tag.DataAccessRights == DataAccess.ReadWrite)
            {
                ModbusEthernetDevice ethernetDevice = device as ModbusEthernetDevice;
                string key = string.Format("{0}.{1}", device.ConnectorAssignment, device.DeviceName);
                switch (tag.DataType)
                {
                    case DataType.Boolean:
                        try
                        {
                            ushort registerAddress = (ushort)(int.Parse(tag.Address) % 100001);
                            Master[key].WriteSingleCoil(registerAddress, (bool)currentValue);
                            currentItem.SetVQT(currentValue, 192, DateTime.Now);
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
                            Master[key].WriteMultipleRegisters(registerAddress, results);
                            currentItem.SetVQT(currentValue, 192, DateTime.Now);
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
                            ushort[] data =new ushort[]{ result};
                            Master[key].WriteMultipleRegisters(registerAddress, data);
                            currentItem.SetVQT(currentValue, 192, DateTime.Now);
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
                            #region previous code
                            //ushort numRegisters = 2;
                            //ushort[] arrayToWrite=null;
                            //byte[] bytes1 = BitConverter.GetBytes(currentValue);
                            //ushort registerAddress = (ushort)(int.Parse(TagObjesct.Address) % 400001);
                            //ushort[] registers1 = master.ReadHoldingRegisters(registerAddress, numRegisters);
                            //var shorts = bytes1.Select(n => System.Convert.ToInt16(n)).ToArray();
                            //master.WriteSingleRegister(registerAddress,(ushort) currentValue);
                            //#region  28 april 2017 tried
                            //ushort[] registers = master.ReadHoldingRegisters(ushort.Parse(TagObjesct.Address.ToString()), numRegisters);
                            //var mybitsmm = BitConverter.GetBytes(currentValue);
                            //byte[] bytes = new byte[4];
                            //bytes[0] = (byte)(mybitsmm[0] & 0xFF);
                            //bytes[1] = (byte)(mybitsmm[0] >> 8);
                            //bytes[2] = (byte)(mybitsmm[1] & 0xFF);
                            //bytes[3] = (byte)(mybitsmm[1] >> 8);
                            //float readValue = BitConverter.ToSingle(bytes, 0);
                            //#endregion 28 april 2017 tried
                            //var mybits = BitConverter.GetBytes(currentValue);
                            //var mydata = ConvertAnalogArray(mybits, EnumDataType.Float64);
                            ////for (ushort objIndex = 0; objIndex < mybits.Length; ++objIndex)
                            ////{
                            ////    mydata[objIndex] = (ushort)((mydata[objIndex * sizeof(UInt16)] << 8) +
                            ////                                                 mydata[objIndex + 1]);
                            ////}
                            ////master.WriteSingleRegister(ushort.Parse(TagObjesct.Address.ToString()), (dynamic)currentValue);
                            #endregion

                            ushort registerAddress = (ushort)(int.Parse(tag.Address) % 400001);
                            //master[key].WriteSingleRegister(registerAddress, (ushort)currentValue);
                            byte[] bytes = BitConverter.GetBytes(currentValue);
                            ushort[] results = ValueConverter.ConvertTOUShort(bytes);
                            //master[key].WriteSingleRegister(registerAddress, results);
                            Master[key].WriteMultipleRegisters(registerAddress, results);
                            currentItem.SetVQT(currentValue, 192, DateTime.Now);
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
                            Byte[] byte1 = WriteString(registerAddress, mydata.ToString(), Master[key], DataType.String);//WriteSingleRegister(registerAddress, mydata.ToString(), Master[key], DataType.String);
                            currentItem.SetVQT(int.Parse(byte1[0].ToString()), 192, DateTime.Now);
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


        /// <summary>
        /// String data type tag values are updated in device
        /// </summary>
        /// <param name="registerAddress"></param>
        /// <param name="value"></param>
        /// <param name="master"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public byte[] WriteString(ushort registerAddress, string value, ModbusIpMaster master, DataType dataType)
        {
            byte[] asciiBytes = Encoding.ASCII.GetBytes(value);
            //byte[] asciiBytes = Encoding.Unicode.GetBytes(value);
            int result = asciiBytes.Max();
            try
            {
                master.WriteSingleRegister(registerAddress, Convert.ToUInt16(value));
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


        #endregion Write

        #region Read Holding Registers Functions

        /// <summary>
        /// Read the Holding Register value on modbus device from the requested registers. Convert the value into equivalent Integer value. 
        /// </summary>
        /// <param name="currentItem"></param>
        /// <param name="tag"></param>
        /// <param name="noOfAddressToRead"></param>
        private void ReadHoldingRegistersInt(ISLIKTag currentItem, Tag tag, ushort noOfAddressToRead, string key)
        {
            try
            {
                if (!IsWritingTag)
                {
                    ushort registerAddress = (ushort)(int.Parse(tag.Address) % 400001);
                    ushort[] returnReadHoldingRegisters = Master[key].ReadHoldingRegisters(registerAddress, noOfAddressToRead);
                    int result = ValueConverter.GetInt32(returnReadHoldingRegisters[1], returnReadHoldingRegisters[0]);

                    if (tag.PrevoiusIntegerTagValue != result)
                    {
                        currentItem.SetVQT(result, (short)QualityStatusEnum.sdaGood, DateTime.Now);
                        tag.PrevoiusIntegerTagValue = result;
                    }
                }
            }
            catch (Exception exception)
            {
                //currentItem.SetVQT(tag.PrevoiusIntegerTagValue, (short)QualityStatusEnum.sdaBadNotConnected, DateTime.Now);
                throw exception;
            }
        }

        /// <summary>
        /// Read the Coil value on modbus device from the requested registers.
        /// </summary>
        /// <param name="currentItem"></param>
        /// <param name="tag"></param>
        /// <param name="noOfAddressToRead"></param>
        private void ReadCoils(ISLIKTag currentItem, Tag tag, ushort noOfAddressToRead, string key)
        {
            bool[] returnReadCoils;
            try
            {
                if (!IsWritingTag)
                {
                    returnReadCoils = Master[key].ReadCoils(ushort.Parse(tag.Address), noOfAddressToRead);
                    if (tag.PrevoiusBooleanTagValue != returnReadCoils[0])
                    {
                        tag.PrevoiusBooleanTagValue = returnReadCoils[0];
                        currentItem.SetVQT(returnReadCoils[0], (short)QualityStatusEnum.sdaGood, DateTime.Now);
                    }

                    //for (int index = 0; index < returnReadCoils.Length; index++)
                    //{
                    //    if (tag.PrevoiusBooleanTagValue != returnReadCoils[index])
                    //    {
                    //        tag.PrevoiusBooleanTagValue = returnReadCoils[index];
                    //        currentItem.SetVQT(returnReadCoils[index], (short)QualityStatusEnum.sdaGood, DateTime.Now);
                    //    }

                    //}
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Read the Holding Register value on modbus device from the requested registers. Convert the value into equivalent String value. 
        /// </summary>
        /// <param name="currentItem"></param>
        /// <param name="tag"></param>
        /// <param name="noOfAddresstoRead"></param>
        private void ReadHoldingRegisterString(ISLIKTag currentItem, Tag tag, ushort noOfAddresstoRead, string key)
        {
            try
            {
                if (!IsWritingTag)
                {
                    ushort registerAddress = (ushort)(int.Parse(tag.Address) % 400001);
                    ushort[] returnReadHoldingRegister = Master[key].ReadHoldingRegisters(registerAddress, noOfAddresstoRead);
                    byte[] asciiBytes = Modbus.Utility.ModbusUtility.GetAsciiBytes(returnReadHoldingRegister);
                    string result = Encoding.ASCII.GetString(asciiBytes);
                    if (tag.PrevoiusStringTagValue.ToString() != result.ToString())
                    {
                        tag.PrevoiusStringTagValue = result.ToString();
                        currentItem.SetVQT(result, (short)QualityStatusEnum.sdaGood, DateTime.Now);
                        //ElpisServer.Addlogs("Information", @"Server\Data Read", string.Format("value at Address is:{0}-->{1}", TagObjesct.Address, inputs), LogStatus.Information);
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Read the Holding Register value on modbus device from the requested registers. Convert the value into equivalent Double value. 
        /// </summary>
        /// <param name="currentItem"></param>
        /// <param name="tag"></param>
        /// <param name="noOfAddressToRead"></param>
        private void ReadHoldingRegistersDouble(ISLIKTag currentItem, Tag tag, ushort noOfAddressToRead, string key)
        {
            try
            {
                if (!IsWritingTag)
                {
                    ushort registerAddress = (ushort)(int.Parse(tag.Address) % (Int32)ModbusAddressType.HoldingRegister);
                    ushort[] returnReadHoldingRegister = Master[key].ReadHoldingRegisters(registerAddress, noOfAddressToRead);
                    if (returnReadHoldingRegister.Count() == 4)
                    {
                        double doubleResult = ValueConverter.GetDouble(returnReadHoldingRegister[3], returnReadHoldingRegister[2], returnReadHoldingRegister[1], returnReadHoldingRegister[0]);
                        if (tag.PrevoiusDoubleTagValue != doubleResult)
                        {
                            tag.PrevoiusDoubleTagValue = doubleResult;
                            currentItem.SetVQT(doubleResult, (short)QualityStatusEnum.sdaGood, DateTime.Now);
                            //ElpisServer.Addlogs("Information", @"Server\Data Read", string.Format("value at Address is:{0}-->{1}", TagObjesct.Address, mydata), LogStatus.Information);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Read the Holding Register value on modbus device from the requested registers. Convert the value into equivalent Short value. 
        /// </summary>
        /// <param name="currentItem"></param>
        /// <param name="tag"></param>
        /// <param name="noOfAddresstoRead"></param>
        private void ReadHoldingRegistersShort(ISLIKTag currentItem, Tag tag, ushort noOfAddresstoRead, string key)
        {
            try
            {
                if (!IsWritingTag)
                {
                    ushort registerAddress = (ushort)(int.Parse(tag.Address) % 400001);
                    ushort readResult = Master[key].ReadHoldingRegisters(registerAddress, noOfAddresstoRead)[0];
                    byte[] value = BitConverter.GetBytes(readResult).ToArray();
                    short result = BitConverter.ToInt16(value, 0);
                    //IntResult = ModbusTcpMasterReadSingleRegister(TagObjesct.Address.ToString(), master);
                    if (tag.PrevoiusShortTagValue != result)
                    {
                        tag.PrevoiusShortTagValue = result;
                        currentItem.SetVQT(result, 192, DateTime.Now);
                    }
                    else if (currentItem.Quality != (short)QualityStatusEnum.sdaGood)
                    {
                        currentItem.SetVQT(result, (short)QualityStatusEnum.sdaGood, DateTime.Now);
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
        #endregion Read Holding Registers Functions

        #region ModbusIPMaster Destroy
        /// <summary>
        /// This Method destroy the ModbusIPMaster object.
        /// </summary>
        /// <param name="master"></param>
        private void DeleteModbusConnection(ModbusIpMaster master)
        {
            if (master != null)
            {
                master.Dispose();
                master = null;
            }
        }
        #endregion ModbusIPMaster Destroy

        #region ModbusIPMaster Creation
        /// <summary>
        /// This Method Creates a new master based on the device.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private bool CreateModbusConnection(DeviceBase device, int scanRate)
        {
            string key = string.Format("{0}.{1}.{2}", scanRate, device.ConnectorAssignment, device.DeviceName);
            if (!(device is ModbusEthernetDevice))
            {
                Debug.Assert(false);
                //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                    ElpisServer.Addlogs("Configuration", @"Elpis/Communication", "Problem in creating Modbus Connection", LogStatus.Error);
                //}), DispatcherPriority.Normal, null);

                return false;
            }
            ModbusEthernetDevice modbusEthernetDevice = (ModbusEthernetDevice)device;
            try
            {
                TcpClient tcpClient = new TcpClient(modbusEthernetDevice.IPAddress, modbusEthernetDevice.Port);
                Master[key] = ModbusIpMaster.CreateIp(tcpClient);
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return true;
        }
        #endregion ModbusIPMaster Creation


        #region ModbusTcpMasterWriteRegisters Method

        /// <summary>
        /// Simple Modbus TCP master read inputs example.
        /// </summary>
        public byte[] WriteSingleRegister(ushort startAddress, dynamic value, ModbusIpMaster master, DataType dataType)
        {
            //TODO: --Done Re Write this Function Call for different data types 
            if (dataType == DataType.Integer || dataType == DataType.Double || dataType == DataType.Short)
            {
                if (value <= 65535)
                {
                    try
                    {
                        var mybits = BitConverter.GetBytes(value);
                        ushort[] myushort = new ushort[4];
                        //for (ushort objIndex = 0; objIndex < myushort.Length; ++objIndex)
                        //{
                        //    myushort[objIndex] = (ushort)((mybits[objIndex * sizeof(UInt16)] << 8) +
                        //                          mybits[objIndex + 1]);
                        //}

                        ushort des = BitConverter.ToUInt16(mybits, 0);
                        //byte[] val = BitConverter.GetBytes();                       
                        master.WriteSingleRegister(startAddress, Convert.ToUInt16(value));
                        return (BitConverter.GetBytes(value));
                    }
                    catch (Exception e)
                    {
                        //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        //{
                            ElpisServer.Addlogs("Configuration", @"ElpisServer/Configuration/WriteSingleRegister", e.Message, LogStatus.Error);
                        //}), DispatcherPriority.Normal, null);
                    }
                }
                return (BitConverter.GetBytes(65535));
            }
            else
            {
                byte[] asciiBytes = Encoding.ASCII.GetBytes(value);
                //byte[] asciiBytes = Encoding.Unicode.GetBytes(value);
                int result = asciiBytes.Max();
                try
                {
                    master.WriteSingleRegister(startAddress, Convert.ToUInt16(value));
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
            //return Encoding.ASCII.GetBytes("");
        }

        #endregion ModbusTcpMasterWriteRegisters Method



        #region new Subscribe implemented on 05-Mar-2018

        internal void Subscribe(List<ISLIKTag> slikdaTagList, DeviceBase deviceBaseObject, ObservableCollection<Tag> tagsCollections, ObservableCollection<TagGroup> groupsCollection, ushort noOfAddresstoRead = 1)
        {
            string key = string.Format("{0}.{1}.{2}", tagsCollections[0].ScanRate, deviceBaseObject.ConnectorAssignment, deviceBaseObject.DeviceName);
            try
            {
                if (deviceBaseObject != null)
                {
                    ModbusEthernetDevice modbusEthernetDevice = deviceBaseObject as ModbusEthernetDevice;
                    foreach (ISLIKTag slikdaTag in slikdaTagList)
                    {
                        if (slikdaTag.Active)
                        {
                            if (Master[key] != null)
                            {
                                Tag tag = null;
                                string[] tagDescription = slikdaTag.Name.Split('.');
                                if (tagDescription.Count() == 4)
                                {
                                    tag = tagsCollections.FirstOrDefault(t => t.TagName == tagDescription[3]);
                                }
                                else
                                {
                                    TagGroup tagGroup = groupsCollection.FirstOrDefault(g => g.GroupName == tagDescription[3]);
                                    tag = tagGroup.TagsCollection.FirstOrDefault(t => t.TagName == tagDescription[4]);
                                }
                                switch (slikdaTag.DataType)
                                {
                                    case (short)DataType.Boolean:
                                        ReadCoils(slikdaTag, tag, noOfAddresstoRead, key);
                                        break;

                                    case (short)DataType.Integer:
                                        ReadHoldingRegistersInt(slikdaTag, tag, 2, key);
                                        break;

                                    case (short)DataType.Short:
                                        ReadHoldingRegistersShort(slikdaTag, tag, noOfAddresstoRead, key);
                                        break;

                                    case (short)DataType.Double:
                                        ReadHoldingRegistersDouble(slikdaTag, tag, 4, key);
                                        break;

                                    case (short)DataType.String:
                                        ReadHoldingRegisterString(slikdaTag, tag, noOfAddresstoRead, key);
                                        break;
                                }
                            }


                            else
                            {
                                if (slikdaTag.Quality == (short)QualityStatusEnum.sdaGood)
                                    slikdaTag.SetVQT(null, (short)QualityStatusEnum.sdaBadNotConnected, DateTime.Now);

                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
               
        #endregion

    }
    #endregion ModbusEthernetConnector class

}
#endregion OPCEngine Namespace


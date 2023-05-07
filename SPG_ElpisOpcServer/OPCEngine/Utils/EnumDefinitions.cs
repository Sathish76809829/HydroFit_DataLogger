#region Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#endregion Namespaces

#region OPCEngine namespace
namespace Elpis.Windows.OPC.Server
{

    #region ConnectionType Enum
    /// <summary>
    /// ConnectionType Enum
    /// </summary>
    public enum ConnectionType
    {
        Ethernet,
        Port,
        None
    }

    #endregion End Of ConnectionType Enum

    #region DataAndAccessType Enum
    /// <summary>
    ///  DataAndAccessType Enum
    /// </summary>
    public enum DataAndAccessType
    {
        Read,
        Write,
        ReadWrite,
        Integer,
        Boolean,
        String,
        ReplaceWithZero,
        UnModified,
        IPAddress,
        NetWorkCard,
        TCP_IP,
        UDP

    }
    #endregion End Of DataAndAccessType Enum

    #region DataType Enum
    /// <summary>
    /// DataType Enum
    /// </summary>
    public enum DataType
    {
        //Boolean,
        //Integer,
        //Short,
        //Double,
        //String

       // Array = 0x2000,
        Boolean = 11,
      //  Byte = 0x11,
      //  Char = 0x12,
      //  Currency = 6,
      //  DataObject = 13,
       // Date = 7,
       // Decimal = 14,
        Double = 5,
      //  Empty = 0,
      //  Error = 10,
      Float,
        Integer = 3,
       // Long = 20,
       // Null = 1,
        //Object = 9,
        Short = 2,
       // Single = 4,
        String = 8,
       // UserDefinedType = 0x24,
       // Variant = 12

    }
    #endregion End Of DataType Enum

    #region Blocktype
    public enum BlockTypes
    {
        None,
        Block1,
        Block2
    }
    #endregion BlockType
    #region DataAccess Enum
    /// <summary>
    /// DataType Enum
    /// </summary>
    public enum DataAccess
    {
        Read,
        Write,
        ReadWrite
    }
    #endregion End Of DataAccess Enum

    #region ControlType Enum
    /// <summary>
    ///  ControlType Enum
    /// </summary>
    public enum ControlType
    {
        TextBlock,
        TextBox,
        ComboBox,
    }
    #endregion End Of ControlType Enum

    public enum ReturnStatus
    {
        Success,
        Fail
    }

    public enum ConnectorType
    {
       
        ModbusEthernet = 0,
        ModbusSerial = 1,    
        ABMicroLogixEthernet = 2,
        ABControlLogix = 3,
        ABCompactLogix = 4,
        TcpSocket = 5

    }

    public enum InvalidFloatValues    
    {
        ReplaceWithZero,
        UnModified
    }

    //public enum NetworkAdaptor //TODO: Need to delete --Done
    //{
    //    IPAddress,
    //    NetWorkCard
    //}

    public enum DeviceType
    {
        ModbusEthernet,
        ModbusSerial,
        ABControlLogix,
        ABCompactLogix,
        ABMicroLogixEthernet,
        TcpSocketDevice
    }

    public enum AccessPermissions 
    {
        sdaReadAccess,
        sdaWriteAccess
    }

    public enum IPType { TCP_IP,UDP}
    
    public enum LogStatus
    {
        Information,
        Error,
        Warning
    }

    public enum ImageStatus
    {
        InfoImage, 
        ErrorImage,
        WarningImage
    }


    public enum EnumDataType
    {
        Bool = 1,
        Byte = 2,
        SByte = 3,
        Int16 = 4,
        UInt16 = 5,
        Int32 = 6,
        UInt32 = 7,
        Int64 = 8,
        UInt64 = 9,
        Float32 = 10,
        Float64 = 11,
        String = 12 
    }
    public enum ModbusAddressType
    {
        Coil= 000001,
        DiscreteInput= 100001,
        InputRegister= 300001,
        HoldingRegister= 400001
    }

    public enum Baudrate
    {
        B_9600=9600
    }
    public enum AllenBbadleyModel
    {
        //LGX,
        //SLC,
        //PLC5,
        //micro800
        ControlLogix,
        CompactLogix,
        MicroLogix,        
        Micro800,
        PLC5
    }

    public enum ChannelType
    {
         ADC,
         RPM,
         _2AMS
    }

   
}

#endregion OPCEngine namespace


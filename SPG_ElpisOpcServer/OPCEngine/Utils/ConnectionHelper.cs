#region Namespaces
using Modbus.Device;
using System.Collections.Generic;
using System.IO.Ports;
using System.Net.Sockets;
#endregion Namespaces

#region OPCEngine namespace
namespace Elpis.Windows.OPC.Server
{
    #region ConnectionHelper class
    /// <summary>
    /// ConnectionHelper class will store all connection Configurations such as TCP and Modbus 
    /// </summary>
    public class ConnectionHelper
    {
        #region public members
        public Dictionary<string, TcpClient> tcpClientDictionary { get; set; }
        public Dictionary<string, ModbusIpMaster> ModbusIPMasterCollection { get; set; }
        //Added on 25-Feb-2018 by Harikrishna
        public Dictionary<string, SerialPort>ModbusSerialPortCollection {get;set;}
        public Dictionary<string,ModbusSerialMaster> ModbusSerialMasterCollection { get; set; }

        #endregion public members

        #region constructor
        public ConnectionHelper()
        {
            tcpClientDictionary = new Dictionary<string, TcpClient>();
            ModbusIPMasterCollection = new Dictionary<string, ModbusIpMaster>();
            ModbusSerialMasterCollection = new Dictionary<string, ModbusSerialMaster>();
            ModbusSerialPortCollection = new Dictionary<string, SerialPort>();
        }
        #endregion constructor
    }
    #endregion ConnectionHelper class
}
#endregion OPCEngine namespace
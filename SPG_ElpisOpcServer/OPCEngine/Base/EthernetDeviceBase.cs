using Elpis.Windows.OPC.Server;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;

namespace Elpis.Windows.OPC.Server
{
    /// <summary>
    /// This class is base for Ethernet based device.
    /// </summary>
    [Serializable()]
    public class EthernetDeviceBase : DeviceBase
    {
        #region Constructor
        /// <summary>
        /// EthernetDeviceBase constructor for Serialization
        /// </summary>
        public EthernetDeviceBase() : base()
        {

        }
        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public EthernetDeviceBase(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
        #endregion

       
    }
}

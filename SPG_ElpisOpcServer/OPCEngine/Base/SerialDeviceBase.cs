using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Elpis.Windows.OPC.Server
{
    /// <summary>
    /// Serial Device Properties.
    /// </summary>
    public class SerialDeviceBase: DeviceBase
    {
        #region Constructor
        /// <summary>
        /// Serialization Constructor
        /// </summary>
        public SerialDeviceBase() : base()
        {

        }
        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public SerialDeviceBase(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
        #endregion Constructor

    }
}

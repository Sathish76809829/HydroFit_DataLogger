using Elpis.Windows.OPC.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDI.SLIKDA.Interop;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace OPCEngine.Connectors.Allen_Bradley
{
    [DisplayName("Micrologix Ethernet Connector")]
    [Serializable]
    public class ABMicrologixEthernetConnector : ConnectorBase, IConnector
    {

        #region Constructor
        /// <summary>
        /// Serialization Constructor
        /// </summary>
        public ABMicrologixEthernetConnector() : base()
        {
            // master = new Dictionary<string, ModbusIpMaster>();
        }
        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public ABMicrologixEthernetConnector(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
        #endregion Constructor
        #region Properties 
        string IConnector.Name { get; set; }

        public void Read(ISLIKTag currentItem, DeviceBase device, Tag tagObject, ushort noOfPointToRead = 1)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(ISLIKTag currentItem, DeviceBase device, Tag tagObject, ushort noOfPointToRead = 1)
        {
            throw new NotImplementedException();
        }

        public void Write(ISLIKTag currentItem, object currentValue, DeviceBase device, Tag tagObject, ushort noOfPointToRead = 1)
        {
            throw new NotImplementedException();
        }
        #endregion Properties
    }
}

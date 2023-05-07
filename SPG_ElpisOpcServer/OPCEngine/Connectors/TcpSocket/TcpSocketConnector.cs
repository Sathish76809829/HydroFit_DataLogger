using NDI.SLIKDA.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
#region OPCEngine Namespace
namespace Elpis.Windows.OPC.Server
{
    #region TcpSocketConnetor Class
    /// <summary>
    /// TcpSocket Connector Class.
    /// </summary>
    [DisplayName("TcpSocket Connector")]
    [Serializable]
    public class TcpSocketConnector : ConnectorBase, IConnector
    {
        #region Constructor
        public TcpSocketConnector():base()
        {

        }

        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public TcpSocketConnector(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
        #endregion Constructor
        string IConnector.Name { get; set; }
       // public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
    }
    #endregion TcpSocketConnetor Class
}
#endregion OPCEngine Namespace
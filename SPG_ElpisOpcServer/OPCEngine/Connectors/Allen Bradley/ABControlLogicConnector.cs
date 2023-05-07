#region Usings
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDI.SLIKDA.Interop;
#endregion Usings

namespace Elpis.Windows.OPC.Server
{
    #region ABControlLogic class
    [Serializable, DisplayName("Allen-Bradly Control Logic Protocol")]
    public class ABControlLogicConnector : ConnectorBase, IConnector
    {
        #region Properties 
        string IConnector.Name { get; set; }
        #endregion Properties

        #region Read
        public void Read(ISLIKTag currentItem, DeviceBase device, Tag TagObjesct, ushort noOfAddressToRead = 1)
        {
            throw new NotImplementedException();
        }
        #endregion Read

        #region Subscribe
        public void Subscribe(ISLIKTag currentItem, DeviceBase device, Tag tag, ushort noOfAddressToRead = 1)
        {

        }
        #endregion Subscribe

        #region Write
        public void Write(ISLIKTag currentItem, object currentValue, DeviceBase device, Tag TagObjesct, ushort noOfAddressToRead = 1)
        {
            throw new NotImplementedException();
        }

        #endregion Write

    }
    #endregion ABControlLogic class
}

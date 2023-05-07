#region Namespaces
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDI.SLIKDA.Interop;
using System.Net.Sockets;
using Elpis.Windows.OPC.Server;
#endregion Namespaces

#region OPCEngine namespace
namespace Elpis.Windows.OPC.Server
{
    #region FileHandler class
    [Serializable]
    public class FileHandler
    {
        //[field: Serializable]
        public ObservableCollection<IConnector> AllCollectionFileHandling { get; set; }
        public ObservableCollection<UserAuthenticationViewModel> UserCollectionFileHandling { get; set; }

        //12 01 2017
        public Dictionary<string, TcpClient> tcpClientCollectionFileHandling { get; set; }
        public ObservableCollection<MQTT> MqttCollectionFilHandling { get; set; }
        public ObservableCollection<AzureIoTHub> AzureIoTFileHandling { get; set; }
        public ObservableCollection<LoggerViewModel> LoggerFileHandling { get; set; }

    }
    #endregion FileHandler class
}
#endregion OPCEngine namespace
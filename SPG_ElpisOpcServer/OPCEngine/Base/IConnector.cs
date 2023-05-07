#region Namespaces
using NDI.SLIKDA.Interop;
using OPCEngine;
using System;
#endregion Namespaces

#region OPCEngine namespace
namespace Elpis.Windows.OPC.Server
{    
    public interface IConnector
    {
        //TODO: Add Async Read and Write
        string Name { get; set; } 
        void Read(ISLIKTag currentItem, DeviceBase device,  Tag tagObject, ushort noOfPointToRead=1);
        void Write(ISLIKTag currentItem, object currentValue, DeviceBase device, Tag tagObject, ushort noOfPointToRead = 1);
        void Subscribe(ISLIKTag currentItem, DeviceBase device, Tag tagObject, ushort noOfPointToRead = 1);
        
    }
}

#endregion OPCEngine namespace
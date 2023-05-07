using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElpisOpcServer.SocketService.Net
{
    public enum MessageType :byte
    {
        None=0,
        FromClient=1,
        FromDevice=2,
        ToDevice=4,
        ToClient=8,
        Ping=16,
        DeviceReplay=32,
        DeviceData=44,
        External=225

    }
    public enum DeliveryQuality
    {
        None=0,
        AtLeaseOnce=1,
        ExactlyOnce=2,
        Synchronous=4
    }

}

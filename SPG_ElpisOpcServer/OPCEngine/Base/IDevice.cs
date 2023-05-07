#region Namespaces

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion Namespaces

#region OPCEngine namespace

namespace Elpis.Windows.OPC.Server
{
    public interface IDevice //TODO: --Done Remove IDevice Interface. Move this name property to DeviceBase class
    {
        string Name { get; set; }
    }
}
#endregion OPCEngine namespace
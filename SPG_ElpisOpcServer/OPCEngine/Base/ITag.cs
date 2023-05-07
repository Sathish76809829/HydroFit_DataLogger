using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elpis.Windows.OPC.Server
{
    public interface ITag //TODO: Remove ITag Interface. Move this name property to Tag class
    {
        string Name { get; set; }        
    }
}

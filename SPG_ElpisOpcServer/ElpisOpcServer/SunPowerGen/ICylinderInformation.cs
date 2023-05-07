using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElpisOpcServer.SunPowerGen
{
    internal interface ICylinderInformation : INotifyPropertyChanged
    {
        uint BoreSize { get; set; }
        uint RodSize { get; set; }
        uint StrokeLength { get; set; }
        string CylinderNumber { get; set; }
    }
}

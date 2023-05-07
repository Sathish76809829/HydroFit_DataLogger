using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElpisOpcServer.SunPowerGen
{
    public class Slip_StickTestInformation :CeritificateInformation
    {
        private ushort pressure;
        private ushort flow;

        public ushort Pressure
        {
            get { return pressure; }
            set
            {
                pressure = value;
                OnPropertyChanged("Pressure");
            }
        }
        public ushort Flow
        {
            get { return flow; }
            set
            {
                flow = value;
                OnPropertyChanged("Flow");
            }

        }
    }
}

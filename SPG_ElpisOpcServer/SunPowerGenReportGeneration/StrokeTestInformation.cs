using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElpisOpcServer.SunPowerGen
{
    public class StrokeTestInformation : CeritificateInformation
    {
        private ushort pressure;
        private ushort flow;
        private ushort noOfCycles;
        private ushort maximumAllowableTestPressure;

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
        public ushort NoofCycles
        {
            get { return noOfCycles; }
            set
            {
                noOfCycles = value;
                OnPropertyChanged("NoofCycles");
            }
        }

        public ushort MaximumAllowableTestPressure
        {
            get { return maximumAllowableTestPressure; }
            set
            {
                maximumAllowableTestPressure = value;
                OnPropertyChanged("MaximumAllowableTestPressure");
            }
        }

    }
}

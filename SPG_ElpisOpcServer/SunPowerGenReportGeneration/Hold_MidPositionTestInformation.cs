using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElpisOpcServer.SunPowerGen
{
    public class Hold_MidPositionTestInformation :CeritificateInformation
    {

        private ushort holdingTimeLineA;
        private ushort holdingTimeLineB;
        private ushort holdingTimeLineA_B;
        private ushort allowablePressureDrop;

        public ushort HoldingTimeLineA
        {
            get { return holdingTimeLineA; }
            set
            {
                holdingTimeLineA = value;
                OnPropertyChanged("Holding Time Line A");
            }
        }
        public ushort HoldingTimeLineB
        {
            get { return holdingTimeLineB; }
            set
            {
                holdingTimeLineB = value;
                OnPropertyChanged("HoldingTimeLineB");
            }

        }
        public ushort HoldingTimeLineA_B
        {
            get { return holdingTimeLineA_B; }
            set
            {
                holdingTimeLineA_B = value;
                OnPropertyChanged("HoldingTimeLineA_B ");
            }
        }

        public ushort AllowablePressureDrop
        {
            get { return allowablePressureDrop; }
            set
            {
                allowablePressureDrop = value;
                OnPropertyChanged("AllowablePressureDrop");
            }
        }
        



    }
}

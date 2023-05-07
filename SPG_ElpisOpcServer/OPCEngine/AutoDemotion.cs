using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Elpis.Windows.OPC.Server
{
    public class AutoDemotion
    {
        public Timer demotionPeriod { get; set; }
        public int demotionRetryCount { get; set; }

        public AutoDemotion()
        {
            demotionPeriod = new Timer();
            demotionPeriod.Interval = 30000;
            demotionRetryCount = 0;
        }
    }
}

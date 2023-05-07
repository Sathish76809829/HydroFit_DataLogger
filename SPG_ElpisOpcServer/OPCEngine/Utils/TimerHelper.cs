#region Namespaces

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Net.Sockets;

#endregion Namespaces

#region OPCEngine namespace
namespace OPCEngine
{

    #region TimerHelper Class
    public static class TimerHelper
    {   
        #region Properties

        private static DispatcherTimer twohrDemo;
        public static DispatcherTimer TwohrDemo
        {
            get
            {
                if (twohrDemo == null)
                {
                    twohrDemo = new DispatcherTimer();
                }
                return twohrDemo;
            }
        }

        private static DispatcherTimer timer;
        public static DispatcherTimer Timer
        {
            get
            {
                if (timer == null)
                {
                    timer = new DispatcherTimer();
                }
                return timer;
            }
        }

        #endregion Properties
    }
    #endregion TimerHelper Class

}
#endregion OPCEngine namespace
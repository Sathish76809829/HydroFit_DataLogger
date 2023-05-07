using System;
using System.Windows.Threading;

namespace Elpis.Windows.OPC.Server
{
    public class DemoTimer
    {
        public DemoTimer()
        {
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            ElpisServer.isDemoExpired = false;
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 30, 0); //Set Demo Time //sets How much time server run in demo version.
            dispatcherTimer.Start();  
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            ElpisServer.isDemoExpired = true;
            ((System.Windows.Threading.DispatcherTimer)sender).IsEnabled = false;

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Elpis.Windows.OPC.Server
{
    public class ThreadHelper
    {
        public Dictionary<string, Thread> ThreadCollection { get; set; }

        public ThreadHelper()
        {
            ThreadCollection = new Dictionary<string, Thread>();
        }
    }
}

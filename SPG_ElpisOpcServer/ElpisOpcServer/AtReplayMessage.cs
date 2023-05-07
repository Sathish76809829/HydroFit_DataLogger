using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElpisOpcServer
{
    public class AtReplayMessage
    {
        public static readonly int[] AtHeaderreplay = new int[] { 65, 84, 43 };
        public static bool ValidateHeader(byte[] header)
        {
            for (int i = 1; i < 3; i++)
            {
                if (AtHeaderreplay[i] != header[i])
                    return false;
            }
            return true;
        }
    }
}

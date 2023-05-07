using System;
using System.Net;

namespace RMS.SocketServer.Models
{
    /// <summary>
    /// Tcp end point information
    /// </summary>
    public class TcpEndPointModel
    { /// <summary>
        /// Listen Address
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Listen Port
        /// </summary>
        public int Port { get; set; } = 1883;

        /// <summary>
        /// Read IPv4
        /// </summary>
        /// <returns></returns>
        public bool TryReadAddress(out IPAddress address)
        {
            if (Address == "*")
            {
                address = IPAddress.Any;
                return true;
            }

            if (Address == "localhost")
            {
                address = IPAddress.Loopback;
                return true;
            }

            if (Address == "disable")
            {
                address = IPAddress.None;
                return true;
            }

            if (IPAddress.TryParse(Address, out var ip))
            {
                address = ip;
                return true;
            }

            throw new Exception($"Could not parse IP address: {Address}");
        }
    }
}

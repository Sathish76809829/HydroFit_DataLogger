using System.Collections.Generic;

namespace RMS.SocketServer.Configurations
{
    /// <summary>
    /// Web socket settings model used for mapping Websocket endPoint
    /// </summary>
    public class WebSocketEndPointConfiguration
    {
        public string Path { get; set; } = "/rms";

        public int ReceiveBufferSize { get; set; } = 1024;

        public int KeepAliveInterval { get; set; } = 120;

        public List<string> AllowedOrigins { get; set; }
    }
}

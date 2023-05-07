namespace RMS.SocketServer.Configurations
{
    /// <summary>
    /// Socket configuration for RMS Socket Application
    /// </summary>
    public class SocketConfiguration
    {
        /// <summary>
        /// Web socket options for RMS Socket
        /// </summary>
        public WebSocketEndPointConfiguration WebSocketEndPoint { get; set; }

        /// <summary>
        /// Web socket options for RMS Socket
        /// </summary>
        public TcpEndPointConfiguration TcpEndPoint { get; set; }
    }
}

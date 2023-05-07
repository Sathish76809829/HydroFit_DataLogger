using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RMS.SocketServer.Extensions;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RMS.SocketServer.Controllers
{
    /// <summary>
    /// RMS Socket Controller API
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/[controller]")]
    public class SocketController : ControllerBase
    {
        private readonly ILogger<SocketController> _logger;

        private readonly Net.SocketServer _server;

        public SocketController(ILogger<SocketController> logger, Net.SocketServer server)
        {
            _logger = logger;
            _server = server;

        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Get()
        {
            _logger.LogInformation("v1/ Get() info");
            return Ok("v1.0.0");
        }


        /// <summary>
        /// Get the number of clients connected to socket
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("clients")]
        public ActionResult<IList<Models.ClientInfo>> GetClients()
        {
            return Ok(_server.GetClients());
        }

        /// <summary>
        /// Disconnect client from the server
        /// </summary>
        /// <param name="clientId">Client Id for socket</param>
        /// <returns>Return 200 if success.</returns>
        [HttpDelete]
        [Route("stop")]
        public IActionResult Disconnect(string clientId)
        {
            if (_server.Disconnect(clientId))
            {
                return Ok();
            }
            return StatusCode(StatusCodes.Status404NotFound);
        }

        /// <summary>
        /// Tunnel data from socket server to different client. Use for experimental purpose onlt
        /// </summary>
        /// <param name="tcpEndPoint">Client end point to tunnel</param>
        /// <param name="factory">Log factory from ASP</param>
        /// <returns>200 - If successfully connected to client</returns>
        [HttpPost]
        [Route("tunnel")]
        public async Task<IActionResult> TunnelToTcp([FromBody] Models.TcpEndPointModel tcpEndPoint, [FromServices]ILoggerFactory factory)
        {
            if (!tcpEndPoint.TryReadAddress(out IPAddress address4)
                   || !(tcpEndPoint.Port > 0))
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid IP address or port");
            }

            var endPoint = new IPEndPoint(address4, tcpEndPoint.Port);

            // avoid circular dependency
            if (endPoint.Equals(_server.LocalEndPoint))
                return StatusCode(StatusCodes.Status400BadRequest, "Circular dependency tunnel");
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            if (await socket.ConnectAsync(endPoint, 20000))
            {
                return Ok(_server.AddSocketClient(new Net.TcpSocketTunnel(socket, factory.CreateLogger("Tcp.SocketTunnel"))));
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Unable to connect to " + endPoint);
        }
    }
}

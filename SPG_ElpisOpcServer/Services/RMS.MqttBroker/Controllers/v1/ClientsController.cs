using Microsoft.AspNetCore.Mvc;
using MQTTnet.Server.Status;
using RMS.Broker.Mqtt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RMS.Broker.Controllers
{
    /// <summary>
    /// Client API controller for Broker
    /// </summary>
    [ApiController]
    public class ClientsController : Controller
    {
        private readonly MqttServerService _mqttServerService;

        public ClientsController(MqttServerService mqttServerService)
        {
            _mqttServerService = mqttServerService ?? throw new ArgumentNullException(nameof(mqttServerService));
        }

        [Route("api/v1/clients")]
        [HttpGet]
        public async Task<ActionResult<IList<IMqttClientStatus>>> GetClients()
        {
            return new ObjectResult(await _mqttServerService.GetClientStatusAsync());
        }

        [Route("api/v1/config-user")]
        [HttpPost]
        public IActionResult ConfigUser([FromBody]Models.User user)
        {
            if(_mqttServerService.TryGetUserSession(user.UserAcountId, out var session))
            {
                session.Update(user);
                var topics = session.Subscribers.Values.SelectMany(topic => topic).ToHashSet();
                foreach (var topic in topics)
                {
                    _mqttServerService.ClearTopic(topic, session);
                    _mqttServerService.UpdateTopic(topic, session);

                }
            }
            return Ok();
        }
    }
}

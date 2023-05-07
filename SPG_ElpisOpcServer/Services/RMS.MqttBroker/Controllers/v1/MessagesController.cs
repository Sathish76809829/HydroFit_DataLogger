using Microsoft.AspNetCore.Mvc;
using MQTTnet;
using RMS.Broker.Mqtt;
using RMS.Broker.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace RMS.Broker.Controllers
{
    /// <summary>
    /// Messages API Controller for Broker
    /// </summary>
    [ApiController]
    public class MessagesController : Controller
    {
        private readonly MqttServerService _mqttServerService;

        public MessagesController(MqttServerService mqttServerService)
        {
            _mqttServerService = mqttServerService ?? throw new ArgumentNullException(nameof(mqttServerService));
        }

        [Route("api/v1/messages")]
        [HttpPost]
        public async Task<ActionResult> PostMessage(MqttApplicationMessage message)
        {
            await _mqttServerService.PublishAsync(message);
            return Ok();
        }

        /// <summary>
        /// Publish mqtt messages to mqtt user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userSignals"></param>
        /// <returns></returns>
        [Route("api/v1/message/publish")]
        [HttpPost]
        public async Task<ActionResult> PublishMessage([FromQuery] long userId, [FromBody] Models.UserSignalPublishModel[] userSignals)
        {
            if (_mqttServerService.TryGetUserSession(userId, out var session))
            {
                var topics = session.Topics;
                var userPreference = session.UserPreference
                    .Select(s => s.SignalModel.SignalId).ToArray() ?? Array.Empty</*int*/string>();
                var groupPrefrence = session.GroupUserPreferences
                    .Select(s => s.SignalModel.SignalId).ToArray() ?? Array.Empty</*int*/string>();
                var signalLookup = userSignals.ToLookup(s => s.DeviceId);
                var topicMessages = new List<Models.UserSignalPublishModel>();
                using var writer = new JsonBufferWriter();
                foreach (var topic in topics)
                {
                    topicMessages.Clear();
                    if ((topic.Filter & TopicFilterType.Individual) == TopicFilterType.Individual)
                    {
                        foreach (var signal in userSignals)
                        {
                            if (Array.Exists(userPreference, s => s == signal.SignalId))
                            {
                                topicMessages.Add(signal);
                            }
                        }
                    }
                    else if ((topic.Filter & TopicFilterType.Group) == TopicFilterType.Group)
                    {
                        foreach (var signal in userSignals)
                        {
                            if (Array.Exists(groupPrefrence, s => s == signal.SignalId))
                            {
                                topicMessages.Add(signal);
                            }
                        }
                    }
                    if (topicMessages.Count > 0)
                    {
                        await _mqttServerService.PublishAsync(new MqttApplicationMessage
                        {
                            Topic = topic.Value,
                            Payload = await writer.GetBytesAsync(topicMessages)
                        });
                    };
                }
            }
            return Ok();
        }
    }
}

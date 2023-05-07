using MQTTnet;
using MQTTnet.Server;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RMS.Broker.Mqtt
{
    /// <summary>
    /// Retain messages for mqtt broker
    /// </summary>
    public class MqttServerStorage : IMqttServerStorage
    {
        private readonly List<MqttApplicationMessage> messages;

        public MqttServerStorage()
        {
            messages = new List<MqttApplicationMessage>();
        }

        public Task<IList<MqttApplicationMessage>> LoadRetainedMessagesAsync()
        {
            return Task.FromResult<IList<MqttApplicationMessage>>(messages.ToArray());
        }

        public Task SaveRetainedMessagesAsync(IList<MqttApplicationMessage> messages)
        {
            foreach (var message in messages)
            {
                Add(message);
            }
            // Todo retain messages to file
            return Task.CompletedTask;
        }

        internal void Add(MqttApplicationMessage message)
        {
            messages.Add(message);
        }
    }
}

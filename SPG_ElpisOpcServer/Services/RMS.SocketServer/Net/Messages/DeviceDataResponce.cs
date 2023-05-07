using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RMS.SocketServer.Net.Messages
{
    public class DeviceDataResponce : IMessageResponse
    {
        private readonly string _clientId;

        private readonly IUserMessage _message;

        public DeviceDataResponce(string clientId, IUserMessage message)
        {
            _clientId = clientId;
            _message = message;
        }

        public MessageType RequestType => _message.Type;

        public MessageType ResponseType => MessageType.DeviceData;

        public string ClientId => _clientId;

        public IUserMessage Message => _message;

        public void SetResult(bool status)
        {
        }
    }
}

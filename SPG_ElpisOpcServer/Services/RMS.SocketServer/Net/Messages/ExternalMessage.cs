using System;

namespace RMS.SocketServer.Net.Messages
{
    /// <summary>
    /// Extenal message that can used for tcp tunnel
    /// </summary>
    public readonly struct RedirectMessage : IMessage
    {
        public readonly RawMessagePacket Original;

        public RedirectMessage(RawMessagePacket original)
        {
            Original = original;
        }

        internal static IMessageResponse GetResponse(ClientMessage message)
        {
            if (message.Value is RedirectMessage m)
                return new MessageResponse(m.Original, m.Original.Type, message.ClientId);
            if (message.Value is RawMessagePacket r)
            {
                return new MessageResponse(r, r.Type, message.ClientId);
            }
            return default(MessageResponse);

        }

        public IUserMessage GetMessage()
        {
            return new UserMessage
            {
                Type = MessageType.External,
                Value = Original.Value,
                Quality = Original.Quality,
                Retain = Original.Retain
            };
        }
    }
}

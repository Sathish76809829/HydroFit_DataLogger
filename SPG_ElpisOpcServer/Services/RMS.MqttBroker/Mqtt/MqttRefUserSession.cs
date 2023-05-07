namespace RMS.Broker.Mqtt
{
    /// <summary>
    /// Mqtt Reference User such as admin
    /// </summary>
    public class MqttRefUserSession : MqttUserSession
    {
        public MqttRefUserSession(MqttUserSession underlyingSession) : base(underlyingSession)
        {
            UnderlyingSession = underlyingSession;
        }

        internal bool TryRemove(string clientId)
        {
            return Subscribers.TryRemove(clientId, out _);
        }

        public MqttRefUserSession() { }

        public MqttUserSession UnderlyingSession { get; }
    }
}

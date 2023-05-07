using MQTTnet.Server;

namespace RMS.Broker.Extensions
{
    /// <summary>
    /// Mqtt Extensions
    /// </summary>
    public static class MqttExtensions
    {
        public static object AdminRoleId = 1;

        public static bool IsAdmin(this MqttSubscriptionInterceptorContext self)
        {
            if(self.SessionItems.TryGetValue(Mqtt.MqttServerService.RoleIdKey, out var roldeId))
            {
                return roldeId.Equals(AdminRoleId);
            }
            return false;
        }
    }
}

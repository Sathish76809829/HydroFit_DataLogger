using System;

namespace RMS.Broker.Mqtt
{
    /// <summary>
    /// Subscription topic information
    /// ex: rms/user/5
    /// <list type="bullet">
    /// <item>UserId: 5</item>
    /// <item>Filter: Individual</item>
    /// </list>
    /// </summary>
    public readonly struct MqttTopicSubscription
    {
        public readonly string Value;
        public readonly TopicFilterType Filter;

        private MqttTopicSubscription(string value, TopicFilterType filter)
        {
            Value = value;
            Filter = filter;
        }

        /// <summary>
        /// Hash code override for Dictionary or HashSet
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(Value, Filter);
        }

        public override bool Equals(object obj)
        {
            return obj is MqttTopicSubscription subscriber &&
                   Value == subscriber.Value &&
                   Filter == subscriber.Filter;
        }

        /// <summary>
        /// User Id for Topic
        /// </summary>
        public long UserId
        {
            get
            {
                if (Filter != TopicFilterType.None)
                {
                    var index = Value.IndexOf("user/");
                    string remain = Value[(index + 5)..];
                    if (index > -1 && long.TryParse(remain, out var userId))
                    {
                        return userId;
                    }
                }
                return 0;
            }
        }

        /// <summary>
        /// Parse topic endpoint
        /// </summary>
        public static bool TryParse(string topic, out MqttTopicSubscription value)
        {
            TopicFilterType filter;
            if (topic.StartsWith("rms/user"))
            {
                filter = TopicFilterType.Individual;
            }
            else if (topic.StartsWith("rms/group"))
            {
                filter = TopicFilterType.Group;
            }
            else if (topic.StartsWith("rms/admin/"))
            {
                var remain = topic[10..];
                filter = TopicFilterType.Admin;
                if (remain.StartsWith("user/"))
                {
                    filter |= TopicFilterType.Individual;
                }
                else if(remain.StartsWith("group/"))
                {
                    filter |= TopicFilterType.Group;
                }
            }
            else
            {
                value = default;
                return false;
            }
            value = new MqttTopicSubscription(topic, filter);
            return true;
        }
    }
}

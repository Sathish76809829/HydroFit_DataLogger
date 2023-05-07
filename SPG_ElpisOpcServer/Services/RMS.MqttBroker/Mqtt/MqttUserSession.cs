using RMS.Broker.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace RMS.Broker.Mqtt
{
    public enum TopicFilterType
    {
        None = 0,
        Individual = 1,
        Group = 2,
        Admin = 4,

    }

    /// <summary>
    /// User sessions information such as User Preference &amp; Group Preference
    /// </summary>
    public class MqttUserSession
    {
        static readonly IReadOnlyDictionary</*int*/string, IList</*int*/string>> EmptyPreference = new Dictionary</*int*/string, IList</*int*/string>>();

        public MqttUserSession()
        {
            Subscribers = new ConcurrentDictionary<string, HashSet<MqttTopicSubscription>>();
            UserPreference = Array.Empty<UserPreference>();
            GroupUserPreferences = Array.Empty<GroupUserPreference>();
        }

        public MqttUserSession(MqttUserSession other)
        {
            Subscribers = new ConcurrentDictionary<string, HashSet<MqttTopicSubscription>>(other.Subscribers);
            UserPreference = other.UserPreference;
            GroupUserPreferences = other.GroupUserPreferences;
        }

        public int Count => Subscribers.Count;

        public UserPreference[] UserPreference { get; set; }

        public GroupUserPreference[] GroupUserPreferences { get; set; }

        // mqtt user -> topics
        public ConcurrentDictionary<string, HashSet<MqttTopicSubscription>> Subscribers { get; }

        public IReadOnlyCollection<MqttTopicSubscription> Topics
        {
            get
            {
                return Subscribers.Values.SelectMany(t => t).ToHashSet();
            }
        }

        internal bool TryAdd(string clientId)
        {
            return Subscribers.TryAdd(clientId, new HashSet<MqttTopicSubscription>());
        }

        internal bool TryAdd(string clientId, MqttTopicSubscription topic)
        {
            var subscribes = Subscribers;
            if (subscribes.TryGetValue(clientId, out var topics) == false)
            {
                topics = new HashSet<MqttTopicSubscription>();
                subscribes.TryAdd(clientId, topics);
            }
            return topics.Add(topic);
        }

        static bool TryParseGroupId(string topic, out int groupId)
        {
            // ex: rms/group/3/user/5
            var index = topic.IndexOf("group/");
            if (index > -1)
            {
                string remain = topic[(index + 6)..];
                // 3/user/5
                int i = remain.IndexOf('/');
                if (i > 0)
                {
                    if (int.TryParse(remain.Substring(0, i), out groupId))
                    {
                        return true;
                    }
                }
                return int.TryParse(remain, out groupId);
            }
            groupId = -1;
            return false;
        }

        internal bool TryRemove(string clientId, MqttTopicSubscription topic)
        {
            if (Subscribers.TryGetValue(clientId, out var topics))
            {
                return topics.Remove(topic);
            }

            return false;
        }

        static IReadOnlyDictionary</*int*/string, IList</*int*/string>> GetPreference(UserPreference[] userPreference)
        {
            Dictionary</*int*/string, IList</*int*/string>> result = new Dictionary</*int*/string, IList</*int*/string>>();
            foreach (var pref in userPreference
                .GroupBy(p => p.SignalModel.DeviceId, p => p.SignalModel.SignalId))
            {
                result[pref.Key] = pref.ToArray();
            }
            return result;
        }

        IReadOnlyDictionary</*int*/string, IList</*int*/string>> MapPreference(string clientId, MqttTopicSubscription subscription, UserPreference[] preference)
        {
            if (preference.Length == 0)
                return null;
            Dictionary</*int*/string, IList</*int*/string>> result = new Dictionary</*int*/string, IList</*int*/string>>();
            var subscribers = Subscribers;
            if (subscribers.TryGetValue(clientId, out var topics) == false)
            {
                topics = new HashSet<MqttTopicSubscription>();
                subscribers[clientId] = topics;
            }
            topics.Add(subscription);
            foreach (var pref in preference
            .GroupBy(p => p.SignalModel.DeviceId, p => p.SignalModel.SignalId))
            {
                result[pref.Key] = pref.ToArray();
            }

            return result;
        }

        internal bool HasFilter(MqttTopicSubscription subscription)
        {
            return Subscribers.Values.Any(topics =>
            {
                return topics.Any(t => t.Equals(subscription));
            });
        }

        internal bool HasTopics(string clientId)
        {
            if (Subscribers.TryGetValue(clientId, out var topics))
            {
                return topics.Count > 0;
            }
            return false;
        }

        internal bool TryRemove(string clientId, out IReadOnlyCollection<MqttTopicSubscription> value)
        {
            if (Subscribers.TryRemove(clientId, out var topics))
            {
                value = topics;
                return true;
            }
            value = null;
            return false;
        }

        internal IReadOnlyDictionary</*int*/string, IList</*int*/string>> PreferenceFor(MqttTopicSubscription topic)
        {
            if ((topic.Filter & TopicFilterType.Individual) == TopicFilterType.Individual)
            {
                return GetPreference(UserPreference);
            }
            else if ((topic.Filter & TopicFilterType.Group) == TopicFilterType.Group && TryParseGroupId(topic.Value, out int groupId))
            {
                return GetPreference(GroupUserPreferences
                    .Where(p => p.SignalGroupModel.SignalGroupId == groupId).ToArray());
            }
            return EmptyPreference;
        }

        internal void Update(User user)
        {
            if (user.UserPreference != null)
            {
                UserPreference = JsonSerializer.Deserialize<UserPreference[]>(user.UserPreference);
            }
            if (user.GroupUserPreference != null)
            {
                GroupUserPreferences = JsonSerializer.Deserialize<GroupUserPreference[]>(user.GroupUserPreference);
            }
        }
    }
}

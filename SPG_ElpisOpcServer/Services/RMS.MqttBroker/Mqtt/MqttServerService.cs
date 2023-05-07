using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Adapter;
using MQTTnet.Client.Publishing;
using MQTTnet.Implementations;
using MQTTnet.Server;
using MQTTnet.Server.Status;
using RMS.Broker.Configuration;
using RMS.Broker.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace RMS.Broker.Mqtt
{
    /// <summary>
    /// RMS Mqtt Client Info 
    /// </summary>
    public readonly struct ClientInfo
    {
        public readonly string ClientId;
        public readonly long UserId;

        public ClientInfo(string clientId, long userId)
        {
            ClientId = clientId;
            UserId = userId;
        }

        public override bool Equals(object obj)
        {
            return obj is ClientInfo info &&
                   ClientId == info.ClientId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ClientId);
        }
    }

    /// <summary>
    /// A Mqtt Service class used for broker
    /// </summary>
    public class MqttServerService : IMqttServerConnectionValidator,
        IMqttServerUnsubscriptionInterceptor,
        IMqttServerSubscriptionInterceptor,
        IMqttServerClientDisconnectedHandler
    {
        /// <summary>
        /// User Id Key for Mqtt Session
        /// </summary>
        internal static readonly object UserIdKey = "userId";

        /// <summary>
        /// Role Id Key for Mqtt Session
        /// </summary>
        internal static readonly object RoleIdKey = "roleId";

        private readonly MqttFactory factory;

        internal readonly MqttSettingsModel Settings;

        private readonly IMqttServer _mqttServer;

        private readonly MqttWebSocketServerAdapter _socketServerAdaptor;

        private readonly Services.ClientService _userService;

        private readonly ILogger<MqttServerService> _logger;

        private readonly HashSet<ClientInfo> clients;

        private readonly MqttServerStorage storage;

        // deviceId -> signals[] -> [topics]
        private readonly ConcurrentDictionary</*int*/string, IDictionary</*int*/string, HashSet<MqttTopicSubscription>>> deviceSubscribes;

        private readonly ConcurrentDictionary<long, MqttUserSession> connectedUsers;

        public MqttServerService(MqttSettingsModel mqttSettings, Services.ClientService authService, ILogger<MqttServerService> logger)
        {
            Settings = mqttSettings;
            _logger = logger;
            factory = new MqttFactory();
            _socketServerAdaptor = new MqttWebSocketServerAdapter(factory.DefaultLogger);
            var adapters = new List<IMqttServerAdapter>
            {
                new MqttTcpServerAdapter(factory.DefaultLogger)
                {
                    TreatSocketOpeningErrorAsWarning = true // Opening other ports than for HTTP is not allows in Azure App Services.
                },
                _socketServerAdaptor
            };
            _mqttServer = factory.CreateMqttServer(adapters);
            _userService = authService;
            storage = new MqttServerStorage();
            deviceSubscribes = new ConcurrentDictionary</*int*/string, IDictionary</*int*/string, HashSet<MqttTopicSubscription>>>();
            connectedUsers = new ConcurrentDictionary<long, MqttUserSession>();
            clients = new HashSet<ClientInfo>();
        }

        public Task<IList<IMqttClientStatus>> GetClientStatusAsync()
        {
            return _mqttServer.GetClientStatusAsync();
        }

        public async Task ValidateConnectionAsync(MqttConnectionValidatorContext context)
        {
            var details = await _userService.AuthAsync(context.Username, context.Password);
            if (details != null && details.User is Models.User user)
            {
                clients.Add(new ClientInfo(context.ClientId, user.UserAcountId));
                AddUserPreference(context.ClientId, user);
                context.SessionItems[UserIdKey] = user.UserAcountId;
                context.SessionItems[RoleIdKey] = user.UserRoleId;
                context.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.Success;
            }
            else
            {
                context.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.NotAuthorized;
            }
        }

        void AddUserPreference(string clientId, Models.User user)
        {
            if (connectedUsers.TryGetValue(user.UserAcountId, out var session) == false)
            {
                session = new MqttUserSession();
                connectedUsers[user.UserAcountId] = session;
            }
            session.TryAdd(clientId);
            session.Update(user);
        }

        public bool TryGetUserSession(long userAccountId, out MqttUserSession session)
        {
            return connectedUsers.TryGetValue(userAccountId, out session);
        }

        public bool TryClientInfo(string clientId, out ClientInfo info)
        {
            return clients.TryGetValue(new ClientInfo(clientId, -1), out info);
        }

        internal IDictionary</*int*/string, HashSet<MqttTopicSubscription>> ForDeviceKey(/*int*/string key)
        {
            if (deviceSubscribes.TryGetValue(key, out var signals))
            {
                return signals;
            }
            return null;
        }

        public async void Configure()
        {
            _mqttServer.ClientDisconnectedHandler = this;
            await _mqttServer.StartAsync(CreateMqttServerOptions()).ConfigureAwait(false);

            _logger.LogInformation("MQTT server started.");
        }

        public Task RunWebSocketConnectionAsync(WebSocket webSocket, HttpContext httpContext)
        {
            return _socketServerAdaptor.RunWebSocketConnectionAsync(webSocket, httpContext);
        }

        public Task<MqttClientPublishResult> PublishAsync(MqttApplicationMessage applicationMessage)
        {
            if (applicationMessage == null)
                throw new ArgumentNullException(nameof(applicationMessage));
            if (_mqttServer.IsStarted)
                return _mqttServer.PublishAsync(applicationMessage, CancellationToken.None);
            storage.Add(applicationMessage);
            return Task.FromResult(new MqttClientPublishResult
            {
                ReasonCode = MqttClientPublishReasonCode.UnspecifiedError
            });
        }

        public async Task PublishAsync(IList<MqttApplicationMessage> applicationMessages)
        {
            if (applicationMessages == null)
                throw new ArgumentNullException(nameof(applicationMessages));
            if (_mqttServer.IsStarted)
            {
                foreach (var message in applicationMessages)
                {
                    await _mqttServer.PublishAsync(message, CancellationToken.None);
                }
                return;
            }
            await storage.SaveRetainedMessagesAsync(applicationMessages);
        }

        #region Options
        IMqttServerOptions CreateMqttServerOptions()
        {
            var options = new MqttServerOptionsBuilder()
                .WithMaxPendingMessagesPerClient(Settings.MaxPendingMessagesPerClient)
                .WithDefaultCommunicationTimeout(TimeSpan.FromSeconds(Settings.CommunicationTimeout))
                .WithConnectionValidator(this)
                .WithStorage(storage)
                .WithSubscriptionInterceptor(this)
                .WithUnsubscriptionInterceptor(this);

            // Configure unencrypted connections
            if (Settings.TcpEndPoint.Enabled)
            {
                options.WithDefaultEndpoint();

                if (Settings.TcpEndPoint.TryReadIPv4(out var address4))
                {
                    options.WithDefaultEndpointBoundIPAddress(address4);
                }

                if (Settings.TcpEndPoint.TryReadIPv6(out var address6))
                {
                    options.WithDefaultEndpointBoundIPV6Address(address6);
                }

                if (Settings.TcpEndPoint.Port > 0)
                {
                    options.WithDefaultEndpointPort(Settings.TcpEndPoint.Port);
                }
            }
            else
            {
                options.WithoutDefaultEndpoint();
            }

            // Configure encrypted connections
            if (Settings.EncryptedTcpEndPoint.Enabled)
            {
#if NETCOREAPP3_1 || NET5_0
                options
                    .WithEncryptedEndpoint()
                    .WithEncryptionSslProtocol(SslProtocols.Tls13);
#else
                options
                    .WithEncryptedEndpoint()
                    .WithEncryptionSslProtocol(SslProtocols.Tls12);
#endif

                if (!string.IsNullOrEmpty(Settings.EncryptedTcpEndPoint?.Certificate?.Path))
                {
                    IMqttServerCertificateCredentials certificateCredentials = null;

                    if (!string.IsNullOrEmpty(Settings.EncryptedTcpEndPoint?.Certificate?.Password))
                    {
                        certificateCredentials = new MqttServerCertificateCredentials
                        {
                            Password = Settings.EncryptedTcpEndPoint.Certificate.Password
                        };
                    }

                    options.WithEncryptionCertificate(Settings.EncryptedTcpEndPoint.Certificate.ReadCertificate(), certificateCredentials);
                }

                if (Settings.EncryptedTcpEndPoint.TryReadIPv4(out var address4))
                {
                    options.WithEncryptedEndpointBoundIPAddress(address4);
                }

                if (Settings.EncryptedTcpEndPoint.TryReadIPv6(out var address6))
                {
                    options.WithEncryptedEndpointBoundIPV6Address(address6);
                }

                if (Settings.EncryptedTcpEndPoint.Port > 0)
                {
                    options.WithEncryptedEndpointPort(Settings.EncryptedTcpEndPoint.Port);
                }
            }
            else
            {
                options.WithoutEncryptedEndpoint();
            }

            if (Settings.ConnectionBacklog > 0)
            {
                options.WithConnectionBacklog(Settings.ConnectionBacklog);
            }

            if (Settings.EnablePersistentSessions)
            {
                options.WithPersistentSessions();
            }

            return options.Build();
        }
        #endregion

        #region Unsubscribe
        public Task InterceptUnsubscriptionAsync(MqttUnsubscriptionInterceptorContext context)
        {
            long userId = (long)context.SessionItems[UserIdKey];
            if (MqttTopicSubscription.TryParse(context.Topic, out var topic)
                && connectedUsers.TryGetValue(userId, out var session)
                && session.TryRemove(context.ClientId, topic))
            {
                // if unsubscribe from admin topic
                if ((topic.Filter & TopicFilterType.Admin) == TopicFilterType.Admin)
                {
                    ClearAdminTopic(context.ClientId, topic);
                    return Task.CompletedTask;
                }
                // if any user not using topic
                if (session.HasFilter(topic) == false)
                {
                    ClearTopic(topic, session);
                }
            }
            return Task.CompletedTask;
        }

        void ClearAdminTopic(string clientId, MqttTopicSubscription topic)
        {
            long userId = topic.UserId;
            if (userId == 0)
                return;
            if (connectedUsers.TryGetValue(userId, out var session)
                && session.TryRemove(clientId, topic)
                && session is MqttRefUserSession mqttRefUser
                && session.HasTopics(clientId) == false
                && mqttRefUser.TryRemove(clientId)
                && mqttRefUser.Subscribers.Count == 0)
            {
                if (mqttRefUser.UnderlyingSession is object)
                {
                    // no admin session and put user session back
                    connectedUsers[userId] = new MqttUserSession(session);
                    return;
                }
                // not an user session
                if (mqttRefUser.UnderlyingSession is null)
                    connectedUsers.TryRemove(userId, out _);
            }
        }

        internal void ClearTopic(MqttTopicSubscription topic, MqttUserSession session)
        {
            var deviceUsers = deviceSubscribes;
            foreach (var item in session.PreferenceFor(topic))
            {
                if (deviceUsers.TryGetValue(item.Key, out var signals))
                {
                    foreach (var signal in item.Value)
                    {
                        // if no of user in the topic is zero
                        signals[signal].Remove(topic);
                    }
                }
            }
        }
        #endregion

        public async Task InterceptSubscriptionAsync(MqttSubscriptionInterceptorContext context)
        {
            long userId = (long)context.SessionItems[UserIdKey];
            if (MqttTopicSubscription.TryParse(context.TopicFilter.Topic, out var topic)
                && connectedUsers.TryGetValue(userId, out var session)
                && session.TryAdd(context.ClientId, topic))
            {
                if ((topic.Filter & TopicFilterType.Admin) == TopicFilterType.Admin)
                {
                    context.AcceptSubscription = context.IsAdmin() && await SubscribeToUserTopic(context.ClientId, topic);
                    return;
                }

                UpdateTopic(topic, session);
            }
        }

        /// <summary>
        /// Make a user session for admin
        /// </summary>
        /// <returns></returns>
        async Task<bool> SubscribeToUserTopic(string clientId, MqttTopicSubscription topic)
        {
            // if user session exist
            long userId = topic.UserId;
            if (userId == 0)
                return false;
            if (connectedUsers.TryGetValue(userId, out var session) == false)
            {
                var preference = await _userService.GetPreferenceAsync(userId);
                if (preference == null)
                    return false;
                session = new MqttRefUserSession();
                connectedUsers[userId] = session;
                session.Update(preference);
            }
            // add admin as client to user
            session.TryAdd(clientId, topic);
            UpdateTopic(topic, session);
            return true;
        }

        internal void UpdateTopic(MqttTopicSubscription topic, MqttUserSession session)
        {
            var deviceUsers = deviceSubscribes;
            foreach (var item in session.PreferenceFor(topic))
            {
                if (deviceUsers.TryGetValue(item.Key, out var signals) == false)
                {
                    signals = new Dictionary</*int*/string, HashSet<MqttTopicSubscription>>();
                    deviceUsers[item.Key] = signals;
                }
                // add user to signal
                foreach (var signal in item.Value)
                {
                    if (signals.TryGetValue(signal, out var users) == false)
                    {
                        users = new HashSet<MqttTopicSubscription>();
                        signals[signal] = users;
                    }

                    users.Add(topic);

                }
            }
        }

        public Task HandleClientDisconnectedAsync(MqttServerClientDisconnectedEventArgs eventArgs)
        {
            if (clients.TryGetValue(new ClientInfo(eventArgs.ClientId, -1), out var actualClient)
                && clients.Remove(actualClient)
                && connectedUsers.TryGetValue(actualClient.UserId, out var session))
            {
                if (session.TryRemove(actualClient.ClientId, out var value))
                {
                    foreach (var topic in value)
                    {
                        if (!session.HasFilter(topic))
                        {
                            ClearTopic(topic, session);
                        }
                    }
                }
                // if no mqtt user
                if (session.Subscribers.Count == 0)
                {
                    connectedUsers.TryRemove(actualClient.UserId, out _);
                }
            }
            return Task.CompletedTask;
        }
    }
}

using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RMS.SocketServer.Net.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace RMS.SocketServer.Net
{
    /// <summary>
    /// TCP socket server listen for socket clients and manages socket connection
    /// </summary>
    public class SocketServer
    {
        private readonly ConcurrentDictionary<string, SocketConnection> clients;

        private readonly RetainedMessages retainedMessages;

        private readonly BlockingCollection<ClientMessage> messages;

        private readonly CancellationTokenSource cts;

        private readonly ILogger _logger;

        Socket tcpListener;

        internal EndPoint LocalEndPoint;

        private Task serverTask;
        private Task dispatchTask;

        private readonly ILoggerFactory _loggerFactory;

        internal readonly Configurations.SocketConfiguration SocketConfiguration;

        public SocketServer(Configurations.SocketConfiguration configuration, ILoggerFactory loggerFactory, IMemoryCache cache)
        {
            SocketConfiguration = configuration;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger("Tcp.SocketServer");
            clients = new ConcurrentDictionary<string, SocketConnection>();
            messages = new BlockingCollection<ClientMessage>();
            cts = new CancellationTokenSource();
            retainedMessages = new RetainedMessages(cache);
        }

        /// <summary>
        /// Get all socket client connection instances
        /// </summary>
        /// <returns></returns>

        public IList<Models.ClientInfo> GetClients()
        {
            var res = new List<Models.ClientInfo>();
            foreach (var connection in clients.Values)
            {
                res.Add(connection.ClientInfo);
            }
            return res;
        }

        /// <summary>
        /// Use web socket connection as client
        /// </summary>
        /// <param name="socket">Websocket instance to use</param>
        /// <param name="context">Http Web context for connection</param>
        /// <returns>Task for the connection</returns>
        public async Task UseSocket(WebSocket socket, HttpContext context)
        {
            var client = new WebSocketConnection(context.Connection, socket, SocketConfiguration.WebSocketEndPoint);
            client.Recieved += OnMessageReceived;
            try
            {
                clients.TryAdd(client.ClientId, client);
                _logger.LogInformation("Web socket connected from " + context.Connection.RemoteIpAddress);
                client.Start(cts.Token);
                await client.RunAsync();
            }
            catch (ConnectionAbortedException) { }
            finally
            {
                clients.TryRemove(client.ClientId, out _);
                _logger.LogInformation("Web socket closed from " + context.Connection.RemoteIpAddress);
                client.Dispose();
            }
        }

        /// <summary>
        /// Start the server listen
        /// </summary>
        public void Start()
        {
            tcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverTask = TaskFx.Start(BeginServer, cts.Token);
            dispatchTask = TaskFx.Start(DispatchMessages, cts.Token);
        }

        void BeginServer()
        {
            var settings = SocketConfiguration.TcpEndPoint;
            if (settings.Enabled == false)
                return;
            if (!settings.TryReadAddress(out var address4)
                || !(settings.Port > 0))
            {
                throw new Exception("Could not start Tcp Server");
            }

            var token = cts.Token;
            Socket listener = tcpListener;
            var endPoint = new IPEndPoint(address4, settings.Port);
            LocalEndPoint = listener.LocalEndPoint;
            try
            {
                listener.Bind(endPoint);
                tcpListener.Listen((int)SocketOptionName.MaxConnections);
            }
            catch (SocketException ex)
            {
                _logger.LogError(ex, "Tcp Socket Error " + ex.Message);
                StopServer();
                return;
            }
            _logger.LogInformation("Started Tcp Socket at " + endPoint);
            for (; ; )
            {
                if (token.IsCancellationRequested)
                    return;
                try
                {
                    var client = listener.Accept();
                    if (client != null)
                    {
                        AddSocketClient(new TcpSocketConnection(client, _loggerFactory.CreateLogger("Tcp.SocketClient")));
                    }
                }
                catch (SocketException)
                {
                    // Timeout or some other error happened.
                }
            }
        }

        void StopServer(int timeout = 0)
        {
            if (tcpListener != null)
            {
                tcpListener.Close(timeout);
                tcpListener = null;
            }
        }

        internal string AddSocketClient(SocketConnection connection)
        {
            connection.Recieved += OnMessageReceived;
            connection.Disconnected += OnClientDisconnected;
            connection.Start(cts.Token);
            clients.TryAdd(connection.ClientId, connection);
            _logger.LogInformation("Tcp connection connected from " + connection);
            if (retainedMessages.TryGetRetainedMessages(out var messages))
            {
                for (int i = 0; i < messages.Count; i++)
                {
                    connection.Enqueue(messages[i]);
                }
            }
            return connection.ClientId;
        }

        internal bool Disconnect(string clientId)
        {
            if (clients.TryRemove(clientId, out var connection))
            {
                connection.Stop();
                return true;
            }
            return false;
        }

        void OnClientDisconnected(SocketConnection obj)
        {
            clients.TryRemove(obj.ClientId, out _);
            Clean(obj);
        }

        void Clean(SocketConnection obj)
        {
            obj.Recieved -= OnMessageReceived;
            obj.Disconnected -= OnClientDisconnected;
            obj.Dispose();
            _logger.LogInformation("Tcp connection closed from " + obj);
        }

        void OnMessageReceived(ClientMessage value)
        {
            messages.Add(value);
        }


        /// <summary>
        /// Handle the client messages and create response
        /// </summary>
        internal void DispatchMessages()
        {
            var token = cts.Token;
            try
            {
                for (; ; )
                {
                    if (token.IsCancellationRequested)
                        return;
                    var message = messages.Take();

                    IMessageResponse result;
                    string clientId = message.ClientId;
                    IUserMessage userMessage = message.Value.GetMessage();
                    switch (userMessage.Type)
                    {
                        case MessageType.Ping:
                            // Ignore ping message
                            continue;
                        case MessageType.FromClient:
                            result = new DeviceResponse(clientId, userMessage);
                            break;
                        case MessageType.FromDevice:
                            result = new ClientResponse(clientId, userMessage);
                            break;
                        case MessageType.External:
                            result = RedirectMessage.GetResponse(message);
                            break;
                        case MessageType.DeviceData:
                            result = new DeviceDataResponce(clientId, userMessage);
                            break;
                        case MessageType.DeviceReply:
                            continue;
                        default:
                            continue;
                    }
                    if ((userMessage.Quality & DeliveryQuality.Synchronous) == 0)
                    {
                        int deliveryCount = 0;
                        foreach (var client in clients.Values)
                        {
                            if (client.ClientId.Equals(clientId))
                                continue;
                            client.Enqueue(result);
                            deliveryCount++;
                        }
                        if (deliveryCount == 0 && userMessage.Retain)
                        {
                            retainedMessages.AddMessage(clientId, result);
                        }
                    }
                    else
                    {
                        Task.Factory.StartNew(DispatchSync, result, token, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
                    }
                }
            }
            catch (System.Text.Json.JsonException)
            {
                DispatchMessages();
                return;
            }
            catch (OperationCanceledException)
            {
                // Cancelled because server stop
            }
            catch (ObjectDisposedException)
            {
                // Cancelled because server stop
            }
        }

        async void DispatchSync(object state)
        {
            var response = (IMessageResponse)state;
            int deliveryCount = 0;
            string clientId = response.ClientId;
            SocketConnection client;
            SemaphoreSlim semaphore = new SemaphoreSlim(0);
            var e = clients.Values.GetEnumerator();
            try
            {
                var message = new Dispatcher.MessageResponseAwaiter(response, semaphore);
                while (e.MoveNext())
                {
                    client = e.Current;
                    if (client.ClientId.Equals(clientId))
                        continue;
                    client.Enqueue(message);
                }
                if (response.Message.Retain)
                {
                    retainedMessages.AddMessage(clientId, message);
                }
                CancellationToken token = cts.Token;
                int deliveryTimeout = SocketConfiguration.TcpEndPoint.DeliveryTimeout;
                // Wait for all message delivery

                if (await message.WaitOneAsync(deliveryTimeout, token))
                    deliveryCount++;
                // Remove the message after Delivery time elpased
                retainedMessages.RemoveMessage(clientId);
                // Update the delivery result to client
                if (deliveryCount > 0 && clients.TryGetValue(clientId, out client))
                {
                    client.Enqueue(new DeliveryResponse(clientId, response.Message));
                }
            }
            finally
            {
                e.Dispose();
                semaphore.Dispose();
            }
        }

        public async Task Stop(CancellationToken cancellationToken)
        {
            try
            {
                cts.Cancel();
            }
            finally
            {
                StopServer(2000);
            }
            await Task.WhenAny(dispatchTask, serverTask, Task.Delay(-1, cancellationToken));
            foreach (var client in clients.Values)
            {
                client.Stop();
                client.Dispose();
            }
        }

        public void Dispose()
        {
            cts.Dispose();
            messages.Dispose();
            if (retainedMessages != null)
            {
                retainedMessages.Dispose();
            }
        }
    }
}

using Microsoft.Extensions.Logging;
using RMS.SocketServer.Extensions;
using RMS.SocketServer.Models;
using RMS.SocketServer.Net.Formatters;
using RMS.SocketServer.Net.Messages;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RMS.SocketServer.Net
{
    /// <summary>
    /// Tcp connection for handling receive and sending of messages
    /// </summary>
    public class TcpSocketConnection : SocketConnection
    {
        public readonly Socket Socket;

        private readonly EndPoint endPoint;

        private readonly ILogger logger;

        private readonly Dispatcher.PacketDispatcher dispatcher;

        private readonly MessageIdGenerator idGenerator;

        private char StartBit = '#';
        private char EndBit = '$';

        public TcpSocketConnection(Socket socket, ILogger logger) : base(Guid.NewGuid().ToString())
        {
            Socket = socket;
            this.logger = logger;
            endPoint = socket.RemoteEndPoint;
            Writter = new CharWriter();
            Reader = new CharReader();
            dispatcher = new Dispatcher.PacketDispatcher();
            idGenerator = new MessageIdGenerator();
        }

        public TcpSocketConnection(Socket socket, ILogger logger, IPacketReader reader, IPacketWriter writer) : base(Guid.NewGuid().ToString())
        {
            Socket = socket;
            this.logger = logger;
            endPoint = socket.RemoteEndPoint;
            Writter = writer;
            Reader = reader;
        }

        public IPacketReader Reader { get; }

        public IPacketWriter Writter { get; }

        protected override void OnStart(CancellationToken token)
        {
            TaskFx.Start(BeginReceive, token);
        }


        private byte[] headerBytes;
        private byte[] data;

        void BeginReceive()
        {
            var token = CancellationTokenSource.Token;
            var reader = Reader;
            try
            {

                for (; ; )
                {
                    headerBytes = reader.CreateHeader();
                    if (token.IsCancellationRequested)
                        return;
                    int res = ReadHeaderBytes(headerBytes, 0, headerBytes.Length);
                    if (res == headerBytes.Length)
                    {
                        if (headerBytes[0] == 'A' && AtReplyMessage.ValidateHeader(headerBytes))
                        {
                            logger.LogWarning("Got At+ Command {0}", Socket.ReceiveCommands(headerBytes));
                            continue;
                        }
                        var header = reader.ReadHeader(headerBytes);
                        logger.LogInformation("Message Source: " + header.Type.ToString());
                        int length = header.Length;
                        if (length < 0)
                            return;
                        if (length == 0)
                        {
                            HeaderRecieve(header);
                            continue;
                        }



                        //data = new byte[length];

                        //res = ReadDataBytes(data, 0, length);
                        //logger.LogInformation("data: " + Encoding.UTF8.GetString(data));
                        //if (res == length)
                        //{
                        //    logger.LogInformation("exactData: " + Encoding.UTF8.GetString(data));
                        //    ReceivedMessage(new RawMessagePacket(header, data));
                        //    continue;
                        //}
                        string dataTest = null;
                        try
                        {
                            dataTest = Encoding.UTF8.GetString(headerBytes).Substring(12, length);
                        }
                        catch (Exception customException)
                        {
                            var errorMessage = customException.Message;
                        }
                        


                        //data = new byte[length];

                        //res = ReadDataBytes(data, 0, length);
                        logger.LogInformation("data: " + dataTest);
                        if (dataTest.Length == length)
                        {
                            logger.LogInformation("exactData: " + dataTest);
                            ReceivedMessage(new RawMessagePacket(header, Encoding.UTF8.GetBytes(dataTest)));
                            continue;
                        }



                    }
                    //Reached End Of stream
                    if (res == 0)
                    {
                        logger.LogWarning("Closing Client from " + ClientId);
                        return;
                    }
                }
            }
            catch (SocketException ex)
            {
                logger.LogError("Recieve() : " + ex.Message);
            }
            catch (IndexOutOfRangeException ex)
            {
                logger.LogError("Recieve() : " + ex.Message);
            }
            finally
            {
                OnDisconnected();
            }
        }

        void HeaderRecieve(MessageHeader header)
        {
            if (header.Type == MessageType.Ping)
                return;
            if (header.Type == MessageType.DeviceReply)
            {
                dispatcher.Dispatch(header.Id);
            }
        }

        void ReceivedMessage(RawMessagePacket message)
        {
            var type = message.Header.Type;
            if (type == MessageType.Ping)
            {
                return;
            }
            base.ReceivedMessage(message);
        }

       

        int ReadHeaderBytes(byte[] buffer, int offset, int count)
        {
            string buffData=string.Empty;
            while (true)
            {

                int read = Socket.Receive(buffer, 0, count, SocketFlags.None);
                buffData += Encoding.ASCII.GetString(buffer, 0, buffer.Length);
                logger.LogInformation("First Point of header data: " + buffData);
                if (read == 0)
                    return 0;
               // offset += read;
                if (buffData.Contains(StartBit) && buffData.Contains(EndBit))
                {
                   
                    var rawData = buffData.Split(StartBit)[1].Split(EndBit)[0];
                    logger.LogInformation("Final header Data: " + rawData);
                    headerBytes = Encoding.UTF8.GetBytes(rawData);
                    buffData = string.Empty;
                    break;

                }
            }
           
            return headerBytes.Length;
        }


        int ReadDataBytes(byte[] buffer, int offset, int count)
        {
            int read = Socket.Receive(buffer, offset, count - offset, SocketFlags.None);
            logger.LogInformation("Data: " + Encoding.UTF8.GetString(buffer, 0, buffer.Length));
            if (read == 0)
                return 0;
            offset += read;
            if (offset < count)
            {
                return ReadDataBytes(buffer, offset, count);
            }
            return offset;

            //string buffData = string.Empty;
            //while (true)
            //{

            //    int read = Socket.Receive(buffer, 0, count, SocketFlags.None);
            //    buffData += Encoding.ASCII.GetString(buffer, 0, buffer.Length);
            //    logger.LogInformation("First Point of  data: " + buffData);
            //    if (read == 0)
            //        return 0;
            //    // offset += read;
            //    if (buffData.Contains(StartBit) && buffData.Contains(EndBit))
            //    {

            //        var rawData = buffData.Split(StartBit)[1].Split(EndBit)[0];
            //        logger.LogInformation("Final  Data: " + rawData);
            //        data = Encoding.UTF8.GetBytes(rawData);
            //        buffData = string.Empty;
            //        break;

            //    }
            //}

            //return headerBytes.Length;
        }

        public override ClientInfo ClientInfo
        {
            get
            {
                return new ClientInfo
                {
                    Id = ClientId,
                    Address = Socket.RemoteEndPoint.ToString()
                };
            }
        }



        public override void Stop()
        {
            try
            {
                Socket.Disconnect(false);
                OnDisconnected();
            }
            finally
            {
                Socket.Close();
            }
        }

        public override string ToString()
        {
            return endPoint.ToString();
        }

        protected async override Task SendAsync(IMessageResponse res)
        {
            int byteSent;
            Writter.Reset();
            var message = res.Message;
            var value = message.Value;
            if (res.ResponseType == MessageType.ToDevice)
            {
                ushort id = idGenerator.GetNextPacketIdentifier();
                Writter.WriteChar('[')
                       .WriteIdentifier(id)
                       .WriteChar(',')
                       .Write(value)
                       .WriteChar(']');
                if (message.Quality == DeliveryQuality.None)
                {
                    byteSent = await Socket.SendAsync(Writter.GetBytes(), SocketFlags.None);
                    res.SetResult(byteSent > 0);
                    return;
                }
                using var awaiter = dispatcher.AddAwaiter(id);
                byteSent = await Socket.SendAsync(Writter.GetBytes(), SocketFlags.None);
                // Wait for some interval to get acknoledgement
                if (await awaiter.WaitOneAsync(TimeSpan.FromSeconds(15)) || (message.Quality & DeliveryQuality.ExactlyOnce) == 0)
                {
                    res.SetResult(true);
                    return;
                }
                await Socket.SendAsync(Writter.GetBytes(), SocketFlags.None);
                res.SetResult(await awaiter.WaitOneAsync(TimeSpan.FromSeconds(15)));
                return;
            }
            Writter.WriteByte((byte)res.ResponseType)
                   .WriteUInt16(value.Length)
                   .WriteByte((byte)res.RequestType)
                   .WriteUInt16(message.Id)
                   .Write(value);
            byteSent = Socket.Send(Writter.GetBytes(), SocketFlags.None);
            res.SetResult(byteSent > 0);
        }

        protected override void OnDispose()
        {
            dispatcher.CancelAll();
            base.OnDispose();
        }
    }
}

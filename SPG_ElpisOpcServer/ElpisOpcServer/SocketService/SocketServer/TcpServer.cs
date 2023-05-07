using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ElpisOpcServer.SocketService.SocketServer
{
    using Elpis.Windows.OPC.Server;
    using ElpisOpcServer.SunPowerGen;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Windows;
    
    public class TcpServer
    {
        
        private static readonly int port;
        private static readonly string ipAddress;
        public IPEndPoint IPEndPoint { get; }
         
        private static TcpListener listener;
        private static bool isRunning = false;
        public string id;
        public static  TcpClient client;
        public TcpClient  clientScoket;
        public static string ServerResponse;
        public TcpServer()
        {
            id = Guid.NewGuid().ToString();
        }
        public TcpServer(TcpClient client)
        {
            this.clientScoket = client;
            id = Guid.NewGuid().ToString();
        }
        //public TcpServer(/*IPEndPoint localEP*/string ipAddress,int port)
        //{
        //    // this.IPEndPoint = localEP;
        //    this.ipAddress = ipAddress;
        //    this.port = port;
        //    // listener = new TcpListener(ipAddress, port);
        //}

        //public void Start()
        //{
        //    isRunning = true;
        //    listener.Start();
        //    Console.WriteLine($"Server started on {ipAddress}:{port}");

        //    while (isRunning)
        //    {
        //        // Accept incoming client connection
        //        TcpClient client = listener.AcceptTcpClient();
        //        Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");

        //        // Handle client connection in a new thread
        //        Thread clientThread = new Thread(() => HandleClient(client));
        //        clientThread.Start();
        //    }
        //}
        static bool result;
        //public bool ServerStart(string ipAddress, int port)
        //{
           
        //    listener = new TcpListener(IPAddress.Parse(ipAddress), port);
        //    isRunning = true;
        //    listener.Start();
        //    MessageBox.Show($"Server started on {ipAddress}:{port}");

        //    while (isRunning)
        //    {
        //        // Accept incoming client connection
        //        TcpClient client = listener.AcceptTcpClient();
        //        MessageBox.Show($"Client connected: {client.Client.RemoteEndPoint}");

        //        // Handle client connection in a new thread
        //        Thread clientThread = new Thread(() => HandleClient(client));
        //        clientThread.Start();
        //        result = client.Connected;
        //    }
        //    return result;
        //}


        public static TcpClient Start(string ipAddress, int port)
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                //listener = new TcpListener(IPAddress.Parse(ipAddress), port);
                isRunning = true;
                listener.Start();
                MessageBox.Show($"Server started on {ipAddress}:{port}");
                while (isRunning)
                {
                    // Accept incoming client connection
                        client = listener.AcceptTcpClient();
                        /// Devcie configration details as a packet we need send to device.
                        /// in  that packet  should have the header and footer with data.
                       
                        MessageBox.Show($"Client connected: {client.Client.RemoteEndPoint}");
                        if (client.Connected)
                            ElpisServer.Addlogs("All", "SPG Reporting Tool", string.Format("Device connected Successfully, with IP:{0} and Port:{1}", ipAddress, port), LogStatus.Information);
                        else
                            ElpisServer.Addlogs("All", "SPG Reporting Tool", string.Format("Device not Connected.IP:{0}, Port:{1}", ipAddress, port), LogStatus.Error);


                        // Handle client connection in a new thread
                        Thread clientThread = new Thread(() => HandleClient(client));
                        clientThread.Start();

                        return client;
                }
                   
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred: {ex.Message}");
            }

            return null;
        }
        public static void ServerStop()
        {
            isRunning = false;
            listener.Stop();
        }

        public static void HandleClient(TcpClient client)
        {
            try
            {
                // Get client stream for reading and writing
                NetworkStream stream = client.GetStream();

                while (isRunning)
                {
                    // Read data from client
                    if(ServerResponse==null)
                    {
                        int bytesRead = Requestfromclient(client, stream);
                        if (bytesRead == 0)
                        {
                            // Client disconnected
                            MessageBox.Show($"Client disconnected: {client.Client.RemoteEndPoint}");
                            break;
                        }
                    }
                    else if(ServerResponse.Contains("My name is client"))
                    {
                        Sendreponsetoclient(stream);
                    }
                    
                    // device config pack req to the device
                    ///device config pack respon to the device
                    /// ack from the device
                    /// signal config packet req to the device
                    /// signal config pack respon to the device
                    /// ack from the device
                    /// start command to the device
                    ///receving  real data from the device 
                    ///  stop command to the device


                    // Send response to client
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error handling client {client.Client.RemoteEndPoint}: {ex.Message}");       
            }
            finally
            {
                // Close client connection
                client.Close();
            }
        }

        private static int Requestfromclient(TcpClient client, NetworkStream stream)
        {
            byte[] buffer = new byte[4096];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            // Process received data
            ServerResponse = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
          //  MessageBox.Show($"Received from client {client.Client.RemoteEndPoint}: {ServerResponse}");
            return bytesRead;
        }

        private static void Sendreponsetoclient(NetworkStream stream)
        {
            byte[] response = System.Text.Encoding.UTF8.GetBytes($"Server received: {ServerResponse}");
            stream.Write(response, 0, response.Length);

            byte[] buffer = new byte[4096];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            // Process received data
            ServerResponse = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
            MessageBox.Show($"Received from client : {ServerResponse}");
        }
    }

}

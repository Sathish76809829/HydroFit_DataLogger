using Elpis.Windows.OPC.Server;
using LibplctagWrapper;
using LiveCharts;
using LiveCharts.Wpf;
using Modbus.Device;
using Newtonsoft.Json;
using OPCEngine.Connectors.Allen_Bradley;
using OPCEngine.View_Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ElpisOpcServer.SocketService.Dispatcher;
using ElpisOpcServer.SocketService.Net.Formatters;
using ElpisOpcServer.SocketService.Net;
using ElpisOpcServer.SocketService.SocketServer;

namespace ElpisOpcServer.SunPowerGen
{
    public class Helper
    {
        private static readonly int DataTimeout = 2000;
        private static byte slaveId;
        public readonly Socket Socket;
        private readonly PacketDispatecher dispatcher=new PacketDispatecher();
        public static string DeviceResponse;
        public  static TcpClient client;
        public static NetworkStream stream;
        // public static TcpServer server = new TcpServer(IPAddress.Parse("127.0.0.1"),5008);
        //SocketService socket;
        public static TcpServer tcpserver = new TcpServer();
        private List<string> selectedTagNames = new List<string>();
        public ObservableCollection<Elpis.Windows.OPC.Server.Tag> TagsCollection { get; private set; }

        public static Dictionary<string, Tuple<LibplctagWrapper.Tag, Elpis.Windows.OPC.Server.DataType>> MappedTagList { get; private set; }
        private static char StartBit = '#';
        private static char EndBit = '$';
        private byte[] headerBytes;
        protected CancellationTokenSource CancellationTokenSource;
       
        public IPacketReader Reader { get; }
        public static TcpClient CreateTcpClient(string ipAddress, ushort port)
        {
            
             client = new TcpClient();
            try
            {
                //string fileLocation = string.Format("{0}//DeviceInfo.txt", Directory.GetCurrentDirectory());
                //string[] fileContent= File.ReadAllLines(fileLocation);
                //if(fileContent.Count()==2)
                //{
                //    deviceIP = fileContent[0].Split(':')[1];
                //    port = ushort.Parse(fileContent[1].Split(':')[1].ToString());
                //}
               
                // ModbusEthernetDevice ethernetDevice = SunPowerGenMainPage.DeviceObject as ModbusEthernetDevice;
                IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();

                //bool isInUse = false;
                //foreach (IPEndPoint endPoint in ipEndPoints)
                //{
                //    if (endPoint.Port == port)
                //    {
                //        isInUse = true;
                //        break;
                //    }
                //}
                //if(isInUse)
                //    ElpisServer.Addlogs("All", "SPG Reporting Tool", "Port is in Use.", LogStatus.Information);
                //else
                //    ElpisServer.Addlogs("All", "SPG Reporting Tool", "Port is in Free.", LogStatus.Information);
                var result = client.BeginConnect(ipAddress, port, null, null);

                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));
                // client.Connect(ipAddress, port);
                if (client.Connected)
                    ElpisServer.Addlogs("All", "SPG Reporting Tool", string.Format("Device connected Successfully, with IP:{0} and Port:{1}", ipAddress, port), LogStatus.Information);
                else
                    ElpisServer.Addlogs("All", "SPG Reporting Tool", string.Format("Device not Connected.IP:{0}, Port:{1}", ipAddress, port), LogStatus.Error);

                return client;
            }
            catch (Exception ex)
            {
                ElpisServer.Addlogs("All", "SPG Reporting Tool", string.Format("Error in Creating TcpClient.\n{0}", ex.Message), LogStatus.Error);
                //ElpisServer.Addlogs("All", "SPG Reporting Tool", string.Format(ex.StackTrace), LogStatus.Error);
                //ElpisServer.Addlogs("All", "SPG Reporting Tool", string.Format(ex.Source), LogStatus.Error);

                ElpisServer.Addlogs("All", "SPG Reporting Tool", string.Format("AddressFamily: {0}", client.Client.AddressFamily), LogStatus.Error);
                ElpisServer.Addlogs("All", "SPG Reporting Tool", string.Format("Blocking: {0}", client.Client.Blocking), LogStatus.Error);
                ElpisServer.Addlogs("All", "SPG Reporting Tool", string.Format("DualMode: {0}", client.Client.DualMode), LogStatus.Error);
                ElpisServer.Addlogs("All", "SPG Reporting Tool", string.Format("ExclusiveAddressUse: {0}", client.Client.ExclusiveAddressUse), LogStatus.Error);
                ElpisServer.Addlogs("All", "SPG Reporting Tool", string.Format("IsBound: {0}", client.Client.IsBound), LogStatus.Error);
                ElpisServer.Addlogs("All", "SPG Reporting Tool", string.Format("Protocol Type: {0}", client.Client.ProtocolType), LogStatus.Error);
                ElpisServer.Addlogs("All", "SPG Reporting Tool", string.Format("SocketService Type: {0}", client.Client.SocketType), LogStatus.Error);
                ElpisServer.Addlogs("All", "SPG Reporting Tool", string.Format("Enable Broadcast: {0}", client.Client.EnableBroadcast), LogStatus.Error);

                MessageBox.Show(ex.Message);
            }
            return null;
        }
        
        #region SendConfigData
        public static string formatConfigData(List<Tuple<string, Elpis.Windows.OPC.Server.Tag>> selectedItems)
        {
            
           
            
            string SignalList = "SignalDataList";
            var tcpdevice = SunPowerGenMainPage.DeviceObject as TcpSocketDevice;
            string[] signalData = new string[selectedItems.Count];
            for (int i = 0; i < selectedItems.Count; i++)
            {
                //Description=channel
                //TODO: need to add datatype
                //string value = $"{selectedItems[i].Item2.Address};{selectedItems[i].Item2.ChannelNo};T;{selectedItems[i].Item2.DataType}";
                //string value1 = $"{selectedItems[i].Item2.TagName};{selectedItems[i].Item2.ChannelNo};{selectedItems[i].Item2.Address};{selectedItems[i].Item2.DataType};{selectedItems[i].Item2.MinValue};{selectedItems[i].Item2.MaxValue}";
                string value = $"{selectedItems[i].Item2.Address};T;{selectedItems[i].Item2.DataType};{tcpdevice.SamplingRate}";
                string Rvalue = '"' + value + '"';
                string formatString = "{0}";
                signalData[i] = string.Format(formatString, Rvalue);
                //string str = "{"+'"'+ value +  '"'+"}" /*+','*/;
                //string formatString = "{0}";
                //signalData[i] = string.Format(formatString, value);
                //signalData[i] = string.Format("{Signal:{0}}", value);
                //string formatString = "Signal:{0}";
                //signalData[i] = string.Format(formatString, value);
                //string formatString = "{0}:{1}";
                //signalData[i] = string.Format(formatString,"Signal", value);

            }
            string numbersString = string.Join(",", signalData);
            //string data = $"{SignalList}:[{numbersString}]";
            string data = $"[{numbersString}]";
            int count = Encoding.UTF8.GetByteCount(data);
            string hexvalue = Pump_Test.DecimalToHexTwosComplement(count);
            //string list = "ch1;T;Float";
            //string data1 = $"{SignalList}:[{data}]";
            //string data = $"deviceId:{tcpdevice.DeviceId};{SignalList}:[{signalData}];{samplingRate}:{tcpdevice.SamplingRate}";
            string header = $"#01{hexvalue}000000{data}$";
            return header;
        }
        #endregion SendConfigData

        public static void SendConfigPacketToServer(string SendConfig, TcpClient tcpClient)
        {
            string header = SendConfig;
            
            if (header != null)
            {

                string deviceRes = string.Empty;
                //SendingStartStopCmdtoserver(header, tcpClient,ref deviceRes);
              SendChannelConfigPacketToServer(SendConfig, tcpClient,ref deviceRes);
                //return value;
            }
            //return string.Empty;
            //SendingStartStopCmdtoserver(SendConfig);
        }
        #region generic SendData

        public static void SendChannelConfigPacketToServer(string formatConfigData,TcpClient client, ref string deviceRes)
        {
            //bool result = false;
            int startIndex = deviceRes.IndexOf('#');
            int endIndex = deviceRes.LastIndexOf('$');
            var deviceInfo = DeviceFactory.GetDevice(SunPowerGenMainPage.DeviceObject) as TcpSocketDevice;
            //byte[] sendBuffer = Encoding.UTF8.GetBytes(formatConfigData);
            //stream.Write(sendBuffer, 0, sendBuffer.Length);

            // to do: write the logic to get the acknowlogdemet for that call header formatConfigData() from rms 
            //validate ack  && call TcpDeviceData
            NetworkStream stream = client.GetStream();

            //byte[] bytes = new byte[2048];
            //int bytesRead = stream.Read(bytes, 0, bytes.Length);
            //DeviceResponse = Encoding.ASCII.GetString(bytes, 0, bytesRead);

            byte[] sendBuffer = Encoding.UTF8.GetBytes(formatConfigData);
            stream.Write(sendBuffer, 0, sendBuffer.Length);
           
            #region Real code
            //string str = "#2C0018000000getsignallist;HydFit_001$";
            //if (DeviceResponse.Contains(StartBit) && DeviceResponse.Contains(EndBit))
            //{
            //    string deviceID = DeviceResponse.Split(';')[1].Split('$')[0];
            //    if (deviceID == deviceInfo.DeviceId)
            //    {
            //        byte[] sendBuffer = Encoding.UTF8.GetBytes(formatConfigData);
            //        stream.Write(sendBuffer, 0, sendBuffer.Length);
            //    }
            //}
            #endregion Real code
             
        }
        public static bool SendingStartStopCmdtoserver(string formatConfigData, TcpClient client/*,ref string deviceRes*/)
        {
            bool result = false;
        
            try
            {
                 
                NetworkStream stream = client.GetStream();
                // MessageBox.Show(stream.ToString());
                //byte[] bytes = new byte[2048];
                //int bytesRead = stream.Read(bytes, 0, bytes.Length);
                //DeviceResponse = Encoding.ASCII.GetString(bytes, 0, bytesRead);

                byte[] sendBuffer = Encoding.UTF8.GetBytes(formatConfigData);
                stream.Write(sendBuffer, 0, sendBuffer.Length);
                result = true;
                //byte[] sendBuffer = Encoding.UTF8.GetBytes(formatConfigData);
                //stream.Write(sendBuffer, 0, sendBuffer.Length);
                //result = true;
                //string deviceRes = DeviceResponse;
                #region Real time Work Code
                //if (DeviceResponse.Contains(StartBit) && DeviceResponse.Contains(EndBit))
                //{
                //    string deviceID = DeviceResponse.Split(';')[1].Split('$')[0];
                //    if (deviceID == deviceInfo.DeviceId)
                //    {
                //        byte[] sendBuffer = Encoding.UTF8.GetBytes(formatConfigData);
                //        stream.Write(sendBuffer, 0, sendBuffer.Length);
                //        result = true;
                //    }
                //}
                #endregion Real time Work Code
                //if (deviceRes!=null)
                //{
                //    byte[] sendBuffer = Encoding.UTF8.GetBytes(formatConfigData);
                //    stream.Write(sendBuffer, 0, sendBuffer.Length);
                //    result = true;
                //}
                /*var tcp*///client = CreateTcpClient("127.0.0.1", 5008);
                           // NetworkStream stream = client.GetStream();

            }
            catch (Exception ex)
            {
                result = false;
                
                throw;
            }
            /*var tcp*///client = CreateTcpClient("127.0.0.1", 5008);
                       //NetworkStream stream = client.GetStream();
                       //byte[] sendBuffer = Encoding.UTF8.GetBytes(formatConfigData);
                       //stream.Write(sendBuffer, 0, sendBuffer.Length);
                       //// to do: write the logic to get the acknowlogdemet for that call header formatConfigData() from rms 
                       ////validate ack  && call TcpDeviceData
                       //byte[] bytes = new byte[2048];
                       ////string receivedBuffer = Encoding.ASCII.GetString(bytes);//Convert.ToInt32(Encoding.ASCII.GetString(bytes));
                       ////stream.Read(bytes, 0, receivedBuffer.Length);
                       //int bytesRead = stream.Read(bytes, 0, bytes.Length);
                       //DeviceResponse = Encoding.ASCII.GetString(bytes, 0, bytesRead);

            return result;
        }
        #endregion generic sendData
        #region sendData
        //public static void CilentDataToServer(DeviceDataModel deviceData)
        //{
        //    #region tcpmethod

        //    TcpClient clientSocket = new TcpClient();
        //    NetworkStream stream = clientSocket.GetStream();
        //    byte[] sendBuffer = Encoding.UTF8.GetBytes(deviceData.ToString());
        //    stream.Write(sendBuffer, 0, sendBuffer.Length);

        //    #endregion tcpmethod
        //    #region Socketmethod

        //    // byte[] msg = Encoding.ASCII.GetBytes(formatConfigData);
        //    //socket.Send(msg);
        //    #endregion Socketmethod
        //}
        #endregion sendData
        public static string TcpDeviceData(TcpClient client)
        {
            string Jsondata = string.Empty;
            
            stream= client.GetStream();
            #region stream readfun() by sathish
            byte[] bytes = new byte[1024];
            string receivedBuffer = Encoding.ASCII.GetString(bytes);
            int bytesRead = stream.Read(bytes, 0, receivedBuffer.Length);
            
            Jsondata = Encoding.ASCII.GetString(bytes, 0, bytesRead);
            //stream.Flush();
            return Jsondata;
            #endregion stream readfun() by sathish
            #region Old stream function
            //StreamReader sr = new StreamReader(stream);

            //try
            //{
            //    if (stream.DataAvailable)
            //    {

            //        try
            //        {
            //            Jsondata = sr.ReadLine();
            //        }
            //        catch (Exception ex)
            //        {
            //        }
            //        return Jsondata;
            //    }


            //    #region Dummy Data for Testing
            //    //string jsonData = @"
            //    //                    {
            //    //                        ""device"": ""HydFit_001>2023-03-02 06:33:05"",
            //    //                        ""SignalDataList"": [
            //    //                            ""ch1>27808"",
            //    //                            ""ch2>18098"",
            //    //                            ""ch3>18115"",
            //    //                            ""ch4>18106"",
            //    //                            ""ch5>24569"",
            //    //                            ""ch6>28016"",
            //    //                            ""ch7>31037"",
            //    //                            ""ch8>9870"",
            //    //                            ""ch9>11923"",
            //    //                            ""ch10>12983"",
            //    //                            ""ch11>8803"",
            //    //                            ""ch12>15234"",
            //    //                            ""ch13>0.000000"",
            //    //                            ""ch14>0.000000"",
            //    //                            ""ch15>0.000000"",
            //    //                            ""ch16>0.000000""
            //    //                        ]
            //    //                    }
            //    //                    ";
            //    //string JsonData= @"{""device"":""HydFit_001>2023-03-02 06:33:05"",""SignalDataList"":[""ch1>27808"",""ch2>18098"",""ch3>18115"",""ch4>18106"",""ch5>24569"",""ch6>28016"",""ch7>31037"",""ch8>9870"",""ch9>11923"",""ch10>12983"",""ch11>8803"",""ch12>15234"",""ch13>0.000000"",""ch14>0.000000"",""ch15>0.000000"",""ch16>0.000000""]}";
            //    #endregion Dummy Data for Testing


            //}
            //catch (Exception ex)
            //{

            //    return ex.Message;
            //}

            //return Jsondata;
            #endregion Old stream function
        }



        int ReadHeaderBytes(byte[] buffer,int offset,int count)
        {
            TcpClient client = new TcpClient();
            string buffData = string.Empty;
            while(true)
            {
                //TO change TCP Client
                NetworkStream stream = client.GetStream();
                int read = stream.Read(buffer, 0, buffer.Length);
                //TO change Socket
                //int read = Socket.Receive(buffer, 0, count, SocketFlags.None);
                buffData += Encoding.ASCII.GetString(buffer, 0, buffer.Length);
                if (read == 0)
                    return 0;
                if(buffData.Contains(StartBit)&& buffData.Contains(EndBit))
                {
                    var rawData = buffData.Split(StartBit)[1].Split(EndBit)[0];
                    headerBytes = Encoding.ASCII.GetBytes(rawData);
                    buffData = string.Empty;
                    break;
                        
                }
            }
            return buffData.Length;
        }
        int Read(byte[] buffer, int offset, int count)
        {
            int read = Read(buffer, offset, count - offset);
            var dat = Encoding.UTF8.GetString(buffer);
            if (read == 0)
                return 0;
            offset += read;
            if (offset < count)
            {
                return Read(buffer, offset, count);
            }
            return offset;
        }
        public void BeginReceive()
        {
            var token = CancellationTokenSource.Token;
            var reader = Reader;
            try
            {
                headerBytes = reader.CreateHeader();
                for ( ; ; )
                {
                    
                    if (token.IsCancellationRequested)
                        return;
                    int res = Read(headerBytes,0,headerBytes.Length);
                    if(res==headerBytes.Length)
                    {
                        var header = reader.ReadHeader(headerBytes);
                        if (header.Length < 0)
                            return;
                        var data = new byte[header.Length];
                        res = Read(data, 0, header.Length);
                        if(res==header.Length)
                        {
                            OnReceiveMessage(header, data);
                            continue;
                        }
                        if(res==0)
                        {
                            
                        }
                    }
                    
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        void OnReceiveMessage(MessageHeader header,byte[] data)
        {
            string Convertdata = Encoding.Default.GetString(data);
            switch (header.Type)
            {
                case MessageType.DeviceReplay:
                    dispatcher.Dispatch(header.Id);
                    break;
                case MessageType.DeviceData:
                     ParsedDeviceDatas(Convertdata);
                    //HydrofitParsedDeviceDatas(Convertdata);
                    break;
               
            }
        }
        void headerRecieve(MessageHeader header)
        {
            //if (header.Type == MessageType.Ping)
            //    return;
            //if(header.Type==MessageType.DeviceReply)
            //{
            //    dispatcher.Dispatch(header.Id);
            //}
        }

        #region Hydrofit DataPacket Parsing Function
        //public static List<parsedDeviceData> HydrofitParsedDeviceDatas(string jsonData)
        //{
        //    try
        //    {
        //        var deviceData = JsonConvert.DeserializeObject<List<DeviceDataModel>>(jsonData);

        //        List<parsedDeviceData> parsedDeviceDatas = new List<parsedDeviceData>();
        //        foreach (var data in deviceData)
        //        {
        //            string deviceID = data.device.Split('>')[0];
        //            DateTime timestamp = DateTime.Parse(data.device.Split('>')[1]);

        //            foreach (var item in data.SignalDataList)
        //            {
        //                var signalId = item.Split('>')[0];
        //                var value = item.Split('>')[1];
        //                var date = item.Split('@')[1];

        //                parsedDeviceData parsedDevice = new parsedDeviceData
        //                {
        //                    deviceId = deviceID,
        //                    signalId = signalId,
        //                    value = value,
        //                    timeStamp = timestamp,
        //                    //datatype = "float" // Set the datatype to a default value
        //                };
        //                parsedDeviceDatas.Add(parsedDevice);
        //            }
        //        }

        //        return parsedDeviceDatas;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        return null;
        //    }
        //}
        #endregion  Hydrofit DataPacket Parsing Function
        #region Hydrofit DataPacket Parsing Function
        //public static List<parsedDeviceData> HydrofitParsedDeviceDatas(string JsonData)
        //{
        //    try
        //    {
        //        //string rawdata = Encoding.Default.GetString(JsonData);
        //        var datadevice = JsonConvert.DeserializeObject<DeviceDataModel>(JsonData);

        //        string deviceID = datadevice.device.Split('>')[0];
        //        string Timestamp = datadevice.device.Split('>')[1];
        //        List<parsedDeviceData> parsedDeviceDatas = new List<parsedDeviceData>();
        //        foreach (var item in datadevice.SignalDataList)
        //        {
        //            var date = item.Split('@')[1];

        //            parsedDeviceData parsedDevice = new parsedDeviceData
        //            {

        //                deviceId = deviceID,
        //                signalId = item.Split('>')[0],
        //                value = item.Split('>')[1],//.Split('@')[0],
        //               // timeStamp = Convert.ToDateTime(item.Split('@')[1])
        //            };
        //            parsedDeviceDatas.Add(parsedDevice);
        //        }

        //        return parsedDeviceDatas;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        return null;
        //    }
        //}
        #endregion Hydrofit DataPacket Parsing Function
        #region Pumptest DataPacket Parsing Function
        #endregion Parsing Machanisim
        #region Hydrofit DataPacket Parsing Function working
        public static List<parsedDeviceData> ParsedDeviceDatas(string JsonData)
        {
            string RawData = string.Empty;
           
            try
            {
                //List<DeviceDataModel> deviceDataList = JsonConvert.DeserializeObject<List<DeviceDataModel>>(JsonData);
                RawData = JsonData.Split('#')[1].Split('$')[0];
                var datadevice = JsonConvert.DeserializeObject<DeviceDataModel>(RawData);
                string deviceID = datadevice.device.Split('>')[0];
                DateTime time = Convert.ToDateTime(datadevice.device.Split('>')[1]);
                List<parsedDeviceData> parsedDeviceDatas = new List<parsedDeviceData>();
                foreach (var item in datadevice.SignalDataList)
                {
                    //var date = item.Split('@')[1];

                    parsedDeviceData parsedDevice = new parsedDeviceData
                    {

                        deviceId = deviceID,
                        signalId = item.Split('>')[0],
                        value = item.Split('>')[1],//.Split('@')[0],
                        //value = item.Split('>')[1].Split('@')[0],
                        //timeStamp = Convert.ToDateTime(item.Split('@')[1]),
                        timeStamp = time

                    };
                    parsedDeviceDatas.Add(parsedDevice);
                }

                return parsedDeviceDatas;
                //return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
        #endregion Hydrofit DataPacket Parsing Function working

        public static SerialPort CreateSerialPort()
        {
            try
            {
                ModbusSerialDevice serialDevice = SunPowerGenMainPage.DeviceObject as ModbusSerialDevice;
                if (SunPowerGenMainPage.DeviceSerialPort == null)
                {
                    using (SerialPort port = new SerialPort(serialDevice.COMPort.ToUpper(), serialDevice.BaudRate, serialDevice.ConnectorParityBit, serialDevice.DataBits, serialDevice.ConnectorStopBits))
                    {
                        port.Open();
                        port.ReadTimeout = 500;

                        if (SunPowerGenMainPage.DeviceSerialPort != null)
                            SunPowerGenMainPage.DeviceSerialPort.Dispose();
                        SunPowerGenMainPage.DeviceSerialPort = port;
                        if (!SunPowerGenMainPage.DeviceSerialPort.IsOpen)
                            SunPowerGenMainPage.DeviceSerialPort.Open();

                    }
                }

                return SunPowerGenMainPage.DeviceSerialPort;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Create Serial Port", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }


        public static ObservableCollection<Elpis.Windows.OPC.Server.Tag> GetTagsCollection(TestType type, string connectorName, string deviceName)
        {
            string projectFilePath = string.Format(@"{0}\opcSunPowerGen.elp", Directory.GetCurrentDirectory());
            ObservableCollection<IConnector> ConnectorCollection = null;
            FileHandler FileHandle = new FileHandler();
            if (File.Exists(projectFilePath))
            {
                Stream stream = File.Open(projectFilePath, FileMode.OpenOrCreate);

                BinaryFormatter bformatter = new BinaryFormatter();
                try
                {
                    using (StreamWriter wr = new StreamWriter(stream))
                    {
                        if (FileHandle == null)
                            FileHandle = new FileHandler();
                        if (FileHandle != null)
                        {
                            FileHandle = (FileHandler)bformatter.Deserialize(stream);
                            ConnectorCollection = FileHandle.AllCollectionFileHandling;
                        }
                        wr.Close();
                    }
                    stream.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }
            if (ConnectorCollection != null && ConnectorCollection.Count > 0)
            {
                ConnectorBase connector = null;
                if (connectorName == null)
                    connector = ConnectorCollection[0] as ConnectorBase;
                else
                    connector = ConnectorCollection.FirstOrDefault(c => c.Name.ToLower() == connectorName.ToLower()) as ConnectorBase;
                if (connector.DeviceCollection != null && connector.DeviceCollection.Count > 0)
                {
                    if (deviceName == null)
                        SunPowerGenMainPage.DeviceObject = connector.DeviceCollection[0];
                    else
                        SunPowerGenMainPage.DeviceObject = connector.DeviceCollection.FirstOrDefault(d => d.DeviceName.ToLower() == deviceName.ToLower());
                    ObservableCollection<TagGroup> tagGroupsCollection = SunPowerGenMainPage.DeviceObject.GroupCollection;
                    if (tagGroupsCollection != null)
                    {
                        TagGroup taggroup = tagGroupsCollection.FirstOrDefault(t => t.GroupName.ToLower() == type.ToString().ToLower());
                        ObservableCollection<Elpis.Windows.OPC.Server.Tag> tagsCollection = new ObservableCollection<Elpis.Windows.OPC.Server.Tag>();
                        if (taggroup != null)
                        {
                            tagsCollection = taggroup.TagsCollection;
                            if (tagsCollection == null || !(tagsCollection.Count > 0))
                                MessageBox.Show("Please check configuration file don't have Tags, create tags and start test.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            MessageBox.Show("Please check configuration file having the same Group Name as follows:\n" + type.ToString(), "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Warning);
                            tagsCollection = null;
                        }

                        return tagsCollection;
                    }
                    else
                        return null;
                }
                else
                {
                    MessageBox.Show("Device not found in current Connector, please create a Device.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }
            }
            else
                return null;
        }

        internal static T CreateModbusMaster<T>(DeviceType deviceType)
        {
            // bool isCreated=false;
            try
            {
                if (deviceType == DeviceType.ModbusEthernet)
                {
                    ModbusIpMaster master = ModbusIpMaster.CreateIp(SunPowerGenMainPage.DeviceTcpClient);
                    ElpisServer.Addlogs("All", "SPG Reporting Tool", "Modbus Master Created Successfully.", LogStatus.Information);
                    return (T)Convert.ChangeType(master, typeof(T));

                }
                else if (deviceType == DeviceType.ModbusSerial)
                {
                    if (!SunPowerGenMainPage.DeviceSerialPort.IsOpen)
                        SunPowerGenMainPage.DeviceSerialPort.Open();
                    ModbusSerialMaster master = ModbusSerialMaster.CreateRtu(SunPowerGenMainPage.DeviceSerialPort);
                    ElpisServer.Addlogs("All", "SPG Reporting Tool", "Modbus Master Created Successfully.", LogStatus.Information);
                    return (T)Convert.ChangeType(master, typeof(T));
                }

            }
            catch (Exception ex)
            {
                ElpisServer.Addlogs("All", "SPG Reporting Tool", string.Format("Error in creating ModbusMaster.{0}", ex.Message), LogStatus.Information);
            }
            return (T)Convert.ChangeType(null, typeof(T));


        }


        public static bool SaveToPng(FrameworkElement visual, string fileName = null)
        {
            var encoder = new PngBitmapEncoder();
            return (EncodeVisual(visual, fileName, encoder));
        }

        private static bool EncodeVisual(FrameworkElement visual, string fileName, BitmapEncoder encoder)
        {
            var bitmap = new RenderTargetBitmap((int)visual.ActualWidth, (int)visual.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            var frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);
            using (var stream = File.Create(fileName))
            {
                encoder.Save(stream);
                if (File.Exists(fileName))
                    return true;
                else
                    return false;

            }

            //System.Drawing.Image image;
            //using (MemoryStream outStream = new MemoryStream())
            //{
            //    BitmapEncoder enc = new BmpBitmapEncoder();
            //    enc.Frames.Add(BitmapFrame.Create(frame));
            //    enc.Save(outStream);
            //    image = new System.Drawing.Bitmap(outStream);
            //    //image.Save(outStream, System.Drawing.Imaging.ImageFormat.Png);
            //}
            //return image;
        }


        internal static List<string> BuildStrig(IChartValues chartValues, string header)
        {
            if (chartValues != null && header != null)
            {
                List<string> dataList = new List<string>();
                if (chartValues != null)
                {
                    dataList.Add(header);
                    foreach (var value in chartValues)
                    {
                        dataList.Add(value.ToString());
                    }
                    return dataList;
                }
            }
            return null;
        }

        internal static List<string> BuildStrig(List<string> values, string header)
        {
            if (values != null && header != null)
            {
                List<string> dataList = new List<string>();
                if (values != null)
                {
                    dataList.Add(header);
                    foreach (var value in values)
                    {
                        dataList.Add(value.ToString());
                    }
                    return dataList;
                }
            }
            return null;
        }

        internal static dynamic GetTestObject(TestType testType, ICeritificateInformation testData)
        {
            switch (testType)
            {
                case TestType.StrokeTest:
                    return testData as StrokeTestInformation;
                case TestType.HoldMidPositionTest:
                    return testData as Hold_MidPositionLineATestInformation;
                case TestType.HoldMidPositionLineBTest:
                    return testData as Hold_MidPositionLineBTestInformation;
                case TestType.SlipStickTest:
                    return testData as Slip_StickTestInformation;
                case TestType.PumpTest:
                    return testData as PumpTestInformation;

            }
            return null;
        }

        internal static ObservableCollection<LineSeries> GetSeriesCollection(string fileName, int numberofSeries, TestType testType)
        {
            ObservableCollection<LineSeries> seriesCollection = new ObservableCollection<LineSeries>();
            Brush[] graphStrokes = new Brush[] { Brushes.DarkOrange, Brushes.DarkGreen, Brushes.DarkBlue, Brushes.Brown };

            List<string> seriesNames = null;


            int nameIndex = 0;
            bool isDataLineFound = false;
            if (TestType.StrokeTest == testType)
            {
                seriesNames = new List<string>() { "Flow", "Stroke Length", "Pressure LineA", "Pressure LineB" };
            }
            else if (testType == TestType.HoldMidPositionTest)
            {
                seriesNames = new List<string>() { "HoldingPressure LineA", "Cylinder Movement"/*, "HoldingPressure LineB"*/ };
            }
            else if (testType == TestType.HoldMidPositionLineBTest)
            {
                seriesNames = new List<string>() { "HoldingPressure LineB", "Cylinder Movement"/*, "HoldingPressure LineA"*/ };
            }
            else if (testType == TestType.SlipStickTest)
            {
                seriesNames = new List<string>() { "Pressure" };
            }

            if (File.Exists(fileName))
            {
                using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {

                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            string[] data = line.Split(',');
                            if (data[0] == "" && (!isDataLineFound))
                                isDataLineFound = true;
                            if (data.Count() == (numberofSeries * 2) && isDataLineFound)
                            {
                                bool isValid = false;
                                if (numberofSeries == 4)
                                {
                                    isValid = data[0] != "" && data[2] != "" && data[4] != "" && data[6] != "";
                                }
                                if (numberofSeries == 3)
                                {
                                    isValid = data[0] != "" && data[2] != "" && data[4] != "";
                                }
                                else if (numberofSeries == 2)
                                {
                                    isValid = data[0] != "" && data[2] != "";
                                }
                                else if (numberofSeries == 1)
                                {
                                    isValid = data[0] != "";
                                }
                                if (isValid)// formatConfigData[2] != "" && formatConfigData[3] != "" && formatConfigData[4] != "" && formatConfigData[5] != "")
                                {
                                    //if (seriesCollection.Count == 0)
                                    // line = reader.ReadLine();
                                    //string[] formatConfigData = line.Split(',');
                                    for (int i = 1; i < data.Length; i = i + 2)
                                    {
                                        if (seriesCollection.Count >= numberofSeries)
                                        {
                                            seriesCollection[((i - 1) / 2)].Values.Add(double.Parse(data[i]));
                                        }
                                        else
                                        {
                                            LineSeries series = new LineSeries() { Values = new ChartValues<double>(), Stroke = graphStrokes[nameIndex], Title = seriesNames[nameIndex], PointGeometrySize = 5, StrokeThickness = 1 };
                                            series.Values.Add(double.Parse(data[i]));
                                            seriesCollection.Add(series);
                                            nameIndex++;
                                        }
                                    }
                                }

                            }
                        }
                    }
                    return seriesCollection;
                }
            }
            return null;
        }

        internal static object GetTestInformation(string fileName, TestType testType)
        {
            dynamic testInormation = TestTypeFactory.CreateTestInformationObject(testType);

            if (File.Exists(fileName))
            {
                using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            if (line != "")
                            {
                                if (line.Split(',')[0].ToLower() == "customername")
                                    testInormation.CustomerName = line.Split(',')[1];
                                else if (line.Split(',')[0].ToLower() == "jobnumber")
                                    testInormation.JobNumber = line.Split(',')[1].Substring(1);
                                else if (line.Split(',')[0].ToLower() == "reportnumber")
                                {
                                    testInormation.ReportNumber = line.Split(',')[1];
                                    testInormation.JobNumber = GetJobNumber(line.Split(',')[1]);
                                }
                                //else if (line.Split(',')[0].ToLower() == "test date")
                                //{
                                //    string[] formatConfigData = fileName.Split('\\');
                                //    string[] filedata = formatConfigData[formatConfigData.Length - 1].Split('_');
                                //    string fname = string.Format("{0}_{1}_{2}", filedata[filedata.Length - 3], filedata[filedata.Length - 2], filedata[filedata.Length - 1].Substring(0, filedata[filedata.Length-1].LastIndexOf('.'))); ///5,6,7
                                //    testInormation.TestDateTime = fname.Replace('_',':');
                                //    //testInormation.TestDateTime = line.Split(',')[1];
                                //}
                                else if (line.Split(',')[0].ToLower() == "rod size")
                                    testInormation.RodSize = uint.Parse(line.Split(',')[1]);
                                else if (line.Split(',')[0].ToLower() == "bore size")
                                    testInormation.BoreSize = uint.Parse(line.Split(',')[1]);
                                else if (line.Split(',')[0].ToLower() == "stroke length")
                                    testInormation.StrokeLength = uint.Parse(line.Split(',')[1]);
                                else if (line.Split(',')[0].ToLower() == "cylindernumber" || line.Split(',')[0].ToLower() == "cylinder number")
                                    testInormation.CylinderNumber = (line.Split(',')[1]).ToString();

                                else if (testType == TestType.StrokeTest)
                                {
                                    if (line.Split(',')[0].ToLower() == "pressurelinea")
                                        testInormation.PressureLineA = (line.Split(',')[1]);
                                    if (line.Split(',')[0].ToLower() == "pressurelineb")
                                        testInormation.PressureLineB = (line.Split(',')[1]);
                                    else if (line.Split(',')[0].ToLower() == "flow")
                                        testInormation.Flow = (line.Split(',')[1]);
                                    else if (line.Split(',')[0].ToLower() == "lineapressureinput")
                                        testInormation.LineAPressureInput = line.Split(',')[1];
                                    else if (line.Split(',')[0].ToLower() == "linebpressureinput")
                                        testInormation.LineBPressureInput = line.Split(',')[1];
                                    else if (line.Split(',')[0].ToLower() == "number of cycles")
                                        testInormation.NoofCycles = double.Parse(line.Split(',')[1]);
                                    else if (line.Split(',')[0].ToLower() == "strokelengthvalue")
                                        testInormation.StrokeLengthValue = (line.Split(',')[1]);
                                    else if (line.Split(',')[0].ToLower() == "comment")
                                        testInormation.Comment = (line.Split(',')[1]);
                                    else if (line.Split(',')[0].ToLower() == "test date")
                                    {
                                        testInormation.TestDateTime = line.Split(',')[1];
                                    }
                                    else if (line.Split(',')[0].ToLower() == "number of cycles completed")
                                    {
                                        StrokeTestWindow.NoofCyclesCompleted = double.Parse(line.Split(',')[1]);
                                        testInormation.NoofCyclesCompleted = double.Parse(StrokeTestWindow.NoofCyclesCompleted.ToString());
                                    }

                                }

                                else if (testType == TestType.SlipStickTest)
                                {
                                    Slip_StickTestInformation SlipStickTestInfo = testInormation as Slip_StickTestInformation;
                                    if (line.Split(',')[0].ToLower() == "flow")
                                        SlipStickTestInfo.Flow = (line.Split(',')[1]);
                                    if (line.Split(',')[0].ToLower() == "pressure")
                                        SlipStickTestInfo.PressureAfterFirstCylinderMovement = (line.Split(',')[1]);
                                    else if (line.Split(',')[0].ToLower() == "initial pressure")
                                        SlipStickTestInfo.InitialPressure = (line.Split(',')[1]);
                                    else if (line.Split(',')[0].ToLower() == "cylinder movement ")
                                        SlipStickTestInfo.CylinderFirstMovement = line.Split(',')[1];
                                    else if (line.Split(',')[0].ToLower() == "initial cylinder movement")
                                        SlipStickTestInfo.InitialCylinderMovement = line.Split(',')[1];
                                    else if (line.Split(',')[0].ToLower() == "test date")
                                    {
                                        testInormation.TestDateTime = line.Split(',')[1];
                                    }

                                }
                                else if (testType == TestType.HoldMidPositionTest)
                                {
                                    Hold_MidPositionLineATestInformation holdmidPositionLineA = testInormation as Hold_MidPositionLineATestInformation;
                                    if (line.Split(',')[0].ToLower() == "allowable pressure drop")
                                        holdmidPositionLineA.AllowablePressureDrop = (line.Split(',')[1]);
                                    if (line.Split(',')[0].ToLower() == "holding pressurelinea")
                                        holdmidPositionLineA.HoldingPressureLineA = (line.Split(',')[1]);
                                    else if (line.Split(',')[0].ToLower() == "holding pressurelineb")
                                        holdmidPositionLineA.HoldingPressureLineB = (line.Split(',')[1]);
                                    else if (line.Split(',')[0].ToLower() == "cylinder movement")
                                        holdmidPositionLineA.CylinderMovement = line.Split(',')[1];
                                    else if (line.Split(',')[0].ToLower() == "holding timelinea")
                                        holdmidPositionLineA.HoldingTimeLineA = line.Split(',')[1];
                                    else if (line.Split(',')[0].ToLower() == "holding line a initial pressure")
                                        holdmidPositionLineA.HoldingLineAInitialPressure = line.Split(',')[1];
                                    else if (line.Split(',')[0].ToLower() == "cylinder movement initial value")
                                        holdmidPositionLineA.InitialCylinderMovement = line.Split(',')[1];
                                    else if (line.Split(',')[0].ToLower() == "test result")
                                        holdmidPositionLineA.TestStatusA = line.Split(',')[1];
                                    else if (line.Split(',')[0].ToLower() == "test date")
                                    {
                                        testInormation.TestDateTime = line.Split(',')[1];
                                    }
                                }

                                else if (TestType.HoldMidPositionLineBTest == testType)
                                {
                                    Hold_MidPositionLineBTestInformation holdmidPositionLineA = testInormation as Hold_MidPositionLineBTestInformation;
                                    if (line.Split(',')[0].ToLower() == "allowable pressure drop")
                                        holdmidPositionLineA.AllowablePressureDrop = (line.Split(',')[1]);
                                    if (line.Split(',')[0].ToLower() == "holding pressurelinea")
                                        holdmidPositionLineA.HoldingPressureLineA = (line.Split(',')[1]);
                                    else if (line.Split(',')[0].ToLower() == "holding pressurelineb")
                                        holdmidPositionLineA.HoldingPressureLineB = (line.Split(',')[1]);
                                    else if (line.Split(',')[0].ToLower() == "cylinder movement")
                                        holdmidPositionLineA.CylinderMovement = line.Split(',')[1];
                                    else if (line.Split(',')[0].ToLower() == "holding timelineb")
                                        holdmidPositionLineA.HoldingTimeLineB = line.Split(',')[1];
                                    else if (line.Split(',')[0].ToLower() == "initial pressure line b")
                                        holdmidPositionLineA.InitialPressureLineB = line.Split(',')[1];
                                    else if (line.Split(',')[0].ToLower() == "initial cylinder movement")
                                        holdmidPositionLineA.InitialCylinderMovement = line.Split(',')[1];
                                    else if (line.Split(',')[0].ToLower() == "test result")
                                        holdmidPositionLineA.TestStatusB = line.Split(',')[1];
                                    else if (line.Split(',')[0].ToLower() == "test date")
                                    {
                                        testInormation.TestDateTime = line.Split(',')[1];
                                    }
                                }

                            }
                        }
                    }
                }

                return testInormation;
            }
            return null;
        }

        internal static object WriteEthernetIPDevice(LibplctagWrapper.Tag tag, Elpis.Windows.OPC.Server.DataType dataType)
        {
            if (SunPowerGenMainPage.DeviceTcpClient != null && SunPowerGenMainPage.DeviceTcpClient.Connected != false)
            {
                Libplctag client = SunPowerGenMainPage.ABEthernetClient;

                if (client != null)
                {
                    client.AddTag(tag);
                    var status = client.GetStatus(tag);

                    if (status != Libplctag.PLCTAG_STATUS_OK)
                    {
                        client.RemoveTag(tag);
                        string error = ($"Error setting up tag internal state.  Error {status}");
                        ElpisServer.Addlogs("Report Tool/WriteTag", "Communication", string.Format("after add Tag Status:{0}", error), LogStatus.Error);
                        return null; ;
                    }

                    try
                    {
                        var rc = client.WriteBool(tag, HomePage.slipStickTestInformation.OffSetValue, true, DataTimeout);


                        if (rc != Libplctag.PLCTAG_STATUS_OK)
                        {
                            client.RemoveTag(tag);
                            string error = ($"ERROR: Unable to write the formatConfigData! Got error code {rc}: {client.DecodeError(rc)}");
                            ElpisServer.Addlogs("Report Tool/WriteTag", "status in write byte with writebool method", string.Format(" Writeboolen Tag error in check status function:{0}", error), LogStatus.Error);
                            ElpisServer.Addlogs("Report tool", "Wrtie tag information with writebool method ", string.Format("Tagname:{0} uniquekey:{1}", tag.Name, tag.UniqueKey), LogStatus.Information);
                            HomePage.slipStickTestInformation.TriggerTestAddress = tag.Name;
                            HomePage.strokeTestInfo.TriggerStatus = "OFF";
                            return null;
                        }
                        else
                        {
                            HomePage.slipStickTestInformation.TriggerTestAddress = tag.Name;
                            HomePage.strokeTestInfo.TriggerStatus = "ON";




                        }
                        client.RemoveTag(tag);
                        ElpisServer.Addlogs("Report Tool/WriteTag", "client is not null", string.Format("Client in writebool method :{0}", client), LogStatus.Information);
                        ElpisServer.Addlogs("Report Tool/WriteTag", "Tag information with writebool method", string.Format("tag name :{0} tag ipaddress:{1} tag cpu:{2} tag element count:{3} tag element size:{4}", tag.Name, tag.IpAddress, tag.Cpu, tag.ElementCount, tag.ElementSize), LogStatus.Information);
                        ElpisServer.Addlogs("Report Tool/WriteTag", "return status information with writebool method", string.Format("add tag status :{0}", status), LogStatus.Information);
                        ElpisServer.Addlogs("Report tool", "Wrtie tag information  with writebool method", string.Format("After Writing tag -Tagname:{0} uniquekey:{1}", tag.Name, tag.UniqueKey), LogStatus.Information);
                        ElpisServer.Addlogs("Report Tool/WriteTag", "return status information with writebool method", string.Format("write tag status :{0}", rc), LogStatus.Information);
                    }
                    catch (Exception e)
                    {
                        ElpisServer.Addlogs("Write", "Writing formatConfigData to plc with writebool method error:{0}", e.Message, LogStatus.Error);
                    }




                }
            }



            return null;
        }

        //jey
        internal static int WriteEthernetIPDevice1(LibplctagWrapper.Tag tag, int offSetValue, bool value)
        {
            int writeStatus = -1;
            if (SunPowerGenMainPage.DeviceTcpClient != null && SunPowerGenMainPage.DeviceTcpClient.Connected != false)
            {
                Libplctag client = SunPowerGenMainPage.ABEthernetClient;

                if (client != null)
                {
                    client.AddTag(tag);
                    var status = client.GetStatus(tag);

                    if (status != Libplctag.PLCTAG_STATUS_OK)
                    {
                        client.RemoveTag(tag);
                        string error = ($"Error setting up tag internal state.  Error {status}");
                        ElpisServer.Addlogs("Report Tool/WriteTag", "Communication", string.Format("after add Tag Status:{0}", error), LogStatus.Error);

                    }

                    try
                    {
                        writeStatus = client.WriteBool(tag, offSetValue, value, DataTimeout);


                        if (writeStatus != Libplctag.PLCTAG_STATUS_OK)
                        {
                            client.RemoveTag(tag);
                            string error = ($"ERROR: Unable to write the formatConfigData! Got error code {writeStatus}: {client.DecodeError(writeStatus)}");
                            ElpisServer.Addlogs("Report Tool/WriteTag", "status in write byte with writebool method", string.Format(" Writeboolen Tag error in check status function:{0}", error), LogStatus.Error);
                            ElpisServer.Addlogs("Report tool", "Wrtie tag information with writebool method ", string.Format("Tagname:{0} uniquekey:{1}", tag.Name, tag.UniqueKey), LogStatus.Information);
                            HomePage.strokeTestInfo.TriggerTestAddress = tag.Name;
                            HomePage.strokeTestInfo.TriggerStatus = "OFF";
                            //return writeStatus;
                        }

                        client.RemoveTag(tag);
                        ElpisServer.Addlogs("Report Tool/WriteTag", "client is not null", string.Format("Client in writebool method :{0}", client), LogStatus.Information);
                        ElpisServer.Addlogs("Report Tool/WriteTag", "Tag information with writebool method", string.Format("tag name :{0} tag ipaddress:{1} tag cpu:{2} tag element count:{3} tag element size:{4}", tag.Name, tag.IpAddress, tag.Cpu, tag.ElementCount, tag.ElementSize), LogStatus.Information);
                        ElpisServer.Addlogs("Report Tool/WriteTag", "return status information with writebool method", string.Format("add tag status :{0}", status), LogStatus.Information);
                        ElpisServer.Addlogs("Report tool", "Wrtie tag information  with writebool method", string.Format("After Writing tag -Tagname:{0} uniquekey:{1}", tag.Name, tag.UniqueKey), LogStatus.Information);
                        ElpisServer.Addlogs("Report Tool/WriteTag", "return status information with writebool method", string.Format("write tag status :{0}", writeStatus), LogStatus.Information);
                    }
                    catch (Exception e)
                    {
                        ElpisServer.Addlogs("Write", "Writing formatConfigData to plc with writebool method error:{0}", e.Message, LogStatus.Error);
                    }




                }
            }



            return writeStatus;
        }


        internal static object WriteEthernetIPDeviceStop(LibplctagWrapper.Tag tag, Elpis.Windows.OPC.Server.DataType dataType)
        {
            if (SunPowerGenMainPage.DeviceTcpClient != null && SunPowerGenMainPage.DeviceTcpClient.Connected != false)
            {
                Libplctag client = SunPowerGenMainPage.ABEthernetClient;

                if (client != null)
                {
                    client.AddTag(tag);
                    var status = client.GetStatus(tag);

                    if (status != Libplctag.PLCTAG_STATUS_OK)
                    {
                        client.RemoveTag(tag);
                        string error = ($"Error setting up tag internal state.  Error {status}");
                        ElpisServer.Addlogs("Report Tool/WriteTag", "Communication", string.Format("after add Tag Status:{0}", error), LogStatus.Error);
                        return null; ;
                    }

                    try
                    {
                        var rc = client.WriteBool(tag, HomePage.slipStickTestInformation.OffSetValue, false, DataTimeout);


                        if (rc != Libplctag.PLCTAG_STATUS_OK)
                        {
                            client.RemoveTag(tag);
                            string error = ($"ERROR: Unable to write the formatConfigData! Got error code {rc}: {client.DecodeError(rc)}");
                            ElpisServer.Addlogs("Report Tool/WriteTag to stop", "status in write byte with writebool method", string.Format(" Writeboolen Tag error in check status function:{0}", error), LogStatus.Error);
                            ElpisServer.Addlogs("Report tool", "Wrtie tag information with writebool method to stop ", string.Format("Tagname:{0} uniquekey:{1}", tag.Name, tag.UniqueKey), LogStatus.Information);
                            HomePage.strokeTestInfo.TriggerTestAddress = tag.Name;

                            ElpisServer.Addlogs("Report tool", "Wrtie tag for stop Status:", "ON", LogStatus.Information);

                            return null;
                        }
                        else
                        {
                            HomePage.strokeTestInfo.TriggerTestAddress = tag.Name;
                            HomePage.strokeTestInfo.TriggerStatus = "OFF";
                            ElpisServer.Addlogs("Report tool", "Wrtie tag for stop Status:", "OFF", LogStatus.Information);




                        }
                        client.RemoveTag(tag);
                        ElpisServer.Addlogs("Report Tool/WriteTag", "client is not null", string.Format("Client in writebool method :{0}", client), LogStatus.Information);
                        ElpisServer.Addlogs("Report Tool/WriteTag", "Tag information with writebool method", string.Format("tag name :{0} tag ipaddress:{1} tag cpu:{2} tag element count:{3} tag element size:{4}", tag.Name, tag.IpAddress, tag.Cpu, tag.ElementCount, tag.ElementSize), LogStatus.Information);
                        ElpisServer.Addlogs("Report Tool/WriteTag", "return status information with writebool method", string.Format("add tag status :{0}", status), LogStatus.Information);
                        ElpisServer.Addlogs("Report tool", "Wrtie tag information  with writebool method", string.Format("After Writing tag -Tagname:{0} uniquekey:{1}", tag.Name, tag.UniqueKey), LogStatus.Information);
                        ElpisServer.Addlogs("Report Tool/WriteTag", "return status information with writebool method", string.Format("write tag status :{0}", rc), LogStatus.Information);
                    }
                    catch (Exception e)
                    {
                        ElpisServer.Addlogs("Write", "Writing formatConfigData to plc with writebool method error:{0}", e.Message, LogStatus.Error);
                    }




                }
            }



            return null;
        }

        internal static object WriteEthernetIntegerIPDevice(LibplctagWrapper.Tag tag, Elpis.Windows.OPC.Server.DataType dataType)
        {
            try
            {
                if (SunPowerGenMainPage.DeviceTcpClient != null && SunPowerGenMainPage.DeviceTcpClient.Connected != false)
                {
                    Libplctag client = SunPowerGenMainPage.ABEthernetClient;

                    if (client != null)
                    {
                        client.AddTag(tag);
                        var status = client.GetStatus(tag);

                        if (status != Libplctag.PLCTAG_STATUS_OK)
                        {
                            client.RemoveTag(tag);
                            string error = ($"Error setting up tag internal state.  Error {status}");
                            ElpisServer.Addlogs("Report Tool/WriteTag", "Communication", string.Format("after add Tag Status:{0}", error), LogStatus.Error);
                            return null; ;
                        }
                        client.SetInt16Value(tag, HomePage.slipStickTestInformation.OffSetValue, 1);
                        var rc = client.WriteTag(tag, DataTimeout);

                        if (rc != Libplctag.PLCTAG_STATUS_OK)
                        {
                            client.RemoveTag(tag);
                            string error = ($"ERROR: Unable to write the formatConfigData! Got error code {rc}: {client.DecodeError(rc)}");
                            ElpisServer.Addlogs("Report Tool/WriteTag", "status in write byte with setint method", string.Format(" Writeboolen Tag error in check status function:{0}", error), LogStatus.Error);
                            ElpisServer.Addlogs("Report tool", "Wrtie tag information with setint method ", string.Format("Tagname:{0} uniquekey:{1}", tag.Name, tag.UniqueKey), LogStatus.Information);
                            HomePage.strokeTestInfo.TriggerTestAddress = tag.Name;
                            HomePage.strokeTestInfo.TriggerStatus = "OFF";
                            return null;
                        }
                        else
                        {
                            HomePage.strokeTestInfo.TriggerTestAddress = tag.Name;
                            HomePage.strokeTestInfo.TriggerStatus = "ON";




                        }
                        client.RemoveTag(tag);
                        ElpisServer.Addlogs("Report Tool/WriteTag", "client is not null", string.Format("Client in setint methos :{0}", client), LogStatus.Information);
                        ElpisServer.Addlogs("Report Tool/WriteTag", "Tag information with SETINT method", string.Format("tag name :{0} tag ipaddress:{1} tag cpu:{2} tag element count:{3} tag element size:{4}", tag.Name, tag.IpAddress, tag.Cpu, tag.ElementCount, tag.ElementSize), LogStatus.Information);
                        ElpisServer.Addlogs("Report Tool/WriteTag", "return status information with SETINT method", string.Format("add tag status :{0}", status), LogStatus.Information);
                        ElpisServer.Addlogs("Report tool", "Wrtie tag information  with SETINT method", string.Format("After Writing tag -Tagname:{0} uniquekey:{1}", tag.Name, tag.UniqueKey), LogStatus.Information);
                        ElpisServer.Addlogs("Report Tool/WriteTag", "return status information with SETINT method", string.Format("write tag status :{0}", rc), LogStatus.Information);
                    }

                }
            }
            catch (Exception e)
            {
                ElpisServer.Addlogs("Write", "Writing formatConfigData to plc with writetag and setint16value method error:{0}", e.Message, LogStatus.Error);
            }
            return null;
        }
        private static string GetJobNumber(string reportNumber)
        {
            string[] ReportSplit = reportNumber.Split('_');
            if (ReportSplit[2] != null)
                return ReportSplit[2];
            else
                return "InvalidData";
        }

        internal static ObservableCollection<List<string>> GetLabelCollection(string fileName, int numberofLabels)
        {
            ObservableCollection<List<string>> labelCollection = new ObservableCollection<List<string>>();
            bool isDataLineFound = false;
            if (File.Exists(fileName))
            {
                using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            if (line.Split(',')[0] == "" && (!isDataLineFound))
                                isDataLineFound = true;
                            if (line.Split(',').Count() == (numberofLabels * 2) && isDataLineFound)
                            {
                                if (labelCollection.Count == 0)
                                    line = reader.ReadLine();
                                string[] data = line.Split(',');
                                bool isValid = false;
                                if (numberofLabels == 4)
                                {
                                    isValid = data[1] != "" && data[3] != "" && data[5] != "" && data[7] != "";
                                }
                                if (numberofLabels == 3)
                                {
                                    isValid = data[1] != "" && data[3] != "" && data[5] != "";
                                }
                                else if (numberofLabels == 2)
                                {
                                    isValid = data[1] != "" && data[3] != "";
                                }
                                else if (numberofLabels == 1)
                                {
                                    isValid = data[1] != "";
                                }
                                if (isValid) // (formatConfigData[2] != "" && formatConfigData[3] != "" && formatConfigData[4] != "" && formatConfigData[5] != "")
                                {
                                    for (int i = 0; i < data.Length; i = i + 2)
                                    {
                                        int count = Regex.Matches(data[i], @"[a-zA-Z]").Count;
                                        if (count == 0)
                                        {
                                            if (labelCollection.Count >= numberofLabels)
                                            {
                                                labelCollection[((i) / 2)].Add(data[i]);
                                            }
                                            else
                                            {
                                                List<string> labels = new List<string>();
                                                labels.Add(data[i]);
                                                labelCollection.Add(labels);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return labelCollection;
                }
            }
            return null;
        }

        internal static string ReadEthernetIPDevice(LibplctagWrapper.Tag tag, Elpis.Windows.OPC.Server.DataType dataType)  //string ipAddress, string tagAddress, Elpis.Windows.OPC.Server.DataType dataType
        {
            try
            {
                if (SunPowerGenMainPage.DeviceTcpClient != null && SunPowerGenMainPage.DeviceTcpClient.Connected != false)
                {
                    Libplctag client = SunPowerGenMainPage.ABEthernetClient;
                    #region old
                    //LibplctagWrapper.Tag tag = null;
                    //switch (dataType)
                    //{
                    //    case Elpis.Windows.OPC.Server.DataType.Boolean:
                    //        tag = new LibplctagWrapper.Tag(ipAddress, CpuType.SLC, tagAddress, 2, 1);
                    //        break;
                    //    case Elpis.Windows.OPC.Server.DataType.Double:
                    //        tag = new LibplctagWrapper.Tag(ipAddress, CpuType.SLC, tagAddress, 4, 1);
                    //        break;
                    //    case Elpis.Windows.OPC.Server.DataType.Integer:
                    //        tag = new LibplctagWrapper.Tag(ipAddress, CpuType.SLC, tagAddress, 2, 1);
                    //        break;
                    //    case Elpis.Windows.OPC.Server.DataType.Short:
                    //        tag = new LibplctagWrapper.Tag(ipAddress, CpuType.SLC,tagAddress, 1, 1);
                    //        break;
                    //    case Elpis.Windows.OPC.Server.DataType.String:
                    //        tag = new LibplctagWrapper.Tag(ipAddress, CpuType.SLC, tagAddress, 88, 1);
                    //        break;
                    //    default:
                    //        break;
                    //}
                    #endregion
                    if (client != null)
                    {
                        client.AddTag(tag);

                        var status = client.GetStatus(tag);
                        if (status != Libplctag.PLCTAG_STATUS_OK)
                        {
                            client.RemoveTag(tag);
                            string error = ($"Error setting up tag internal state.  Error {status}");
                            ElpisServer.Addlogs("Report Tool/ReadTag", "Communication", string.Format("Not able to get Tag Status:{0}", error), LogStatus.Error);
                            return null; ;
                        }

                        /* get the formatConfigData */
                        var rc = client.ReadTag(tag, DataTimeout);

                        if (rc != Libplctag.PLCTAG_STATUS_OK)
                        {
                            client.RemoveTag(tag);
                            string error = ($"ERROR: Unable to read the formatConfigData! Got error code {rc}: {client.DecodeError(rc)}");
                            ElpisServer.Addlogs("Report Tool/ReadTag", "Communication", string.Format("Not able to Read Tag:{0}", error), LogStatus.Error);
                            return null;
                        }

                        /* print out the formatConfigData */
                        for (int i = 0; i < tag.ElementCount; i++)
                        {
                            string data = null;
                            switch (dataType)
                            {
                                case Elpis.Windows.OPC.Server.DataType.Boolean:
                                    data = ($"{client.GetInt8Value(tag, (i * tag.ElementSize))}");
                                    break;
                                case Elpis.Windows.OPC.Server.DataType.Double:
                                    data = ($"{client.GetUint32Value(tag, (i * tag.ElementSize))}");
                                    break;
                                case Elpis.Windows.OPC.Server.DataType.Float:
                                    data = ($"{client.GetFloat32Value(tag, (i * tag.ElementSize))}");
                                    break;
                                case Elpis.Windows.OPC.Server.DataType.Integer:
                                    data = ($"{client.GetUint16Value(tag, (i * tag.ElementSize))}");
                                    break;
                                case Elpis.Windows.OPC.Server.DataType.Short:
                                    data = ($"{client.GetUint8Value(tag, (i * tag.ElementSize))}");
                                    break;
                                case Elpis.Windows.OPC.Server.DataType.String:
                                    data = ($"{client.GetUint32Value(tag, (i * tag.ElementSize))}");
                                    break;

                                default:
                                    break;
                            }
                            client.RemoveTag(tag);
                            ElpisServer.Addlogs("Report Tool/ReadTag", "Communication", string.Format("Tag Address:{0}  Tag Value:{1}", tag.Name, data), LogStatus.Information);
                            return data;
                        }
                    }
                    else
                    {
                        ElpisServer.Addlogs("Report Tool/ReadTag", "Communication", string.Format("LibPlcTag Client is Null"), LogStatus.Error);
                        return null;
                    }

                }
                else
                {
                    ElpisServer.Addlogs("Report Tool/ReadTag", "Communication", string.Format("Tcp Client is Null"), LogStatus.Error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                ElpisServer.Addlogs("Report Tool/ReadTag", "Communication", string.Format("Problem occurred in Tag Read.:{0}", ex.Message), LogStatus.Error);
            }
            return null;

        }


        /// <summary>
        /// Browse the CSV file from the machine
        /// </summary>
        /// <returns>file path </returns>
        internal static string BrowseFile(string folderPath = null)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            //dialog.DefaultExt = ".der";
            dialog.Filter = "Text File|*.csv|All Files|*.*";
            dialog.Multiselect = false;
            dialog.ValidateNames = true;
            dialog.Title = "Select Data File";
            dialog.FileName = null;
            dialog.RestoreDirectory = true;
            dialog.InitialDirectory = folderPath;
            if (!dialog.ShowDialog().Value)
            {
                return null;
            }
            MessageBoxResult result = MessageBox.Show(string.Format("Do you want to load formatConfigData from the following path?\n\n{0}", dialog.FileName), "SPG Reporting Tool", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.Yes)
            {
                return dialog.FileName;
            }
            return null;
        }


        internal static string ReadDeviceHoldingRegisterValue(ushort tagAddress, Elpis.Windows.OPC.Server.DataType dataType, DeviceType deviceType, int startPosition, ushort slaveId)
        {
            dynamic master = null;
            if (deviceType == DeviceType.ModbusEthernet)
                master = SunPowerGenMainPage.ModbusTcpMaster;
            else if (deviceType == DeviceType.ModbusSerial)
                master = SunPowerGenMainPage.ModbusSerialPortMaster;
            string dataRead = string.Empty;
            ushort tagAddress1 = ushort.Parse(tagAddress.ToString().Substring(startPosition));
            if (master != null)
            {
                switch (dataType)
                {
                    case Elpis.Windows.OPC.Server.DataType.Short:
                        if (deviceType == DeviceType.ModbusEthernet)
                            dataRead = master.ReadHoldingRegisters(tagAddress1, 1)[0].ToString();
                        else if (deviceType == DeviceType.ModbusSerial)
                            dataRead = master.ReadHoldingRegisters(slaveId, tagAddress1, 1)[0].ToString();
                        break;
                    case Elpis.Windows.OPC.Server.DataType.Integer:
                        ushort[] returnReadHoldingRegisters = null;
                        if (deviceType == DeviceType.ModbusEthernet)
                            returnReadHoldingRegisters = master.ReadHoldingRegisters(tagAddress1, 2);
                        else if (deviceType == DeviceType.ModbusSerial)
                            returnReadHoldingRegisters = master.ReadHoldingRegisters(slaveId, tagAddress1, 2);
                        int result = ValueConverter.GetInt32(returnReadHoldingRegisters[1], returnReadHoldingRegisters[0]);
                        dataRead = result.ToString();
                        break;

                }
            }
            ElpisServer.Addlogs("All", "SPG Reporting Tool", string.Format("Address:{0} and Value:{1}", tagAddress1, dataRead), LogStatus.Information);
            //return modbusTcpMaster.ReadHoldingRegisters(tagAddress, 1)[0].ToString();
            return dataRead;
        }


        internal static string ReadDeviceCoilsRegisterValue(ushort tagAddress, DeviceType deviceType, ushort slaveId)
        {
            dynamic master = null;
            if (deviceType == DeviceType.ModbusEthernet)
                master = SunPowerGenMainPage.ModbusTcpMaster;
            else if (deviceType == DeviceType.ModbusSerial)
                master = SunPowerGenMainPage.ModbusSerialPortMaster;
            string dataRead = string.Empty;
            ushort tagAddress1 = ushort.Parse(tagAddress.ToString().Substring(1));
            if (master != null)
            {

                if (deviceType == DeviceType.ModbusEthernet)
                    dataRead = master.ReadCoils(tagAddress1, 1)[0].ToString();
                else if (deviceType == DeviceType.ModbusSerial)
                    dataRead = master.ReadCoils(slaveId, tagAddress1, 1)[0].ToString();
                // return modbusTcpMaster.ReadInputRegisters(tagAddress, 1)[0].ToString();
            }
            return dataRead;
        }


        internal static string ReadDeviceDiscreteInputRegisterValue(ushort tagAddress, DeviceType deviceType, ushort slaveId)
        {
            dynamic master = null;
            if (deviceType == DeviceType.ModbusEthernet)
                master = SunPowerGenMainPage.ModbusTcpMaster;
            else if (deviceType == DeviceType.ModbusSerial)
                master = SunPowerGenMainPage.ModbusSerialPortMaster;
            string dataRead = string.Empty;
            ushort tagAddress1 = ushort.Parse(tagAddress.ToString().Substring(1));
            if (master != null)
            {
                if (deviceType == DeviceType.ModbusEthernet)
                    dataRead = master.ReadInputs(tagAddress1, 1)[0].ToString();
                else if (deviceType == DeviceType.ModbusSerial)
                    dataRead = master.ReadInputs(slaveId, tagAddress1, 1)[0].ToString();
                // return modbusTcpMaster.ReadInputRegisters(tagAddress, 1)[0].ToString();
            }
            return dataRead;
        }

        internal static string ReadDeviceInputRegisterValue(ushort tagAddress, Elpis.Windows.OPC.Server.DataType dataType, DeviceType deviceType, int startPosition, ushort slaveId)
        {
            dynamic master = null;
            if (deviceType == DeviceType.ModbusEthernet)
                master = SunPowerGenMainPage.ModbusTcpMaster;
            else if (deviceType == DeviceType.ModbusSerial)
                master = SunPowerGenMainPage.ModbusSerialPortMaster;
            string dataRead = string.Empty;
            ushort tagAddress1 = ushort.Parse(tagAddress.ToString().Substring(startPosition));
            try
            {
                if (master != null)
                {
                    switch (dataType)
                    {
                        case Elpis.Windows.OPC.Server.DataType.Short:
                            if (deviceType == DeviceType.ModbusEthernet)
                                dataRead = master.ReadInputRegisters(tagAddress1, 1)[0].ToString();
                            else if (deviceType == DeviceType.ModbusSerial)
                                dataRead = master.ReadInputRegisters(slaveId, tagAddress1, 1)[0].ToString();
                            break;
                        case Elpis.Windows.OPC.Server.DataType.Integer:
                            ushort[] returnReadHoldingRegisters = null;
                            if (deviceType == DeviceType.ModbusEthernet)
                                returnReadHoldingRegisters = master.ReadInputRegisters(tagAddress1, 2);
                            else if (deviceType == DeviceType.ModbusSerial)
                                returnReadHoldingRegisters = master.ReadInputRegisters(slaveId, tagAddress1, 2);
                            int result = ValueConverter.GetInt32(returnReadHoldingRegisters[1], returnReadHoldingRegisters[0]);
                            dataRead = result.ToString();
                            break;
                    }
                }
                // return modbusTcpMaster.ReadInputRegisters(tagAddress, 1)[0].ToString();
                ElpisServer.Addlogs("All", "SPG Reporting Tool", string.Format("Address:{0} and Value:{1}", tagAddress1, dataRead), LogStatus.Information);
            }
            catch (Exception ex)
            {
                ElpisServer.Addlogs("All", "SPG Reporting Tool", string.Format("Not able to read formatConfigData from Device.{0}", ex.Message), LogStatus.Information);
            }
            return dataRead;
        }



        internal static CpuType GetABDeviceModelCPUType(AllenBbadleyModel deviceModelType)
        {
            CpuType cpuType = CpuType.SLC;
            switch (deviceModelType)
            {
                case AllenBbadleyModel.ControlLogix:
                case AllenBbadleyModel.CompactLogix:
                    cpuType = CpuType.LGX;
                    break;
                case AllenBbadleyModel.MicroLogix:
                    cpuType = CpuType.SLC;
                    break;
                case AllenBbadleyModel.Micro800:
                    cpuType = CpuType.Micro800;
                    break;
                case AllenBbadleyModel.PLC5:
                    cpuType = CpuType.PLC5;
                    break;
                default:
                    break;
            }

            return cpuType;
        }


        internal static int GetElementSize(Elpis.Windows.OPC.Server.DataType dataType)
        {

            if (dataType == Elpis.Windows.OPC.Server.DataType.Boolean)
                return 2;
            else if (dataType == Elpis.Windows.OPC.Server.DataType.Short)
                return 1;
            else if (dataType == Elpis.Windows.OPC.Server.DataType.Integer)
                return 2;
            else if (dataType == Elpis.Windows.OPC.Server.DataType.Double || dataType == Elpis.Windows.OPC.Server.DataType.Float)
                return 4;
            else if (dataType == Elpis.Windows.OPC.Server.DataType.String)
                return 88;

            return 4;
        }


        public static bool ConnectingDevice(bool isConnected, ObservableCollection<Elpis.Windows.OPC.Server.Tag> tagsCollection)
        {
            if (SunPowerGenMainPage.DeviceObject != null)
            {
                DeviceType deviceType = SunPowerGenMainPage.DeviceObject.DeviceType;
                if (deviceType == DeviceType.ModbusEthernet)
                {
                    SunPowerGenMainPage.DeviceTcpClient = Helper.CreateTcpClient(((ModbusEthernetDevice)SunPowerGenMainPage.DeviceObject).IPAddress, ((ModbusEthernetDevice)SunPowerGenMainPage.DeviceObject).Port);
                    if (SunPowerGenMainPage.DeviceTcpClient != null)
                    {
                        SunPowerGenMainPage.ModbusTcpMaster = Helper.CreateModbusMaster<ModbusIpMaster>(SunPowerGenMainPage.DeviceObject.DeviceType);
                        SunPowerGenMainPage.ModbusTcpMaster.Transport.ReadTimeout = 2000;
                        isConnected = true;
                    }
                }
                else if (deviceType == DeviceType.ModbusSerial)
                {
                    Helper.CreateSerialPort();
                    SunPowerGenMainPage.ModbusSerialPortMaster = Helper.CreateModbusMaster<ModbusSerialMaster>(SunPowerGenMainPage.DeviceObject.DeviceType);
                    slaveId = ((ModbusSerialDevice)SunPowerGenMainPage.DeviceObject).SlaveId;
                    SunPowerGenMainPage.DeviceSerialPort.ReadTimeout = 500;
                    string data = SunPowerGenMainPage.ModbusSerialPortMaster.ReadHoldingRegisters(slaveId, ushort.Parse(tagsCollection[0].Address), 1)[0].ToString();
                    isConnected = true;
                }
                else if (deviceType == DeviceType.ABMicroLogixEthernet)
                {
                    SunPowerGenMainPage.DeviceTcpClient = Helper.CreateTcpClient(((ABMicrologixEthernetDevice)SunPowerGenMainPage.DeviceObject).IPAddress, ((ABMicrologixEthernetDevice)SunPowerGenMainPage.DeviceObject).Port);
                    if (SunPowerGenMainPage.DeviceTcpClient != null && SunPowerGenMainPage.DeviceTcpClient.Connected)
                    {
                        //if list having the previous test formatConfigData, clear it.
                        if (MappedTagList != null)
                            MappedTagList.Clear();
                        //If list is null it creates a new list.
                        else
                            MappedTagList = new Dictionary<string, Tuple<LibplctagWrapper.Tag, Elpis.Windows.OPC.Server.DataType>>();
                        CreateMappedTagList(tagsCollection);
                        if (SunPowerGenMainPage.ABEthernetClient != null)
                            SunPowerGenMainPage.ABEthernetClient.Dispose();
                        SunPowerGenMainPage.ABEthernetClient = new LibplctagWrapper.Libplctag();
                        isConnected = true;
                    }
                }
                #region TcpSocketServer and connection check
                else if (deviceType == DeviceType.TcpSocketDevice)
                {
                    /*bool cli = tcpserver.ServerStart(((TcpSocketDevice)SunPowerGenMainPage.DeviceObject).IPAddress, ((TcpSocketDevice)SunPowerGenMainPage.DeviceObject).Port);*/
                    SunPowerGenMainPage.DeviceTcpClient = TcpServer.Start(((TcpSocketDevice)SunPowerGenMainPage.DeviceObject).IPAddress, ((TcpSocketDevice)SunPowerGenMainPage.DeviceObject).Port);
                    if (SunPowerGenMainPage.DeviceTcpClient != null && SunPowerGenMainPage.DeviceTcpClient.Connected)
                    {
                        //Thread clientThread = new Thread(() => TcpServer.HandleClient(SunPowerGenMainPage.DeviceTcpClient));
                        //clientThread.Start();
                        isConnected = true;
                    }
                    //if (cli == true)
                    //{
                    //    isConnected = true;
                    //}
                    //else
                    //{

                    //}

                }
                #endregion TcpSocketServer and connection check
                #region tcpsocketclient-device 
                // Change the client code as server code ny application work like server so these line commented by sathish
                //else if (deviceType == DeviceType.TcpSocketDevice)
                //{
                //    SunPowerGenMainPage.DeviceTcpSocketClient = Helper.CreateTcpClient(((TcpSocketDevice)SunPowerGenMainPage.DeviceObject).IPAddress, ((TcpSocketDevice)SunPowerGenMainPage.DeviceObject).Port);
                //    if (SunPowerGenMainPage.DeviceTcpSocketClient != null && SunPowerGenMainPage.DeviceTcpSocketClient.Connected)
                //    {
                //        ///// Commented by sathish
                //        ////if list having the previous test formatConfigData, clear it.
                //        //if (MappedTagList != null)
                //        //    MappedTagList.Clear();
                //        ////If list is null it creates a new list.
                //        //else
                //        //    MappedTagList = new Dictionary<string, Tuple<LibplctagWrapper.Tag, Elpis.Windows.OPC.Server.DataType>>();
                //        //CreateMappedTagList(tagsCollection);
                //        //if (SunPowerGenMainPage.TcpSocketClient != null)
                //        //    SunPowerGenMainPage.TcpSocketClient.Dispose();
                //        isConnected = true;
                //    }
                //    else
                //    {

                //    }
                //}
                #endregion tcpsocketclient-device
            }

            return isConnected;
        }

        private static void CreateMappedTagList(ObservableCollection<Elpis.Windows.OPC.Server.Tag> tagsCollection)
        {
            try
            {
                ABMicrologixEthernetDevice abDevice = SunPowerGenMainPage.DeviceObject as ABMicrologixEthernetDevice;
                CpuType cpuType = Helper.GetABDeviceModelCPUType(abDevice.DeviceModelType);
                foreach (var tag in tagsCollection)
                {
                    string key = tag.TagName;
                    int elementSize = GetElementSize(tag.DataType);
                    //if (tag.TagName == "PLCTrigger")
                    //{
                    //    LibplctagWrapper.Tag eipTag = new LibplctagWrapper.Tag(abDevice.IPAddress, (LibplctagWrapper.CpuType)cpuType, tag.Address, elementSize, 1,1);
                    //    Tuple<LibplctagWrapper.Tag, Elpis.Windows.OPC.Server.DataType> value = new Tuple<LibplctagWrapper.Tag, Elpis.Windows.OPC.Server.DataType>(eipTag, tag.DataType);
                    //    MappedTagList.Add(key, value);
                    //}
                    //else
                    //{
                    LibplctagWrapper.Tag eipTag = new LibplctagWrapper.Tag(abDevice.IPAddress, (LibplctagWrapper.CpuType)cpuType, tag.Address, elementSize, 1, 0);
                    Tuple<LibplctagWrapper.Tag, Elpis.Windows.OPC.Server.DataType> value = new Tuple<LibplctagWrapper.Tag, Elpis.Windows.OPC.Server.DataType>(eipTag, tag.DataType);
                    MappedTagList.Add(key, value);
                    //}
                }
                //SunPowerGenMainPage.EIPTags = eipTagsCollection;
            }
            catch (Exception e)
            {
                ElpisServer.Addlogs("All", "SPG Reporting Tool", string.Format("Error in creating MappedTagList.{0}.", e.Message), LogStatus.Information);
            }
        }

        public static object TriggerPLC_not_used(LibplctagWrapper.Tag tag, string tagName, string plcTriggerTagAddress, int offSetValue, bool value)
        {
            try
            {
                #region ABMicroLogicxEthernet
                if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ABMicroLogixEthernet)
                {
                    if (plcTriggerTagAddress != null)
                    {
                        //ABMicrologixEthernetDevice abDevice = SunPowerGenMainPage.DeviceObject as ABMicrologixEthernetDevice;
                        //CpuType cpuType = Helper.GetABDeviceModelCPUType(abDevice.DeviceModelType);
                        var a = Helper.WriteEthernetIPDevice1(tag, offSetValue, value);
                        ElpisServer.Addlogs("Report Tool", "Trigger PLC Helper", string.Format("tag details:{0} tag datatype:{1}", tag.UniqueKey, tag.Name), LogStatus.Information);
                        return a;
                    }
                }
                #endregion

                #region ModBus Ethernet
                else if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ModbusEthernet)
                {
                    throw new NotImplementedException();
                }
                #endregion

                #region ModBus Serial
                else if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ModbusSerial)
                {
                    throw new NotImplementedException();
                }
                #endregion

            }
            catch (Exception e)
            {
                ElpisServer.Addlogs("Report tool", "Plc trigger in stroke test", e.Message, LogStatus.Warning);

                //StopTest();
            }
            return null;
        }
    }
}

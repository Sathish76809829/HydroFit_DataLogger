#region Namespaces

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
//using Microsoft.VisualBasic; //TODO: Add variant type enum to DataType enum
using Modbus.Device;
using System.Windows.Threading;
using System.IO.Ports;
using System.Diagnostics;
using NDI.SLIKDA.Interop;
using OPCEngine.Connectors.Allen_Bradley;

#endregion Namespaces

#region OPCEngine namespace

namespace Elpis.Windows.OPC.Server
{
    #region ElpisServer class
    /// <summary>
    /// All Server & UI related stuffs done here
    /// </summary>
    public class ElpisServer
    {
        #region Properties
        public ObservableCollection<IConnector> ConnectorCollection { get; set; }
        public ObservableCollection<IConnector> ConnectorCollectionForCommunication { get; set; } //TODO: Analyze it's Required...
        public Dictionary<ISLIKTag, int> TagDictionary { get; set; }

        public ISLIKTags OpcTags { get; set; } //TODO: --Done Rename to OpcTags
        public TcpClient TcpClient { get; set; }

        public SerialPort port { get; set; }

        public SLIKServer SlikServerObject { get; set; }

        AccessPermissionsEnum ReadWriteAccess = AccessPermissionsEnum.sdaReadAccess | AccessPermissionsEnum.sdaWriteAccess;
        public FileHandler FileHandle { get; set; }
        public string RunTimeDisplay { get; set; }
        public int TagCount { get; set; }  //TODO: --Done Rename to  TagCount        
        public string NewDeviceName { get; set; }
        CancellationTokenSource tokenSource = null;

        public ConnectionHelper ConnectionHelperObj { get; set; }
        public Dictionary<string, TcpClient> TcpClientCollection { get; set; }

        //31-Aug-17
        public Dictionary<string, ConnectionHelper> ConnectionCollection { get; set; }
        public static string LogFilePath { get; set; }
        public AutoDemotion autoDemotion { get; set; }
        public static MQTT mqttObj { get; set; }
        public static AzureIoTHub AzureIoTHubObj { get; set; }//TODO: Separate Class for a mqqt/Azure
        //public static IoTHub IotHubObj { get; set; }
        public ObservableCollection<MQTT> MqttClientCollection { get; set; }
        public ObservableCollection<AzureIoTHub> AzureIoTCollection { get; set; }
        public static ObservableCollection<LoggerViewModel> LoggerCollection { get; set; }
        public ObservableCollection<LoggerViewModel> ConfigurationLogCollection { get; set; }
        public ObservableCollection<LoggerViewModel> UAConfigurationLogCollection { get; set; }
        public ObservableCollection<LoggerViewModel> UACertificateLogCollection { get; set; }
        public ObservableCollection<LoggerViewModel> IoTLogCollection { get; set; }
        public ThreadHelper threadHelper { get; set; }
        public string OpenedProjectFilePath { get; set; }
        public string CurrentProjectFilePath { get; set; }
        public Dictionary<string, List<ISLIKTag>> ScanrateGroup { get; set; }
        public Dictionary<string, TcpClient> ScanrateClientGroup { get; set; }

        public int retryCount { get; set; }//TODO: --Done  get max number of times retry to connect device. 

        public static bool isDemoExpired = false;
        public Dictionary<string, Dictionary<ISLIKTag, Tag>> ListofMappedtag { get; set; }

        public DispatcherTimer dispacher { get; set; }
        #endregion Properties

        #region Constructor

        public ElpisServer()
        {

            OpcTags = null;
#if !SunPowerGen
            try
            {
                if (SlikServerObject == null)
                    SlikServerObject = SLIKDAHelper.SlikdaObject;

                SlikServerObject.OnClientConnect += SlikServer_OnClientConnect;
                SlikServerObject.OnClientDisconnect += SlikServer_OnClientDisconnect;
                SlikServerObject.OnWrite += SlikServer_OnWrite;
                SlikServerObject.OnRead += SlikServer_OnRead;
                SlikServerObject.OnUpdate += SlikServerObject_OnUpdate;
                OpcTags = SlikServerObject.SLIKTags;
            }
            catch(Exception)
            {

            }

#endif
            ConnectorCollection = new ObservableCollection<IConnector>();
            ConnectorCollectionForCommunication = new ObservableCollection<IConnector>();
            TagDictionary = new Dictionary<ISLIKTag, int>();

            ConnectionHelperObj = new ConnectionHelper();
            TcpClientCollection = new Dictionary<string, TcpClient>();

            autoDemotion = new AutoDemotion();
            autoDemotion.demotionPeriod.Start();
            // TimerCount = 0;

            // mqttObj = new MQTT();
            //AzureIoTHubObj = new AzureIoTHub();
            MqttClientCollection = new ObservableCollection<MQTT>();
            AzureIoTCollection = new ObservableCollection<AzureIoTHub>();
            //for only log
            LoggerCollection = new ObservableCollection<LoggerViewModel>();
            ConfigurationLogCollection = new ObservableCollection<LoggerViewModel>();
            UAConfigurationLogCollection = new ObservableCollection<LoggerViewModel>();
            UACertificateLogCollection = new ObservableCollection<LoggerViewModel>();
            IoTLogCollection = new ObservableCollection<LoggerViewModel>();
            threadHelper = new ThreadHelper();
            ConnectionCollection = new Dictionary<string, ConnectionHelper>();
            OpenedProjectFilePath = string.Format("{0}\\opcproject.elp", Directory.GetCurrentDirectory());
            ScanrateGroup = new Dictionary<string, List<ISLIKTag>>();
            ScanrateClientGroup = new Dictionary<string, TcpClient>();
            //retryCount = 3;
            TagCount = 0;

            ListofMappedtag = new Dictionary<string, Dictionary<ISLIKTag, Tag>>();

            dispacher = new DispatcherTimer();
            dispacher.Tick += Dispacher_Tick;
            //SLIKDA Registration and starting the server at this moment
            //SlikServerObject.RegisterServer();
            //SlikServerObject.StartServer();

        }

        private void Dispacher_Tick(object sender, EventArgs e)
        {
            foreach (var currentGrp in ScanrateGroup)
            {
                tokenSource = new CancellationTokenSource();
                CancellationToken token = tokenSource.Token;
                ObservableCollection<Tag> TagsCollections = null;
                ObservableCollection<TagGroup> groupsCollection = null;
                string[] keyItems = currentGrp.Key.Split('.');
                string connectorName = keyItems[1];
                string deviceName = keyItems[2];

                //Get the Connector Object
                IConnector connectorConn = ConnectorFactory.GetConnectorByName(connectorName, ConnectorCollectionForCommunication); //ConnectorCollection);
                dynamic connectorObj = ConnectorFactory.GetConnector(connectorConn);
                if (connectorObj != null)
                {
                    //Get the Device Object
                    DeviceBase deviceObj = DeviceFactory.GetDeviceByName(deviceName, connectorObj.DeviceCollection);
                    dynamic deviceObject = DeviceFactory.GetDevice(deviceObj);
                    TagsCollections = deviceObject.TagsCollection;
                    groupsCollection = deviceObject.GroupCollection;
                    //Adding or Updating clientGroup.
                    if (deviceObject.DeviceType == DeviceType.ModbusEthernet)
                    {
                        TcpClient NewTcpClient = GetNewTcpClient(connectorName, deviceObject);
                        if (!ScanrateClientGroup.ContainsKey(currentGrp.Key))
                        {
                            ScanrateClientGroup.Add(currentGrp.Key, NewTcpClient);
                        }
                        else
                        {
                            ScanrateClientGroup[currentGrp.Key] = NewTcpClient;
                        }
                        if (NewTcpClient != null && NewTcpClient.Connected)
                        {
                            string key = string.Format("{0}.{1}", connectorName, deviceName);
                            ConnectionHelperObj.tcpClientDictionary[key] = NewTcpClient;
                            CreateModbusIPMaster(key, NewTcpClient);
                            ModbusIpMaster currentMaster = GetCurrentModbusIPMaster(deviceName);
                        }
                    }
                    else
                    {
                        //CreateModbusSerialMaster(protocolName, deviceName);
                        //ModbusSerialMaster currentMaster = GetCurrentModbusSerailMaster(deviceName);
                    }

                }
                string scanRate = currentGrp.Key.Split('.')[0];
                int iscanRate = Convert.ToInt32(scanRate);
                try
                {

                    SubscribeTag(currentGrp.Value, connectorConn, TagsCollections, groupsCollection);


                    // tc.Start();
                    // tc.Wait(iscanRate);
                    //taskList.Add(tc);
                    //taskList.Add(Task.Factory.StartNew(() => OPCEngine.TaskHelper.RunPeriodically<ISLIKTag, IConnector, ObservableCollection<Tags>>((a, b, c) => SubscribeTag(item.Value, protocolConn, TagCollections), item.Value, protocolConn, TagCollections, TimeSpan.FromMilliseconds(iscanRate), token), token));
                }
                catch (Exception ex)
                {
                    Addlogs("Task Creation", @"Elpis/Configuration", ex.Message, LogStatus.Error);
                }

            }
        }

        #endregion Constructor

        #region SLIKDA Events

        #region SlikServer_OnClientConnect Event
        /// <summary>
        /// This Event called when client connected to the OPC Server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        public void SlikServer_OnClientConnect(object sender, SLIKServer.OnClientConnectEventArgs eventArgs)
        {
            UpdateCommunicationList();
            //GetCommunicationElements();
            ////GetCommunicationElements();
            ////dispacher.Interval = TimeSpan.FromMilliseconds(100);
            ////dispacher.Start();

            ////Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            //// {
            //if (taskList != null && taskList.Count > 0)
            //{
            //    //if (tokenSource != null)
            //    //{
            //    //    tokenSource.Cancel();
            //    //    StartTask();
            //    //}


            //}
            //StartTask();
            ////else
            ////{
            ////    StartTask();
            ////}
            ////}), DispatcherPriority.Normal, null);
        }

        private void UpdateCommunicationList()
        {
            SaveLastLoadedProject();
            try
            {
                if (File.Exists("opcproject.elp"))
                {
                    Stream stream = File.Open("opcproject.elp", FileMode.OpenOrCreate);

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
                                ConnectorCollectionForCommunication = FileHandle.AllCollectionFileHandling;
                                foreach (var connector in ConnectorCollectionForCommunication)
                                {
                                    ConnectorBase connectorBase = connector as ConnectorBase;
                                    foreach (var device in connectorBase.DeviceCollection)
                                    {
                                        device.ConnectorAssignment = connectorBase.ConnectorName;
                                    }
                                }
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
            }
            catch (Exception ex)
            {
                ElpisServer.Addlogs("Configuration", @"Server/LoadConfiguration", "Failed to load configuration file. " + ex.Message, LogStatus.Error);
            }
        }
        #endregion SlikServer_OnClientConnect Event


        #region SlikServer_OnClientDisconnect Event
        /// <summary>
        /// It disconnect server from the client when user stops the server or client disconnected from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        public void SlikServer_OnClientDisconnect(object sender, SLIKServer.OnClientDisconnectEventArgs eventArgs)
        {
            //dispatcherTimer.Stop();

            //if (tokenSource != null)
            //{
            //    tokenSource.Cancel();
            //}

            //TaskHelper.isRunningTask = false;
            // setQualityofTags();
            //foreach (var item in tokenSourceList)
            //{
            //    item.Cancel();
            //}


        }
        #endregion SlikServer_OnClientDisconnect Event


        #region StartTask
        List<Task> taskList { get; set; }
        List<CancellationTokenSource> tokenSourceList { get; set; }
        /// <summary>
        /// This method starts the tasks of subscribing tags
        /// </summary>
        private void StartTask()
        {
            #region old code
            ////Parallel.ForEach(ScanrateGroup,(currentGrp) => {

            ////    string scanRate = currentGrp.Key.Split('.')[0];
            ////    int iScanRate = Convert.ToInt32(scanRate);
            ////    Thread.Sleep(iScanRate);
            ////    foreach (ISLIKTag tagItem in currentGrp.Value)
            ////    {
            ////        try
            ////        {
            ////            int address = TagDictionary[tagItem];
            ////            IConnector protocolObject = ConnectorFactory.GetConnector(tagItem.Name, ProtocolCollection);
            ////            dynamic protocolObj = ConnectorFactory.GetConnectorObj(protocolObject);

            ////            if (protocolObject != null)
            ////            {
            ////                //Get the Device Object
            ////                IDevice deviceObj = DeviceFactory.GetDevice(tagItem.Name, protocolObj.DeviceCollection);
            ////                dynamic deviceObject = DeviceFactory.GetDeviceObjbyDevice(deviceObj);

            ////                //Get the Tag Object
            ////                dynamic tagObject = TagFactory.GetTagObjectByName(tagItem.Name, deviceObject.TagsCollection);

            ////                TcpClient NewTcpClient = GetNewTcpClient(protocolObject, deviceObject);
            ////                if (NewTcpClient.Connected == true)
            ////                {
            ////                    ModbusIpMaster currentMaster = CreateModbusMaster(deviceObject.DeviceName, NewTcpClient);
            ////                    protocolObject.Subscribe(tagItem, currentMaster, tagObject);
            ////                    //break;
            ////                }
            ////                else
            ////                {
            ////                    ModbusIpMaster currentMaster = GetCurrentModbusDevice(deviceObject.DeviceName);
            ////                    protocolObject.Subscribe(tagItem, currentMaster, tagObject);
            ////                }
            ////            }
            ////        }
            ////        catch (Exception ex)
            ////        {

            ////        }
            ////    }
            ////});
            #endregion
            Task tc = null;
            taskList = new List<Task>();
            tokenSourceList = new List<CancellationTokenSource>();
            taskList.Clear();
            ConnectionHelperObj.ModbusIPMasterCollection.Clear();
            ConnectionHelperObj.ModbusSerialMasterCollection.Clear();
            foreach (var item in ConnectionHelperObj.ModbusSerialPortCollection)
            {
                item.Value.Close();
                item.Value.Dispose();
            }
            ListofMappedtag.Clear();
            ConnectionHelperObj.ModbusSerialPortCollection.Clear();
            ConnectionHelperObj.tcpClientDictionary.Clear();
            //GetCommunicationElements();
            //Parallel.ForEach(ScanrateGroup, (currentGrp) =>
            //{

            foreach (var currentGrp in ScanrateGroup)
            {
                tokenSource = new CancellationTokenSource();
                CancellationToken token = tokenSource.Token;
                DeviceBase deviceBaseObject = null;
                ObservableCollection<Tag> TagsCollections = null;
                ObservableCollection<TagGroup> groupsCollection = null;
                string[] keyItems = currentGrp.Key.Split('.');
                string connectorName = keyItems[1];
                string deviceName = keyItems[2];
                Dictionary<ISLIKTag, Tag> TagMapped = null;

                //Get the Connector Object
                IConnector connectorConn = ConnectorFactory.GetConnectorByName(connectorName, ConnectorCollectionForCommunication); //ConnectorCollection);
                dynamic connectorObj = ConnectorFactory.GetConnector(connectorConn);
                if (connectorObj != null)
                {
                    //Get the Device Object
                    deviceBaseObject = DeviceFactory.GetDeviceByName(deviceName, connectorObj.DeviceCollection);
                    DeviceBase deviceObj = DeviceFactory.GetDeviceByName(deviceName, connectorObj.DeviceCollection);
                    dynamic deviceObject = DeviceFactory.GetDevice(deviceObj);
                    TagsCollections = deviceObject.TagsCollection;
                    groupsCollection = deviceObject.GroupCollection;
                    //Adding or Updating clientGroup.
                    #region ModbusEthernet
                    if (deviceObject.DeviceType == DeviceType.ModbusEthernet)
                    {
                        TcpClient NewTcpClient = GetNewTcpClient(connectorName, deviceObject);
                        if (!ScanrateClientGroup.ContainsKey(currentGrp.Key))
                        {
                            ScanrateClientGroup.Add(currentGrp.Key, NewTcpClient);
                        }
                        else
                        {
                            ScanrateClientGroup[currentGrp.Key] = NewTcpClient;
                        }
                        if (NewTcpClient != null && NewTcpClient.Connected)
                        {
                            string key = string.Format("{0}.{1}", connectorName, deviceName);
                            if (ConnectionHelperObj.tcpClientDictionary.ContainsKey(key))
                                ConnectionHelperObj.tcpClientDictionary[key] = NewTcpClient;
                            else
                                ConnectionHelperObj.tcpClientDictionary.Add(key, NewTcpClient);
                            CreateModbusIPMaster(key, NewTcpClient);
                            ModbusIpMaster currentMaster = GetCurrentModbusIPMaster(key);
                        }
                    }
                    #endregion ModbusEthernet
                    else if (deviceObj.DeviceType == DeviceType.ModbusSerial)
                    {
                        string key = string.Format("{0}.{1}", connectorName, deviceName);
                        CreateModbusSerialMaster(connectorName, deviceName);
                        ModbusSerialMaster currentMaster = GetCurrentModbusSerailMaster(key); //(deviceName);
                        TagMapped = MapTags(currentGrp.Value, TagsCollections, groupsCollection);
                        ListofMappedtag.Add(currentGrp.Key, TagMapped);
                    }

                }
                string scanRate = currentGrp.Key.Split('.')[0];
                int iscanRate = Convert.ToInt32(scanRate);
                try
                {
                    TaskHelper.isTaskRunning = true;
                    if (deviceBaseObject.DeviceType == DeviceType.ModbusEthernet)
                        tc = TaskHelper.RunPeriodically1<ISLIKTag, IConnector, DeviceBase, ObservableCollection<Tag>, ObservableCollection<TagGroup>>((a, b, c, d, e) => SubscribeTag(currentGrp.Value, connectorConn, deviceBaseObject, TagsCollections, groupsCollection), currentGrp.Value, connectorConn, deviceBaseObject, TagsCollections, groupsCollection, TimeSpan.FromMilliseconds(iscanRate), token);
                    else if (deviceBaseObject.DeviceType == DeviceType.ModbusSerial)
                        tc = TaskHelper.RunPeriodicallySerial<Dictionary<ISLIKTag, Tag>, IConnector, DeviceBase, int>((a, b, c, d) => SubscribeTagSerial(TagMapped, connectorConn, deviceBaseObject, iscanRate), TagMapped, connectorConn, deviceBaseObject, iscanRate, TimeSpan.FromMilliseconds(iscanRate), token);
                    //tc.Start();
                    // tc.Wait(iscanRate);
                    taskList.Add(tc);
                    //taskList.Add(Task.Factory.StartNew(() => OPCEngine.TaskHelper.RunPeriodically<ISLIKTag, IConnector, ObservableCollection<Tags>>((a, b, c) => SubscribeTag(item.Value, protocolConn, TagCollections), item.Value, protocolConn, TagCollections, TimeSpan.FromMilliseconds(iscanRate), token), token));
                    tokenSourceList.Add(tokenSource);

                }
                catch (Exception ex)
                {
                    Addlogs("Task Creation", @"Elpis/Configuration", ex.Message, LogStatus.Error);
                }


            } //);
              //}
        }

        private Dictionary<ISLIKTag, Tag> MapTags(List<ISLIKTag> slikdaTagCollection, ObservableCollection<Tag> tagsCollections, ObservableCollection<TagGroup> groupsCollection)
        {
            Tag tag = null;
            Dictionary<ISLIKTag, Tag> mappedTagCollection = new Dictionary<ISLIKTag, Tag>();
            foreach (var item in slikdaTagCollection)
            {
                string[] tagDescription = item.Name.Split('.');
                if (tagDescription.Count() == 4)
                {
                    tag = tagsCollections.FirstOrDefault(t => t.TagName == tagDescription[3]);
                }
                else
                {
                    TagGroup tagGroup = groupsCollection.FirstOrDefault(g => g.GroupName == tagDescription[3]);
                    tag = tagGroup.TagsCollection.FirstOrDefault(t => t.TagName == tagDescription[4]);
                }
                mappedTagCollection.Add(item, tag);
            }

            return mappedTagCollection;
        }

        private void SubscribeTag(List<ISLIKTag> slikdaTagList, IConnector connectorConn, DeviceBase deviceBaseObject, ObservableCollection<Tag> tagsCollections, ObservableCollection<TagGroup> groupsCollection)
        {
            if (!isDemoExpired)
            {
                try
                {
                    #region Modbus Ethernet
                    if (deviceBaseObject.DeviceType == DeviceType.ModbusEthernet)
                    {
                        string clientKey = string.Format("{0}.{1}.{2}", tagsCollections[0].ScanRate, connectorConn.Name, deviceBaseObject.DeviceName);
                        TcpClient tcpClient1 = ScanrateClientGroup[clientKey];
                        if (tcpClient1 != null)
                        {
                            if (tcpClient1.Client.Connected)
                            {
                                dynamic connector = ConnectorFactory.GetConnector(connectorConn);
                                if (connector != null)
                                {
                                    //Get the Device Object
                                    //deviceObj = DeviceFactory.GetDeviceByName(deviceName, connector.DeviceCollection);
                                    ModbusEthernetDevice modbusEthernetDevice = deviceBaseObject as ModbusEthernetDevice;
                                    ModbusIpMaster currentMaster = GetCurrentModbusIPMaster(deviceBaseObject.ConnectorAssignment + "." + deviceBaseObject.DeviceName);
                                    if (currentMaster != null)
                                    {
                                        ModbusEthernetConnector con = connector as ModbusEthernetConnector;
                                        if (!con.IsWritingTag)
                                        {
                                            if (con.Master == null)
                                                con.Master = new Dictionary<string, ModbusIpMaster>();

                                            if (!(con.Master.ContainsKey(clientKey)))
                                                con.Master.Add(clientKey, currentMaster);
                                            else
                                                con.Master[clientKey] = currentMaster;
                                            try
                                            {
                                                con.Subscribe(slikdaTagList, deviceBaseObject, tagsCollections, groupsCollection); //(tagItem, deviceObject, tagObject);
                                            }
                                            catch (Exception e)
                                            {
                                                Addlogs("Communication", @"Elpis/OPC/Ethernet/Communication", e.Message, LogStatus.Error);
                                                ReConnectClient(deviceBaseObject.ConnectorAssignment, deviceBaseObject.DeviceName, clientKey);
                                            }
                                        }

                                    }
                                    else
                                    {
                                        SetQualityofTags(slikdaTagList);
                                        CreateModbusIPMaster(deviceBaseObject.ConnectorAssignment + "." + deviceBaseObject.Name, new TcpClient(modbusEthernetDevice.IPAddress, (int)modbusEthernetDevice.Port));
                                    }
                                }
                            }
                            else
                            {
                                SetQualityofTags(slikdaTagList);
                                System.Diagnostics.Trace.TraceInformation("Reconnecting client in case of TCP not connected");
                                ConnectorBase connector = connectorConn as ConnectorBase;
                                if (deviceBaseObject.RetryCounter > 0) // retryCount > 0)
                                {
                                    if (deviceBaseObject.DeviceType == DeviceType.ModbusEthernet)
                                        ReConnectClient(deviceBaseObject.ConnectorAssignment, deviceBaseObject.DeviceName, clientKey);
                                    deviceBaseObject.RetryCounter = (deviceBaseObject.RetryCounter) - 1;
                                }
                                else
                                {
                                    if (deviceBaseObject.RetryCounter <= -50)
                                    {
                                        deviceBaseObject.RetryCounter = (int)deviceBaseObject.RetryCount;
                                    }
                                    else
                                    {
                                        deviceBaseObject.RetryCounter = deviceBaseObject.RetryCounter - 1;
                                    }
                                }
                            }
                        }
                        else
                        {
                            ConnectorBase connector = connectorConn as ConnectorBase;
                            SetQualityofTags(slikdaTagList);
                            if (deviceBaseObject.RetryCounter > 0) // retryCount > 0)
                            {
                                if (deviceBaseObject.DeviceType == DeviceType.ModbusEthernet)
                                    ReConnectClient(connectorConn.Name, deviceBaseObject.DeviceName, clientKey);
                                deviceBaseObject.RetryCounter = deviceBaseObject.RetryCounter - 1;

                            }
                            else
                            {
                                if (deviceBaseObject.RetryCounter <= -50) //  retryCount <= -50)
                                {
                                    deviceBaseObject.RetryCounter = (int)deviceBaseObject.RetryCount;
                                }
                                else
                                {
                                    deviceBaseObject.RetryCounter = deviceBaseObject.RetryCounter - 1;
                                }
                            }
                        }
                    }
                    #endregion Modbus Ethernet

                    #region Modbus Serial
                    else if (deviceBaseObject.DeviceType == DeviceType.ModbusSerial)
                    {
                        ModbusSerialConnector modbusSerialConnetor = connectorConn as ModbusSerialConnector;
                        string clientKey = string.Format("{0}.{1}.{2}", tagsCollections[0].ScanRate, connectorConn.Name, deviceBaseObject.DeviceName);
                        if (modbusSerialConnetor != null)
                        {
                            ModbusSerialDevice serialDevice = deviceBaseObject as ModbusSerialDevice;
                            if (serialDevice.Port == null)
                            {
                                serialDevice.Port = ConnectionHelperObj.ModbusSerialPortCollection[deviceBaseObject.ConnectorAssignment + "." + deviceBaseObject.DeviceName];
                                if (!serialDevice.Port.IsOpen)
                                    serialDevice.Port.Open();
                            }
                            if (serialDevice.Port.IsOpen)
                            {
                                ModbusSerialMaster currentMaster = GetCurrentModbusSerailMaster(modbusSerialConnetor.ConnectorName + "." + deviceBaseObject.DeviceName);
                                if (currentMaster != null)
                                {
                                    if (!modbusSerialConnetor.IsWritingTag)
                                    {
                                        // serialDevice.Master = currentMaster;
                                        if (!(modbusSerialConnetor.Master.ContainsKey(clientKey)))
                                            modbusSerialConnetor.Master.Add(clientKey, currentMaster);
                                        else
                                        {
                                            modbusSerialConnetor.Master[clientKey] = currentMaster;
                                        }
                                        try
                                        {
                                            modbusSerialConnetor.Subscribe(slikdaTagList, deviceBaseObject, tagsCollections, groupsCollection);
                                        }
                                        catch (TimeoutException)
                                        {
                                            CreateModbusSerialMaster(deviceBaseObject.ConnectorAssignment, deviceBaseObject.DeviceName);
                                        }
                                        catch (Exception e)
                                        {
                                            SetQualityofTags(slikdaTagList);
                                            Addlogs("Communication", @"Elpis/OPC/Communication/SerialPort", e.Message, LogStatus.Error);
                                        }
                                    }

                                }

                                else
                                {
                                    SetQualityofTags(slikdaTagList);
                                    CreateModbusSerialMaster(serialDevice.ConnectorAssignment, serialDevice.DeviceName);
                                }
                            }
                            else
                            {
                                if (serialDevice.Port != null)
                                    serialDevice.Port.Open();
                            }

                        }
                    }
                    #endregion Modbus Serial
                }
                catch (TimeoutException)
                {
                    CreateModbusSerialMaster(deviceBaseObject.ConnectorAssignment, deviceBaseObject.DeviceName);
                }
                catch (Exception ex)
                {
                    Addlogs("Communication", @"Server/Tag/Subscribe", ex.Message, LogStatus.Error);
                }
            }

            else
            {
                StartStop();
                //MessageBox.Show(@"'Elpis OPC Sever' demo period is expired, restart server again.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }
        #endregion StartTask

        private void SubscribeTagSerial(Dictionary<ISLIKTag, Tag> mappedList, IConnector connectorObject, DeviceBase deviceBaseObject, int scanRate)
        {

            if (!isDemoExpired)
            {
                #region Modbus Serial                    

                ModbusSerialConnector modbusSerialConnetor = connectorObject as ModbusSerialConnector;
                string clientKey = string.Format("{0}.{1}.{2}", scanRate, connectorObject.Name, deviceBaseObject.DeviceName);
                if (modbusSerialConnetor != null)
                {
                    ModbusSerialDevice serialDevice = deviceBaseObject as ModbusSerialDevice;
                    if (serialDevice.Port == null)
                    {
                        serialDevice.Port = ConnectionHelperObj.ModbusSerialPortCollection[deviceBaseObject.ConnectorAssignment + "." + deviceBaseObject.DeviceName];
                        if (!serialDevice.Port.IsOpen)
                            serialDevice.Port.Open();
                    }
                    if (serialDevice.Port.IsOpen)
                    {
                        ModbusSerialMaster currentMaster = GetCurrentModbusSerailMaster(modbusSerialConnetor.ConnectorName + "." + deviceBaseObject.DeviceName);
                        if (currentMaster != null)
                        {
                            if (!modbusSerialConnetor.IsWritingTag)
                            {
                                //serialDevice.Master = currentMaster;
                                if (!(modbusSerialConnetor.Master.ContainsKey(clientKey)))
                                    modbusSerialConnetor.Master.Add(clientKey, currentMaster);
                                else
                                {
                                    modbusSerialConnetor.Master[clientKey] = currentMaster;
                                }
                                try
                                {
                                    modbusSerialConnetor.Subscribe(mappedList, deviceBaseObject, scanRate);
                                }
                                catch (TimeoutException te)
                                {
                                    SetQualityofTags(mappedList.Keys.ToList());
                                    Addlogs("Communication", @"Elpis/OPC/Communication/SerialPort", "Device Timeout" + te.Message, LogStatus.Error);
                                    // CreateModbusSerialMaster(deviceBaseObject.ConnectorAssignment, deviceBaseObject.DeviceName);
                                }
                                catch (Exception e)
                                {
                                    SetQualityofTags(mappedList.Keys.ToList());
                                    Addlogs("Communication", @"Elpis/OPC/Communication/SerialPort", e.Message, LogStatus.Error);
                                }
                            }

                        }

                        else
                        {
                            SetQualityofTags(mappedList.Keys.ToList());
                            CreateModbusSerialMaster(serialDevice.ConnectorAssignment, serialDevice.DeviceName);
                        }
                    }
                    else
                    {
                        if (serialDevice.Port != null)
                            serialDevice.Port.Open();
                    }

                }
                #endregion Modbus Serial
            }

            else
            {
                StartStop();
                //MessageBox.Show(@"'Elpis OPC Sever' demo period is expired, restart server again.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }

        #region GetCommunicationElements
        /// <summary>
        /// Get all Communication elements from the file and used for client and server communication.
        /// </summary>
        private void GetCommunicationElements()
        {
            try
            {
                if (File.Exists("opcproject.elp"))
                {
                    Stream stream = File.Open("opcproject.elp", FileMode.OpenOrCreate);

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
                                ConnectorCollectionForCommunication = FileHandle.AllCollectionFileHandling;
                                //MqttClientCollection = fileHandler.MqttCollectionFilHandling;
                                //AzureIoTCollection = fileHandler.AzureIoTFileHandling;
                                LoadConfigurationElements(ConnectorCollectionForCommunication);

                                foreach (var connector in ConnectorCollectionForCommunication)
                                {
                                    ConnectorBase connectorBase = connector as ConnectorBase;
                                    foreach (var device in connectorBase.DeviceCollection)
                                    {
                                        device.ConnectorAssignment = connectorBase.ConnectorName;
                                    }
                                }
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
            }
            catch (Exception ex)
            {
                ElpisServer.Addlogs("Configuration", @"Server/LoadConfiguration", "Failed to load configuration file. " + ex.Message, LogStatus.Error);
            }
        }
        #endregion GetCommunicationElements       



        #region SlikServer_OnRead Event
        /// <summary>
        /// This Event called when user reads the tag value from the OPC Client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        public void SlikServer_OnRead(object sender, SLIKServer.OnReadEventArgs eventArgs)
        {
            try
            {
                // Iterate  each/every item that the OPC Client has requested us to READ
                for (int i = 0; i <= (eventArgs.Count - 1); i++)
                {
                    ISLIKTag currentItem = eventArgs.Tags[i];
                    // Check whether tag is active or not
                    if (currentItem.Active)
                    {
                        if (TagDictionary.Keys.Contains(currentItem))
                        {
                            string address = TagDictionary[currentItem].ToString();
                            IConnector iConnector = ConnectorFactory.GetConnector(currentItem.Name, ConnectorCollectionForCommunication);
                            dynamic connectorObj = ConnectorFactory.GetConnector(iConnector);
                            //Get the Device Object
                            DeviceBase deviceObj = DeviceFactory.GetDevice(currentItem.Name, connectorObj.DeviceCollection);
                            string tagName = null;
                            var element = currentItem.Name.Split('.');
                            if (element.Count() == 5)
                                tagName = element[4];
                            else
                                tagName = element[3];

                            if (deviceObj.DeviceType == DeviceType.ModbusEthernet)
                            {
                                ModbusEthernetDevice deviceObject = DeviceFactory.GetDevice(deviceObj);
                                //Get the Tag Object
                                Tag tagObject = deviceObject.TagsCollection.FirstOrDefault(c => c.TagName == tagName);
                                //dynamic tagObject = TagFactory.GetTagObjectByName(currentItem.Name, deviceObject.TagsCollection);

                                //bool connected = IsConnected();
                                //if (connected == true)
                                //{

                                foreach (KeyValuePair<string, TcpClient> tcpclient in ConnectionHelperObj.tcpClientDictionary)
                                {
                                    if (tcpclient.Key == (iConnector.Name + "." + deviceObject.DeviceName))
                                    {

                                        TcpClient NewTcpClient = tcpclient.Value;
                                        if (!(NewTcpClient != null && NewTcpClient.Connected))
                                        {
                                            NewTcpClient = IsConnected(tcpclient.Value, deviceObject.IPAddress, deviceObject.Port);
                                        }

                                        if (NewTcpClient != null)
                                        {
                                            if (NewTcpClient.Connected == true)
                                            {
                                                string key = deviceObject.ConnectorAssignment + "." + deviceObject.DeviceName;
                                                //connectionHelper.tcpClientDictionary[deviceObject.DeviceName] = NewTcpClient;
                                                ConnectionHelperObj.tcpClientDictionary[key] = NewTcpClient;
                                                CreateModbusIPMaster(key, NewTcpClient);
                                                ModbusIpMaster currentMaster = GetCurrentModbusIPMaster(deviceObject.DeviceName);
                                                try
                                                {
                                                    //if (currentDevice.Transport.ReadTimeout != -1)
                                                    //{
                                                    currentMaster = ModbusIpMaster.CreateIp(NewTcpClient);

                                                    ConnectionHelperObj.ModbusIPMasterCollection[deviceObject.DeviceName] = currentMaster;
                                                    // }
                                                }
                                                catch (Exception ex)
                                                {
                                                    string ErrMessage = ex.Message;
                                                    currentMaster = ModbusIpMaster.CreateIp(NewTcpClient);
                                                    ConnectionHelperObj.ModbusIPMasterCollection[deviceObject.DeviceName] = currentMaster;
                                                }

                                                try
                                                {
                                                    iConnector.Read(currentItem, deviceObject, tagObject);
                                                }
                                                catch (Exception)
                                                {
                                                    //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                                    //{
                                                    Addlogs("Configuration", @"Elpis/Configuration/OnWrite", "Problem in reading tag value.", LogStatus.Error);
                                                    //}), DispatcherPriority.Normal, null);
                                                }

                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (deviceObj.DeviceType == DeviceType.ModbusSerial)
                            {
                                ModbusSerialDevice deviceObject = DeviceFactory.GetDevice(deviceObj);
                                //Get the Tag Object
                                Tag tagObject = deviceObject.TagsCollection.FirstOrDefault(c => c.TagName == tagName);

                                SerialPort serialPort = ConnectionHelperObj.ModbusSerialPortCollection[deviceObject.ConnectorAssignment + "." + deviceObject.DeviceName];
                                if (serialPort != null && !serialPort.IsOpen)
                                {
                                    serialPort.Open();
                                }
                                CreateModbusSerialMaster(deviceObject.ConnectorAssignment, deviceObject.DeviceName);
                                ModbusSerialMaster currentMaster = GetCurrentModbusSerailMaster(deviceObject.DeviceName);
                                ModbusSerialConnector serialConnector = iConnector as ModbusSerialConnector;
                                string key = string.Format("{0}.{1}.{2}", tagObject.ScanRate, deviceObject.ConnectorAssignment, deviceObject.DeviceName);
                                if (serialConnector.Master.ContainsKey(key))
                                {
                                    serialConnector.Master[key] = currentMaster;
                                }
                                else
                                {
                                    serialConnector.Master.Add(key, currentMaster);
                                }
                                //try
                                //{
                                iConnector.Read(currentItem, deviceObject, tagObject);
                                //}
                                //catch (Exception)
                                //{
                                //    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                //    {
                                //        Addlogs("Configuration", @"Elpis/Configuration/OnWrite", "Problem in reading tag value.", LogStatus.Error);
                                //    }), DispatcherPriority.Normal, null);
                                //}
                            }

                        }
                    }
                    // Specify that the Item at *this* position is NOT in error!
                    eventArgs.Errors[i] = (int)OPCDAErrorsEnum.sdaSOK;
                }
                // Now specify that we completed this event successfully
                eventArgs.Result = (int)OPCDAErrorsEnum.sdaSOK;
            }
            catch (Exception ex)
            {
                //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                Addlogs("Configuration", @"Elpis/Configuration/OnWrite", "Problem in reading tag value." + ex.Message, LogStatus.Error);
                //}), DispatcherPriority.Normal, null);
            }
        }
        #endregion  SlikServer_OnRead Event

        #region  SlikServer_OnWrite Event
        public void SlikServer_OnWrite(object sender, SLIKServer.OnWriteEventArgs eventArgs)
        {
            try
            {
                for (int i = 0; i <= (eventArgs.Count - 1); i++)
                {
                    ISLIKTag currentItem = eventArgs.Tags[i];
                    if (TagDictionary.Keys.Contains(currentItem))
                    {
                        dynamic currentValue = eventArgs.Values[i];
                        string[] elements = currentItem.Name.Split('.');

                        //string address = TagDictionary[currentItem].ToString();
                        IConnector protocolObject = ConnectorFactory.GetConnector(currentItem.Name, ConnectorCollectionForCommunication);// ConnectorCollection);
                        dynamic protocolObj = ConnectorFactory.GetConnector(protocolObject);
                        // 12 01 2107
                        //Get the Device Object
                        DeviceBase deviceObj = DeviceFactory.GetDevice(currentItem.Name, protocolObj.DeviceCollection);
                        dynamic deviceObject = DeviceFactory.GetDevice(deviceObj);
                        Tag tagObject = null;
                        //Get the Tag Object
                        if (elements.Length == 5)
                            tagObject = GetTagObject(elements[4], deviceObject.TagsCollection);
                        else
                            tagObject = GetTagObject(elements[3], deviceObject.TagsCollection);

                        //bool connected = IsConnected();
                        //if (connected == true)
                        //{
                        if (deviceObj.DeviceType == DeviceType.ModbusEthernet)
                        {
                            foreach (KeyValuePair<string, TcpClient> tcpclient in ConnectionHelperObj.tcpClientDictionary)
                            {
                                try
                                {
                                    if (tcpclient.Key == (protocolObject.Name + "." + deviceObject.DeviceName))
                                    {
                                        TcpClient NewTcpClient = IsConnected(tcpclient.Value, deviceObject.IPAddress, deviceObject.Port);
                                        if (NewTcpClient != null)
                                        {
                                            if (NewTcpClient.Connected == true)
                                            {
                                                string key = protocolObject.Name + "." + deviceObject.DeviceName;
                                                //connectionHelper.tcpClientDictionary[deviceObject.DeviceName] = NewTcpClient;
                                                ConnectionHelperObj.tcpClientDictionary[key] = NewTcpClient;
                                                //CreateModbusIPMaster(key, NewTcpClient);
                                                ModbusIpMaster currentMaster = GetCurrentModbusIPMaster(key);
                                                //try
                                                //{
                                                //    //if (currentDevice.Transport.ReadTimeout != -1)
                                                //    //{
                                                //    currentMaster = ModbusIpMaster.CreateIp(NewTcpClient);

                                                //    ConnectionHelperObj.ModbusIPMasterCollection[deviceObject.DeviceName] = currentMaster;
                                                //    // }
                                                //}
                                                //catch (Exception ex)
                                                //{
                                                //    string ErrMessage = ex.Message;
                                                //    currentMaster = ModbusIpMaster.CreateIp(NewTcpClient);
                                                //    ConnectionHelperObj.ModbusIPMasterCollection[deviceObject.DeviceName] = currentMaster;
                                                //}
                                                if (currentMaster != null)
                                                {
                                                    string clientKey = string.Format("{0}.{1}", tagObject.ScanRate, key);
                                                    ModbusEthernetConnector con = protocolObj as ModbusEthernetConnector;
                                                    if (con.Master == null)
                                                        con.Master = new Dictionary<string, ModbusIpMaster>();

                                                    if (!(con.Master.ContainsKey(clientKey)))
                                                        con.Master.Add(clientKey, currentMaster);
                                                    //else  Commented on 24-Mar-2018
                                                    //    con.Master[clientKey] = currentMaster;
                                                    con.IsWritingTag = true;
                                                    protocolObject.Write(currentItem, currentValue, deviceObject, tagObject);
                                                    con.IsWritingTag = false;
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                    //{
                                    Addlogs("Configuration", @"Configuration/ModbusEthernet/OnWrite", ex.Message, LogStatus.Error);
                                    //}), DispatcherPriority.Normal, null);
                                }
                            }
                        }
                        else if (deviceObj.DeviceType == DeviceType.ModbusSerial)
                        {

                            try
                            {
                                string key = protocolObject.Name + "." + deviceObject.DeviceName;
                                ModbusSerialMaster currentMaster = ConnectionHelperObj.ModbusSerialMasterCollection[key];

                                ModbusSerialDevice serialDevice = deviceObj as ModbusSerialDevice;
                                string clientKey = string.Format("{0}.{1}", tagObject.ScanRate, key);
                                ModbusSerialConnector con = protocolObj as ModbusSerialConnector;
                                if (serialDevice.Port == null)
                                {
                                    serialDevice.Port = ConnectionHelperObj.ModbusSerialPortCollection[deviceObj.ConnectorAssignment + "." + deviceObj.DeviceName];
                                    if (!serialDevice.Port.IsOpen)
                                        serialDevice.Port.Open();
                                }
                                if (serialDevice.Port.IsOpen)
                                {
                                    con.IsWritingTag = true;
                                    // serialDevice.Master = currentMaster;

                                    if (con.Master == null)
                                        con.Master = new Dictionary<string, ModbusSerialMaster>();

                                    if (!(con.Master.ContainsKey(clientKey)))
                                        con.Master.Add(clientKey, currentMaster);
                                    //else  Commented on 24-Mar-2018
                                    //    con.Master[clientKey] = currentMaster;

                                    protocolObject.Write(currentItem, currentValue, deviceObject, tagObject);
                                    con.IsWritingTag = false;
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                //{
                                Addlogs("Configuration", @"Configuration/ModbusSerial/OnWrite", ex.Message, LogStatus.Error);
                                //}), DispatcherPriority.Normal, null);
                            }
                        }
                    }

                    // Specify that the Item at *this* position is NOT in error!
                    eventArgs.Errors[i] = (int)OPCDAErrorsEnum.sdaSOK;
                }
                // Now specify that we completed this event successfully
                eventArgs.Result = (int)OPCDAErrorsEnum.sdaSOK;
            }
            catch (Exception ex)
            {

                //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                Addlogs("Configuration", @"Configuration/OnWrite", ex.Message, LogStatus.Error);
                //}), DispatcherPriority.Normal, null);
            }
        }
        #endregion  SlikServer_OnWrite Event

        #region DispatcherTimer Event
        //<summary>
        //DispatcherTimer for Timing intervals for all tags
        //</summary>
        //<param name="sender"></param>
        //<param name="e"></param>
        public void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            //SubscribeTag();
        }
        #endregion DispatcherTimer Event

        #region SubscribeTag
        /// <summary>
        /// Subscribing a Tag from the TagList. The tag list is grouped based on the ScanRates of the Tags of a Device. Subscribe tags with groups
        /// </summary>
        /// <param name="tagList"></param>
        /// 
        private void SubscribeTag(List<ISLIKTag> TagList, IConnector connectorObject, ObservableCollection<Tag> tagsCollection, ObservableCollection<TagGroup> groups)
        {
            if (!isDemoExpired)
            {
                Parallel.ForEach(TagList, (Action<ISLIKTag, ParallelLoopState>)((currentItem, state) =>
                // foreach (ISLIKTag currentItem in TagList)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                      {
                          string protocolName = string.Empty;
                          string deviceName = string.Empty;
                          string clientKey = string.Empty;
                          Tag tagObject = null;
                          DeviceBase deviceObj = null;
                          dynamic deviceObject = null;
                          try
                          {
                              if (currentItem.Active)
                              {
                                  string[] tagItems = currentItem.Name.Split('.');
                                  protocolName = tagItems[1];
                                  deviceName = tagItems[2];
                                  string key = protocolName + "." + deviceName;
                                  if (tagItems.Count() == 5)
                                  {
                                      var group = GetTagGroup(tagItems[3], groups); // groups.Where(g => g.GroupName == tagItems[3]).Select(g => g.TagsCollection).ToList();
                                      ObservableCollection<Tag> tagsCol = new ObservableCollection<Tag>(group.TagsCollection);
                                      tagObject = GetTagObject(tagItems[4], tagsCol);   // tagsCol.FirstOrDefault(t => t.TagName == tagItems[4]);                                
                                  }
                                  else
                                  {
                                      tagObject = GetTagObject(tagItems[3], tagsCollection);   //tagsCollection.FirstOrDefault(t => t.TagName == tagItems[3]); //GetTagObject(currentItem.Name, tagsCollection);// TagFactory.GetTagObjectByName(currentItem.Name, tagsCollection);
                                  }

                                  ConnectorBase currentConnector = connectorObject as ConnectorBase;

                                  #region Modbus Ethernet
                                  if (currentConnector.TypeofConnector == ConnectorType.ModbusEthernet)
                                  {
                                      clientKey = string.Format("{0}.{1}", (object)tagObject.ScanRate, key);
                                      TcpClient tcpClient1 = ScanrateClientGroup[clientKey];
                                      if (tcpClient1 != null)
                                      {
                                          if (tcpClient1.Client.Connected)
                                          {
                                              dynamic connector = ConnectorFactory.GetConnector(connectorObject);
                                              if (connector != null)
                                              {
                                                  //Get the Device Object
                                                  deviceObj = DeviceFactory.GetDeviceByName(deviceName, connector.DeviceCollection);
                                                  deviceObject = DeviceFactory.GetDevice(deviceObj);
                                                  ModbusIpMaster currentMaster = GetCurrentModbusIPMaster(deviceName);
                                                  if (currentMaster != null)
                                                  {
                                                      ModbusEthernetConnector con = connector as ModbusEthernetConnector;
                                                      if (con.Master == null)
                                                          con.Master = new Dictionary<string, ModbusIpMaster>();

                                                      if (!(con.Master.ContainsKey(clientKey)))
                                                          con.Master.Add(clientKey, currentMaster);
                                                      else
                                                          con.Master[clientKey] = currentMaster;

                                                      ISLIKTag tagItem = currentItem;

                                                      //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                                      //{
                                                      try
                                                      {
                                                          connectorObject.Subscribe(tagItem, deviceObject, tagObject);
                                                      }
                                                      catch (SocketException e)
                                                      {
                                                          Addlogs("Communication", @"Elpis/OPC/Configuration", e.Message, LogStatus.Error);
                                                      }
                                                      catch (Exception e)
                                                      {
                                                          Addlogs("Communication", @"Elpis/OPC/Configuration", e.Message, LogStatus.Error);
                                                          ReConnectClient(protocolName, deviceName, clientKey);
                                                      }

                                                      // }), DispatcherPriority.Normal, null);
                                                  }
                                                  else
                                                  {
                                                      CreateModbusIPMaster(deviceObj.ConnectorAssignment + "." + deviceObj.Name, new TcpClient(deviceObject.IPAddress, (int)deviceObject.Port));
                                                  }
                                                  //retryCount = 3;
                                              }
                                          }
                                          else
                                          {
                                              SetQualityofTags(TagList);
                                              //setQualityofCurrentItem(currentItem, tagObject);
                                              System.Diagnostics.Trace.TraceInformation("Reconnecting client in case of TCP not connected");

                                              ConnectorBase connector = connectorObject as ConnectorBase;
                                              deviceObj = DeviceFactory.GetDeviceByName(deviceName, connector.DeviceCollection);
                                              DeviceBase device = deviceObj as DeviceBase;

                                              if (device.RetryCounter > 0) // retryCount > 0)
                                              {
                                                  if (device.DeviceType == DeviceType.ModbusEthernet)
                                                      ReConnectClient(protocolName, deviceName, clientKey);
                                                  //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                                  //{
                                                  //    ReConnectClient(protocolName, deviceName, clientKey);
                                                  //}), DispatcherPriority.Normal, null);
                                                  //Task reconnectTask= Task.Factory.StartNew(() =>  ReConnectClient(protocolName, deviceName, clientKey), TaskCreationOptions.None);
                                                  //ReConnectClient(protocolName, deviceName, clientKey);
                                                  //retryCount--;
                                                  device.RetryCounter = (device.RetryCounter) - 1;
                                              }
                                              else
                                              {
                                                  if (device.RetryCounter <= -50)
                                                  {
                                                      device.RetryCounter = (int)device.RetryCount;
                                                      //Thread ResetThread = new Thread(this.ResetRetryCount);
                                                      //ResetThread.IsBackground = true;
                                                      //// if(! ResetThread.IsAlive)
                                                      //ResetThread.Start();
                                                  }
                                                  else
                                                  {
                                                      // retryCount--;
                                                      device.RetryCounter = device.RetryCounter - 1;
                                                  }
                                              }
                                              //state.Break();
                                              Thread.Sleep(100);
                                              //  return;
                                          }
                                      }
                                      else
                                      {
                                          ConnectorBase connector = connectorObject as ConnectorBase;
                                          deviceObj = DeviceFactory.GetDeviceByName(deviceName, connector.DeviceCollection);
                                          DeviceBase device = deviceObj as DeviceBase;

                                          //setQualityofCurrentItem(currentItem, tagObject);
                                          SetQualityofTags(TagList);
                                          if (device.RetryCounter > 0) // retryCount > 0)
                                          {
                                              //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                              //{
                                              if (device.DeviceType == DeviceType.ModbusEthernet)
                                                  ReConnectClient(protocolName, deviceName, clientKey);
                                              // }), DispatcherPriority.Normal, null);
                                              //Task.Factory.StartNew(() => ReConnectClient(protocolName, deviceName, clientKey), TaskCreationOptions.None);
                                              //ReConnectClient(protocolName, deviceName, clientKey);
                                              //retryCount--;
                                              device.RetryCounter = device.RetryCounter - 1;

                                          }
                                          else
                                          {
                                              if (device.RetryCounter <= -50) //  retryCount <= -50)
                                              {

                                                  device.RetryCounter = (int)device.RetryCount;
                                                  //Thread resetThread = new Thread(this.ResetRetryCount);
                                                  //resetThread.IsBackground = true;
                                                  //// if(! ResetThread.IsAlive)
                                                  //resetThread.Start();
                                              }
                                              else
                                              {
                                                  // retryCount--;
                                                  device.RetryCounter = device.RetryCounter - 1;
                                              }
                                          }
                                          // state.Break();
                                          Thread.Sleep(100);
                                      }
                                  }
                                  #endregion Modbus Ethernet

                                  #region Modbus Serial
                                  else if (currentConnector.TypeofConnector == ConnectorType.ModbusSerial)
                                  {
                                      clientKey = string.Format("{0}.{1}", (object)tagObject.ScanRate, key);
                                      //ConnectorBase connector = connectorObject as ConnectorBase;
                                      deviceObject = DeviceFactory.GetDeviceByName(deviceName, currentConnector.DeviceCollection);
                                      ISLIKTag tagItem = currentItem;
                                      if (currentConnector != null)
                                      {
                                          #region Comment
                                          ModbusSerialConnector serialConnector = currentConnector as ModbusSerialConnector;
                                          ////Get the Device Object
                                          //deviceObj = DeviceFactory.GetDeviceByName(deviceName, connector.DeviceCollection);
                                          //deviceObject = DeviceFactory.GetDevice(deviceObj);
                                          ModbusSerialDevice serialDevice = deviceObject as ModbusSerialDevice;
                                          // if (serialDevice.Port != null && serialDevice.Port.IsOpen)
                                          //  {
                                          ModbusSerialMaster currentMaster = GetCurrentModbusSerailMaster(serialConnector.ConnectorName + "." + deviceName);
                                          if (currentMaster != null)
                                          {
                                              // ModbusSerialConnector con = connector as ModbusSerialConnector;
                                              //con.ServerConnectorCollection = ConnectorCollectionForCommunication;
                                              //  if (con.Master == null)
                                              //      con.Master = new Dictionary<string, ModbusSerialMaster>();

                                              if (!(serialConnector.Master.ContainsKey(clientKey)))
                                                  serialConnector.Master.Add(clientKey, currentMaster);
                                              else
                                                  serialConnector.Master[clientKey] = currentMaster;
                                              #endregion Comment

                                              //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                              //{
                                              try
                                              {
                                                  connectorObject.Subscribe(tagItem, deviceObject, tagObject);
                                              }
                                              catch (Exception e)
                                              {
                                                  Addlogs("Communication", @"Elpis/OPC/Communication/SerialPort", e.Message, LogStatus.Error);
                                              }

                                              //}), DispatcherPriority.Normal, null);
                                          }
                                          //  }
                                          //else
                                          //{
                                          //    if (serialDevice.RetryCounter > 0)
                                          //    {
                                          //        if (serialDevice.Port != null)
                                          //        {
                                          //            try
                                          //            {
                                          //                serialDevice.Port.Open();
                                          //                int count = serialDevice.RetryCounter;
                                          //                serialDevice.RetryCounter--;
                                          //            }
                                          //            catch (Exception ex)
                                          //            {

                                          //            }
                                          //        }
                                          //    }
                                          //    else
                                          //    {
                                          //        if (serialDevice.RetryCounter < -50)
                                          //        {
                                          //            serialDevice.RetryCounter = (int)serialDevice.RetryCount;
                                          //        }
                                          //        else
                                          //        {
                                          //            serialDevice.RetryCounter--;
                                          //        }
                                          //    }
                                          //}
                                      }
                                  }
                                  #endregion Modbus Serial
                              }


                          }

                          catch (SocketException)
                          {
                              ConnectorBase connector = connectorObject as ConnectorBase;
                              deviceObj = DeviceFactory.GetDeviceByName(deviceName, connector.DeviceCollection);
                              DeviceBase device = deviceObj as DeviceBase;

                              //setQualityofCurrentItem(currentItem, tagObject);
                              SetQualityofTags(TagList);
                              //System.Diagnostics.Trace.TraceInformation("Reconnecting client in case of socket exception");
                              if (device.RetryCounter > 0) // retryCount > 0)
                              {
                                  if (device.DeviceType == DeviceType.ModbusEthernet)
                                  {
                                      ReConnectClient(protocolName, deviceName, clientKey);
                                  }
                                  device.RetryCounter = device.RetryCounter - 1;
                              }
                              else
                              {
                                  if (device.RetryCounter <= -50) // retryCount <= -50)
                                  {
                                      device.RetryCounter = (int)device.RetryCount;
                                  }
                                  else
                                  {
                                      device.RetryCounter = device.RetryCounter - 1;
                                      //retryCount--;
                                  }
                              }
                              //state.Break();
                              Thread.Sleep(100);
                          }
                          catch (InvalidOperationException)
                          {
                              ConnectorBase connector = connectorObject as ConnectorBase;
                              deviceObj = DeviceFactory.GetDeviceByName(deviceName, connector.DeviceCollection);
                              DeviceBase device = deviceObj as DeviceBase;

                              SetQualityofTags(TagList);
                              //setQualityofCurrentItem(currentItem, tagObject);
                              System.Diagnostics.Trace.TraceInformation("Reconnecting client in case of Invalid Operation exception");
                              if (device.RetryCounter > 0) // retryCount > 0)
                              {
                                  if (device.DeviceType == DeviceType.ModbusEthernet)
                                  {
                                      ReConnectClient(protocolName, deviceName, clientKey);
                                  }
                                  //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                  //{
                                  //    ReConnectClient(protocolName, deviceName, clientKey);
                                  //}), DispatcherPriority.Normal, null);

                                  //Task.Factory.StartNew(() => ReConnectClient(protocolName, deviceName, clientKey), TaskCreationOptions.None);
                                  //ReConnectClient(protocolName, deviceName, clientKey);
                                  /// retryCount--;
                                  int retryCount = device.RetryCounter;
                                  device.RetryCounter = retryCount - 1;
                              }
                              else
                              {
                                  if (device.RetryCounter <= -50) // retryCount <= -50) // retryTime= 50 * ScanRate of device 
                                  {
                                      device.RetryCounter = (int)device.RetryCount;
                                      //Thread ResetThread = new Thread(this.ResetRetryCount);
                                      //ResetThread.IsBackground = true;
                                      //// if(! ResetThread.IsAlive)
                                      //ResetThread.Start();
                                  }
                                  else
                                  {
                                      int retryCount = device.RetryCounter;
                                      device.RetryCounter = retryCount - 1;
                                      //retryCount--;
                                  }
                              }

                              //state.Break();
                              Thread.Sleep(100);
                              //throw ex;
                          }
                          catch (Exception)
                          {
                              SetQualityofTags(TagList);
                              // throw ex;
                          }
                      }

                    ), DispatcherPriority.Background, null);

                }));

            }
            else
            {
                StartStop();
                //MessageBox.Show(@"'Elpis OPC Sever' demo period is expired, restart server again.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion SubscribeTag

        #region GetTagObject
        /// <summary>
        /// Get Tag Object from the Tag list based on the tag name
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="tagsCollection"></param>
        /// <returns></returns>
        private Tag GetTagObject(string tagName, ObservableCollection<Tag> tagsCollection)
        {
            try
            {
                //if (string.IsNullOrEmpty(tagName))
                //    return null;
                //var element = tagName.Split('.');
                //if (element.Count() == 5)
                //    tagName = element[4];
                //else
                //    tagName = element[3];
                //Tag sTag = tagsCollection.FirstOrDefault(c => c.TagName == tagName);

                Tag sTag = tagsCollection.FirstOrDefault(t => t.TagName == tagName);
                return sTag;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion GetTagObject

        #region SetQualityofTags
        /// <summary>
        /// It sets the quality of the tags to bad when device is disconnected
        /// </summary>
        /// <param name="tagList"></param>
        private void SetQualityofTags(List<ISLIKTag> tagList)
        {
            Parallel.ForEach(tagList, (item) =>
             {
                 if (item.Quality != (short)QualityStatusEnum.sdaBadNotConnected)
                     item.SetVQT(null, (short)QualityStatusEnum.sdaBadNotConnected, DateTime.Now);
             });
        }
        #endregion SetQualityofTags               

        #region IsConnected
        /// <summary>
        /// Create and Connect to the TCP Client based on IP and Port.
        /// </summary>
        /// <param name="tcpClientCheck"></param>
        /// <param name="IP"></param>
        /// <param name="Port"></param>
        /// <returns></returns>
        public TcpClient IsConnected(TcpClient tcpClientCheck, string IP, int Port)
        {
            if (tcpClientCheck != null && tcpClientCheck.Connected == true)
            {
                return tcpClientCheck;
            }
            tcpClientCheck = new TcpClient();
            try
            {
                System.Net.IPAddress ipAddr = System.Net.IPAddress.Parse(IP);
                tcpClientCheck.Connect(ipAddr, Port);
            }
            catch (SocketException e)
            {
                throw e;
            }
            return tcpClientCheck;
        }
        #endregion IsConnected

        #region ReConnectClient
        /// <summary>
        /// Reconnect TCP Client, when it disconnected in mean time of communication. 
        /// </summary>
        /// <param name="protocolName"></param>
        /// <param name="deviceName"></param>
        /// <param name="key"></param>
        private void ReConnectClient(string protocolName, string deviceName, string key)
        {
            try
            {
                IConnector connectorConn = ConnectorFactory.GetConnectorByName(protocolName, ConnectorCollectionForCommunication); // ConnectorCollection);
                dynamic connectorObject = ConnectorFactory.GetConnector(connectorConn);
                if (connectorObject != null)
                {
                    //Get the Device Object
                    DeviceBase deviceObj = DeviceFactory.GetDeviceByName(deviceName, connectorObject.DeviceCollection);
                    dynamic deviceObject = DeviceFactory.GetDevice(deviceObj);
                    try
                    {
                        TcpClient NewTcpClient = GetNewTcpClient(connectorConn.Name, deviceObject);
                        if (NewTcpClient != null && NewTcpClient.Connected)
                        {
                            string key1 = string.Format("{0}.{1}", deviceObj.ConnectorAssignment, deviceObj.DeviceName);
                            System.Diagnostics.Trace.TraceInformation("Reconnection Successful");
                            CreateModbusIPMaster(key1, NewTcpClient);
                            if (ConnectionHelperObj.tcpClientDictionary.ContainsKey(key1))
                                ConnectionHelperObj.tcpClientDictionary[key1] = NewTcpClient;
                            else
                                ConnectionHelperObj.tcpClientDictionary.Add(key1, NewTcpClient);
                            //retryCount = 3;
                            DeviceBase device = deviceObject as DeviceBase;
                            device.RetryCounter = (int)device.RetryCount;
                        }
                        else
                        {
                            System.Diagnostics.Trace.TraceInformation("Reconnection didn't happen");
                        }

                        if (!ScanrateClientGroup.ContainsKey(key))
                        {
                            ScanrateClientGroup.Add(key, NewTcpClient);

                        }
                        else
                        {
                            ScanrateClientGroup[key] = NewTcpClient;
                        }
                    }
                    catch (Exception)
                    {
                        // throw ex;
                    }
                }
            }
            catch (Exception)
            {
                //throw ex;
            }
        }
        #endregion ReConnectClient

        #region GetNewTcpClient
        /// <summary>
        /// Creates the new tcp client or update tcp client already exists.
        /// </summary>
        /// <param name="protocolObject"></param>
        /// <param name="deviceObject"></param>
        /// <returns></returns>
        private TcpClient GetNewTcpClient(string connectorName, dynamic deviceObject)
        {
            try
            {
                TcpClient newClient = null;
                string key = connectorName + "." + deviceObject.DeviceName;
                if (ConnectionHelperObj.tcpClientDictionary.Keys.Contains(key))
                {
                    newClient = IsConnected(ConnectionHelperObj.tcpClientDictionary[key], deviceObject.IPAddress, deviceObject.Port);
                }
                else
                {
                    newClient = IsConnected(null, deviceObject.IPAddress, deviceObject.Port);
                    //ConnectionHelperObj.tcpClientDictionary.Add(key, newClient);
                }

                if (newClient != null && newClient.Connected == true)
                {
                    //ConnectionHelperObj.tcpClientDictionary[key] = newClient;
                    return newClient;
                }
            }
            catch (Exception)
            {
                //throw ex;
            }

            return null;
        }
        #endregion GetNewTcpClient        

        #region CreateModbusIPMaster
        /// <summary>
        /// It Creates or Updates a Modbus-master in modbus master collection.
        /// </summary>
        /// <param name="deviceName"></param>
        /// <param name="currentTCPClient"></param>
        public void CreateModbusIPMaster(string key, TcpClient currentTCPClient)
        {
            ModbusIpMaster master = ModbusIpMaster.CreateIp(currentTCPClient);
            if (!ConnectionHelperObj.ModbusIPMasterCollection.ContainsKey(key))
            {
                ConnectionHelperObj.ModbusIPMasterCollection.Add(key, master);
            }
            else
            {
                // ConnectionHelperObj.ModbusIPMasterCollection[key] = null;
                ConnectionHelperObj.ModbusIPMasterCollection[key] = master;
            }
        }
        #endregion CreateModbusIPMaster


        #region GetCurrentModbusDevice
        /// <summary>
        /// Get Modbus master from the Modbus master collection based on the device name.
        /// </summary>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        public ModbusIpMaster GetCurrentModbusIPMaster(string deviceName)
        {
            foreach (var master in ConnectionHelperObj.ModbusIPMasterCollection)
            {
                if (deviceName == master.Key)
                {
                    return master.Value;
                }
            }
            return null;
        }
        #endregion GetCurrentModbusDevice



        #region CreateModbusSerialMaster
        /// <summary>
        ///  It Creates or Updates a ModbusSerialMaster.
        /// </summary>
        /// <param name="connectorName"></param>
        /// <param name="deviceName"></param>
        public void CreateModbusSerialMaster(string connectorName, string deviceName)
        {
            try
            {
                string key = string.Format("{0}.{1}", connectorName, deviceName);
                if (!ConnectionHelperObj.ModbusSerialMasterCollection.ContainsKey(key))
                {
                    if (ConnectorCollectionForCommunication.Count > 0)
                    {
                        ModbusSerialConnector connector = ConnectorCollectionForCommunication.FirstOrDefault(c => c.Name == connectorName) as ModbusSerialConnector;
                        ModbusSerialDevice device = connector.DeviceCollection.FirstOrDefault(d => d.DeviceName == deviceName) as ModbusSerialDevice;
                        using (SerialPort port = new SerialPort(device.COMPort.ToUpper(), device.BaudRate, device.ConnectorParityBit, device.DataBits, device.ConnectorStopBits))
                        {
                            // configure serial port
                            //device.Port.BaudRate = device.BaudRate;
                            //device.Port.DataBits = device.DataBits;
                            //device.Port.Parity = device.ConnectorParityBit;
                            //device.Port.StopBits = device.ConnectorStopBits;                   
                            port.Open();
                            port.Handshake = Handshake.RequestToSendXOnXOff;
                            port.ReadTimeout = 1000;
                            //port.WriteTimeout = 500;
                            var adapter = new SerialPortAdapter(port);
                            // create modbus serial master
                            if (device.Port != null)
                            {
                                // device.Port.Close();
                                device.Port.Dispose();
                                device.Port = null;
                            }
                            ModbusSerialMaster master = ModbusSerialMaster.CreateRtu(port);
                            ConnectionHelperObj.ModbusSerialPortCollection.Add(key, port);
                            ConnectionHelperObj.ModbusSerialMasterCollection.Add(key, master);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }

        }

        #endregion CreateModbuSerialPMaster


        #region GetCurrentModbusDevice
        /// <summary>
        /// Get Modbus master from the Modbus Serial master collection based on the Key.
        /// </summary>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        public ModbusSerialMaster GetCurrentModbusSerailMaster(string deviceName)
        {
            foreach (var master in ConnectionHelperObj.ModbusSerialMasterCollection)
            {
                if (deviceName == master.Key)
                {
                    return master.Value;
                }
            }
            return null;
        }
        #endregion GetCurrentModbusDevice

        #region slikServerObject_OnUpdate Event
        private void SlikServerObject_OnUpdate(object sender, SLIKServer.OnUpdateEventArgs eventArgs)
        {
            try
            {
                Debug.Write("Update Call");
                //for (int i = 0; i <= (eventArgs.Count - 1); i++)
                //{
                //    ISLIKTag currentItem = eventArgs.Tags[i];
                //    string address = TagDictionary[currentItem].ToString();
                //    IConnector protocolObject = ConnectorFactory.GetConnector(currentItem.Name, ConnectorCollection);
                //    //protocolObject.Subscribe(address, currentItem, currentItem.Value, protocolObject, tcpClient, master);

                //}
            }
            catch (Exception)
            {
                //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                Addlogs("Configuration", @"Elpis/Configuration/OnUpdate", "Problem in updating tag", LogStatus.Error);
                //}), DispatcherPriority.Normal, null);
            }

        }
        #endregion slikServerObject_OnUpdate Event

        #endregion SLIKDA Events

        #region Main and configuration window functionalities

        #region Configuration window functionalities

        #region  NewConnector Function
        /// <summary>
        /// It Checks if the created connector exist in the connector list.
        /// </summary>
        /// <param name="newConnectorObj"></param>
        /// <param name="connectorType"></param>
        /// <returns></returns>
        public bool NewConnector(Object newConnectorObj, ConnectorType connectorType)
        {
            bool isNewConnector = IsNewConnector(newConnectorObj);
            if (isNewConnector == true)
            {
                if (ConnectorCollection == null)
                    ConnectorCollection = new ObservableCollection<IConnector>();

                if (newConnectorObj != null)
                {
                    switch (connectorType)
                    {
                        case ConnectorType.ModbusEthernet:
                            ModbusEthernetConnector modbusEthernet = newConnectorObj as ModbusEthernetConnector;
                            ConnectorCollection.Add(modbusEthernet);
                            return true;
                        // break;
                        case ConnectorType.ModbusSerial:
                            ModbusSerialConnector modbusSerialObj = newConnectorObj as ModbusSerialConnector;
                            ConnectorCollection.Add(modbusSerialObj);
                            return true;
                        case ConnectorType.ABMicroLogixEthernet:
                            ABMicrologixEthernetConnector ABEthernetObj = newConnectorObj as ABMicrologixEthernetConnector;
                            ConnectorCollection.Add(ABEthernetObj);
                            return true;
                        case ConnectorType.ABControlLogix:
                            ABControlLogicConnector ABControlLogixObj = newConnectorObj as ABControlLogicConnector;
                            ConnectorCollection.Add(ABControlLogixObj);
                            return true;
                        case ConnectorType.TcpSocket:
                            TcpSocketConnector tcpSocketConnector = newConnectorObj as TcpSocketConnector;
                            ConnectorCollection.Add(tcpSocketConnector);
                            return true;

                        default:
                            return false;
                    }
                }
                return false;
            }
            else
            {
                MessageBox.Show("Protocol Name: " + "\"" + NewDeviceName + "\"" + " is already being used !!", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        #region IsNewProtocol
        /// <summary>
        /// Check Connector name is not in existing list.
        /// </summary>
        /// <param name="connectorObj"></param>
        /// <returns></returns>
        public bool IsNewConnector(dynamic connectorObj)
        {
            NewDeviceName = connectorObj.ConnectorName;
            if (ConnectorCollection == null)
                ConnectorCollection = new ObservableCollection<IConnector>();

            //int count = ConnectorCollection.Where(c => c.Name == newName).Count();
            foreach (var protocol in ConnectorCollection)
            {
                if (protocol.Name.ToLower() == NewDeviceName.ToLower())
                {
                    return false;
                }
            }
            return true;
        }
        #endregion IsNewProtocol
        #endregion  NewConnector Function

        #region InstanceMethod
        /// <summary>
        /// 
        /// </summary>
        public void InstanceMethod()
        {
            // Pause for a moment to provide a delay to make
            // threads more apparent.
            Thread.Sleep(3000);
        }
        #endregion InstanceMethod       

        #region  NewDevice Function
        /// <summary>
        /// It Checks if the created Device exist in the device list of a connector.
        /// </summary>
        /// <param name="SelectedConnector"></param>
        /// <param name="connectorType"></param>
        /// <param name="newDeviceObj"></param>
        /// <returns></returns>
        public bool NewDevice(ConnectorBase SelectedConnector, ConnectorType connectorType, Object newDeviceObj)
        {
            bool isNewDevice = IsNewDevice(SelectedConnector, newDeviceObj);
            if (isNewDevice == true)
            {
                #region Modbus Ethernet Device
                if (connectorType == ConnectorType.ModbusEthernet)
                {
                    var deviceObj = newDeviceObj as ModbusEthernetDevice;
                    try
                    {
                        string key = string.Format("{0}.{1}", SelectedConnector.ConnectorName, deviceObj.DeviceName);
                        //connectionHelper.tcpClientDictionary.Add(deviceObj.DeviceName,tcpClient);
                        if (!ConnectionHelperObj.tcpClientDictionary.ContainsKey(key))
                            ConnectionHelperObj.tcpClientDictionary.Add(key, TcpClient);
                        if (SelectedConnector.DeviceCollection == null)
                            SelectedConnector.DeviceCollection = new ObservableCollection<DeviceBase>();
                        SelectedConnector.DeviceCollection.Add(deviceObj);
                        return true;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                #endregion Modbus Ethernet Device

                #region Modbus Serial Device
                else if (connectorType == ConnectorType.ModbusSerial)
                {
                    var deviceObj = newDeviceObj as ModbusSerialDevice;

                    try
                    {
                        if (SelectedConnector.DeviceCollection == null)
                            SelectedConnector.DeviceCollection = new ObservableCollection<DeviceBase>();
                        SelectedConnector.DeviceCollection.Add(deviceObj);
                        return true;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                #endregion Modbus Serial Device

                #region Micrologix Ethernet Device
                else if (connectorType == ConnectorType.ABMicroLogixEthernet)
                {
                    var deviceObj = newDeviceObj as ABMicrologixEthernetDevice;

                    try
                    {
                        if (SelectedConnector.DeviceCollection == null)
                            SelectedConnector.DeviceCollection = new ObservableCollection<DeviceBase>();
                        SelectedConnector.DeviceCollection.Add(deviceObj);
                        return true;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                #endregion Micrologix Ethernet Device
                #region TcpSocket Device
                else if (connectorType == ConnectorType.TcpSocket)
                {
                    var deviceObj = newDeviceObj as TcpSocketDevice;

                    try
                    {
                        if (SelectedConnector.DeviceCollection == null)
                            SelectedConnector.DeviceCollection = new ObservableCollection<DeviceBase>();
                        SelectedConnector.DeviceCollection.Add(deviceObj);
                        return true;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                #endregion TcpSocket Device
                return false;
            }
            else
            {
                MessageBox.Show("Device Name: " + "\"" + NewDeviceName + "\"" + " is already being used !!", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        #region IsNewDevice
        /// <summary>
        /// Check any device with same name or not in device list.
        /// </summary>
        /// <param name="SelectedConnector"></param>
        /// <param name="NewDevice"></param>
        /// <returns></returns>
        public bool IsNewDevice(ConnectorBase SelectedConnector, dynamic NewDevice)
        {
            NewDeviceName = NewDevice.DeviceName;
            if (SelectedConnector.DeviceCollection == null)
                SelectedConnector.DeviceCollection = new ObservableCollection<DeviceBase>();

            foreach (var device in SelectedConnector.DeviceCollection)
            {
                //string existingName = device.Name.ToUpper();
                //string newName = NewDevice.DeviceName.ToUpper();
                if (device.Name.ToLower() == NewDeviceName.ToLower())
                {
                    return false;
                }
            }
            return true;
        }
        #endregion IsNewDevice
        #endregion  NewDevice Function

        #region  NewTag Function
        /// <summary>
        /// It Checks if the created tag exist in the tag list
        /// </summary>
        /// <param name="SelectedDevice"></param>
        /// <param name="SelectedTag"></param>
        /// <returns></returns>
        public bool IsNewTag(dynamic SelectedDevice, dynamic SelectedTag)
        {
            //var val = TagDictionary.Values.Where(s => s == ((ushort.Parse)(SelectedTag.Address.ToString())));

            foreach (var tagKey in TagDictionary)
            {
                string existKey = tagKey.Key.Name.ToUpper();
                string newKey = string.Format("User.{0}.{1}.{2}", SelectedDevice.ConnectorAssignment, SelectedDevice.DeviceName, SelectedTag.TagName).ToUpper();
                if (existKey == newKey)
                {
                    //MessageBox.Show("Tag Name: " + "\"" + SelectedTag.TagName + "\"" + " is already being used !!", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);

                    return false;
                }
            }
            return true;
            //var key = TagDictionary.Keys.Where(s => s.Name == "User." + SelectedTag.TagName);

            //if (key.Count() > 0)
            //{
            //    return false;
            //}
            //return true;
        }

        /// <summary>
        /// Check any tag with same name or not in tag list.
        /// </summary>
        /// <param name="selectedDevice"></param>
        /// <param name="groupCollection"></param>
        /// <param name="currentObject"></param>
        /// <returns></returns>
        public bool IsNewTag(dynamic selectedDevice, ObservableCollection<TagGroup> groupCollection, Tag currentObject)
        {

            int count = groupCollection.Select(g => g.GroupName == currentObject.SelectedGroup).Count();

            string newKey = string.Format("User.{0}.{1}.{2}.{3}", selectedDevice.ConnectorAssignment, selectedDevice.DeviceName, ((Tag)currentObject).SelectedGroup, currentObject.TagName).ToUpper();

            //old code
            //foreach (var tagKey in TagDictionary)
            //{
            //    string existKey = tagKey.Key.Name.ToUpper();

            //    if (existKey == newKey)
            //    {
            //        //MessageBox.Show("Tag Name: " + "\"" + SelectedTag.TagName + "\"" + " is already being used !!", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);

            //        return false;
            //    }
            //}

            var tags = groupCollection.FirstOrDefault(e => e.GroupName == currentObject.SelectedGroup).TagsCollection;

            foreach (var item in tags)
            {
                ITag tag = item as ITag;
                if(tag.Name==currentObject.TagName)
                {
                    return false;
                }


            }

            return true;
          // return tags.FirstOrDefault(e=>e.TagName==currentObject.TagName)!=null?false:true;

            
        }

        /// <summary>
        /// Check tag address is new or any tags with same address
        /// </summary>
        /// <param name="SelectedTag"></param>
        /// <returns></returns>
        public bool IsNewTagAddress(dynamic SelectedTag)
        {
            var val = TagDictionary.Values.Where(s => s == ((int.Parse)(SelectedTag.Address.ToString())));

            //var key = TagDictionary.Keys.Where(s => s.Name == "User." + SelectedTag.TagName);

            if (val.Count() > 0)
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// Check for tag existence
        /// </summary>
        /// <param name="SelectedDevice"></param>
        /// <param name="newTagObj"></param>
        /// <returns></returns>
        public bool NewTag(DeviceBase SelectedDevice, Tag newTagObj)
        {
            int id = 1;
            bool isNewTag;
            if (newTagObj.SelectedGroup == null || newTagObj.SelectedGroup == "None")
                isNewTag = IsNewTag(SelectedDevice, newTagObj);
            else
            {
                isNewTag = IsNewTag(SelectedDevice, SelectedDevice.GroupCollection, newTagObj);
            }


            if (isNewTag == true)
            {
                ITag iTag = newTagObj as ITag;
                iTag.Name = newTagObj.TagName;

                string name = null;
                if (newTagObj.SelectedGroup == null || newTagObj.SelectedGroup == "None")
                    name = string.Format("User.{0}.{1}.{2}", SelectedDevice.ConnectorAssignment, SelectedDevice.DeviceName, newTagObj.TagName);   //"User." + newTagObj.TagName;
                else
                    name = string.Format("User.{0}.{1}.{2}.{3}", SelectedDevice.ConnectorAssignment, SelectedDevice.DeviceName, newTagObj.SelectedGroup, newTagObj.TagName);   //"User." + newTagObj.TagName;
                string address = newTagObj.Address;

                //if (TagDictionary.Keys.Count > 0)
                //{
                //    try
                //    {
                //        var key = TagDictionary.Keys.Where(s => s.Name == name);
                //        var val = TagDictionary.Values.Where(s => s == ((int.Parse)(address.ToString())));
                //        #region commented
                //        //foreach (var tagKey in TagDictionary)
                //        //{
                //        //    string existKey = tagKey.Key.Name.ToUpper();
                //        //    string newKey = name.ToUpper();
                //        //    if (existKey == newKey)
                //        //    {
                //        //        MessageBox.Show("Tag Name: " + "\"" + newTagObj.TagName + "\"" + " is already being used !!", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                //        //        return false;
                //        //    }
                //        //}


                //        //if (val.Count() > 0)
                //        //{
                //        //    MessageBox.Show("Tag Address: " + "\"" + newTagObj.Address + "\"" + " is already being used !!", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                //        //    return false;
                //        //}
                //        //if (key.Count() > 0)
                //        //{
                //        //    MessageBox.Show("Tag with same name is already defined. Please select different tag name", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                //        //    return false;
                //        //}
                //        #endregion
                //    }
                //    catch (Exception)
                //    {
                //        MessageBox.Show("Please Enter the correct Address");
                //        return false;
                //    }
                //}
#if !SunPowerGen
                #region Add Slikda Tag to server
                string key1 = string.Empty;
                if (newTagObj.SelectedGroup == null || newTagObj.SelectedGroup == "None")
                {
                    key1 = string.Format("User.{0}.{1}.{2}", SelectedDevice.ConnectorAssignment, SelectedDevice.DeviceName, newTagObj.TagName);
                }
                else
                {
                    key1 = string.Format("User.{0}.{1}.{2}.{3}", SelectedDevice.ConnectorAssignment, SelectedDevice.DeviceName, newTagObj.SelectedGroup, newTagObj.TagName);

                }

                //if (false) //Changed on 26 dec 2017, for proper updates
                //{
                if (SlikServerObject.ServerStatus != ServerStatusEnum.sdaStatusRunning)
                {
                    try
                    {
                        //readWriteAccess = AccessPermissions.sdaReadAccess;

                        // myOpcTags.Add(name, (int)readWriteAccess, 0, 192, DateTime.Now, null);//change name to devicename+name
                        OpcTags.Add(key1, (int)ReadWriteAccess, 0, 192, DateTime.Now, null);//change name to devicename+name

                        switch (newTagObj.DataType)
                        {
                            case DataType.Boolean:
                                OpcTags[key1].DataType = (short)DataType.Boolean;
                                newTagObj.PrevoiusBooleanTagValue = false;
                                break;
                            case DataType.Integer:
                                OpcTags[key1].DataType = (short)DataType.Integer;
                                newTagObj.PrevoiusIntegerTagValue = 0;
                                break;
                            case DataType.Short:
                                OpcTags[key1].DataType = (short)DataType.Short;
                                newTagObj.PrevoiusIntegerTagValue = 0;
                                break;
                            case DataType.String:
                                OpcTags[key1].DataType = (short)DataType.String;
                                newTagObj.PrevoiusIntegerTagValue = 0;
                                break;
                            case DataType.Double:
                                OpcTags[key1].DataType = (short)DataType.Double;
                                newTagObj.PrevoiusIntegerTagValue = 0;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    try
                    {
                        TagDictionary.Add(OpcTags[key1], (int.Parse)(address.ToString()));
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Elpis OPC Server Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                        //MessageBox.Show("Please Enter the correct IP Address");
                    }
                }
                //  }
                #endregion


                


                //Commented on 06-Mar-2018 by Hari:  ScanrateGroup updating when server is starting
                if (ScanrateGroup.Keys.Count == 0)
                {
                    if (SelectedDevice.TagsCollection != null)
                    {
                        //string keyScanrate = newTagObj.ScanRate.ToString() + SelectedDevice.DeviceName;
                        foreach (var item in SelectedDevice.TagsCollection)
                        {
                            //string presentKey = string.Format("User.{0}.{1}.{2}", SelectedDevice.ProtocolAssignment, SelectedDevice.DeviceName, item.TagName);
                            AddTagtoScanRateList(item, SelectedDevice.ConnectorAssignment, SelectedDevice.DeviceName);
                        }
                    }
                }

                //Add tag to Scan Rate Group
                AddTagtoScanRateList(newTagObj, SelectedDevice.ConnectorAssignment, SelectedDevice.DeviceName);
#endif
                if (SelectedDevice.TagsCollection == null)
                    SelectedDevice.TagsCollection = new ObservableCollection<Tag>();
                TagGroup groupList = GetTagGroup(newTagObj.SelectedGroup, SelectedDevice.GroupCollection); //Tags newTagObj

                if (newTagObj.SelectedGroup != null && newTagObj.SelectedGroup != "None")
                {
                    id = id + groupList.TagsCollection.Count + SelectedDevice.TagsCollection.Count;
                    //newTagObj.SlaveId = Convert.ToByte(id);
                    groupList.TagsCollection.Add(newTagObj);
                    //AddTagtoList(newTagObj, SelectedDevice.ProtocolAssignment, SelectedDevice.DeviceName);

                }
                else
                {
                    id = id + SelectedDevice.TagsCollection.Count;
                    //newTagObj.SlaveId = Convert.ToByte(id);
                    SelectedDevice.TagsCollection.Add(newTagObj);
             
                }

                ////Add tag to Scan Rate Group
                //      AddTagtoScanRateList(newTagObj, SelectedDevice.ConnectorAssignment, SelectedDevice.DeviceName);      

                return true;
            }
            else
            {
                MessageBox.Show("Tag Name: " + "\"" + newTagObj.TagName + "\"" + " is already being used !!", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        /// <summary>
        /// Get the TagGroup object for adding tag to group.
        /// </summary>
        /// <param name="selectedGroup"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        private TagGroup GetTagGroup(string selectedGroup, ObservableCollection<TagGroup> groupCollection)   //Tags tagObject
        {
            foreach (var item in groupCollection)
            {
                if (item.GroupName == selectedGroup)
                {
                    if (item.TagsCollection == null)
                        item.TagsCollection = new ObservableCollection<Tag>();
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// This method add the current SlikDa tag to ScanRateGroup list based on the scan rate of tag.
        /// </summary>
        /// <param name="currentTag"></param>
        /// <param name="key1"></param>
        private void AddTagtoScanRateList(Tag currentTag, string connectorName, string deviceName)
        {
            string presentKey = string.Empty;
            if (currentTag.SelectedGroup != null && currentTag.SelectedGroup != "None")
            {
                presentKey = string.Format("User.{0}.{1}.{2}.{3}", connectorName, deviceName, currentTag.SelectedGroup, currentTag.TagName);
            }
            else
            {
                presentKey = string.Format("User.{0}.{1}.{2}", connectorName, deviceName, currentTag.TagName);
            }

            try
            {
                string keyGroupedScanRate = currentTag.ScanRate.ToString() + "." + connectorName + "." + deviceName;
                ISLIKTag tag = OpcTags[presentKey];
                //ITagAdditionalInfo tagData = (ITagAdditionalInfo)tag;
                //tagData.DeviceName = deviceName;
                //tagData.ProtocolName = protocolName;

                if (ScanrateGroup.Keys.Contains(keyGroupedScanRate))
                {
                    ScanrateGroup[keyGroupedScanRate].Add(tag);
                }
                else
                {
                    ScanrateGroup.Add(keyGroupedScanRate, new List<ISLIKTag>() { tag });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //try
            //{
            //    string keyGroupedScanRate = currentTag.ScanRate.ToString() + "." + deviceName;

            //    if (ScanrateGroup.Keys.Contains(keyGroupedScanRate))
            //    {
            //        ISLIKTag tag = myOpcTags[key1];
            //        ScanrateGroup[keyGroupedScanRate].Add(tag);     
            //        //foreach (var item in ScanrateGroup)
            //        //{
            //        //    if (item.Key == currentTag.ScanRate)
            //        //    {
            //        //        ISLIKTag tag = myOpcTags[key1];
            //        //        item.Value.Add(tag);
            //        //    }
            //        //}
            //    }
            //    else
            //    {
            //        ISLIKTag tag = myOpcTags[key1];
            //        //TagColections.Add(tag);
            //        ScanrateGroup.Add(keyGroupedScanRate, new List<ISLIKTag>() { tag });
            //    }
            //}
            //catch (Exception ex)
            //{

            //}
        }

        #endregion  NewTag Function

        #region  SaveLastLoadedProject Function
        /// <summary>
        /// Save the last loaded configuration to file.
        /// </summary>
        public void SaveLastLoadedProject()
        {
            FileHandle = new FileHandler();
            FileHandle.AllCollectionFileHandling = ConnectorCollection;
            //12 01 2017
            FileHandle.tcpClientCollectionFileHandling = TcpClientCollection;
            //15 03 2107
            FileHandle.MqttCollectionFilHandling = MqttClientCollection;
            FileHandle.AzureIoTFileHandling = AzureIoTCollection;

            var binarySerialized = new Dictionary<string, byte>();
            var binaryDeserialized = new Dictionary<string, TcpClient>();

#if SunPowerGen

            Stream stream = File.Open("opcSunPowerGen.elp", FileMode.OpenOrCreate);
            CurrentProjectFilePath = Directory.GetCurrentDirectory() + "\\opcSunPowerGen.elp";
#else
            Stream stream = File.Open("opcproject.elp", FileMode.OpenOrCreate);
            CurrentProjectFilePath = Directory.GetCurrentDirectory() + "\\opcproject.elp";
#endif
            //Stream stream = File.Open("opcproject.elp", FileMode.OpenOrCreate);
            //CurrentProjectFilePath = Directory.GetCurrentDirectory() + "\\opcproject.elp";

            BinaryFormatter bformatter = new BinaryFormatter();
            try
            {
                using (StreamWriter wr = new StreamWriter(stream))
                {
                    bformatter.Serialize(stream, FileHandle);
                    bformatter.Serialize(stream, TcpClientCollection);
                }

            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                MessageBox.Show(errMsg, "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            stream.Close();
            //to Save into the last  opened file path.
            if (OpenedProjectFilePath != null)
            {
                Stream stream1 = File.Open(OpenedProjectFilePath, FileMode.OpenOrCreate);
                try
                {
                    using (StreamWriter wr = new StreamWriter(stream1))
                    {
                        bformatter.Serialize(stream1, FileHandle);
                        bformatter.Serialize(stream1, TcpClientCollection);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                stream1.Close();
            }
            //if (RunTimeDisplay== "Stop Server")
            //{
            //    System.Windows.Forms.DialogResult dr = (System.Windows.Forms.DialogResult)MessageBox.Show("Update changes in client side.?", "Elpis OPC Server", MessageBoxButton.YesNo, MessageBoxImage.Question);

            //    if (dr.ToString() == "Yes")
            //    {
            //        GetCommunicationElements();
            //    }

            //}

            // slikServerForMainWindowViewModel.UnregisterServer();
            //08-10-2017
            //LoadConfigurationElements(ConnectorCollection);
        }
        #endregion  SaveLastLoadedProject Function

        #region SaveAs
        /// <summary>
        /// Save the configuration file in desired location.
        /// </summary>
        public void SaveAs()
        {
            FileHandle = new FileHandler();
            FileHandle.AllCollectionFileHandling = ConnectorCollection;
            FileHandle.MqttCollectionFilHandling = MqttClientCollection;

            // Displays a SaveFileDialog so the user can save the Image
            // assigned to Button2.
            Microsoft.Win32.SaveFileDialog saveFileDialog1 = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog1.Filter = "Elpis Project File|*.elp|Project File|*.elp|All Files|*.*";
            saveFileDialog1.Title = "Save the current project File";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                // Saves the Image via a FileStream created by the OpenFile method.
                System.IO.FileStream fs =
                   (System.IO.FileStream)saveFileDialog1.OpenFile();

                BinaryFormatter bformatter = new BinaryFormatter();
                try
                {
                    using (StreamWriter wr = new StreamWriter(fs))
                    {
                        bformatter.Serialize(fs, FileHandle);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                //stream.Close();
                fs.Close();
            }
        }

        #endregion SaveAs

        #region  OpenLastLoadedProject Function
        public void OpenLastLoadedProject()
        {
#if SunPowerGen
            string projectFilePath = string.Format(@"{0}\opcSunPowerGen.elp", Directory.GetCurrentDirectory());
#else
            string projectFilePath = string.Format(@"{0}\opcproject.elp", Directory.GetCurrentDirectory());
#endif

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
                            //stream.Close();
                            ConnectorCollection = FileHandle.AllCollectionFileHandling;
                            MqttClientCollection = FileHandle.MqttCollectionFilHandling;
                            AzureIoTCollection = FileHandle.AzureIoTFileHandling;
#if !SunPowerGen
                            LoadConfigurationElements(ConnectorCollection);
#endif
                            LoadDataLogCollection("All", "Elpis OPC Server//Configuration",
                                string.Format("project is loaded successfully from Project file:" + projectFilePath), LogStatus.Information);
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
            // GetCommunicationElements();

        }
        #endregion  OpenLastLoadedProject Function       

        #region LoadConfigurationElements
        /// <summary>
        /// Load the Configuration elements from the connector collection
        /// </summary>
        /// <param name="connectorCollection"></param>
        public void LoadConfigurationElements(ObservableCollection<IConnector> connectorCollection)
        {
            TagCount = 0;
            if (OpcTags != null)
                OpcTags.Clear();
            TagDictionary.Clear();
            ScanrateGroup.Clear();
            if (connectorCollection != null)
            {
                if (connectorCollection.Count > 0)
                {
                    for (int i = 0; i < connectorCollection.Count; i++)
                    {
                        IConnector connector = connectorCollection[i] as IConnector;
                        dynamic connectorObject = ConnectorFactory.GetConnector(connector);

                        if (connectorObject != null)
                        {

                            try
                            {
                                Thread currentThread = new Thread(InstanceMethod);
                                currentThread.Name = connectorObject.ConnectorName;
                                if (!threadHelper.ThreadCollection.ContainsKey(connectorObject.ConnectorName))
                                {
                                    threadHelper.ThreadCollection.Add(connectorObject.ConnectorName, currentThread);
                                }

                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }

                            if (connectorObject.DeviceCollection == null)
                            {
                                connectorObject.DeviceCollection = new ObservableCollection<DeviceBase>();
                            }
                            for (int j = 0; j < connectorObject.DeviceCollection.Count; j++)
                            {
                                DeviceBase device = connectorObject.DeviceCollection[j] as DeviceBase;
                                DeviceBase deviceObj = DeviceFactory.GetDevice(device);

                                //11 01 2017
                                if (deviceObj != null)
                                {
                                    try
                                    {
                                        deviceObj.ConnectorAssignment = connectorObject.ConnectorName;
                                        string key = string.Format("{0}.{1}", connector.Name, deviceObj.DeviceName);
                                        //connectionHelper.tcpClientDictionary.Add(deviceObj.DeviceName, tcpClient);
                                        if (device.DeviceType == DeviceType.ModbusEthernet)
                                        {
                                            if (!ConnectionHelperObj.tcpClientDictionary.ContainsKey(key))
                                            {
                                                ConnectionHelperObj.tcpClientDictionary.Add(key, TcpClient);
                                            }
                                        }

                                        else if (device.DeviceType == DeviceType.ModbusSerial)
                                        {

                                            ModbusSerialDevice serialDevice = device as ModbusSerialDevice;
                                            if (serialDevice != null)
                                            {
                                                if (!ConnectionHelperObj.ModbusSerialMasterCollection.ContainsKey(key))
                                                {
                                                    CreateModbusSerialMaster(serialDevice.ConnectorAssignment, serialDevice.DeviceName);
                                                    // serialDevice.Master = ConnectionHelperObj.ModbusSerialMasterCollection[key];
                                                    serialDevice.Port = ConnectionHelperObj.ModbusSerialPortCollection[key];

                                                }
                                                else
                                                {
                                                    //ConnectionHelperObj.ModbusSerialMasterCollection.Remove(key);
                                                    //CreateModbusSerialMaster(serialDevice.ConnectorAssignment, serialDevice.DeviceName);
                                                    //serialDevice.Master = ConnectionHelperObj.ModbusSerialMasterCollection[key];
                                                    serialDevice.Port = ConnectionHelperObj.ModbusSerialPortCollection[key];
                                                }
                                            }
                                        }

                                    }
                                    catch (Exception e)
                                    {
                                        string errMessage = e.Message;
                                    }
#if !SunPowerGen
                                    ObservableCollection<TagGroup> tagGroups = deviceObj.GroupCollection;
                                    if (tagGroups == null)
                                    {
                                        tagGroups = new ObservableCollection<TagGroup>();
                                    }
                                    else
                                    {
                                        foreach (var group in tagGroups)
                                        {
                                            OPCTagCreation(deviceObj, group.TagsCollection, group.GroupName);
                                        }

                                    }
                                    ObservableCollection<Tag> tag = deviceObj.TagsCollection;
                                    if (tag == null)
                                    {
                                        tag = new ObservableCollection<Tag>();
                                    }
                                    if (tag != null && tag.Count > 0)
                                    {
                                        OPCTagCreation(deviceObj, tag);
                                    }
#endif
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This creates OPC Tags for the tag group.
        /// </summary>
        /// <param name="deviceObj"></param>
        /// <param name="tagsCollection"></param>
        /// <param name="groupName"></param>
        private void OPCTagCreation(dynamic deviceObj, ObservableCollection<Tag> tagsCollection, string groupName)
        {
            for (int k = 0; k < tagsCollection.Count; k++)
            {
                string address = tagsCollection[k].Address;
                string value = string.Format("User.{0}.{1}.{2}.{3}", deviceObj.ConnectorAssignment, deviceObj.DeviceName, groupName, tagsCollection[k].TagName);
                try
                {
                    if (OpcTags.Count == TagCount)
                    {
                        TagCount++;

                        OpcTags.Add(value, (int)ReadWriteAccess, (int)0, 192, DateTime.Now, null);

                        switch (tagsCollection[k].DataType)
                        {
                            case DataType.Boolean:
                                OpcTags[value].DataType = (short)DataType.Boolean;
                                break;
                            case DataType.Integer:
                                OpcTags[value].DataType = (short)DataType.Integer;

                                break;
                            case DataType.Short:
                                OpcTags[value].DataType = (short)DataType.Short;

                                break;
                            case DataType.String:
                                OpcTags[value].DataType = (short)DataType.String;
                                break;
                            case DataType.Double:
                                OpcTags[value].DataType = (short)DataType.Double;
                                break;
                        }

                    }
                    if (!TagDictionary.ContainsKey(OpcTags[value]))
                    {
                        TagDictionary.Add(OpcTags[value], ((int.Parse)(tagsCollection[k].Address.ToString())));
                        AddTagtoScanRateList(tagsCollection[k], deviceObj.ConnectorAssignment, deviceObj.DeviceName);
                    }



                }
                catch (Exception e)
                {
                    //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    //{
                    Addlogs("Configuration", @"Elpis/Communication", e.Message, LogStatus.Error);
                    //}), DispatcherPriority.Normal, null);
                }
            }
        }

        /// <summary>
        /// It creates OPC Tags for the Tag.
        /// </summary>
        /// <param name="deviceObj"></param>
        /// <param name="tag"></param>
        private void OPCTagCreation(dynamic deviceObj, ObservableCollection<Tag> tag)
        {
            for (int k = 0; k < tag.Count; k++)
            {
                // string tagName = "User." + tag[k].TagName;
                //string tagName = tag[k].TagName;

                string address = tag[k].Address;
                #region old code
                //if (TagDictionary.Keys.Count > 0)
                //{
                //    var val = TagDictionary.Values.Where(s => s == ((ushort.Parse)(address.ToString())));
                //    var key = TagDictionary.Keys.Where(s => s.Name == tagName);
                //    if (val.Count() > 0)
                //    {
                //        //MessageBox.Show("Tag is already defined for this address. Please select different address", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                //        return;
                //    }
                //    else if (key.Count() > 0)
                //    {
                //        MessageBox.Show("Tag with same name is already defined. Please select different tag name", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                //        return;
                //    }
                //}
                #endregion
                string value = string.Format("User.{0}.{1}.{2}", deviceObj.ConnectorAssignment, deviceObj.DeviceName, tag[k].TagName);

                try
                {
                    if (OpcTags.Count == TagCount)
                    {

                        OpcTags.Add(value, (int)ReadWriteAccess, (int)0, 192, DateTime.Now, null);

                        switch (tag[k].DataType)
                        {
                            case DataType.Boolean:
                                OpcTags[value].DataType = (short)DataType.Boolean;
                                break;
                            case DataType.Integer:
                                OpcTags[value].DataType = (short)DataType.Integer;

                                break;
                            case DataType.Short:
                                OpcTags[value].DataType = (short)DataType.Short;

                                break;
                            case DataType.String:
                                OpcTags[value].DataType = (short)DataType.String;
                                break;
                            case DataType.Double:
                                OpcTags[value].DataType = (short)DataType.Double;
                                break;
                        }

                    }
                    if (!TagDictionary.ContainsKey(OpcTags[value]))
                    {
                        TagDictionary.Add(OpcTags[value], ((int.Parse)(tag[k].Address.ToString())));
                        AddTagtoScanRateList(tag[k], deviceObj.ConnectorAssignment, deviceObj.DeviceName);
                    }
                    TagCount++;
                }
                catch (Exception e)
                {
                    //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    //{
                    Addlogs("Configuration", @"ElpisServer/OPCTagsCreation", e.Message, LogStatus.Error);
                    //}), DispatcherPriority.Normal, null);
                }
            }
        }

        #endregion LoadConfigurationElements

        #region DeleteConnector Function
        /// <summary>
        /// To Delete the Connector from the list of Connector
        /// </summary>
        /// <param name="connectorToDelete"></param>
        public void DeleteConnector(dynamic connectorToDelete) //TODO: Refactor Code. Remove Opctags for TagsGroups and Device Tags Collection.
        {
            try
            {
                for (int i = 0; i < ConnectorCollection.Count; i++)
                {
                    if (connectorToDelete.ConnectorName.ToLower() == ConnectorCollection[i].Name.ToLower())
                    {
                        dynamic connectorObj = ConnectorFactory.GetConnector(connectorToDelete);

                        if (connectorObj.DeviceCollection != null)
                        {
                            for (int j = connectorObj.DeviceCollection.Count; j > 0; j--)
                            {
                                DeviceBase device = connectorObj.DeviceCollection[j - 1] as DeviceBase;
                                dynamic deviceObj = DeviceFactory.GetDevice(device);
                                DeleteDeviceformConnector(deviceObj);
                                connectorObj.DeviceCollection.Remove(device);
                                //connectionHelper.tcpClientDictionary.Remove(deviceObj.DeviceName);
                                //ConnectionHelperObj.tcpClientDictionary.Remove(connectorObj.ConnectorName + "." + deviceObj.DeviceName);
                                //ConnectionHelperObj.ModbusIPMasterCollection.Remove(deviceObj.DeviceName);
                            }

                        }
                        //if (ConnectorCollection.Count() == 1)
                        //    ConnectorCollection.Clear();
                        //else
                        //{
                        IConnector iConnector = connectorToDelete as IConnector;
                        ConnectorCollection.Remove(connectorToDelete);

                        //if (OpcTags != null)
                        //{
                        //    OpcTags.Clear();
                        //    ScanrateGroup.Clear();
                        //    TagDictionary.Clear(); ;
                        //    SlikServerObject.SLIKTags.Clear();
                        //}
                        // }

                        LoadDataLogCollection("Configuration", @"Elpis OPC Serve/Configuration", string.Format("Connector with Name:{0} Deleted Successfully.", connectorToDelete.ConnectorName), LogStatus.Information);
                        return;
                    }
                }

            }
            catch (Exception ex)
            {
                LoadDataLogCollection("Configuration", @"Elpis OPC Serve/Configuration", string.Format("Connector with Name:{0} error:{1}", connectorToDelete.ConnectorName,ex.Message), LogStatus.Error);

            }
        }

        private void DeleteDeviceformConnector(dynamic deviceObj)
        {
            if (deviceObj.TagsCollection != null)
            {
                ObservableCollection<Tag> tagsCollection = deviceObj.TagsCollection;
                for (int tc = tagsCollection.Count; tc > 0; tc--)
                {
                    Tag tagToDelete = tagsCollection[tc - 1];
                    DeleteTagfromDevice(deviceObj, tagToDelete);

                    //slikServerObject.SLIKTags
                    deviceObj.TagsCollection.Remove(tagToDelete);
                }
            }
            if (deviceObj.GroupCollection != null)
            {
                ObservableCollection<TagGroup> tagGroupsCollection = deviceObj.GroupCollection;
                for (int tgc = tagGroupsCollection.Count; tgc > 0; tgc--)
                {
                    TagGroup tagGroup = tagGroupsCollection[tgc - 1];
                    ObservableCollection<Tag> tagCollection = tagGroup.TagsCollection;
                    for (int tagIndex = tagCollection.Count - 1; tagIndex >= 0; tagIndex--)
                    {
                        Tag tag = tagCollection[tagIndex];
                        DeleteTagfromDevice(deviceObj, tag);
                        tagGroup.TagsCollection.Remove(tag);
                    }
                    deviceObj.GroupCollection.Remove(tagGroup);
                }

            }

            string deviceKey = string.Format("{0}.{1}", deviceObj.ConnectorAssignment, deviceObj.DeviceName);
            if (deviceObj.DeviceType == DeviceType.ModbusEthernet)
            {
                if (ConnectionHelperObj.tcpClientDictionary != null && ConnectionHelperObj.tcpClientDictionary.ContainsKey(deviceKey))
                    ConnectionHelperObj.tcpClientDictionary.Remove(deviceKey);
                if (ConnectionHelperObj.ModbusIPMasterCollection != null && ConnectionHelperObj.ModbusIPMasterCollection.ContainsKey(deviceKey))
                    ConnectionHelperObj.ModbusIPMasterCollection.Remove(deviceKey);
            }
            else if (deviceObj.DeviceType == DeviceType.ModbusSerial)
            {
                ModbusSerialDevice serialDevice = deviceObj as ModbusSerialDevice;
                if (serialDevice.Port != null)
                {
                    serialDevice.Port.Dispose();
                    serialDevice.Port = null;
                }

                if (ConnectionHelperObj.ModbusSerialMasterCollection.ContainsKey(deviceKey))
                {
                    ConnectionHelperObj.ModbusSerialMasterCollection.Remove(deviceKey);
                }
                if (ConnectionHelperObj.ModbusSerialPortCollection.ContainsKey(deviceKey))
                {
                    ConnectionHelperObj.ModbusSerialPortCollection[deviceKey].Dispose();
                    ConnectionHelperObj.ModbusSerialPortCollection.Remove(deviceKey);
                }
            }

        }

        private void DeleteTagfromDevice(dynamic deviceObj, Tag tagToDelete)
        {
            if (OpcTags != null)
            {
                string delTagName = string.Empty;
                if (tagToDelete.SelectedGroup == null || tagToDelete.SelectedGroup == "None")
                    delTagName = string.Format("User.{0}.{1}.{2}", deviceObj.ConnectorAssignment, deviceObj.DeviceName, tagToDelete.TagName);
                else
                    delTagName = string.Format("User.{0}.{1}.{2}.{3}", deviceObj.ConnectorAssignment, deviceObj.DeviceName, tagToDelete.SelectedGroup, tagToDelete.TagName);

                foreach (var tagSlikda in OpcTags)
                {
                    try
                    {
                        ISLIKTag tags = tagSlikda as ISLIKTag;
                        if (tags.Name.ToLower() == delTagName.ToLower())
                        {
                            TagDictionary.Remove(tags);
                            SlikServerObject.SLIKTags.Remove(tags.Name);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                try
                {
                    OpcTags.Remove(delTagName);
                    string keytoRemove = string.Format("{0}.{1}.{2}", tagToDelete.ScanRate, deviceObj.ConnectorAssignment, deviceObj.DeviceName);
                    RemoveTagfromScanRateGroup(keytoRemove, tagToDelete);

                }
                catch (Exception)
                {
                    //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    //{
                    Addlogs("Configuration", @"ElpisServer/Configuration", "Problem in deleting Connector", LogStatus.Error);
                    //}), DispatcherPriority.Normal, null);
                }
            }
        }

        #endregion DeleteConnector Function

        #region DeleteDevice Function
        /// <summary>
        /// To Delete the device from the list of Devices
        /// </summary>
        /// <param name="DeviceToDelete"></param>
        public void DeleteDevice(dynamic DeviceToDelete)
        {
            try
            {
                for (int i = 0; i < ConnectorCollection.Count; i++)
                {
                    IConnector connector = ConnectorCollection[i] as IConnector;
                    dynamic connectorObj = ConnectorFactory.GetConnector(connector);

                    if (DeviceToDelete.ConnectorAssignment.ToLower() == connectorObj.ConnectorName.ToLower() && connectorObj.DeviceCollection != null)
                    {
                        for (int j = 0; j < connectorObj.DeviceCollection.Count; j++)
                        {
                            DeviceBase device = connectorObj.DeviceCollection[j] as DeviceBase;
                            dynamic deviceObj = DeviceFactory.GetDevice(device);

                            if (DeviceToDelete.DeviceName == device.Name)
                            {

                                DeleteDeviceformConnector(deviceObj);
                                connectorObj.DeviceCollection.Remove(DeviceToDelete);
                                LoadDataLogCollection("Configuration", @"Elpis OPC Serve/Configuration", string.Format("Device with Name:{0} Deleted Successfully.", DeviceToDelete.DeviceName), LogStatus.Information);
                                return;

                                #region old code
                                ////to delete all the tag from the device first
                                //if (DeviceToDelete.TagsCollection != null)
                                //{

                                //foreach (var tagToDelete in DeviceToDelete.TagsCollection)
                                //{
                                //    string tagName = string.Format("User.{0}.{1}.{2}", DeviceToDelete.ConnectorAssignment, DeviceToDelete.DeviceName, tagToDelete.TagName);
                                //    if (OpcTags!=null && OpcTags.Count > 0)
                                //    {
                                //        foreach (var tagSlikda in OpcTags)
                                //        {
                                //            ISLIKTag tags = tagSlikda as ISLIKTag;

                                //            if (tags.Name.ToLower() == tagName.ToLower())
                                //            {
                                //                TagDictionary.Remove(tags);
                                //                //slikServerObject.SLIKTags.Remove(tags.Name);
                                //            }
                                //        }
                                //        try
                                //        {
                                //            OpcTags.Remove(tagName);
                                //            string keytoRemove = string.Format("{0}.{1}.{2}", tagToDelete.ScanRate, deviceObj.ConnectorAssignment, deviceObj.DeviceName);
                                //            ScanrateGroup.Remove(keytoRemove);
                                //            //slikServerObject.SLIKTags
                                //        }
                                //        catch (Exception e)
                                //        {
                                //            throw e;
                                //        }
                                //    }
                                //}
                                //DeviceToDelete.TagsCollection.Clear();
                                //DeviceToDelete.GroupCollection.Clear(); //TODO: Clear TagGroups TagsCollection and OPC tags from OpcTags Collection.
                                //connectorObj.DeviceCollection.Remove(DeviceToDelete);
                                //if (device.DeviceType == DeviceType.ModbusEthernet)
                                //{
                                //    ConnectionHelperObj.tcpClientDictionary.Remove(DeviceToDelete.ConnectorAssignment + "." + DeviceToDelete.DeviceName);
                                //    ConnectionHelperObj.ModbusIPMasterCollection.Remove(DeviceToDelete.ConnectorAssignment + "." + DeviceToDelete.DeviceName);
                                //}
                                //else if (device.DeviceType == DeviceType.ModbusSerial)
                                //{
                                //    ModbusSerialDevice serialDevice = device as ModbusSerialDevice;
                                //    if (serialDevice.Port != null)
                                //    {
                                //        serialDevice.Port.Dispose();
                                //        serialDevice.Port = null;
                                //    }

                                //    if (ConnectionHelperObj.ModbusSerialMasterCollection.ContainsKey(DeviceToDelete.ConnectorAssignment + "." + DeviceToDelete.DeviceName))
                                //    {
                                //        ConnectionHelperObj.ModbusSerialMasterCollection.Remove(DeviceToDelete.ConnectorAssignment + "." + DeviceToDelete.DeviceName);
                                //    }
                                //    if (ConnectionHelperObj.ModbusSerialPortCollection.ContainsKey(DeviceToDelete.ConnectorAssignment + "." + DeviceToDelete.DeviceName))
                                //    {
                                //        ConnectionHelperObj.ModbusSerialPortCollection[DeviceToDelete.ConnectorAssignment + "." + DeviceToDelete.DeviceName].Dispose();
                                //        ConnectionHelperObj.ModbusSerialPortCollection.Remove(DeviceToDelete.ConnectorAssignment + "." + DeviceToDelete.DeviceName);
                                //    }

                                //}


                                //DeleteDeviceformConnector(deviceObj);
                                //connectorObj.DeviceCollection.Remove(DeviceToDelete);
                                //LoadDataLogCollection("Configuration", @"Elpis OPC Serve/Configuration", string.Format("Device with Name:{0} Deleted Successfully.", DeviceToDelete.DeviceName), LogStatus.Information);
                                //return;
                                //}
                                //else
                                //{
                                //    connectorObj.DeviceCollection.Remove(DeviceToDelete);
                                //    //ConnectionHelperObj.tcpClientDictionary.Remove(DeviceToDelete.ConnectorAssignment + "." + DeviceToDelete.DeviceName);
                                //    //ConnectionHelperObj.ModbusIPMasterCollection.Remove(DeviceToDelete.DeviceName);
                                //    string deviceKey = string.Format("{0}.{1}", DeviceToDelete.ConnectorAssignment, DeviceToDelete.DeviceName);
                                //    if (device.DeviceType == DeviceType.ModbusEthernet)
                                //    {
                                //        if (ConnectionHelperObj.tcpClientDictionary != null && ConnectionHelperObj.tcpClientDictionary.ContainsKey(deviceKey))
                                //            ConnectionHelperObj.tcpClientDictionary.Remove(deviceKey);
                                //        if (ConnectionHelperObj.ModbusIPMasterCollection != null && ConnectionHelperObj.ModbusIPMasterCollection.ContainsKey(deviceKey))
                                //            ConnectionHelperObj.ModbusIPMasterCollection.Remove(deviceKey);
                                //    }
                                //    else if (device.DeviceType == DeviceType.ModbusSerial)
                                //    {
                                //        ModbusSerialDevice serialDevice = device as ModbusSerialDevice;
                                //        if (serialDevice.Port != null)
                                //        {
                                //            serialDevice.Port.Dispose();
                                //            serialDevice.Port = null;
                                //        }

                                //        if (ConnectionHelperObj.ModbusSerialMasterCollection.ContainsKey(deviceKey))
                                //        {
                                //            ConnectionHelperObj.ModbusSerialMasterCollection.Remove(deviceKey);
                                //        }
                                //        if (ConnectionHelperObj.ModbusSerialPortCollection.ContainsKey(deviceKey))
                                //        {
                                //            ConnectionHelperObj.ModbusSerialPortCollection[deviceKey].Dispose();
                                //            ConnectionHelperObj.ModbusSerialPortCollection.Remove(deviceKey);
                                //        }
                                //    }
                                //    LoadDataLogCollection("Configuration", @"Elpis OPC Serve/Configuration", string.Format("Device with Name:{0} Deleted Successfully.", DeviceToDelete.DeviceName), LogStatus.Information);
                                //    return;
                                //}
                                #endregion
                            }
                            //to delete the selected device 
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoadDataLogCollection("Configuration", @"Elpis OPC Serve/Configuration", string.Format("Error in deleting Device: {0}  Error :{1}", DeviceToDelete.DeviceName,ex.Message), LogStatus.Information);
            }
        }

        #endregion DeleteDevice Function

        #region DeleteTag Function
        /// <summary>
        /// To Delete the tag from the list of tags
        /// </summary>
        /// <param name="SelectedTag"></param>
        public void DeleteTag(Tag SelectedTag,string connectorName, string deviceName)  //TODO: Refactor and Check DeleteTag.
        {
            try
            {
                for (int i = 0; i < ConnectorCollection.Count; i++)
                {
                    IConnector connector = ConnectorCollection[i] as IConnector;
                    dynamic connectorObj = ConnectorFactory.GetConnector(connector);

                    if (connectorObj.ConnectorName==connectorName && connectorObj.DeviceCollection != null)
                    {
                        for (int j = 0; j < connectorObj.DeviceCollection.Count; j++)
                        {
                            DeviceBase device = connectorObj.DeviceCollection[j] as DeviceBase;
                            if (device.DeviceName.ToLower() == deviceName.ToLower())
                            {
                                dynamic deviceObj = DeviceFactory.GetDevice(device);
                                string tagName = string.Empty;
                                ObservableCollection<Tag> tagsCollection = null;
                                if (SelectedTag.SelectedGroup == null || SelectedTag.SelectedGroup == "None")
                                {
                                    tagName = string.Format("User.{0}.{1}.{2}", deviceObj.ConnectorAssignment, deviceObj.DeviceName, SelectedTag.TagName);
                                    tagsCollection = deviceObj.TagsCollection;
                                }
                                else
                                {
                                    tagName = string.Format("User.{0}.{1}.{2}.{3}", deviceObj.ConnectorAssignment, deviceObj.DeviceName, SelectedTag.SelectedGroup, SelectedTag.TagName);
                                    if (deviceObj.GroupCollection != null)
                                    {
                                        TagGroup tagGroup = GetTagGroup1(((DeviceBase)deviceObj).GroupCollection, SelectedTag.SelectedGroup);
                                        if (tagGroup != null)
                                        {
                                            tagsCollection = tagGroup.TagsCollection;
                                        }
                                    }
                                }

                                if (tagsCollection != null)  //if (deviceObj.TagsCollection != null)
                                {
                                    for (int k = 0; k < tagsCollection.Count; k++) //for (int k = 0; k < deviceObj.TagsCollection.Count; k++)
                                    {
                                        if (SelectedTag.TagName.ToLower() == tagsCollection[k].TagName.ToLower())//if (SelectedTag.TagName.ToLower() == deviceObj.TagsCollection[k].TagName.ToLower())
                                        {
                                            tagsCollection.Remove(SelectedTag); //deviceObj.TagsCollection.Remove(SelectedTag);
                                            if (OpcTags != null && OpcTags.Count > 0)
                                            {
                                                foreach (var tag in OpcTags)
                                                {
                                                    ISLIKTag tags = tag as ISLIKTag;
                                                    //string tagName = string.Format("User.{0}.{1}.{2}", deviceObj.ProtocolAssignment, deviceObj.DeviceName, SelectedTag.TagName);
                                                    if (tags.Name.ToLower() == tagName.ToLower())
                                                    {
                                                        TagDictionary.Remove(tags);
                                                    }
                                                }
                                                try
                                                {
                                                    //string tagName = string.Format("User.{0}.{1}.{2}", deviceObj.ProtocolAssignment, deviceObj.DeviceName, SelectedTag.TagName);

                                                    string keytoRemove = string.Format("{0}.{1}.{2}", SelectedTag.ScanRate, deviceObj.ConnectorAssignment, deviceObj.DeviceName);
                                                    RemoveTagfromScanRateGroup(keytoRemove, SelectedTag);
                                                    OpcTags.Remove(tagName);
                                                    //ScanrateGroup.Remove(keytoRemove);
                                                }
                                                catch (Exception e)
                                                {
                                                    throw e;
                                                }
                                            }
                                            LoadDataLogCollection("Configuration", @"Elpis OPC Serve/Configuration", string.Format("Tag with Name:{0} Deleted Successfully.", SelectedTag.TagName), LogStatus.Information);
                                            return;
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoadDataLogCollection("Configuration", @"Elpis OPC Serve/Configuration", string.Format("Error in Deleting Tag. {0}", ex.Message), LogStatus.Error);
            }

        }

        private void RemoveTagfromScanRateGroup(string keytoRemove, Tag selectedTag)
        {
            foreach (var item in ScanrateGroup)
            {
                if (item.Key == keytoRemove)
                {
                    string tagtoDelete = string.Empty;
                    string[] protocolDevice = keytoRemove.Split('.');
                    if (selectedTag.SelectedGroup == null || selectedTag.SelectedGroup == "None")
                        tagtoDelete = string.Format("User.{0}.{1}.{2}", protocolDevice[1], protocolDevice[2], selectedTag.TagName);
                    else
                        tagtoDelete = string.Format("User.{0}.{1}.{2}.{3}", protocolDevice[1], protocolDevice[2], selectedTag.SelectedGroup, selectedTag.TagName);
                    ISLIKTag deletingTag = OpcTags[tagtoDelete];
                    item.Value.Remove(deletingTag);
                    //foreach (var sliktag in item.Value)
                    //{
                    //    if(sliktag.Name==tagtoDelete)
                    //    {
                    //        item.Value.Remove(sliktag);
                    //        break;
                    //    }
                    //}
                    break;
                }
            }
        }

        private TagGroup GetTagGroup1(ObservableCollection<TagGroup> groupCollection, string selectedGroup)
        {
            foreach (var group in groupCollection)
            {
                if (group.GroupName == selectedGroup)
                {
                    return group;
                }
            }
            return null;
        }

        #endregion DeleteTag Function

        #region Delete Tag Group
        public void DeleteTagGroup(TagGroup selectedGroup, ObservableCollection<DeviceBase> deviceCollection)
        {
            try
            {
                var device = DeviceFactory.GetDeviceByName(selectedGroup.DeviceName, deviceCollection);
                DeviceBase deviceObject = DeviceFactory.GetDevice(device);
                for (int i = 0; i < deviceObject.GroupCollection.Count; i++)
                {
                    if (deviceObject.GroupCollection[i].GroupName == selectedGroup.GroupName)
                    {
                        for (int j = 0; j < deviceObject.GroupCollection[i].TagsCollection.Count; j++)
                        {
                            if (OpcTags != null)
                            {
                                string keytoDelete = string.Format("User.{0}.{1}.{2}.{3}", deviceObject.ConnectorAssignment, deviceObject.DeviceName, selectedGroup.GroupName, deviceObject.GroupCollection[i].TagsCollection[j].TagName);
                                ISLIKTag tag = OpcTags[keytoDelete];
                                TagDictionary.Remove(tag);
                                OpcTags.Remove(keytoDelete);
                            }
                            deviceObject.GroupCollection[i].TagsCollection.Remove(deviceObject.GroupCollection[i].TagsCollection[j]);
                            //keytoDelete = null;
                        }
                        deviceObject.GroupCollection.Remove(deviceObject.GroupCollection[i]);
                        LoadDataLogCollection("Configuration", @"Elpis OPC Serve/Configuration", string.Format("Tag Group with Name:{0} Deleted Successfully.", selectedGroup.GroupName), LogStatus.Information);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region Edit Connector Function
        /// <summary>
        /// To Edit the Protocol from the list of protocols
        /// </summary>
        /// <param name="ProtocolToDelete"></param>
        public bool EditConnector(ConnectorBase SelectedConnector)
        {
            try
            {
                if (SelectedConnector != null)
                {
                    IConnector iConnector = SelectedConnector as IConnector;
                    var oldConnectorName = iConnector.Name;

                    bool isNewConnector = IsNewConnector(SelectedConnector);
                    if (isNewConnector)
                    {
                        //ad

                        for (int i = 0; i < SelectedConnector.DeviceCollection.Count; i++)
                        {
                            SelectedConnector.DeviceCollection[i].ConnectorAssignment = SelectedConnector.ConnectorName;
                        }
                        List<string> listItems = ScanrateGroup.Keys.ToList();
                        foreach (var item in listItems)
                        {
                            if (ScanrateGroup.ContainsKey(item))
                            {
                                var value = ScanrateGroup[item];
                                ScanrateGroup.Remove(item);
                                string key = item.Replace(oldConnectorName, SelectedConnector.ConnectorName);
                                ScanrateGroup.Add(key, value);
                            }
                        }
                        iConnector.Name = SelectedConnector.ConnectorName;
                        return true;
                    }
                    else
                    {
                        if (oldConnectorName.ToLower() != SelectedConnector.ConnectorName.ToLower())
                        {
                            MessageBox.Show("Connector Name: " + "\"" + SelectedConnector.ConnectorName + "\"" + " is already being used !!", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                            SelectedConnector.ConnectorName = oldConnectorName;//iprotocol

                            //iProtocol.Name=
                            // MessageBox.Show("Connector with same name already exists, use another name.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);

                            return false;
                        }
                        //else
                        //{                          

                        //    return true;
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return false;
        }
        #endregion Edit Connector Function

        #region Edit Device Function
        public bool EditDevice(ConnectorBase SelectedConnector, dynamic SelectedDevice)
        {
            if (SelectedDevice != null)
            {
                DeviceBase iDevice = SelectedDevice as DeviceBase;
                var oldDeviceName = iDevice.Name;
                if (oldDeviceName != SelectedDevice.DeviceName)
                {
                    bool isNewDevice = IsNewDevice(SelectedConnector, SelectedDevice);
                    if (isNewDevice)
                    {
                        List<string> listItems = ScanrateGroup.Keys.ToList();
                        foreach (var item in listItems)
                        {
                            if (ScanrateGroup.ContainsKey(item))
                            {
                                var value = ScanrateGroup[item];
                                ScanrateGroup.Remove(item);
                                string key = item.Replace(oldDeviceName, SelectedDevice.DeviceName);
                                ScanrateGroup.Add(key, value);
                            }
                        }
                        iDevice.Name = SelectedDevice.DeviceName;
                        ConnectionHelperObj.tcpClientDictionary.Remove(SelectedConnector.ConnectorName + "." + oldDeviceName);

                        ConnectionHelperObj.tcpClientDictionary.Add(SelectedConnector.ConnectorName + "." + SelectedDevice.DeviceName, TcpClient);

                        ConnectionHelperObj.ModbusIPMasterCollection.Remove(oldDeviceName);
                        return true;
                        //EditDevice(oldDeviceName, newDevice.DeviceName);
                    }
                    else
                    {
                        if (oldDeviceName != SelectedDevice.DeviceName)
                        {
                            MessageBox.Show("Device Name: " + "\"" + SelectedDevice.DeviceName + "\"" + " is already being used !!", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                            SelectedDevice.DeviceName = oldDeviceName;
                            return true;
                        }
                    }
                }
                else
                {
                    bool isCorrectIP = Regex.IsMatch(SelectedDevice.IPAddress, @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$");
                    if (isCorrectIP == true && (!string.IsNullOrEmpty(SelectedDevice.IPAddress)))
                    {
                        //try
                        //{
                        //    TcpClient currentTCPClient = GetNewTcpClient(SelectedProtocol, SelectedDevice);
                        //    CreateModbusIPMaster(oldDeviceName, currentTCPClient);
                        //}
                        //catch(Exception ex)
                        //{
                        //}
                        //to change the ip address and Port number of the device
                        //SelectedDevice.Address
                        foreach (var currentTcpClient in ConnectionHelperObj.tcpClientDictionary)
                        {
                            if (currentTcpClient.Value != null)
                            {
                                // if (currentTcpClient.Key == SelectedDevice.DeviceName)
                                if (currentTcpClient.Key == (SelectedConnector.ConnectorName + "." + SelectedDevice.DeviceName))
                                {
                                    try
                                    {
                                        var ip = currentTcpClient.Value.Client.RemoteEndPoint.ToString();
                                        var ipa = ip.Split(':');
                                        if (ipa[0] != SelectedDevice.ID.ToString() || ipa[1] != SelectedDevice.Port.ToString())
                                        {
                                            //connectionHelper.tcpClientDictionary.Remove(SelectedDevice.DeviceName);
                                            //connectionHelper.tcpClientDictionary.Add(SelectedDevice.DeviceName, tcpClient);
                                            TcpClient client = currentTcpClient.Value;
                                            ConnectionHelperObj.tcpClientDictionary.Remove(SelectedConnector.ConnectorName + "." + SelectedDevice.DeviceName);
                                            ConnectionHelperObj.tcpClientDictionary.Add(SelectedConnector.ConnectorName + "." + SelectedDevice.DeviceName, client);
                                            //currentTcpClient.Value = null;                                            
                                            return true;
                                        }
                                        //if (ipa[1] != SelectedDevice.Port.ToString())
                                        //{
                                        //    //connectionHelper.tcpClientDictionary.Remove(SelectedDevice.DeviceName);
                                        //    //connectionHelper.tcpClientDictionary.Add(SelectedDevice.DeviceName, tcpClient);

                                        //    connectionHelper.tcpClientDictionary.Remove(SelectedProtocol.Name+"."+SelectedDevice.DeviceName);
                                        //    connectionHelper.tcpClientDictionary.Add(SelectedProtocol.Name+"."+SelectedDevice.DeviceName, tcpClient);
                                        //    return true;
                                        //}
                                    }
                                    catch (Exception ex)
                                    {
                                        throw ex;
                                    }

                                    bool isCorrectPort = Regex.IsMatch(SelectedDevice.Port, "^[0-9]+$");
                                    if (!isCorrectPort)
                                    {
                                        MessageBox.Show("Enter Valid Port of Device", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    }
                                }
                            }


                        }
                    }
                    else
                    {
                        MessageBox.Show("Enter Valid IP Address of Device", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            return false;
        }
        #endregion Edit Device Function

        #region Edit Tag Function
        public bool EditTag(dynamic SelectedDevice, Tag SelectedTag)
        {
            if (SelectedTag != null)
            {
                ITag iTag = SelectedTag as ITag;
                var oldTagName = iTag.Name;
                var newTagName = SelectedTag.TagName;

                string oldTag = string.Empty;// string.Format("User.{0}.{1}.{2}", SelectedDevice.ProtocolAssignment, SelectedDevice.DeviceName, oldTagName);
                                             //To check the name of the tag is already present or not

                bool isNewTag = false;
                if (oldTagName != newTagName)
                {
                    if (((Tag)SelectedTag).SelectedGroup == null || ((Tag)SelectedTag).SelectedGroup == "None")
                    {
                        isNewTag = IsNewTag(SelectedDevice, SelectedTag);
                        oldTag = string.Format("User.{0}.{1}.{2}", SelectedDevice.ConnectorAssignment, SelectedDevice.DeviceName, oldTagName);
                    }
                    else
                    {
                        isNewTag = IsNewTag(SelectedDevice, SelectedDevice.GroupCollection, SelectedTag);
                        oldTag = string.Format("User.{0}.{1}.{2}.{3}", SelectedDevice.ConnectorAssignment, SelectedDevice.DeviceName, SelectedTag.SelectedGroup, oldTagName);
                    }



                }
                else
                {
                    if (Regex.IsMatch(SelectedTag.Address, "^[0-9]+$"))
                    {

                    }
                    else
                    {
                        MessageBox.Show("Please enter valid Tag Address");
                        return false;
                    }
                    isNewTag = true;
                }

                if (isNewTag)
                {

                    iTag.Name = SelectedTag.TagName;
                    //foreach (var tag in myOpcTags)
                    //{
                    //    ISLIKTag tags = tag as ISLIKTag;                        
                    //    //if (tags.Name == "User."+oldTagName)
                    //    if(tags.Name.ToLower()==oldTag.ToLower())
                    //    {
                    //        TagDictionary.Remove(tags);
                    //    }

                    //}
                    // myOpcTags.Remove("User." + oldTagName);
                    try
                    {
                        //var item = myOpcTags[oldTag];
                        //if (!OpcTags[oldTag].Active)
                        //{

                       // ISLIKTag slikdatag = OpcTags[oldTag];
                       // TagDictionary.Remove(slikdatag);

                       // OpcTags.Remove(oldTag);
                        //string newTag = oldTag.Replace(oldTagName, SelectedTag.TagName);//TODO:ReadWriteAccess from tag
                        //OpcTags.Add(newTag, (int)ReadWriteAccess, 0, 192, DateTime.Now, null); //"User." + SelectedDevice.ProtocolAssignment + "." + SelectedDevice.DeviceName + "." + SelectedTag.TagName
                        string key = string.Format("{0}.{1}.{2}", SelectedTag.ScanRate, SelectedDevice.ConnectorAssignment, SelectedDevice.DeviceName);
                       // var value = ScanrateGroup[key];
                       // ISLIKTag newSlikdaTag = OpcTags[newTag];
                        //EditTagList(key, oldTag, newTag, value);
                      //  EditListofMappedTag(key, slikdatag, newSlikdaTag, SelectedTag);

                      //  TagDictionary.Add(OpcTags[newTag], (int.Parse)(SelectedTag.Address));
                        return true;
                        //}
                        //else
                        //{
                        //    SelectedTag.TagName = oldTagName;
                        //    iTag.Name = oldTagName;
                        //    MessageBox.Show("The Tag is Active at Client. Please make InActive Tag at Client Side", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);

                        //    return false;
                        //}


                        //string keytoDelete = string.Format("{0}.{1}.{2}", SelectedTag.ScanRate, SelectedDevice.ProtocolAssignment, SelectedDevice.DeviceName);

                        //ScanrateGroup.Remove(keytoDelete);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    //AddTagtoList(SelectedTag, SelectedDevice.ProtocolAssignment, SelectedDevice.DeviceName);
                    //string key = string.Format("{0}.{1}.{2}", SelectedTag.ScanRate, SelectedDevice.ProtocolAssignment, SelectedDevice.DeviceName);
                    //var value = ScanrateGroup[key];
                    //ScanrateGroup.Remove(key);                    
                    //key.Replace()

                }
                else
                {
                    if (oldTagName != SelectedTag.TagName)
                    {
                        MessageBox.Show("Tag Name: " + "\"" + SelectedTag.TagName + "\"" + " is already being used !!", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);

                        //e.NewValue = e.OldValue;
                        SelectedTag.TagName = oldTagName;
                        iTag.Name = oldTagName;
                        return true;
                    }
                }

                #region Tried in Same Function
                // Change the existing values

                //foreach (var tag in myOpcTags)
                //{
                //    ISLIKTag tags = tag as ISLIKTag;
                //    if (tags.Name == "User." + oldTagName)
                //    {
                //        TagDictionary.Remove(tags);
                //    }
                //}
                //myOpcTags.Remove("User." + oldTagName);


                //newTagName = "User." + newTagName;
                //myOpcTags.Add(newTagName, (int)readWriteAccess, 0, 192, DateTime.Now, null);
                //if (SelectedTag.DataType.ToString() == "Boolean")
                //    myOpcTags[newTagName].DataType = (short)VariantType.Boolean;
                //else
                //    myOpcTags[newTagName].DataType = (short)VariantType.Integer;


                //try
                //{
                //    TagDictionary.Add(myOpcTags[newTagName], (ushort.Parse)(SelectedTag.Address));
                //}
                //catch (Exception e)
                //{
                //    string ErrMessage = e.Message;
                //}
                #endregion Tried in Same Function


                //to check the data type of tag
                try
                {
                    string tagNew = string.Format("User.{0}.{1}.{2}", SelectedDevice.ConnectorAssignment, SelectedDevice.DeviceName, newTagName);
                    newTagName = tagNew;//"User." + newTagName;

                    string oldDataType;
                    if (OpcTags[newTagName].DataType == 3)// 3 for integer 
                        oldDataType = "Long";
                    else //11 for boolean  8// for string
                        oldDataType = "Boolean";

                    if (SelectedTag.DataType.ToString() != oldDataType)
                    {

                        foreach (var tag in OpcTags)
                        {
                            ISLIKTag tags = tag as ISLIKTag;
                            if (tags.Name == newTagName)
                            {
                                TagDictionary.Remove(tags);
                            }
                        }
                        string value = string.Format("User.{0}.{1}.{2}", SelectedDevice.ConnectorAssignment, SelectedDevice.DeviceName, oldTagName);
                        string newValue = string.Format("{0}.{1}.{2}", SelectedDevice.ConnectorAssignment, SelectedDevice.DeviceName, SelectedTag.TagName);
                        //myOpcTags.Remove("User." + oldTagName);
                        //myOpcTags.Add(newTagName, (int)readWriteAccess, 0, 192, DateTime.Now, null);
                        OpcTags.Remove(value);
                        OpcTags.Add(newTagName, (int)ReadWriteAccess, 0, 192, DateTime.Now, null);
                        string key = string.Format("{0}.{1}.{2}", SelectedTag.ScanRate, SelectedDevice.ConnectorAssignment, SelectedDevice.DeviceName);
                        var value1 = ScanrateGroup[key];
                        string newTag = oldTag.Replace(oldTagName, SelectedTag.TagName);
                        EditTagList(key, oldTag, newTag, value1);
                        if (SelectedTag.DataType.ToString() == "Boolean")
                            OpcTags[newTagName].DataType = (short)DataType.Boolean;
                        else
                            OpcTags[newTagName].DataType = (short)DataType.Integer;
                        TagDictionary.Add(OpcTags[newTagName], (int.Parse)(SelectedTag.Address));
                        return true;
                    }

                    //To check the address

                    bool isNewTagAddress = IsNewTagAddress(SelectedTag);
                    if (isNewTagAddress)
                    {
                        foreach (var tag in OpcTags)
                        {
                            ISLIKTag tags = tag as ISLIKTag;
                            if (tags.Name == "User." + SelectedDevice.ConnectorAssignment + "." + SelectedDevice.DeviceName + "." + SelectedTag.TagName)
                            {
                                TagDictionary[tags] = (int.Parse)(SelectedTag.Address);
                                return true;
                            }
                        }
                    }
                    else
                    {
                        //MessageBox.Show("Tag Address: " + "\"" + SelectedTag.Address + "\"" + " is already being used !!", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                        //return false ;
                    }

                }
                catch (Exception)
                {
                    //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    //{
                    Addlogs("Configuration", @"Elpis/Configuration/EditTag", @"Problem in editing Tag:" + SelectedTag.TagName, LogStatus.Error);
                    //}), DispatcherPriority.Normal, null);
                }

            }

            return false;

        }

        private void EditListofMappedTag(string key, ISLIKTag oldTag, ISLIKTag newTag, Tag selectedTag)
        {
            try
            {
                Dictionary<ISLIKTag, Tag> currentDictionary = ListofMappedtag[key];
                if (currentDictionary.ContainsKey(oldTag))
                {
                    currentDictionary.Remove(oldTag);
                    currentDictionary.Add(newTag, selectedTag);
                }
            }
            catch (Exception)
            {
                Addlogs("Configuration", "Elpis/UpdateTag", "Problem in updating mapping tag dictionary", LogStatus.Information);
            }
        }

        private void EditTagList(string key, string oldTag, string newTag, List<ISLIKTag> value)
        {
            try
            {
                var newValue = OpcTags[newTag];
                foreach (var item in value)
                {
                    if (item.Name == oldTag)
                    {
                        value.Remove(item);
                        value.Add(newValue);
                        // TagDictionary.Remove(item);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #region Edit Tag Group
        /// <summary>
        /// Editing Tag Group properties.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="deviceCollection"></param>
        /// <param name="oldName"></param>
        /// <returns></returns>
        public bool EditGroup(TagGroup group, ObservableCollection<DeviceBase> deviceCollection, string oldName)
        {
            bool edited = false;

            var device = DeviceFactory.GetDeviceByName(group.DeviceName, deviceCollection);
            DeviceBase deviceObject = DeviceFactory.GetDevice(device);
            var count = deviceObject.GroupCollection.Where(gp => gp.GroupName == group.GroupName).Count(); ;// Contains(group).co)
            if (count > 1)
            {
                MessageBox.Show("Tag Group with same name already exists", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else
            {
                if (group.TagsCollection.Count == 0)
                {
                    edited = true;
                    return edited;
                }
                else
                {
#if ! SunPowerGen
                    foreach (var newTag in group.TagsCollection)
                    {
                        newTag.SelectedGroup = group.GroupName;
                        string oldKey = string.Format("User.{0}.{1}.{2}.{3}", deviceObject.ConnectorAssignment, deviceObject.DeviceName, oldName, newTag.TagName);
                        //var tag = myOpcTags[oldKey];
                        string newKey = string.Format("User.{0}.{1}.{2}.{3}", deviceObject.ConnectorAssignment, deviceObject.DeviceName, group.GroupName, newTag.TagName);
                        try
                        {
                            if (OpcTags != null && !OpcTags[oldKey].Active)
                            {
                                OpcTags.Add(newKey, (int)ReadWriteAccess, 0, 192, DateTime.Now, null);
                                OpcTags[newKey].DataType = OpcTags[oldKey].DataType;
                                TagDictionary.Remove(OpcTags[oldKey]);
                                OpcTags.Remove(oldKey);
                                string key = string.Format("{0}.{1}.{2}", newTag.ScanRate, deviceObject.ConnectorAssignment, deviceObject.DeviceName);
                                var value = ScanrateGroup[key];
                                EditTagList(key, oldKey, newKey, value);
                                TagDictionary.Add(OpcTags[newKey], (int.Parse)(newTag.Address));

                                return true;
                            }
                            else
                            {
                                //group.GroupName = oldName;
                                MessageBox.Show("The Tag is Active at Client. Please make InActive Tag at Client Side", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
#else
                    edited = true;
#endif
                }

            }
            return edited;
        }
        #endregion Edit Tag Group

        #endregion Edit Tag Function

        #endregion Configuration window functionalities

        #region Main window functionalities

        #region  StartStop Function
        /// <summary>
        /// It Start Server 
        /// </summary>
        /// <returns></returns>
        public string StartStop()
        {
            //OPCEngine.TaskHelper.isRunningTask = false;
            try
            {
                if (RunTimeDisplay.Contains("Start"))
                {
                    //Added on 22-Mar-2018 -- Update configuration file before client connect.
                    SaveLastLoadedProject();
                    GetCommunicationElements();
                    //SLIKDA Registration and starting the server at this moment
                    SlikServerObject.RegisterServer();
                    SlikServerObject.StartServer();
                    //dispatcherTimer.Start();

                    //starting the Mqtt Protocol Stuffs right here
                    #region IoT
                    //mqttObj.Start();

                    //mqttObj.Load(MqttClientCollection[0]);
                    //AzureIoTHubObj.Load(AzureIoTCollection[0]);

                    //MqttClientCollection.Clear();
                    //MqttClientCollection.Add(mqttObj);
                    ////Starting the Azure Iot stuffs right here
                    //IotHubObj = new IoTHub(ElpisServer.AzureIoTHubObj);

                    #endregion IoT

                    RunTimeDisplay = "Stop Server";
                    if (!TaskHelper.isTaskRunning)
                    {
                        TaskHelper.isTaskRunning = true;
                        //StartTask();
                    }
                    isDemoExpired = false;

                    StartTask();

                }
                else if (isDemoExpired)
                {
                    //TaskHelper.isTaskRunning = false;
                    isDemoExpired = false;
                    UnregisterServer();

                }
                else
                {
                    System.Windows.Forms.DialogResult dr = (System.Windows.Forms.DialogResult)MessageBox.Show("This will stop all the server activities, Do you want to stop the server?", "Elpis OPC Server", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (dr.ToString() == "No")
                    {
                        TaskHelper.isTaskRunning = true;
                        return RunTimeDisplay;
                    }
                    else
                    {
                        TaskHelper.isTaskRunning = false;
                        //Unregister and Stop further SLIKDA Operations
                        UnregisterServer();
                        #region IoT

                        ////disconnect the Mqtt Client from the server and other stuffs
                        //if (mqttObj.mqttClient != null)
                        //{
                        //    if (mqttObj.mqttClient.IsConnected == true)
                        //    {
                        //        mqttObj.mqttClient.Disconnect();
                        //    }

                        //    //Clear all Azure IoT Stuffs
                        //    IotHubObj = null;
                        //}
                        #endregion IoT
                    }
                }
            }
            catch (Exception ex)
            {
                string ErrMessage = ex.Message;
            }
            return RunTimeDisplay;
        }

        /// <summary>
        /// The registered SlikDA server  is unregistered.
        /// </summary>
        public void UnregisterServer()
        {
            //dispatcherTimer.Stop();
            TaskHelper.isTaskRunning = false;
            try
            {

                //if (tokenSource != null)
                //{
                //    tokenSource.Cancel();
                //}

                foreach (var item in tokenSourceList)
                {
                    item.Cancel();
                }

                //SlikServerObject.RegisterServer();
                SlikServerObject.UnregisterServer();
                setQualityofTags();
            }
            catch (Exception ex)
            {
                //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                Addlogs("Configuration", @"Elpis/Communication", ex.Message, LogStatus.Error);
                //}), DispatcherPriority.Normal, null);
            }

            //slikServerObject.RequestDisconnect("Server has Stopped/Under Maintenance please close the client, and Connect after sometime.");
            RunTimeDisplay = "Start Server";// " Start Server        >";
        }

        /// <summary>
        /// To set tag qualities bad after server is stopped.
        /// </summary>
        public void setQualityofTags()
        {
            if (SlikServerObject != null)
            {
                var newlist = SlikServerObject.SLIKTags.OfType<ISLIKTag>();//.Where(s => s.Active == true); 
                TaskHelper.isTaskRunning = false;
                Thread.Sleep(2000);
                foreach (ISLIKTag currentItem in newlist)
                {
                    currentItem.SetVQT(null, 24, DateTime.Now);
                }
            }

        }

        #endregion  StartStop Function

        #region OpenUserProjectFile
        /// <summary>
        /// Open project from the any existing location
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<IConnector> OpenUserProjectFile()
        {
            try
            {
                Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;
                dialog.DefaultExt = ".der";
                dialog.Filter = "Elpis Project File|*.elp|Project File|*.elp|All Files|*.*";
                dialog.Multiselect = false;
                dialog.ValidateNames = true;
                dialog.Title = "Select the project to load";
                dialog.FileName = null;
                dialog.RestoreDirectory = true;

                if (!dialog.ShowDialog().Value)
                {
                    return null;
                }
                OpenedProjectFilePath = dialog.FileName;
                Stream stream = File.Open(dialog.FileName, FileMode.Open);
                BinaryFormatter bformatter = new BinaryFormatter();

                using (StreamReader wr = new StreamReader(stream))
                {
                    FileHandle = (FileHandler)bformatter.Deserialize(stream);
                }
                stream.Close();

                ConnectorCollection = FileHandle.AllCollectionFileHandling;
                MqttClientCollection.Clear();
                MqttClientCollection = FileHandle.MqttCollectionFilHandling;

                //LoadConfigurationElements(ConnectorCollection); Commented on 26-Feb-2018
            }
            catch (Exception)
            {
                //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                Addlogs("Configuration", @"Elpis/Configuration", "Problem in opening project file.", LogStatus.Error);
                //}), DispatcherPriority.Normal, null);
            }
            return ConnectorCollection;
        }

        #endregion OpenUserProjectFile

        #endregion Main window functionalities

        #region Future Things

        public void ImportFromCsv()
        {
            var csvFile = new StreamReader(File.OpenRead(@"C:\\Users\\manikandan\\Desktop\\test.csv"));
            List<string> Header = new List<string>();
            List<string> Items = new List<string>();
            while (!csvFile.EndOfStream)
            {
                //var line = reader.ReadLine();
                string myString = csvFile.ReadToEnd();
                string[] lines = myString.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                foreach (string line in lines)
                {
                    if (line != "")
                    {
                        string[] columns = line.Split(',');
                        if (line.Contains("ItemName,Description,DataType,AutoActive,ReadOnly,EchoOutput,Enabled,InitialValue,Value"))
                            Header.Add(line);
                        else
                            Items.Add(line);
                        //Upload to your DB with your indexes

                    }
                    else
                    {

                    }
                }
            }
        }

        #region MQTT

        public void MqttPart()
        {
            try
            {
                //string DirDebug = System.IO.Directory.GetCurrentDirectory();
                //string path = DirDebug + "\\hivemq-3.1.1\\hivemq-3.1.1\\conf\\config.xml";

                //XmlDocument doc = new XmlDocument();
                //doc.Load(path);

                //XmlNodeList ipAddress = doc.SelectNodes("/hivemq/listeners/tcp-listener/bind-address");

                //mqttClient = new MqttClient(IPAddress.Parse(ipAddress[0].InnerText).ToString());
                //mqttClient.Connect(Guid.NewGuid().ToString());

                //mqttClient.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
                //mqttClient.MqttMsgPublished += Client_MqttMsgPublished;
            }
            catch (Exception e)
            {
                string errMsg = e.Message;
                //MessageBox.Show("Looks like MQTT Broker is not started yet", "Elpis OPC Server-IOT", MessageBoxButton.OK);
            }

        }

        private void Client_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            MessageBox.Show("Published message was received from ..." + e.Topic + " The Message is: " + Encoding.UTF8.GetString(e.Message));
        }

        private void Client_MqttMsgPublished(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishedEventArgs e)
        {
            //MessageBox.Show("Publishing.....\t Published."+ "MessageId = " + e.MessageId + " and Published: " + e.IsPublished);
            //label.Content= "Publishing.....\t Published."+ "MessageId=" + e.MessageId + " and Published:" + e.IsPublished;
        }

        #endregion MQTT

        public void SaveMqttSettingsToFile()
        {
            FileHandle = new FileHandler();
        }

        #endregion Future Things

        #region Log Related Events and Functions
        /// <summary>
        /// This method logs the data into .CSV file, location is Current working directory\Elpis_OPC_Log\ElpisOPC_Log_*.xlsx
        /// this will automatically creates a log file based on the number of the entries in the file.
        /// </summary>
        /// <param name="CurrentLog"></param>

        public ObservableCollection<LoggerViewModel> LoadDataLogCollection(string section, string source, string eventlog, LogStatus logStatus)
        {
            string uri = string.Empty;
            switch (logStatus)
            {
                case LogStatus.Information:
                    uri = @"pack://application:,,,/Images/infoBlue.png";
                    break;
                case LogStatus.Error:
                    uri = @"pack://application:,,,/Images/ErrorRed.png";
                    break;
                case LogStatus.Warning:
                    uri = @"pack://application:,,,/Images/ImportantYellow.png";
                    break;
                default:
                    break;
            }

            LoggerViewModel CurrentLog = new LoggerViewModel
            {

                //ImageStatus.UriSource= new Uri(BitmapImage(@"Images\About.png",UriKind.Absolute)), 
                //ImageStatus= image,
                //Date = DateTime.Now.Date.ToShortDateString(),  
                ImageStatus = new BitmapImage(new Uri(uri)),
                EventType = logStatus,
                Date = DateTime.Now.ToString(),
                //Time = DateTime.Now.ToString("HH:mm:ss tt"),
                Source = source,
                Event = eventlog,
                Module = section
            };

            //Log into File(CSV)
            //LogintoFile(CurrentLog);
            if (string.IsNullOrEmpty(LogFilePath))//TODO:  --Done Change file path from Config file.
            {

                string path = SLIKDAUACONFIG.GetLogFilePath();
                if (path != null)
                {
                    LogFilePath = path;
                }
                else
                {
                    LogFilePath = string.Format(Directory.GetCurrentDirectory() + @"\Elpis_OPC_Log\ElpisOPC_Log_" + DateTime.Now.ToString("dd-MMM-yyyy-HH-mm-ss").Replace(':', '_') + ".csv"); //DateTime.Now.ToString().Replace(':','_')
                }

                //logFilePath =string.Format( Directory.GetCurrentDirectory() + @"\Elpis_OPC_Log\ElpisOPC_Log_"+ DateTime.Now.ToString("dd-MMM-yyyy-HH-mm-ss").Replace(':', '_') + ".csv"); //DateTime.Now.ToString().Replace(':','_')


                //Directory.CreateDirectory(logFilePath.Substring(0, logFilePath.LastIndexOf(@"\")));
                //File.AppendAllText(logFilePath, string.Format(@"Event Type,Date,Source,Event") + Environment.NewLine);
                //LoadDataLogCollection("All", "Elpis OPC Server", string.Format("The log file is create at :{0}", logFilePath), LogStatus.Information);                
            }

            if (CurrentLog != null)
            {
                string logInfo = string.Format(@"{0},{1},{2},{3},{4}{5}", CurrentLog.Module, CurrentLog.EventType, CurrentLog.Date, CurrentLog.Source, CurrentLog.Event, Environment.NewLine);
                if (File.Exists(LogFilePath))
                {
                    try
                    {
                        if (File.ReadLines(LogFilePath).Count() > 100)//Number logs per file. //TODO change lines of logs from config file
                        {
                            StringBuilder path = new StringBuilder();
                            path.Append(Directory.GetCurrentDirectory());
                            path.Append(@"\Elpis_OPC_Log\ElpisOPC_Log_");
                            path.Append(DateTime.Now.ToString("dd-MMM-yyyy-HH-mm-ss").Replace(':', '_'));
                            path.Append(@".csv");
                            LogFilePath = path.ToString();// logFilePath.Substring(0, logFilePath.LastIndexOf("LogInformation") + 14) + DateTime.Now.ToString().Replace(':', '_') + ".csv";                            
                            File.AppendAllText(LogFilePath, string.Format(@"Module,Event Type,Date,Source,Event") + Environment.NewLine);
                            File.AppendAllText(LogFilePath, logInfo);
                        }
                        else
                        {
                            File.AppendAllText(LogFilePath, logInfo);
                        }
                    }
                    catch (Exception)
                    {
                        //MessageBox.Show(ec.Message, "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);

                    }
                }
                else
                {
                    Directory.CreateDirectory(LogFilePath.Substring(0, LogFilePath.LastIndexOf(@"\")));
                    File.AppendAllText(LogFilePath, string.Format(@"Module,Event Type,Date,Source,Event") + Environment.NewLine);
                    LoadDataLogCollection("All", "Elpis OPC Server", string.Format("The log file is create at :{0}", LogFilePath), LogStatus.Information);
                    File.AppendAllText(LogFilePath, logInfo);

                }
            }
            if (LoggerCollection == null)
                LoggerCollection = new ObservableCollection<LoggerViewModel>();

            if (section == "Configuration")
            {
                if (ConfigurationLogCollection == null)
                    ConfigurationLogCollection = new ObservableCollection<LoggerViewModel>();
                ConfigurationLogCollection.Add(CurrentLog);
                LoggerCollection.Add(CurrentLog);
            }
            else if (section == "UA Configuration")
            {
                if (UAConfigurationLogCollection == null)
                    UAConfigurationLogCollection = new ObservableCollection<LoggerViewModel>();
                UAConfigurationLogCollection.Add(CurrentLog);
                LoggerCollection.Add(CurrentLog);
            }
            else if (section == "UA Certificate")
            {
                if (UACertificateLogCollection == null)
                    UACertificateLogCollection = new ObservableCollection<LoggerViewModel>();
                UACertificateLogCollection.Add(CurrentLog);
                LoggerCollection.Add(CurrentLog);
            }
            else if (section == "Internet Of Things")
            {
                if (IoTLogCollection == null)
                    IoTLogCollection = new ObservableCollection<LoggerViewModel>();
                IoTLogCollection.Add(CurrentLog);
                LoggerCollection.Add(CurrentLog);
            }
            else
                LoggerCollection.Add(CurrentLog);

            return LoggerCollection;
        }

        /// <summary>
        /// Clear the logs in the logger window.
        /// </summary>
        public void ClearLog()
        {
            ConfigurationLogCollection.Clear();
            UAConfigurationLogCollection.Clear();
            UACertificateLogCollection.Clear();
            IoTLogCollection.Clear();
            LoggerCollection.Clear();
            #region old
            //ConfigurationLogCollection = null;
            //UAConfigurationLogCollection = null;
            //UACertificateLogCollection = null;
            //IoTLogCollection = null;
            //LoggerCollection = null;
            #endregion

        }

        /// <summary>
        /// Export the logs into desired location.
        /// </summary>
        /// <param name="ListViewLogger"></param>
        public void ExportToCSV(DataGrid ListViewLogger)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog1.Filter = "csv files (*.csv)|*.csv";
            saveFileDialog1.FileName = "logs";
            saveFileDialog1.Title = "Export to Excel";
            StringBuilder sb = new StringBuilder();
            System.Windows.Forms.DialogResult dr = saveFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                StreamWriter sw = new StreamWriter(saveFileDialog1.FileName);
                sw.Write(sb.ToString());
                sw.Close();

                //08-11-2017
                DataGrid dg = ListViewLogger;
                dg.SelectAllCells();
                dg.ClipboardCopyMode = DataGridClipboardCopyMode.ExcludeHeader;
                ApplicationCommands.Copy.Execute(null, dg);
                dg.UnselectAllCells();
                String Clipboardresult = (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);
                StreamWriter swObj = new StreamWriter(saveFileDialog1.FileName);
                swObj.WriteLine(@"Module,EventType,Date,Source,Event");
                swObj.WriteLine(Clipboardresult);
                swObj.Close();
            }
            #region code of GridView
            //foreach (GridViewColumn h in ((GridView)ListViewLogger.View).Columns)
            //{
            //    sb.Append(h.Header.ToString() + ",");
            //}
            //sb.AppendLine();

            //for (int i = 0; i < ListViewLogger.Items.Count; i++)
            //{
            //    ListViewItem lvi = ListViewLogger.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
            //    if (lvi != null)
            //    {
            //        Type str = lvi.Content.GetType();
            //        LoggerViewModel lv = lvi.Content as LoggerViewModel;

            //        if (lv != null)
            //        {
            //            sb.Append(lv.Module+","+ lv.EventType + "," + lv.Date + "," + lv.Source + "," + lv.Event);
            //        }

            //        sb.AppendLine();
            //    }
            //}
            //System.IO.File.WriteAllText(saveFileDialog1.FileName, sb.ToString());
            ////Process.Start(saveFileDialog1.FileName);
            #endregion
        }

        /// <summary>
        /// Copy the current logs into clipboard.
        /// </summary>
        /// <param name="ListViewLogger"></param>
        public void CopyToClipBoard(DataGrid ListViewLogger)
        {
            if (ListViewLogger.Items.Count != 0)
            {
                //where LoggerViewModel is a custom datatype and the listview is bound to a 
                List<LoggerViewModel> selected = new List<LoggerViewModel>();
                StringBuilder sb = new StringBuilder();

                //foreach (GridViewColumn h in ((GridView)ListViewLogger.View).Columns)
                //{
                //    sb.Append(h.Header.ToString() + ",");
                //}
                sb.Append(@"Module,EventType,Date,Source,Event");
                sb.AppendLine();

                for (int i = 0; i < ListViewLogger.Items.Count; i++)
                {

                    LoggerViewModel lv = ListViewLogger.ItemContainerGenerator.Items[i] as LoggerViewModel;
                    if (lv != null)
                    {
                        sb.Append(lv.Module + "," + lv.EventType + "," + lv.Date + "," + lv.Source + "," + lv.Event);
                    }
                    sb.AppendLine();
                    #region for Listview
                    //ListViewItem lvi = ListViewLogger.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                    //if (lvi != null)
                    //{
                    //    Type str = lvi.Content.GetType();
                    //    LoggerViewModel lv = lvi.Content as LoggerViewModel;

                    //    if (lv != null)
                    //    {
                    //        sb.Append(lv.Module+","+ lv.EventType + "," + lv.Date + "," + lv.Source + "," + lv.Event);
                    //    }
                    //    sb.AppendLine();
                    //}
                    #endregion
                }
                try
                {
                    System.Windows.Clipboard.SetData(DataFormats.Text, sb.ToString());
                }
                catch (Exception ex)
                {
                    string ErrMessage = ex.Message;
                    MessageBox.Show("Sorry, unable to copy logs to the clipboard. Try again.");
                }
            }
        }

        /// <summary>
        /// Save the current logs to file.
        /// </summary>
        /// <param name="ListViewLogger"></param>
        public void SaveLogsAsText(DataGrid ListViewLogger)
        {
            if (ListViewLogger.Items.Count > 0)
            {
                System.Windows.Forms.SaveFileDialog saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog1.Filter = "txt files (*.txt)|*.txt";
                saveFileDialog1.FileName = "logs";
                saveFileDialog1.Title = "Save As Text File";
                System.Windows.Forms.DialogResult dr = saveFileDialog1.ShowDialog();

                StringBuilder sb = new StringBuilder();
                if (dr == System.Windows.Forms.DialogResult.OK)
                {


                    //foreach (GridViewColumn h in ((GridView)ListViewLogger.View).Columns)
                    //{
                    //    sb.Append(h.Header.ToString() + ",");
                    //}
                    sb.AppendLine();


                    for (int i = 0; i < ListViewLogger.Items.Count; i++)
                    {
                        ListViewItem lvi = ListViewLogger.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                        if (lvi != null)
                        {
                            Type str = lvi.Content.GetType();
                            LoggerViewModel lv = lvi.Content as LoggerViewModel;

                            if (lv != null)
                            {
                                sb.Append(lv.Module + "," + lv.EventType + "," + lv.Date + "," + lv.Source + "," + lv.Event);
                            }
                            sb.AppendLine();

                            System.IO.File.WriteAllText(saveFileDialog1.FileName, sb.ToString());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create a Logs current working direction.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="source"></param>
        /// <param name="eventlog"></param>
        /// <param name="logStatus"></param>
        public static void Addlogs(string section, string source, string eventlog, LogStatus logStatus)
        {
            string uri = string.Empty;
            switch (logStatus)
            {
                case LogStatus.Information:
                    uri = @"pack://application:,,,/Images/infoBlue.png";
                    break;
                case LogStatus.Error:
                    uri = @"pack://application:,,,/Images/ErrorRed.png";
                    break;
                case LogStatus.Warning:
                    uri = @"pack://application:,,,/Images/ImportantYellow.png";
                    break;
                default:
                    break;
            }

            LoggerViewModel CurrentLog = new LoggerViewModel
            {
                ImageStatus = new BitmapImage(new Uri(uri)),
                EventType = logStatus,
                Date = DateTime.Now.ToString(),
                Source = source,
                Event = eventlog,
                Module = section
            };

            if (string.IsNullOrEmpty(LogFilePath))
            {
                LogFilePath = string.Format(Directory.GetCurrentDirectory() + @"\Elpis_OPC_Log\ElpisOPC_Log_" + DateTime.Now.ToString("dd-MMM-yyyy-HH-mm-ss").Replace(':', '_') + ".csv"); //DateTime.Now.ToString().Replace(':','_')                
            }

            if (CurrentLog != null)
            {
                string logInfo = string.Format(@"{0},{1},{2},{3},{4}{5}", CurrentLog.Module, CurrentLog.EventType, CurrentLog.Date, CurrentLog.Source, CurrentLog.Event, Environment.NewLine);
                if (File.Exists(LogFilePath))
                {
                    try
                    {
                        if (File.ReadLines(LogFilePath).Count() > 100)//Number logs per file. //TODO change lines of logs from config file
                        {
                            StringBuilder path = new StringBuilder();
                            path.Append(Directory.GetCurrentDirectory());
                            path.Append(@"\Elpis_OPC_Log\ElpisOPC_Log_");
                            path.Append(DateTime.Now.ToString("dd-MMM-yyyy-HH-mm-ss").Replace(':', '_'));
                            path.Append(@".csv");
                            LogFilePath = path.ToString();// logFilePath.Substring(0, logFilePath.LastIndexOf("LogInformation") + 14) + DateTime.Now.ToString().Replace(':', '_') + ".csv";                            
                            File.AppendAllText(LogFilePath, string.Format(@"Module,Event Type,Date,Source,Event") + Environment.NewLine);
                            File.AppendAllText(LogFilePath, logInfo);
                        }
                        else
                        {
                            File.AppendAllText(LogFilePath, logInfo);
                        }
                    }
                    catch (Exception ec)
                    {
                        MessageBox.Show(ec.Message, "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);

                    }
                }
                else
                {
                    Directory.CreateDirectory(LogFilePath.Substring(0, LogFilePath.LastIndexOf(@"\")));
                    File.AppendAllText(LogFilePath, string.Format(@"Module,Event Type,Date,Source,Event") + Environment.NewLine);
                    Addlogs("All", "Elpis OPC Server", string.Format("The log file is create at :{0}", LogFilePath), LogStatus.Information);
                    File.AppendAllText(LogFilePath, logInfo);
                }
            }
            //if (LoggerCollection == null)
            //    LoggerCollection = new ObservableCollection<LoggerViewModel>();                  
            //    LoggerCollection.Add(CurrentLog);
        }

        #endregion Log Related Events and Functions

        #endregion Main and configuration window functionalities
    }
    #endregion ElpisServer class
}

#endregion OPCEngine namespace
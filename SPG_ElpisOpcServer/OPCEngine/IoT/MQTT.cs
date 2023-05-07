#region Usings
using System;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using uPLibrary.Networking.M2Mqtt;
#endregion Usings

#region OPCEngine Namespace
namespace Elpis.Windows.OPC.Server
{
    #region MQTT class
    /// <summary>
    /// MQTT class interactions with Mqtt Protocol and settings
    /// </summary>
    [Serializable,Description("MQTT Settings"),DisplayName("MQTT Settings")]
    public class MQTT : INotifyPropertyChanged 
    {
        private string ipAddress;

        #region Properties
        [Description("Specify the ip address of the object."),DisplayName("IP Address")]
        public string IPAddress {
            get
            {
                return ipAddress;
            }
           set
            {
                bool flag = Validate(value);
                if (flag)
                {
                    ipAddress = value;
                    OnPropertyChanged("IPAddress");
                }
                else
                {
                    MessageBox.Show("Check The IP Address");
                }

            }
        }
        public bool Validate(string value)
        {
            bool isCorrectIP = Regex.IsMatch(value, @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$");
            return isCorrectIP;
        }
       
        [Description("Specifies the port number that the remote device is configured to use. The valid range is 0 to 65535. The default is 502. This port number is used when making solicited requests to a device."), DisplayName("Default Port Number")]
        public string DefaultPortNumber { get; set; }

        [Description("Indicate the maximum length of client id mqtt protocol can use."), DisplayName("Maximum Client Id Length")]
        public int MaxClientIdLength { get; set; }

        [Description("Specify the rate, in seconds ,at which the protocol can retry."), DisplayName("Retry Interval (ms)")]
        public int RetryInterval { get; set; }

        [Description("Indicate the maximum number of queued messages mqtt protocol can use."), DisplayName("Maximum Queued Messages")]
        public int MaxQueuedMessages { get; set; }

        [Description("Indicate the maximum number of connections mqtt protocol can use."), DisplayName("Maximum Number of Connections")]
        public int MaxConnections { get; set; }
        
        [Description("Indicate the maximum number of message size mqtt protocol can use."), DisplayName("Maximum Size of the Message")]
        public int MaxMessageSize { get; set; }

        [Description("Set the Limit of outgoing messages from broker via mqtt protocol."), DisplayName("Outgoing Message Limit")]
        public int OutGoingLimit { get; set; }
   
        [Description("Set the Limit of incoming messages from broker via mqtt protocol."), DisplayName("Incoming Message Limit")]
        public int InComingLimit { get; set; }

        [Description("To enable or disable the update for mqtt protocol. The defalut value is true."), DisplayName("Update Check Enabled")]
        public bool UpdateCheckEnabled { get; set; }


        [NonSerialized(),Browsable(false)]
        public MqttClient mqttClient;

        //Path of the configuration file
        [Browsable(false)]
        public string Path = System.IO.Directory.GetCurrentDirectory() + "\\hivemq-3.1.1\\hivemq-3.1.1\\conf\\config.xml";

        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string Property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(Property));
            }
        }
        #endregion Properties

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public MQTT()
        {
            
            //Loading from the directory
            //LoadFromConfigurationFile();
        }
        #endregion Constructor
        
        #region Start function
        public void Start()
        {
            try
            {
                mqttClient = new MqttClient(IPAddress);


                mqttClient.Connect(Guid.NewGuid().ToString());

                mqttClient.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
                mqttClient.MqttMsgPublished += Client_MqttMsgPublished;
            }
            catch (Exception e)
            {
                string errMessage = e.Message;
                MessageBox.Show("Looks like MQTT Broker is not started yet !!  Check This:Internet Of Things-> MQTT Configuration Settings", "Elpis OPC Server-IOT", MessageBoxButton.OK);
            }
        }
        #endregion Start function

        #region ConnectionStauts function
        public void ConnectionStauts()
        {
            if (mqttClient.IsConnected == true)
            {
                mqttClient.Disconnect();
            }
        }
        #endregion ConnectionStauts function

        #region Publish function
        public void Publish(dynamic TagObject)
        {
            if (mqttClient != null)
            {
                mqttClient.Publish("Elpis-IOT/Tags", Encoding.UTF8.GetBytes(TagObject.ToString()));
            }
            //mqttClient.Publish("Elpis-IOT/Tags", Encoding.UTF8.GetBytes(TagObject));
        }

        #region Client_MqttMsgPublishReceived Event
        private void Client_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            MessageBox.Show("Published message was received from ..." + e.Topic + " The Message is: " + Encoding.UTF8.GetString(e.Message));
        }
        #endregion Client_MqttMsgPublishReceived Event

        #region Client_MqttMsgPublished Event
        private void Client_MqttMsgPublished(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishedEventArgs e)
        {
            //MessageBox.Show("Publishing.....\t Published."+ "MessageId = " + e.MessageId + " and Published: " + e.IsPublished);
            //label.Content= "Publishing.....\t Published."+ "MessageId=" + e.MessageId + " and Published:" + e.IsPublished;
        }
        #endregion Client_MqttMsgPublished Event


        #endregion Publish function
        
        #region LoadFromConfigurationFile class
        /// <summary>
        /// Load the the config.xml file from the directory
        /// </summary>
        public void LoadFromConfigurationFile()
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(Path);
            }
            catch (Exception e)
            {
                string errMessage = e.Message;
                MessageBox.Show("Check the neccessary files present or not for mqtt to work");
            }
            XmlNodeList xmlNodeIPAddress = doc.SelectNodes("/hivemq/listeners/tcp-listener/bind-address");
            XmlNodeList xmlNodePort = doc.SelectNodes("/hivemq/listeners/tcp-listener/port");

            XmlNodeList xmlNodeMaxClientIdLength = doc.SelectNodes("/hivemq/mqtt/max-client-id-length");
            XmlNodeList xmlNodeRetryInterval = doc.SelectNodes("/hivemq/mqtt/retry-interval");
            XmlNodeList xmlNodeMaxQueuedMessages = doc.SelectNodes("/hivemq/mqtt/max-queued-messages");

            XmlNodeList xmlNodeMaxConnections = doc.SelectNodes("/hivemq/throttling/max-connections");
            XmlNodeList xmlNodeMaxMessageSize = doc.SelectNodes("/hivemq/throttling/max-message-size");
            XmlNodeList xmlNodeOutgoingLimit = doc.SelectNodes("/hivemq/throttling/outgoing-limit");
            XmlNodeList xmlNodeIncomingLimit = doc.SelectNodes("/hivemq/throttling/incoming-limit");

            XmlNodeList xmlNodeUpdateCheckEnabled = doc.SelectNodes("/hivemq/general/update-check-enabled");

            if (xmlNodeIPAddress.Count >= 1)
                IPAddress = xmlNodeIPAddress[0].InnerText;
            if (xmlNodePort.Count >= 1)
                DefaultPortNumber = xmlNodePort[0].InnerText;

            if (xmlNodeMaxClientIdLength.Count >= 1)
                MaxClientIdLength = int.Parse(xmlNodeMaxClientIdLength[0].InnerText);
            RetryInterval = int.Parse(xmlNodeRetryInterval[0].InnerText);
            MaxQueuedMessages = int.Parse(xmlNodeMaxQueuedMessages[0].InnerText);
            MaxConnections = int.Parse(xmlNodeMaxConnections[0].InnerText);
            OutGoingLimit = int.Parse(xmlNodeOutgoingLimit[0].InnerText);
            InComingLimit = int.Parse(xmlNodeIncomingLimit[0].InnerText);
            UpdateCheckEnabled = bool.Parse(xmlNodeUpdateCheckEnabled[0].InnerText);
        }

        #endregion LoadFromConfigurationFile class

        #region SaveMqttSettings 
        /// <summary>
        /// Save the config.xml file back to the directory
        /// </summary>
        public void SaveMqttSettings(dynamic ObjToSave)
        {
            #region Old Mqtt in the same system
            ////Loading from the directory
            //string dir = System.IO.Directory.GetCurrentDirectory();

            ////Path of the configuration file
            //string Path = dir + "\\hivemq-3.1.1\\hivemq-3.1.1\\conf\\config.xml";

            //RemoveOldSettings();

            //AddNewSettings();
            #endregion Old Mqtt in the same system

            // i have to  store it into .Elp file
            #region Store it into .ELP file


            ElpisServer.mqttObj = ObjToSave;
            
            #endregion Store it into .ELP file

        }
        public void AddNewSettings()
        {
            //using XMLDocument concept
            XmlDocument doc = new XmlDocument();
            doc.Load(Path);
            XmlNode parentNode = doc.SelectSingleNode("/hivemq");

            #region Listeners
           
            XmlNode ListenerNode = doc.CreateNode(XmlNodeType.Element, "listeners", null);

            XmlNode tcpListenerNode = doc.CreateNode(XmlNodeType.Element, "tcp-listener", null);
            XmlNode portNode = doc.CreateNode(XmlNodeType.Element, "port", null);
            portNode.InnerText = DefaultPortNumber;
            XmlNode bindAddressNode = doc.CreateNode(XmlNodeType.Element, "bind-address", null);
            bindAddressNode.InnerText = IPAddress;

            tcpListenerNode.AppendChild(portNode);
            tcpListenerNode.AppendChild(bindAddressNode);
            ListenerNode.AppendChild(tcpListenerNode);

            parentNode.AppendChild(ListenerNode);
            #endregion Listeners
            
            #region MQTT
            XmlNode MqttNode = doc.CreateNode(XmlNodeType.Element, "mqtt", null);

            XmlNode MaxClientIdLengthNode = doc.CreateNode(XmlNodeType.Element, "max-client-id-length", null);
            MaxClientIdLengthNode.InnerText = MaxClientIdLength.ToString();
            XmlNode RetryIntervalNode = doc.CreateNode(XmlNodeType.Element, "retry-interval", null);
            RetryIntervalNode.InnerText = RetryInterval.ToString();
            XmlNode MaxQueuedMessagesNode = doc.CreateNode(XmlNodeType.Element, "max-queued-messages", null);
            MaxQueuedMessagesNode.InnerText = MaxQueuedMessages.ToString();

            MqttNode.AppendChild(MaxClientIdLengthNode);
            MqttNode.AppendChild(RetryIntervalNode);
            MqttNode.AppendChild(MaxQueuedMessagesNode);

            parentNode.AppendChild(MqttNode);

            #endregion MQTT

            #region Throttling
            XmlNode ThrottlingNode = doc.CreateNode(XmlNodeType.Element, "throttling", null);

            XmlNode MaxConnectionsNode = doc.CreateNode(XmlNodeType.Element, "max-connections", null);
            MaxConnectionsNode.InnerText = MaxConnections.ToString();
            XmlNode MaxMessageSizeNode = doc.CreateNode(XmlNodeType.Element, "max-message-size", null);
            MaxMessageSizeNode.InnerText = MaxMessageSize.ToString();
            XmlNode OutgoingLimitNode = doc.CreateNode(XmlNodeType.Element, "outgoing-limit", null);
            OutgoingLimitNode.InnerText = OutGoingLimit.ToString();

            XmlNode IncomingLimitNode = doc.CreateNode(XmlNodeType.Element, "incoming-limit", null);
            IncomingLimitNode.InnerText = InComingLimit.ToString();

            ThrottlingNode.AppendChild(MaxConnectionsNode);
            ThrottlingNode.AppendChild(MaxMessageSizeNode);
            ThrottlingNode.AppendChild(OutgoingLimitNode);
            ThrottlingNode.AppendChild(IncomingLimitNode);

            parentNode.AppendChild(ThrottlingNode);
            #endregion Throttling

            #region General
            XmlNode GeneralNode = doc.CreateNode(XmlNodeType.Element, "general", null);

            XmlNode UpdateCheckEnabledNode = doc.CreateNode(XmlNodeType.Element, "update-check-enabled", null);
            UpdateCheckEnabledNode.InnerText = UpdateCheckEnabled.ToString();

            GeneralNode.AppendChild(UpdateCheckEnabledNode);

            parentNode.AppendChild(GeneralNode);

            #endregion General

            //Final Save
            doc.Save(Path);
        }

        public void RemoveOldSettings()
        {
            //using LINQTOXML Delete
            XDocument xdoc = XDocument.Load(Path);
            xdoc.Descendants("hivemq").Descendants("listeners").Remove();
            xdoc.Descendants("hivemq").Descendants("mqtt").Remove();
            xdoc.Descendants("hivemq").Descendants("throttling").Remove();
            xdoc.Descendants("hivemq").Descendants("general").Remove();
            //to save the file
            xdoc.Save(Path);
        }

        #endregion SaveMqttSettings 

        public void Load(MQTT savedObj)
        {

            if (savedObj != null)
            {
                IPAddress = savedObj.IPAddress;
                DefaultPortNumber = savedObj.DefaultPortNumber;
                MaxClientIdLength = savedObj.MaxClientIdLength;
                RetryInterval = savedObj.RetryInterval;
                MaxQueuedMessages = savedObj.MaxQueuedMessages;
                MaxConnections = savedObj.MaxConnections;
                OutGoingLimit = savedObj.OutGoingLimit;
                InComingLimit = savedObj.InComingLimit;
                UpdateCheckEnabled = savedObj.UpdateCheckEnabled;
            }


        }
      
        
    }
    #endregion MQTT class
}
#endregion OPCEngine Namespace
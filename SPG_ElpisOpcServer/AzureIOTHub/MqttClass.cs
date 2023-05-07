using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using uPLibrary.Networking.M2Mqtt;
using System.Windows;
using System.Net;
using System.Windows.Forms;

namespace AzureIOTHub
{
    public class MqttClass
    {
        public MqttClient mqttClient;

        public void start()
        {
            mqttClient.Connect(Guid.NewGuid().ToString());
        }
        public void isconnected()
        {
            if (mqttClient.IsConnected == true)
            {
                mqttClient.Disconnect();
            }
        }



        #region MQTT
        public void MqttPart()
        {

            string DirDebug = System.IO.Directory.GetCurrentDirectory();
            //string DirProject = DirDebug;

            //for (int counter_slash = 0; counter_slash < 3; counter_slash++)
            //{
            //    DirProject = DirProject.Substring(0, DirProject.LastIndexOf(@"\"));
            //}

            string path = DirDebug + "\\hivemq-3.1.1\\hivemq-3.1.1\\conf\\config.xml";

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            XmlNodeList ipAddress = doc.SelectNodes("/hivemq/listeners/tcp-listener/bind-address");



            mqttClient = new MqttClient(IPAddress.Parse(ipAddress[0].InnerText).ToString());
            try
            {
                mqttClient.Connect(Guid.NewGuid().ToString());

                mqttClient.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
                mqttClient.MqttMsgPublished += Client_MqttMsgPublished;
            }
            catch (Exception ex)
            {
                string ErrMessage = ex.Message;
                MessageBox.Show("Looks like MQTT Broker is not started yet", "Elpis OPC Server-IOT", MessageBoxButtons.OK);
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

        public void Publish()
        {
            //mqttClient.Publish("Elpis-IOT/Tags", Encoding.UTF8.GetBytes("Manikandan"));
            mqttClient.Publish("Elpis-IOT/Tags", Encoding.UTF8.GetBytes("Elpis OPC Server"));
        }
        #endregion MQTT

    }
}

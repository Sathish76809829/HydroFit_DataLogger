#region Namespaces
using Elpis.Windows.OPC.Server;
using OPCEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
#endregion Namespaces

#region ElpisOpcServer Namespace
namespace ElpisOpcServer
{
    #region IoTSettingsUI class
    /// <summary>
    /// Interaction logic for IoTSettingsUI.xaml
    /// </summary>
    public partial class IoTSettingsUI : UserControl
    {
        public AboutPageUI about { get; set; }
        #region Constructor

        public IoTSettingsUI()
        {
            InitializeComponent();
            //LoadMqttSettings();
            //LoadAzureIoTHubSettings();
            about = new AboutPageUI();
        }
        #endregion Constructor    

        #region MqttSettingsBtn_Click Event

        private void MqttSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            AzureIoTGrid.Visibility = Visibility.Hidden;
            MqttSettingsGrid.Visibility = Visibility.Visible;
            HeaderDisplay.Content = "MQTT Protocol Configuration Settings";
        }

        #endregion MqttSettingsBtn_Click Event

        #region AzureIoTSettingsBtn_Click Event
        private void AzureIoTSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            MqttSettingsGrid.Visibility = Visibility.Hidden;
            AzureIoTGrid.Visibility = Visibility.Visible;
            HeaderDisplay.Content = "Azure-IoT Cloud Configuration Settings";
        }
        #endregion AzureIoTSettingsBtn_Click Event

        #region ApplyBtn_Click Event
        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            ConfigurationSettingsUI.elpisServer.LoadDataLogCollection("Internet Of Things", @"Elpis OPC Server\IoT",
              "Mqtt protocol configuration settings are changed successfully.", LogStatus.Information);
            ConfigurationSettingsUI.elpisServer.LoadDataLogCollection("Internet Of Things", @"Elpis OPC Server\IoT",
            "Azure IoT configuration settings are changed successfully.", LogStatus.Information);
        }
        #endregion ApplyBtn_Click Event

        #region FinishBtn_Click Event
        private void FinishBtn_Click(object sender, RoutedEventArgs e)
        {
            
            this.Visibility = Visibility.Hidden;
        }

        public void SaveSettings()
        {
            ElpisServer.mqttObj.SaveMqttSettings(MqttSettingsPropertyGrid.SelectedObject);
            ElpisServer.AzureIoTHubObj.SaveAzureIoTSettings(AzureIoTPropertyGrid.SelectedObject);
            ConfigurationSettingsUI.elpisServer.SaveLastLoadedProject();
        }
        #endregion FinishBtn_Click Event

        #region CancelBtn_Click Event
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }
        #endregion CancelBtn_Click Event

        #region slidebtn_Click Event

        private void slidebtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }
        #endregion slidebtn_Click Event

        #region minbtn_Click Event

        private void minbtn_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            // Minimize
            window.WindowState = WindowState.Minimized;
        }

        #endregion minbtn_Click Event

        #region closebtn_Click Event

        private void closebtn_Click(object sender, RoutedEventArgs e)
        {
            //this.Visibility = Visibility.Hidden;
            Window parent = Window.GetWindow(this);
            parent.Close();

        }

        #region closebtn_Click Event

        #endregion FinishBtnAzureIoT_Click Event
        private void FinishBtnAzureIoT_Click(object sender, RoutedEventArgs e)
        {
            object obj = AzureIoTPropertyGrid.SelectedObject;

        }
        #endregion FinishBtnAzureIoT_Click Event
        
        private void IoTSettings_Loaded(object sender, RoutedEventArgs e)
        {
            ConfigurationSettingsUI.elpisServer.OpenLastLoadedProject();
            LoadMqttSettings();
            LoadAzureIoTHubSettings();
        }


        #region LoadMqttSettings function

        public void LoadMqttSettings()
        {
            if (ConfigurationSettingsUI.elpisServer.MqttClientCollection != null)
            {
                if (ConfigurationSettingsUI.elpisServer.MqttClientCollection.Count == 0)
                    ConfigurationSettingsUI.elpisServer.MqttClientCollection.Add(new MQTT());

                MqttSettingsPropertyGrid.SelectedObject = ConfigurationSettingsUI.elpisServer.MqttClientCollection[0];
                ConfigurationSettingsUI.elpisServer.LoadDataLogCollection("Internet Of Things", @"Elpis OPC Server\IoT",
              "Mqtt protocol settings are loaded successfully.", LogStatus.Information);
            }
        }

        #endregion LoadMqttSettings function


        #region LoadAzureIoTHubSettings function

        public void LoadAzureIoTHubSettings()
        {
            if (ConfigurationSettingsUI.elpisServer.AzureIoTCollection != null)
            {
                if (ConfigurationSettingsUI.elpisServer.AzureIoTCollection.Count == 0)
                    ConfigurationSettingsUI.elpisServer.AzureIoTCollection.Add(new AzureIoTHub());

                AzureIoTPropertyGrid.SelectedObject = ConfigurationSettingsUI.elpisServer.AzureIoTCollection[0];
                ConfigurationSettingsUI.elpisServer.LoadDataLogCollection("Internet Of Things", @"Elpis OPC Server\IoT",
              "Azure IoT configuration settings are loaded successfully.", LogStatus.Information);
            }
        }

        #endregion LoadAzureIotHubSettings function

        private void MqttSettingsPropertyGrid_PropertyValueChanged(object sender, Xceed.Wpf.Toolkit.PropertyGrid.PropertyValueChangedEventArgs e)
        {
            try
            {
                MQTT selectedObject = MqttSettingsPropertyGrid.SelectedObject as MQTT;
                bool isCorrectIP = Regex.IsMatch(selectedObject.IPAddress, @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$");
                if (isCorrectIP)
                {
                    MessageBox.Show("Enter the correct ip Address");
                }
            }
            catch (Exception ex)
            {
                //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                    ElpisServer.Addlogs("Configuration", @"Elpis/IoT", ex.Message, LogStatus.Error);
                //}), DispatcherPriority.Normal, null);
            }
        }

        private void AzureIoTPropertyGrid_PropertyValueChanged(object sender, Xceed.Wpf.Toolkit.PropertyGrid.PropertyValueChangedEventArgs e)
        {
            var selectedObject = AzureIoTPropertyGrid.SelectedObject as AzureIoTHub;
            //bool isCorrectIP = Regex.IsMatch(selectedObject.IPAddress, @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$");
        }
        #region about window
        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;
            Window aboutWindow = new Window();
            aboutWindow.Content = about;
            aboutWindow.WindowStyle = WindowStyle.ToolWindow;
            aboutWindow.Show();
            //about.Visibility = Visibility.Visible;
            //MainWindow2.contentPresenter.Content = about;
        }
        #endregion
    }
    #endregion IoTSettingsUI class
}
#endregion ElpisOpcServer Namespace
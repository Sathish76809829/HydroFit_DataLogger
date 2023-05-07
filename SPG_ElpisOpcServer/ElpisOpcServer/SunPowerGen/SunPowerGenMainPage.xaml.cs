using Elpis.Windows.OPC.Server;
using Modbus.Device;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace ElpisOpcServer.SunPowerGen
{
    /// <summary>
    /// Interaction logic for SunPowerGenMainPage.xaml
    /// </summary>
    public partial class SunPowerGenMainPage : UserControl
    {
        #region Properties
        private StrokeTestWindow StrokeTestWindow { get; set; }
        private Hold_MidPositionTestUI HoldMidPositionTestWindow { get; set; }
        private SlipStickTestUI SlipStickTestWindow { get; set; }
        private AllTestUI AllTestWindow { get; set; }
        public static DeviceBase DeviceObject { get; set; }
        internal static SerialPort DeviceSerialPort { get; set; }
        internal static TcpClient DeviceTcpClient { get; set; }
        internal static TcpClient DeviceTcpSocketClient { get; set; }
        internal static TcpListener listener { get; set; }
        internal static Image image { get; set; }
        internal static ModbusIpMaster ModbusTcpMaster { get; set; }
        internal static ModbusSerialMaster ModbusSerialPortMaster { get; set; }
        internal static LibplctagWrapper.Libplctag ABEthernetClient { get; set; }
        public static ObservableCollection<IConnector> ConnectorCollection { get; private set; }
        public string ReportLocation { get; set; }
        public static string SelectedConnector { get; set; }
        public static string SelectedDevice { get; set; }
        public static bool isTestRunning { get; set; }

        // internal static List<LibplctagWrapper.Tag> EIPTags { get; set; }
        #endregion


        public SunPowerGenMainPage()
        {
            InitializeComponent();
            image = imgLogo;
            ReportLocation = GetConfigValue("ReportLocation"); //ConfigurationManager.AppSettings["ReportLocation"]==null?null: ConfigurationManager.AppSettings["ReportLocation"].ToString();
            if (string.IsNullOrEmpty(ReportLocation) || !(Directory.Exists(ReportLocation)))
            {
                ReportLocation = string.Format("{0}\\Reports", Directory.GetCurrentDirectory());
                UpdateConfigKey("ReportLocation", ReportLocation);
            }
            // tbxReportsPath.Text = ReportLocation;
            this.DataContext = this;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (this.IsLoaded)
            //{
            try
            {
                if (tabControlTest.SelectedIndex == 0)
                {
                    if (StrokeTestWindow == null)
                        StrokeTestWindow = new StrokeTestWindow();
                    contentStrokeTest.Content = StrokeTestWindow;
                    //HomePage page = new HomePage();
                    //contentStrokeTest.Content = page;
                }
                if (tabControlTest.SelectedIndex == 1)
                {
                    if (HoldMidPositionTestWindow == null)
                        HoldMidPositionTestWindow = new Hold_MidPositionTestUI();
                    contentHoldMidTest.Content = HoldMidPositionTestWindow;
                }
                else if (tabControlTest.SelectedIndex == 2)
                {
                    if (SlipStickTestWindow == null)
                        SlipStickTestWindow = new SlipStickTestUI();
                    contentSlipStickTest.Content = SlipStickTestWindow;
                }
            }
            catch (Exception)
            {

            }
            //}
        }

        private void mainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded)
            {
                if (mainTabControl.SelectedIndex == 0)
                {
                    //ReportSettingsData(); 
                    
                    gridStatus.Visibility = Visibility.Hidden;
                }
                else if(mainTabControl.SelectedIndex==1)
                {
                    SaveLoadedData();
                }
            }

        }


        private void txtTimer_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            Regex expr = new Regex("^[0-9]{1,2}$");
            if (tb.Name == "txtTimer")
            {
                if (expr.IsMatch(txtTimer.Text))
                {
                    if (txtTimer.Text.Length == 2)
                    {
                        if (txtTimerSec != null && txtTimerMin != null)
                        {
                            txtTimerMin.SelectAll();
                            txtTimerMin.Focus();
                        }
                    }
                }
                else
                {
                    txtTimer.Text = "00";
                }

            }

            else if (tb.Name == "txtTimerMin")
            {
                if (expr.IsMatch(txtTimerMin.Text))
                {
                    if (txtTimerMin.Text.Length == 2)
                    {
                        if (txtTimerSec != null)
                        {
                            txtTimerSec.SelectAll();
                            txtTimerSec.Focus();
                        }
                    }
                }
                else
                {
                    txtTimerMin.Text = "00";
                }
            }

            else if (tb.Name == "txtTimerSec")
            {
                if (expr.IsMatch(txtTimerSec.Text))
                {
                    if (txtTimerSec.Text.Length == 2)
                    {
                        if (txtTimerSec != null)
                        {
                            // txtTimerSec.SelectAll();
                            //txtTimerSec.Focus();
                        }
                    }
                }
                else
                {
                    txtTimerSec.Text = "00";
                }
            }

        }
        
        private void txtTimerSec_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb.Text == "")
                tb.Text = "00";
            else if (tb.Text.Length == 1)
                tb.Text = string.Format("0{0}", tb.Text);
        }


        private void cmbConnectorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbConnectorList.SelectedIndex > -1)
            {
                gridStatus.Visibility = Visibility.Hidden;
                ConnectorBase connector = ConnectorCollection[cmbConnectorList.SelectedIndex] as ConnectorBase;
                ObservableCollection<DeviceBase> deviceCollection = connector.DeviceCollection;
                List<string> deviceList = null;
                if (deviceCollection != null)
                {
                    deviceList = deviceCollection.Select(d => d.DeviceName).ToList();
                }
                else
                {
                    txtErrorInfo.Text = "";
                    txtStatus.Text = "";                   
                    AddErrorInfo("No Devices found in Selected connector, please create device under Connector.");
                    gridStatus.Visibility = Visibility.Visible;
                }
                cmbDeviceList.ItemsSource = null;
                cmbDeviceList.Items.Clear();
                cmbDeviceList.ItemsSource = deviceList;
                if(deviceList!=null)
                    cmbDeviceList.SelectedIndex = 0;
            }
        }

        private void cmbDeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {           
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ReportSettingsData();
        }

        private void ReportSettingsData()
        {
            if (mainTabControl != null && mainTabControl.SelectedIndex == 0)
            {
                cmbConnectorList.ItemsSource = null;
                cmbDeviceList.ItemsSource = null;
                txtStatus.Text = "";
                txtErrorInfo.Text = "";
                string projectFilePath = string.Format(@"{0}\opcSunPowerGen.elp", Directory.GetCurrentDirectory());
                ConnectorCollection = null;
                FileHandler FileHandle = new FileHandler();
                try
                {
                    if (File.Exists(projectFilePath))
                    {
                        Stream stream = File.Open(projectFilePath, FileMode.OpenOrCreate);

                        BinaryFormatter bformatter = new BinaryFormatter();

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
                    if (ConnectorCollection != null && ConnectorCollection.Count > 0)
                    {
                        List<string> ConnectorList = ConnectorCollection.Select(c => c.Name).ToList();
                        //  ConnectorList.Insert(0, "--Select--");
                        cmbConnectorList.ItemsSource = ConnectorList;
                        cmbConnectorList.SelectedIndex = 0;
                        SelectedConnector = cmbConnectorList.SelectedValue.ToString();
                    }
                    else
                    {
                        gridStatus.Visibility = Visibility.Visible;
                        AddErrorInfo("No Connectors found in Configuration, please configure Connectors.");
                    }
                    cmbConnectorList.IsEnabled = false;
                    btnConnectorSave.IsEnabled = false;
                    btnConnectorEdit.IsEnabled = true;
                    if ((ConnectorCollection!=null && ConnectorCollection.Count>0) &&((ConnectorBase)ConnectorCollection[0]).DeviceCollection != null && ((ConnectorBase)ConnectorCollection[0]).DeviceCollection.Count > 0)
                    {
                        List<string> DeviceList = ((ConnectorBase)ConnectorCollection[0]).DeviceCollection.Select(d => d.DeviceName).ToList();
                        cmbDeviceList.ItemsSource = DeviceList;
                        cmbDeviceList.SelectedIndex = 0;
                        SelectedDevice = cmbDeviceList.SelectedValue.ToString();
                    }
                    else
                    {
                        gridStatus.Visibility = Visibility.Visible;
                        AddErrorInfo("No Devices found in selected Connector.");
                    }

                    cmbDeviceList.IsEnabled = false;
                    btnDeviceSave.IsEnabled = false;
                    btnDeviceEdit.IsEnabled = true;

                    string reportPrefix = GetConfigValue("ReportPrefix");
                    if (reportPrefix == null)
                    {
                        //throw new ArgumentNullException("Key, < ReportPrefix > not found in the Configuration file.");
                        AddErrorInfo("Key, < ReportPrefix > not found in the Configuration file and updated with Default Report Prefix.");
                        reportPrefix = "SPG";
                        UpdateConfigKey("ReportPrefix", reportPrefix);
                    }
                    else if (reportPrefix == "")
                    {
                        AddErrorInfo("Key, < ReportPrefix > updated with Default Report Prefix.");
                        reportPrefix = "SPG";
                        UpdateConfigKey("ReportPrefix", reportPrefix);
                    }
                    txtReportPrefix.Text = reportPrefix;

                    btnReportPrefixSave.IsEnabled = false;
                    btnReportPrefixEdit.IsEnabled = true;
                    txtReportPrefix.IsEnabled = false;

                    string reportLocation = GetConfigValue("ReportLocation");
                    if (reportLocation == "")
                    {
                        // throw new ArgumentNullException("Key, < ReportLocation > not found in the Configuration file.");
                        txtReportLocation.Text = string.Format("{0}\\Reports", Directory.GetCurrentDirectory());
                        UpdateConfigKey("ReportLocation", txtReportLocation.Text);
                    }
                    else if (reportLocation == null)
                        AddErrorInfo("Key, < ReportLocation > not found in the Configuration file.");
                    else
                        txtReportLocation.Text = reportLocation;
                    btnReportLocationSave.IsEnabled = false;
                    btnReportLocationEdit.IsEnabled = true;
                    txtReportLocation.IsEnabled = false;

                    string dataReadInterval = GetConfigValue("DataReadInterval");
                    if (dataReadInterval == null)
                    {
                        // throw new ArgumentNullException("Key, < DataReadInterval > not found in the Configuration file.");
                        AddErrorInfo("Key, < DataReadInterval > not found in the Configuration file, added with Default values.");
                        dataReadInterval = "00:00:05";
                        UpdateConfigKey("DataReadInterval",dataReadInterval);
                    }
                    else
                    {
                        string[] timeIntervals = dataReadInterval.Split(':');
                        if (timeIntervals.Count() == 3)
                        {
                            txtTimer.Text = timeIntervals[0];
                            txtTimerMin.Text = timeIntervals[1];
                            txtTimerSec.Text = timeIntervals[2];
                            gridStatus.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            // throw new ArgumentNullException("Key, < DataReadInterval > contains Invalid value in the Configuration file.");
                            AddErrorInfo("Key, < DataReadInterval > contains Invalid value in the Configuration file.");
                        }
                    }
                    btnDataReadIntervalSave.IsEnabled = false;
                    btnDataReadIntervalEdit.IsEnabled = true;
                    panelDataRead.IsEnabled = false;

                    if (txtStatus.Text.Length == 0)
                        txtStatus.Text = "Configuration Updated Successfully";
                    gridStatus.Visibility = Visibility.Visible;


                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            this.DataContext = this;
        }

        private string GetConfigValue(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                string result = appSettings[key] ?? null;
                return result;
            }
            catch (ConfigurationErrorsException e)
            {
                return null;
            }
            //    XmlDocument xmlDoc = new XmlDocument();
            //    xmlDoc.Load(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\App.config");
            //    XmlNode appSettingsNode = xmlDoc.SelectSingleNode("configuration/appSettings");
            //    foreach (XmlNode childNode in appSettingsNode)
            //    {
            //        if (childNode.Attributes != null && childNode.Attributes["key"].Value == key)
            //            return childNode.Attributes["value"].Value;
            //    }

            //    return null;
        }

        private void txtTimer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                textBox.SelectAll();
            }
        }

        private void btnEditSave_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            //if(button!=null)
            //{
            //    if(button.Content.ToString()=="Edit")
            //    {
            txtReportPrefix.IsEnabled = true;
            txtReportLocation.IsEnabled = true;
            cmbConnectorList.IsEnabled = true;
            cmbDeviceList.IsEnabled = true;
            panelDataRead.IsEnabled = true;
            //btnEditSave.IsEnabled = false;
            //btnSave.IsEnabled = true;
            //    }
            //}
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtErrorInfo.Text = "";
                txtStatus.Text = "";
                // btnEditSave.IsEnabled = true;
                txtReportPrefix.IsEnabled = false;
                txtReportLocation.IsEnabled = false;
                cmbConnectorList.IsEnabled = false;
                cmbDeviceList.IsEnabled = false;
                panelDataRead.IsEnabled = false;
                gridStatus.Visibility = Visibility.Visible;
                UpdateConfigKey("ReportPrefix", txtReportPrefix.Text);
                UpdateConfigKey("ReportLocation", txtReportLocation.Text);
                UpdateConfigKey("DataReadInterval", string.Format("{0}:{1}:{2}", txtTimer.Text, txtTimerMin.Text, txtTimerSec.Text));
                //ReportLocation = txt;
                ///tbxReportsPath.Text = ReportLocation;
               // btnSave.IsEnabled = false;
                txtStatus.Text = "Configuration Updated Successfully";

            }

            catch (ArgumentNullException ex)
            {
                txtStatus.Text = ex.ParamName;
            }
            catch (Exception ex)
            {
                txtStatus.Text = ex.Message;
            }

        }

        public void UpdateConfigKey(string strKey, string newValue)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[strKey] == null)
                {
                    settings.Add(strKey, newValue);
                }
                else
                {
                    settings[strKey].Value = newValue;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);

                //XmlDocument xmlDoc = new XmlDocument();
                //xmlDoc.Load(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\App.config");
                //if (!ConfigKeyExists(strKey))
                //{
                //    //throw new ArgumentNullException("Key", "<" + strKey + "> not found in the Configuration file.");
                //    //XmlNode appSettingsNode1 = xmlDoc.SelectSingleNode("configuration/appSettings");
                //    //XmlElement elem = xmlDoc.CreateElement(strKey);
                //    //elem.InnerText = newValue;
                //    //appSettingsNode1.AppendChild(elem);
                //    //xmlDoc.Save(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\App.config");
                //    //xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                //    Configuration config = ConfigurationManager.OpenExeConfiguration(Directory.GetCurrentDirectory());
                //    config.AppSettings.Settings.Add(strKey, newValue);
                //    config.Save(ConfigurationSaveMode.Minimal);
                //}
                //XmlNode appSettingsNode = xmlDoc.SelectSingleNode("configuration/appSettings");
                //foreach (XmlNode childNode in appSettingsNode)
                //{
                //    if (childNode.Attributes!=null && childNode.Attributes["key"].Value == strKey)
                //        childNode.Attributes["value"].Value = newValue;
                //}
                //xmlDoc.Save(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\App.config");
                //xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            }
            catch (ArgumentNullException ae)
            {

            }
            catch (Exception ex)
            {
                AddErrorInfo(ex.Message);
            }
        }

        public bool ConfigKeyExists(string strKey)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\App.config");
            XmlNode appSettingsNode = xmlDoc.SelectSingleNode("configuration/appSettings");
            foreach (XmlNode childNode in appSettingsNode)
            {
                if (childNode.Attributes != null && childNode.Attributes["key"].Value == strKey)
                    return true;
            }
            return false;
        }

        public void AddErrorInfo(string error)
        {
            if (txtStatus.Text.Length == 0)
                txtStatus.Text = "Configuration have following errors:";
            txtErrorInfo.Text = string.Format("{0}{1}\n", txtErrorInfo.Text, error);
            txtErrorInfo.Foreground = Brushes.Red;
            gridStatus.Visibility = Visibility.Visible;
        }


        private void btnEditOperation_Click(object sender, RoutedEventArgs e)
        {

            txtErrorInfo.Text = "";
            txtStatus.Text = "";
            gridStatus.Visibility = Visibility.Hidden;
            Button button = sender as Button;
            if (!isTestRunning)
            {
                if (button != null)
                {
                    switch (button.Name)
                    {
                        case "btnReportPrefixEdit":
                            btnReportPrefixEdit.IsEnabled = false;
                            btnReportPrefixSave.IsEnabled = true;
                            txtReportPrefix.IsEnabled = true;
                            // UpdateConfigKey("ReportPrefix", txtReportPrefix.Text);
                            break;
                        case "btnReportLocationEdit":
                            btnReportLocationEdit.IsEnabled = false;
                            btnReportLocationSave.IsEnabled = true;
                            txtReportLocation.IsEnabled = true;
                            break;
                        case "btnDataReadIntervalEdit":
                            btnDataReadIntervalEdit.IsEnabled = false;
                            btnDataReadIntervalSave.IsEnabled = true;
                            panelDataRead.IsEnabled = true;
                            break;
                        case "btnConnectorEdit":
                            if (cmbConnectorList.Items.Count > 0)
                            {
                                btnConnectorEdit.IsEnabled = false;
                                btnConnectorSave.IsEnabled = true;
                                cmbConnectorList.IsEnabled = true;
                            }
                            else
                            {
                                AddErrorInfo("Not able to edit, no Connectors found.");
                            }
                            break;
                        case "btnDeviceEdit":
                            if (cmbDeviceList.Items.Count > 0)
                            {
                                btnDeviceEdit.IsEnabled = false;
                                btnDeviceSave.IsEnabled = true;
                                cmbDeviceList.IsEnabled = true;
                            }
                            else
                            {
                                AddErrorInfo("Not able to edit, no Devices found in current Connector.");
                            }
                            break;

                    }
                }
            }
            else
            {
                AddErrorInfo("Currently Test is running and Recording the Data. So, not able to edit configuration.");
            }
        }

        private void btnSaveOperation_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                switch (button.Name)
                {
                    case "btnReportPrefixSave":
                        btnReportPrefixSave.IsEnabled = false;
                        btnReportPrefixEdit.IsEnabled = true;
                        txtReportPrefix.IsEnabled = false;
                        UpdateConfigKey("ReportPrefix", txtReportPrefix.Text);
                        break;
                    case "btnReportLocationSave":
                        try
                        {
                            Directory.CreateDirectory(txtReportLocation.Text);
                            btnReportLocationEdit.IsEnabled = true;
                            btnReportLocationSave.IsEnabled = false;
                            txtReportLocation.IsEnabled = false;
                            UpdateConfigKey("ReportLocation", txtReportLocation.Text);
                            ReportLocation = txtReportLocation.Text;

                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Entered invalid ReportLocation, please enter valid location or format.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Error);

                        }
                        break;
                    case "btnDataReadIntervalSave":
                        btnDataReadIntervalEdit.IsEnabled = true;
                        btnDataReadIntervalSave.IsEnabled = false;
                        panelDataRead.IsEnabled = false;
                        UpdateConfigKey("DataReadInterval", string.Format("{0}:{1}:{2}", txtTimer.Text, txtTimerMin.Text, txtTimerSec.Text));
                        break;
                    case "btnConnectorSave":
                        btnConnectorEdit.IsEnabled = true;
                        btnConnectorSave.IsEnabled = false;
                        cmbConnectorList.IsEnabled = false;
                        SelectedConnector = cmbConnectorList.SelectedValue.ToString() ?? null;
                        break;
                    case "btnDeviceSave":
                        btnDeviceEdit.IsEnabled = true;
                        btnDeviceSave.IsEnabled = false;
                        cmbDeviceList.IsEnabled = false;
                        SelectedDevice = cmbDeviceList.SelectedValue.ToString() ?? null;
                        break;
                }
            }
            this.DataContext = this;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            SaveLoadedData();
        }

        private void SaveLoadedData()
        {
            UpdateConfigKey("ReportPrefix", string.Format("{0}",txtReportPrefix.Text));
            btnReportPrefixEdit.IsEnabled = true;
            btnReportPrefixSave.IsEnabled = false;
            txtReportPrefix.IsEnabled = false;

            UpdateConfigKey("ReportLocation", txtReportLocation.Text);
            btnReportLocationEdit.IsEnabled = true;
            btnReportLocationSave.IsEnabled = false;
            txtReportLocation.IsEnabled = false;

            UpdateConfigKey("DataReadInterval", string.Format("{0}:{1}:{2}", txtTimer.Text, txtTimerMin.Text, txtTimerSec.Text));
            btnDataReadIntervalEdit.IsEnabled = true;
            btnDataReadIntervalSave.IsEnabled = false;
            panelDataRead.IsEnabled = false;

            if (cmbConnectorList.Items.Count > 0)
                SelectedConnector = cmbConnectorList.SelectedValue.ToString();
            btnConnectorEdit.IsEnabled = true;
            btnConnectorSave.IsEnabled = false;
            cmbConnectorList.IsEnabled = false;

            if (cmbDeviceList.Items.Count > 0)
                SelectedDevice = cmbDeviceList.SelectedValue.ToString();
            btnDeviceEdit.IsEnabled = true;
            btnDeviceSave.IsEnabled = false;
            cmbDeviceList.IsEnabled = false;
        }       

        private void txtReportPrefix_TextChanged(object sender, TextChangedEventArgs e)
        {

        }     
        
    }
}

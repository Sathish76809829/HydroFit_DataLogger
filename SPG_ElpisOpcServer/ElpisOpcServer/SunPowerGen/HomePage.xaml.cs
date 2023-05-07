using Elpis.Windows.OPC.Server;
using ElpisOpcServer.SunPowerGen;
using iTextSharp.text.pdf;
using Modbus.Device;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
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
using System.Windows.Navigation;

namespace ElpisOpcServer.SunPowerGen
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : UserControl
    {

        #region Properties
        private StrokeTestWindow StrokeTestWindow { get; set; }
        public static Hold_MidPositionTestUI HoldMidPositionTestWindow { get; set; }
        private SlipStickTestUI SlipStickTestWindow { get; set; }
        public static Pump_Test PumpTestWindow { get; set; }
        public static DeviceBase DeviceObject { get; set; }
        internal static SerialPort DeviceSerialPort { get; set; }
        internal static TcpClient DeviceTcpClient { get; set; }
        internal static Image image { get; set; }
        internal static ModbusIpMaster ModbusTcpMaster { get; set; }
        internal static ModbusSerialMaster ModbusSerialPortMaster { get; set; }
        internal static LibplctagWrapper.Libplctag ABEthernetClient { get; set; }
        public ObservableCollection<IConnector> ConnectorCollection { get; private set; }
        public string ReportLocation { get; set; }
        public static string SelectedConnector { get; set; }
        public static string SelectedDevice { get; set; }
        public static bool isTestRunning { get; set; }

        public static StrokeTestInformation strokeTestInfo { get; set; }
        public static Hold_MidPositionLineATestInformation holdMidPositionLineAInfo { get; set; }
        public static Hold_MidPositionLineBTestInformation holdMidPositionLineBinfo { get; set; }
        public static Slip_StickTestInformation slipStickTestInformation { get; set; }
        public static PumpTestInformation PumpTestInformation { get; set; }
        #endregion
        public HomePage()
        {
            InitializeComponent();
            image = imgLogo;
            
            ReportLocation = GetConfigValue("ReportLocation"); //ConfigurationManager.AppSettings["ReportLocation"]==null?null: ConfigurationManager.AppSettings["ReportLocation"].ToString();
            if (string.IsNullOrEmpty(ReportLocation) || !(Directory.Exists(ReportLocation)))
            {
                ReportLocation = string.Format("{0}\\Reports", Directory.GetCurrentDirectory());
                //ReportLocation = string.Format("{0}\\Reports", @"D:\Sathish.b");
                UpdateConfigKey("ReportLocation", ReportLocation);
                Directory.CreateDirectory(ReportLocation);
            }
            this.tabControlTest.SelectedIndex = 3;
            //ElpisOPCServerMainWindow.pump_Test = new Pump_Test();
        }
        #region txtJobNumber_TextChanged
        private void txtJobNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtJobNumber.Text != null)
            {
                if (txtJobNumber.Text == "")
                    txtReportNumber.Text = "";
                else
                {
                    if (strokeTestInfo != null)
                    {

                        // txtReportNumber.Text = string.Format("SPG_StrokeTest_{0}_R001", txtJobNumber.Text);
                        strokeTestInfo.ReportNumber = string.Format("SPG_StrokeTest_{0}_R001", txtJobNumber.Text);
                    }
                    if (holdMidPositionLineAInfo != null)
                    {
                        // txtReportNumber.Text = string.Format("SPG_HoldMidPositionLineATest_{0}_R002", txtJobNumber.Text);
                        holdMidPositionLineAInfo.ReportNumber = string.Format("SPG_HoldMidPositionLineATest_{0}_R002", txtJobNumber.Text);
                    }

                    if (holdMidPositionLineBinfo != null)
                    {
                        // txtReportNumber.Text = string.Format("SPG_HoldMidPositionLineBTest_{0}_R002", txtJobNumber.Text);
                        holdMidPositionLineBinfo.ReportNumber = string.Format("SPG_HoldMidPositionLineBTest_{0}_R002", txtJobNumber.Text);
                    }

                    if (slipStickTestInformation != null)
                    {
                        // txtReportNumber.Text = string.Format("SPG_SlipStickTest_{0}_R003", txtJobNumber.Text);
                        slipStickTestInformation.ReportNumber = string.Format("SPG_SlipStickTest_{0}_R003", txtJobNumber.Text);
                    }

                }
            }
        }
        #endregion txtJobNumber_TextChanged
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (StrokeTestWindow == null)
                StrokeTestWindow = new StrokeTestWindow();
            if (HoldMidPositionTestWindow == null)
                HoldMidPositionTestWindow = new Hold_MidPositionTestUI();
            if (SlipStickTestWindow == null)
                SlipStickTestWindow = new SlipStickTestUI();
            if (PumpTestWindow == null)
                PumpTestWindow = new Pump_Test();

            string reportNumber = strokeTestInfo.ReportNumber;
            this.gridMain.DataContext = null;
            strokeTestInfo.IsTestStarted = false;
            strokeTestInfo.ReportNumber = reportNumber;
            this.gridMain.DataContext = strokeTestInfo;
            //strokeTestInfo.IsTestStarted = true;

            expanderCertificate.IsExpanded = true;
            //PumpExpanderCertificate.IsExpanded = true;
            ReportSettingsData();
            mainTabControl.SelectedIndex = 1;
            txtSelectedConnector.Text = SelectedConnector;
            txtSelectedDevice.Text = SelectedDevice;


            this.DataContext = this;
        }

        private void expanderCertificate_Expanded(object sender, RoutedEventArgs e)
        {
            //  expanderCertificate.IsExpanded = true;
            // expanderTestInfo.IsExpanded = false;
           //ElpisOPCServerMainWindow.pump_Test.spGenerateReport.Orientation = Orientation.Horizontal;
        }

        private void expanderCertificate_Collapsed(object sender, RoutedEventArgs e)
        {
            //expanderCertificate.IsExpanded = false;
            //expanderTestInfo.IsExpanded = true;
           ElpisOPCServerMainWindow.pump_Test.spGenerateReport.Orientation = Orientation.Horizontal;
        }

        //private void expanderTestInfo_Expanded(object sender, RoutedEventArgs e)
        //{
        //    expanderCertificate.IsExpanded = false;
        //    expanderTestInfo.IsExpanded = true;
        //}

        //private void expanderTestInfo_Collapsed(object sender, RoutedEventArgs e)
        //{
        //    expanderCertificate.IsExpanded = true;
        //    expanderTestInfo.IsExpanded = false;
        //}

        private void btnGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            Generatereport();
        }
        #region normal method for generate the report
        public void Generatereport()
        {
            PumpTestWindow.txtStatusMessage.Text = string.Empty;
           //ElpisOPCServerMainWindow.pump_Test.txtStatusMessage.Text = string.Empty;
            if (tabControlTest.SelectedIndex != 3)
            {
                ReportGeneration reportGeneration = new ReportGeneration();
                bool isCreateNew = false;
                string filesPath = string.Format("{0}\\{1}_{2}\\{3}", ReportLocation, txtJobNumber.Text, txtCustName.Text, txtCylinderNumber.Text);
               ElpisOPCServerMainWindow.pump_Test.panelStatusMessage.Visibility = Visibility.Visible;
                if (Directory.Exists(filesPath))
                {
                    string[] fileNames = Directory.GetFiles(filesPath, "*.csv");
                    if (fileNames.Length > 0)
                    {
                        reportGeneration.GeneratePDFReport(txtJobNumber.Text, txtCustName.Text, txtCylinderNumber.Text, isCreateNew);
                        //txtStatusMessage.Text = HomePage.strokeTestInfo.ReportStatus;

                    }
                    else
                        MessageBox.Show("No Recorded data found. Please, Start Recording data and then generate reports", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information);

                }
                else
                {
                    //MessageBox.Show(string.Format("No Data files found in {0} location",filesPath), "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information);
                    ElpisOPCServerMainWindow.pump_Test.txtStatusMessage.Text = "No Data files found.";
                }
                reportGeneration = null;

                //if (StrokeTestWindow != null)
                //{
                //   // bool isCreated = false;
                //    bool isCreated = StrokeTestWindow.GenereateStrokeTestReport();
                //    panelStatusMessage.Visibility = Visibility.Visible;
                //    if (isCreated)
                //    {

                //        txtStatusMessage.Text = "Reports Generated Successfully.";
                //    }
                //    else
                //    {
                //        txtStatusMessage.Text = "Error in Reports Generation.";
                //    }

                //}           

                //else
                //{
                //    MessageBox.Show("No Recorded data found. Please, Start Recording data and then generate reports", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information);
                //}

            }
            else
            {
                PumpReportGeneration reportGeneration = new PumpReportGeneration();
                bool isCreateNew = false;
                string filesPath = string.Format(@"{0}\{1}\{2}", ReportLocation, txtPumpJobNumber.Text, txtPumpReportNumber.Text);
                //ElpisOPCServerMainWindow.pump_Test.panelStatusMessage.Visibility = Visibility.Visible;
                PumpTestWindow.panelStatusMessage.Visibility = Visibility.Visible;
                if (Directory.Exists(filesPath) && PumpTestInformation.LineSeriesList != null)
                {
                    //  string[] fileNames = Directory.GetFiles(filesPath, "*.csv");
                    //if (fileNames.Length > 0)
                    //{
                    reportGeneration.GeneratePDFReport(txtPumpJobNumber.Text, txtPumpReportNumber.Text, isCreateNew);
                    //txtStatusMessage.Text = HomePage.strokeTestInfo.ReportStatus;

                    //   }
                    // else
                    //   MessageBox.Show("No Recorded data found. Please, Start Recording data and then generate reports", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information);

                }
                else
                {
                    MessageBox.Show("No record found.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information);
                    //ElpisOPCServerMainWindow.pump_Test.txtStatusMessage.Text = "No record found.";
                   PumpTestWindow.txtStatusMessage.Text = "No record found.";
                }
                reportGeneration = null;
            }
        }
        #endregion 
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {

            DisableInputs(false);
            BtnCommentBox.IsChecked = false;
            txtCommentBox.Text = "";
            txtCommnetUpdateMessage.Text = "";
            BtnTestDetails.IsChecked = false;
            txtTestDetailsBox.Text = "";
            txtTestDetailsMessage.Text = "";
            StrokeTestWindow.ResetStrokeTest();
            HoldMidPositionTestWindow.ResetHoldMidPosition();
            SlipStickTestWindow.ResetSlipStickTest();
            PumpTestWindow.ResetPumpTest();

            if (tabControlTest.SelectedIndex == 0)
            {
                strokeTestInfo.IsTestStarted = false;
                gridMain.DataContext = strokeTestInfo;
                strokeTestInfo.IsTestStarted = true;


            }
            else if (tabControlTest.SelectedIndex == 1)
            {
                if (HoldMidPositionTestWindow.rb_LineA.IsChecked == true)
                {
                    holdMidPositionLineAInfo.IsTestStarted = false;
                    gridMain.DataContext = holdMidPositionLineAInfo;
                    holdMidPositionLineAInfo.IsTestStarted = true;

                }
                else if (HoldMidPositionTestWindow.rb_LineB.IsChecked == true)
                {
                    holdMidPositionLineBinfo.IsTestStarted = false;
                    gridMain.DataContext = holdMidPositionLineBinfo;
                    holdMidPositionLineBinfo.IsTestStarted = true;
                    txtStatus.Text = "";
                }
            }
            else if (tabControlTest.SelectedIndex == 2)
            {
                slipStickTestInformation.IsTestStarted = false;
                gridMain.DataContext = slipStickTestInformation;
                slipStickTestInformation.IsTestStarted = true;
            }

            else if (tabControlTest.SelectedIndex == 3)
            {
                PumpTestInformation.IsTestStarted = false;
                gridMain.DataContext = PumpTestInformation;
                PumpTestInformation.IsTestStarted = true;
            }

            ElpisOPCServerMainWindow.pump_Test.panelStatusMessage.Visibility = Visibility.Hidden;
            ElpisOPCServerMainWindow.homePage.ReportTab.IsEnabled = true;
            ElpisOPCServerMainWindow.homePage.txtFilePath.IsEnabled = true;
        }


        private void txtReportPrefix_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex expr = new Regex("^[0-9]{1,2}$");
        }

        private void txtTimer_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;

            if (tb.Name == "txtTimer")
            {
                Regex expr = new Regex("^[0-9]{1,2}$");
                if (expr.IsMatch(txtTimer.Text))
                {
                    if (txtTimer.Text.Length == 2)
                    {
                        if (txtTimerSec != null && txtTimerMin != null && txtTimerMilliSec != null)
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
                Regex expr = new Regex("^[0-9]{1,2}$");
                if (expr.IsMatch(txtTimerMin.Text))
                {
                    if (txtTimerMin.Text.Length == 2)
                    {
                        if (txtTimerSec != null && txtTimerMilliSec != null)
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
                Regex expr = new Regex("^[0-9]{1,2}$");

                if (expr.IsMatch(txtTimerSec.Text))
                {
                    if (txtTimerSec.Text.Length == 2)
                    {
                        if (txtTimerMilliSec != null)
                        {
                            txtTimerMilliSec.SelectAll();
                            txtTimerMilliSec.Focus();
                        }
                    }
                }
                else
                {
                    txtTimerSec.Text = "00";
                }
            }

            else if (tb.Name == "txtTimerMilliSec")
            {
                Regex expr = new Regex("^[0-9][0-9]{1,2}$");
                if (expr.IsMatch(txtTimerMilliSec.Text))
                {
                    if (txtTimerMilliSec.Text.Length == 3)
                    {
                        if (txtTimerMilliSec != null)
                        {
                            //txtTimerSec.SelectAll();
                            //txtTimerSec.Focus();
                        }
                    }
                }
                else
                {
                    txtTimerMilliSec.Text = "000";
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

        private void txtTimer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                textBox.SelectAll();
            }
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
                            tbxReportsPath.Text = ReportLocation;
                            this.DataContext = this;
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Entered invalid ReportLocation, please enter valid location or format.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Error);

                        }
                        break;
                    case "btnDataReadIntervalSave":
                        if (txtTimer.Text == "00" && txtTimerMin.Text == "00" && txtTimerSec.Text == "00" && txtTimerMilliSec.Text == "000")
                        {
                            AddErrorInfo("Key, < DataReadInterval > has invalid value, updated with default values(00:00:05:000).");
                            txtTimer.Text = "00";
                            txtTimerMin.Text = "00";
                            txtTimerSec.Text = "05";
                            txtTimerMilliSec.Text = "000";
                        }
                        btnDataReadIntervalEdit.IsEnabled = true;
                        btnDataReadIntervalSave.IsEnabled = false;
                        panelDataRead.IsEnabled = false;
                        UpdateConfigKey("DataReadInterval", string.Format("{0}:{1}:{2}:{3}", txtTimer.Text, txtTimerMin.Text, txtTimerSec.Text, txtTimerMilliSec.Text));
                        break;
                    case "btnConnectorSave":
                        btnConnectorEdit.IsEnabled = true;
                        btnConnectorSave.IsEnabled = false;
                        cmbConnectorList.IsEnabled = false;
                        SelectedConnector = cmbConnectorList.SelectedValue.ToString() ?? null;
                        txtSelectedConnector.Text = SelectedConnector;
                        if (cmbDeviceList.Items.Count > 0)
                            SelectedDevice = cmbDeviceList.SelectedValue.ToString() ?? null;
                        txtSelectedDevice.Text = SelectedDevice;
                        PumpTestWindow.ConnectorTypeChanged();
                        break;
                    case "btnDeviceSave":
                        btnDeviceEdit.IsEnabled = true;
                        btnDeviceSave.IsEnabled = false;
                        cmbDeviceList.IsEnabled = false;
                        if (cmbDeviceList.Items.Count > 0)
                            SelectedDevice = cmbDeviceList.SelectedValue.ToString() ?? null;
                        txtSelectedDevice.Text = SelectedDevice;
                        PumpTestWindow.ConnectorTypeChanged();
                        break;
                }
            }
            this.DataContext = this;
        }

        private void cmbConnectorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                gridStatus.Visibility = Visibility.Hidden;
                if (cmbConnectorList.SelectedIndex > -1)
                {
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
                    if (deviceList != null && deviceList.Count > 0)
                    {
                        cmbDeviceList.SelectedIndex = 0;
                        if (cmbConnectorList.SelectedItem.ToString() == SelectedConnector)
                        {
                            if (cmbDeviceList.Items.Count > 0)
                                if (SelectedDevice != null && cmbDeviceList.Items.Contains(SelectedDevice))
                                    cmbDeviceList.SelectedItem = SelectedDevice;
                                else
                                    SelectedDevice = cmbDeviceList.SelectedItem.ToString();
                        }
                    }
                    else
                    {
                        SelectedDevice = null;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void cmbDeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void mainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded)
            {
                if (mainTabControl.SelectedIndex == 0)
                {
                    gridStatus.Visibility = Visibility.Hidden;
                }
                else if (mainTabControl.SelectedIndex == 1)
                {
                    SaveLoadedData();
                }
            }
        }


        private void SaveLoadedData()
        {
            UpdateConfigKey("ReportPrefix", string.Format("{0}", txtReportPrefix.Text));
            btnReportPrefixEdit.IsEnabled = true;
            btnReportPrefixSave.IsEnabled = false;
            txtReportPrefix.IsEnabled = false;

            UpdateConfigKey("ReportLocation", txtReportLocation.Text);
            btnReportLocationEdit.IsEnabled = true;
            btnReportLocationSave.IsEnabled = false;
            txtReportLocation.IsEnabled = false;

            UpdateConfigKey("DataReadInterval", string.Format("{0}:{1}:{2}:{3}", txtTimer.Text, txtTimerMin.Text, txtTimerSec.Text, txtTimerMilliSec.Text));
            btnDataReadIntervalEdit.IsEnabled = true;
            btnDataReadIntervalSave.IsEnabled = false;
            panelDataRead.IsEnabled = false;

            if (cmbConnectorList.Items.Count > 0)
                if (SelectedConnector != null && cmbConnectorList.Items.Contains(SelectedConnector))
                    cmbConnectorList.SelectedValue = SelectedConnector;
                else
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

        private void tabControlTest_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (tabControlTest.SelectedIndex == 0)
                {
                    //PumpExpanderCertificate.Visibility = Visibility.Hidden;
                    expanderCertificate.Visibility = Visibility.Visible;
                    txtMainHeading.Text = "Cylinder Testing";
                    if (StrokeTestWindow == null)
                        StrokeTestWindow = new StrokeTestWindow();
                    contentStrokeTest.Content = StrokeTestWindow;
                    if (strokeTestInfo == null) //&& holdMidPositionInfo==null && slipStickTestInformation==null)
                        strokeTestInfo = new StrokeTestInformation();
                    //else
                    //{
                    strokeTestInfo.IsTestStarted = false;
                    strokeTestInfo.CustomerName = txtCustName.Text;
                    strokeTestInfo.JobNumber = txtJobNumber.Text;
                    strokeTestInfo.CylinderNumber = txtCylinderNumber.Text;
                    strokeTestInfo.ReportNumber = string.Format("SPG_StrokeTest_{0}_R001", txtJobNumber.Text);
                    strokeTestInfo.BoreSize = uint.Parse(txtBoreSize.Text);
                    strokeTestInfo.RodSize = uint.Parse(txtRodSize.Text);
                    strokeTestInfo.StrokeLength = uint.Parse(txtStrokeLength.Text);
                    strokeTestInfo.IsTestStarted = true;
                    txtCommentBox.Text = HomePage.strokeTestInfo.Comment;
                    txtCommnetUpdateMessage.Text = HomePage.strokeTestInfo.CommentMessage;

                    //    }
                    strokeTestInfo.IsTestStarted = false;
                    this.gridMain.DataContext = null;
                    this.gridMain.DataContext = strokeTestInfo;
                    strokeTestInfo.IsTestStarted = true;
                   ElpisOPCServerMainWindow.pump_Test.txtStatusMessage.Text = strokeTestInfo.ReportStatus;
                    BtnTestDetails.Visibility = Visibility.Collapsed;
                    this.DataContext = this;
                }
                if (tabControlTest.SelectedIndex == 1)
                {
                    //PumpExpanderCertificate.Visibility = Visibility.Hidden;
                    expanderCertificate.Visibility = Visibility.Visible;
                    txtMainHeading.Text = "Cylinder Testing";
                    if (HoldMidPositionTestWindow == null)
                        HoldMidPositionTestWindow = new Hold_MidPositionTestUI();
                    contentHoldMidTestLineA.Content = HoldMidPositionTestWindow;

                    if (holdMidPositionLineAInfo == null)// && holdMidPositionInfo == null && slipStickTestInformation == null)
                        holdMidPositionLineAInfo = new Hold_MidPositionLineATestInformation();
                    //else
                    //{
                    holdMidPositionLineAInfo.IsTestStarted = false;
                    holdMidPositionLineAInfo.CustomerName = txtCustName.Text;
                    holdMidPositionLineAInfo.JobNumber = txtJobNumber.Text;
                    holdMidPositionLineAInfo.CylinderNumber = txtCylinderNumber.Text;
                    holdMidPositionLineAInfo.ReportNumber = string.Format("SPG_HoldMidPositionLineATest_{0}_R002", txtJobNumber.Text);
                    holdMidPositionLineAInfo.BoreSize = uint.Parse(txtBoreSize.Text);
                    holdMidPositionLineAInfo.RodSize = uint.Parse(txtRodSize.Text);
                    holdMidPositionLineAInfo.StrokeLength = uint.Parse(txtStrokeLength.Text);
                    txtCommentBox.Text = HomePage.holdMidPositionLineAInfo.Comment;
                    txtCommnetUpdateMessage.Text = HomePage.holdMidPositionLineAInfo.CommentMessage;

                    // }

                    if (holdMidPositionLineBinfo == null)// && holdMidPositionInfo == null && slipStickTestInformation == null)
                        holdMidPositionLineBinfo = new Hold_MidPositionLineBTestInformation();
                    //else
                    //{
                    holdMidPositionLineBinfo.IsTestStarted = false;
                    holdMidPositionLineBinfo.CustomerName = txtCustName.Text;
                    holdMidPositionLineBinfo.JobNumber = txtJobNumber.Text;
                    holdMidPositionLineBinfo.CylinderNumber = txtCylinderNumber.Text;
                    holdMidPositionLineBinfo.ReportNumber = string.Format("SPG_HoldMidPositionLineBTest_{0}_R002", txtJobNumber.Text);
                    holdMidPositionLineBinfo.BoreSize = uint.Parse(txtBoreSize.Text);
                    holdMidPositionLineBinfo.RodSize = uint.Parse(txtRodSize.Text);
                    holdMidPositionLineBinfo.StrokeLength = uint.Parse(txtStrokeLength.Text);
                    txtCommentBox.Text = HomePage.holdMidPositionLineBinfo.Comment;
                    txtCommnetUpdateMessage.Text = HomePage.holdMidPositionLineBinfo.CommentMessage;
                    this.gridMain.DataContext = null;
                    this.gridMain.DataContext = holdMidPositionLineAInfo;
                    holdMidPositionLineAInfo.IsTestStarted = true;

                    ElpisOPCServerMainWindow.pump_Test.txtStatusMessage.Text = holdMidPositionLineAInfo.ReportStatus;
                    BtnTestDetails.Visibility = Visibility.Collapsed;
                    this.DataContext = this;

                }

                //if (tabControlTest.SelectedIndex == 2)
                //{
                //    if (HoldMidPositionTestWindow == null)
                //        HoldMidPositionTestWindow = new Hold_MidPositionTestUI();
                //    contentHoldMidTestLineB.Content = HoldMidPositionTestWindow;

                //    if (holdMidPositionLineAInfo == null)// && holdMidPositionInfo == null && slipStickTestInformation == null)
                //        holdMidPositionLineAInfo = new Hold_MidPositionLineATestInformation();
                //    //else
                //    //{
                //        holdMidPositionLineAInfo.IsTestStarted = false;
                //        holdMidPositionLineAInfo.CustomerName = txtCustName.Text;
                //        holdMidPositionLineAInfo.JobNumber = txtJobNumber.Text;
                //        holdMidPositionLineAInfo.CylinderNumber = txtCylinderNumber.Text;
                //        holdMidPositionLineAInfo.ReportNumber = string.Format("SPG_HoldMidPositionLineATest_{0}_R002", txtJobNumber.Text);
                //        holdMidPositionLineAInfo.BoreSize = uint.Parse(txtBoreSize.Text);
                //        holdMidPositionLineAInfo.RodSize = uint.Parse(txtRodSize.Text);
                //        holdMidPositionLineAInfo.StrokeLength = uint.Parse(txtStrokeLength.Text);
                //  //  }

                //    holdMidPositionLineAInfo.IsTestStarted = false;
                //    this.gridMain.DataContext = null;
                //    this.gridMain.DataContext = holdMidPositionLineAInfo;
                //    holdMidPositionLineAInfo.IsTestStarted = true;
                //    this.DataContext = this;
                //}
                else if (tabControlTest.SelectedIndex == 2)
                {
                    //PumpExpanderCertificate.Visibility = Visibility.Hidden;
                    expanderCertificate.Visibility = Visibility.Visible;
                    txtMainHeading.Text = "Cylinder Testing";
                    if (SlipStickTestWindow == null)
                        SlipStickTestWindow = new SlipStickTestUI();
                    contentSlipStickTest.Content = SlipStickTestWindow;

                    if (slipStickTestInformation == null)// && holdMidPositionInfo == null && slipStickTestInformation == null)
                        slipStickTestInformation = new Slip_StickTestInformation();
                    //else
                    //{
                    //holdMidPositionLineAInfo.IsTestStarted = false;
                    slipStickTestInformation.CustomerName = txtCustName.Text;
                    slipStickTestInformation.JobNumber = txtJobNumber.Text;
                    slipStickTestInformation.CylinderNumber = txtCylinderNumber.Text;
                    slipStickTestInformation.ReportNumber = string.Format("SPG_SlipStickTest_{0}_R003", txtJobNumber.Text);
                    slipStickTestInformation.BoreSize = uint.Parse(txtBoreSize.Text);
                    slipStickTestInformation.RodSize = uint.Parse(txtRodSize.Text);
                    slipStickTestInformation.StrokeLength = uint.Parse(txtStrokeLength.Text);
                    // }
                    txtCommentBox.Text = HomePage.slipStickTestInformation.Comment;
                    txtCommnetUpdateMessage.Text = HomePage.slipStickTestInformation.CommentMessage;

                    slipStickTestInformation.IsTestStarted = false;
                    this.gridMain.DataContext = null;
                    this.gridMain.DataContext = slipStickTestInformation;
                    slipStickTestInformation.IsTestStarted = true;

                   //ElpisOPCServerMainWindow.pump_Test.txtStatusMessage.Text = slipStickTestInformation.ReportStatus;
                    BtnTestDetails.Visibility = Visibility.Collapsed;
                    this.DataContext = this;
                }
                else if (tabControlTest.SelectedIndex == 3)
                {
                    //PumpExpanderCertificate.Visibility = Visibility.Visible;
                    expanderCertificate.Visibility = Visibility.Hidden;
                    txtMainHeading.Text = "Pump Testing";
                    if (PumpTestWindow == null)
                        PumpTestWindow = new Pump_Test();
                    contentPumpTest.Content = PumpTestWindow;

                    if (PumpTestInformation == null)// && holdMidPositionInfo == null && slipStickTestInformation == null)
                        PumpTestInformation = new PumpTestInformation();
                    //else
                    //{
                    //holdMidPositionLineAInfo.IsTestStarted = false;
                    PumpTestInformation.EqipCustomerName = txtEqipCustomerName.Text;
                    PumpTestInformation.PumpJobNumber = txtPumpJobNumber.Text;
                    PumpTestInformation.PumpReportName = string.Format("PTR-{0}", txtPumpReportNumber.Text);

                    // }
                    txtCommentBox.Text = HomePage.PumpTestInformation.Comment;
                    txtCommnetUpdateMessage.Text = HomePage.PumpTestInformation.CommentMessage;
                    txtTestDetailsBox.Text = HomePage.PumpTestInformation.TestDeatilsInfo;
                    txtTestDetailsMessage.Text = HomePage.PumpTestInformation.TestDeatilsMessage;
                    PumpTestInformation.IsTestStarted = false;
                    this.PumpGridMain.DataContext = null;
                    this.PumpGridMain.DataContext = PumpTestInformation;
                    PumpTestInformation.IsTestStarted = true;
                   //ElpisOPCServerMainWindow.pump_Test.txtStatusMessage.Text = PumpTestInformation.ReportStatus;
                    BtnTestDetails.Visibility = Visibility.Visible;
                    txtTestDetailsBox.Visibility = Visibility.Visible;
                    this.DataContext = this;
                }
            }
            catch (Exception ex)
            {

            }
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

        }

        private void ReportSettingsData()
        {
            //if (mainTabControl != null && mainTabControl.SelectedIndex == 0)
            //{
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
                    //SelectedConnector = cmbConnectorList.SelectedValue.ToString();

                    //Added
                    if (cmbConnectorList.Items.Count > 0)
                        if (SelectedConnector != null && cmbConnectorList.Items.Contains(SelectedConnector))
                            cmbConnectorList.SelectedValue = SelectedConnector;
                        else
                            SelectedConnector = cmbConnectorList.SelectedValue.ToString();
                }
                else
                {
                    SelectedConnector = null;
                    SelectedDevice = null;
                    gridStatus.Visibility = Visibility.Visible;
                    AddErrorInfo("No Connectors found in Configuration, please configure Connectors.");
                }
                cmbConnectorList.IsEnabled = false;
                btnConnectorSave.IsEnabled = false;
                btnConnectorEdit.IsEnabled = true;
                if ((ConnectorCollection != null && ConnectorCollection.Count > 0) && ((ConnectorBase)ConnectorCollection[0]).DeviceCollection != null && ((ConnectorBase)ConnectorCollection[0]).DeviceCollection.Count > 0)
                {
                    // List<string> DeviceList = ((ConnectorBase)ConnectorCollection[0]).DeviceCollection.Select(d => d.DeviceName).ToList();
                    List<string> DeviceList = ((ConnectorBase)ConnectorCollection.FirstOrDefault(c => c.Name == SelectedConnector)).DeviceCollection.Select(d => d.DeviceName).ToList();
                    cmbDeviceList.ItemsSource = DeviceList;
                    cmbDeviceList.SelectedIndex = 0;
                    // SelectedDevice = cmbDeviceList.SelectedValue.ToString();

                    //Added
                    if (cmbDeviceList.Items.Count > 0)
                        if (SelectedDevice != null && cmbDeviceList.Items.Contains(SelectedDevice))
                            cmbDeviceList.SelectedValue = SelectedDevice;
                        else
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

                //string dataReadInterval = GetConfigValue("DataReadInterval");
                //if (dataReadInterval == null)
                //{
                //    // throw new ArgumentNullException("Key, < DataReadInterval > not found in the Configuration file.");
                //    AddErrorInfo("Key, < DataReadInterval > not found in the Configuration file, added with Default values.");
                //    dataReadInterval = "00:00:05:000";
                //    UpdateConfigKey("DataReadInterval", dataReadInterval);
                //}
                //else
                //{
                //    string[] timeIntervals = dataReadInterval.Split(':');
                //    if (timeIntervals.Count() == 4)
                //    {
                //        txtTimer.Text = timeIntervals[0];
                //        txtTimerMin.Text = timeIntervals[1];
                //        txtTimerSec.Text = timeIntervals[2];
                //        txtTimerMilliSec.Text = timeIntervals[3];
                //        gridStatus.Visibility = Visibility.Hidden;
                //    }
                //    else
                //    {
                //        // throw new ArgumentNullException("Key, < DataReadInterval > contains Invalid value in the Configuration file.");
                //        AddErrorInfo("Key, < DataReadInterval > contains Invalid value in the Configuration file.");
                //    }
                //}
                //btnDataReadIntervalSave.IsEnabled = false;
                //btnDataReadIntervalEdit.IsEnabled = true;
                //panelDataRead.IsEnabled = false;

                if (txtStatus.Text.Length == 0)
                    txtStatus.Text = "Configuration Updated Successfully";
                gridStatus.Visibility = Visibility.Visible;


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            // }
            this.DataContext = this;
        }

        public void AddErrorInfo(string error)
        {
            if (txtStatus.Text.Length == 0)
                txtStatus.Text = "Configuration have following errors:";
            txtErrorInfo.Text = string.Format("{0}{1}\n", txtErrorInfo.Text, error);
            txtErrorInfo.Foreground = Brushes.Red;
            gridStatus.Visibility = Visibility.Visible;
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
            }
            catch (ArgumentNullException ae)
            {

            }
            catch (Exception ex)
            {
                AddErrorInfo(ex.Message);
            }
        }

        public bool CheckCyliderInformation(TestType testType, object testObject)
        {

            if (TestType.StrokeTest == testType)
            {
                this.DataContext = testObject as StrokeTestInformation;
            }
            else if (testType == TestType.HoldMidPositionTest)
            {
                this.DataContext = testObject as Hold_MidPositionLineATestInformation;
            }
            else if (testType == TestType.HoldMidPositionLineBTest)
            {
                this.DataContext = testObject as Hold_MidPositionLineBTestInformation;
            }
            else if (testType == TestType.SlipStickTest)
            {
                this.DataContext = testObject as Slip_StickTestInformation;
            }
            return false;
        }


        internal void DisableInputs(bool isReadonly)
        {
            if (isReadonly)
            {
                txtCustName.IsReadOnly = true;
                txtJobNumber.IsReadOnly = true;
                txtRodSize.IsReadOnly = true;
                txtBoreSize.IsReadOnly = true;
                txtStrokeLength.IsReadOnly = true;
                txtCylinderNumber.IsReadOnly = true;

            }
            else
            {
                txtCustName.IsReadOnly = false;
                txtJobNumber.IsReadOnly = false;
                txtRodSize.IsReadOnly = false;
                txtBoreSize.IsReadOnly = false;
                txtStrokeLength.IsReadOnly = false;
                txtCylinderNumber.IsReadOnly = false;
            }

        }

        internal void PumpTestDisableInputs(bool isReadonly)
        {
            if (isReadonly)
            {
                txtPumpReportNumber.IsReadOnly = true;
                txtPumpJobNumber.IsReadOnly = true;
                txtEqipCustomerName.IsReadOnly = true;
                txtEquipManufacturer.IsReadOnly = true;
                txtEquipModelNo.IsReadOnly = true;
                txtEquipType.IsReadOnly = true;
                txtTestManufacturer.IsReadOnly = true;
                txtTestRange.IsReadOnly = true;
                txtTestSerialNo.IsReadOnly = true;
                txtTestType.IsReadOnly = true;
                txtTestedBy.IsReadOnly = true;
                txtWitnessedBy.IsReadOnly = true;
                txtApprovedBy.IsReadOnly = true;

            }
            else
            {
                txtPumpReportNumber.IsReadOnly = false;
                txtPumpJobNumber.IsReadOnly = false;
                txtEqipCustomerName.IsReadOnly = false;
                txtEquipManufacturer.IsReadOnly = false;
                txtEquipModelNo.IsReadOnly = false;
                txtEquipType.IsReadOnly = false;
                txtTestManufacturer.IsReadOnly = false;
                txtTestRange.IsReadOnly = false;
                txtTestSerialNo.IsReadOnly = false;
                txtTestType.IsReadOnly = false;
                txtTestedBy.IsReadOnly = false;
                txtWitnessedBy.IsReadOnly = false;
                txtApprovedBy.IsReadOnly = false;
            }

        }

        private void btnOpenLocation_click(object sender, RoutedEventArgs e)
        {
            try
            {
                var testReportLocation = ReportLocation;
                string path = string.Empty;
                if (tabControlTest.SelectedIndex == 0 && !string.IsNullOrEmpty(HomePage.strokeTestInfo.JobNumber) && !string.IsNullOrEmpty(HomePage.strokeTestInfo.JobNumber) && !string.IsNullOrEmpty(HomePage.strokeTestInfo.JobNumber))
                {
                    path = string.Format(@"{0}\{1}_{2}\{3}", ReportLocation, HomePage.strokeTestInfo.JobNumber, HomePage.strokeTestInfo.CustomerName, HomePage.strokeTestInfo.CylinderNumber);
                    if (Directory.Exists(path))
                        testReportLocation = path;
                }
                if (tabControlTest.SelectedIndex == 1 && !string.IsNullOrEmpty(HomePage.holdMidPositionLineAInfo.JobNumber) && !string.IsNullOrEmpty(HomePage.holdMidPositionLineAInfo.JobNumber) && !string.IsNullOrEmpty(HomePage.holdMidPositionLineAInfo.JobNumber))
                {
                    path = string.Format(@"{0}\{1}_{2}\{3}", ReportLocation, HomePage.holdMidPositionLineAInfo.JobNumber, HomePage.holdMidPositionLineAInfo.CustomerName, HomePage.holdMidPositionLineAInfo.CylinderNumber);
                    if (Directory.Exists(path))
                        testReportLocation = path;
                }
                if (tabControlTest.SelectedIndex == 2 && !string.IsNullOrEmpty(HomePage.slipStickTestInformation.JobNumber) && !string.IsNullOrEmpty(HomePage.slipStickTestInformation.CustomerName) && !string.IsNullOrEmpty(HomePage.slipStickTestInformation.CylinderNumber))
                {
                    path = string.Format(@"{0}\{1}_{2}\{3}", ReportLocation, HomePage.slipStickTestInformation.JobNumber, HomePage.slipStickTestInformation.CustomerName, HomePage.slipStickTestInformation.CylinderNumber);
                    if (Directory.Exists(path))
                        testReportLocation = path;
                }
                if (tabControlTest.SelectedIndex == 3 && !string.IsNullOrEmpty(HomePage.PumpTestInformation.PumpJobNumber) && !string.IsNullOrEmpty(HomePage.PumpTestInformation.PumpReportNumber))
                {
                    path = string.Format(@"{0}\{1}\{2}", ReportLocation, HomePage.PumpTestInformation.PumpJobNumber, HomePage.PumpTestInformation.PumpReportNumber);
                    if (Directory.Exists(path))
                        testReportLocation = path;
                }


                Process.Start(testReportLocation);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }



        private void btnUpdate_comment(object sender, RoutedEventArgs e)
        {

            if (tabControlTest.SelectedIndex == 0)
            {

                HomePage.strokeTestInfo.Comment = txtCommentBox.Text;
                HomePage.strokeTestInfo.CommentMessage = txtCommnetUpdateMessage.Text = "Stroke Test Comment Added";
            }
            else if (tabControlTest.SelectedIndex == 1)
            {
                if (HoldMidPositionTestWindow.rb_LineA.IsChecked == true)
                {
                    HomePage.holdMidPositionLineAInfo.Comment = txtCommentBox.Text;
                    HomePage.holdMidPositionLineAInfo.CommentMessage = txtCommnetUpdateMessage.Text = "Hold mid lineA Test Comment Added";
                }
                else if (HoldMidPositionTestWindow.rb_LineB.IsChecked == true)
                {
                    HomePage.holdMidPositionLineBinfo.Comment = txtCommentBox.Text;
                    HomePage.holdMidPositionLineBinfo.CommentMessage = txtCommnetUpdateMessage.Text = "Hold mid lineB Test Comment Added";
                }
            }
            else if (tabControlTest.SelectedIndex == 2)
            {
                HomePage.slipStickTestInformation.Comment = txtCommentBox.Text;
                HomePage.slipStickTestInformation.CommentMessage = txtCommnetUpdateMessage.Text = "Slip Stick Test Comment Added";
            }
            else if (tabControlTest.SelectedIndex == 3)
            {
                HomePage.PumpTestInformation.Comment = txtCommentBox.Text;
                HomePage.PumpTestInformation.CommentMessage = txtCommnetUpdateMessage.Text = "Pump Test Comment Added";
            }
            BtnCommentBox.IsChecked = false;

        }

        private void btnCollapse_comment(object sender, RoutedEventArgs e)
        {
            txtCommentBox.Text = "";
            BtnCommentBox.IsChecked = false;
            txtCommnetUpdateMessage.Text = "";
        }

        private void PumpExpanderCertificate_Expanded(object sender, RoutedEventArgs e)
        {

        }

        private void TxtPumpReportNumber_TextChanged(object sender, TextChangedEventArgs e)
        {

            // txtReportNumber.Text = string.Format("SPG_SlipStickTest_{0}_R003", txtJobNumber.Text);
            PumpTestInformation.PumpReportName = string.Format("PTR-{0}", txtPumpReportNumber.Text);

        }

        private void btnTestDeatils_Update(object sender, RoutedEventArgs e)
        {
            HomePage.PumpTestInformation.TestDeatilsInfo = txtTestDetailsBox.Text;
            HomePage.PumpTestInformation.TestDeatilsMessage = txtTestDetailsMessage.Text = "Pump Test Details Added";
            BtnTestDetails.IsChecked = false;
        }

        private void btnCollaps_TestDetails(object sender, RoutedEventArgs e)
        {
            txtTestDetailsBox.Text = "";
            BtnTestDetails.IsChecked = false;
            txtTestDetailsMessage.Text = "";
        }
    }
}

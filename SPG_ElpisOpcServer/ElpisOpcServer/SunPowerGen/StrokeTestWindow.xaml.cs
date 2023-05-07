using Elpis.Windows.OPC.Server;
using LibplctagWrapper;
using LiveCharts;
using LiveCharts.Wpf;
using Modbus;
using Modbus.Device;
using OPCEngine.Connectors.Allen_Bradley;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Linq;
using System.Windows.Media.Imaging;

namespace ElpisOpcServer.SunPowerGen
{
    /// <summary>
    /// Interaction logic for StrokeTestUI1.xaml
    /// </summary>
    public partial class StrokeTestWindow : UserControl
    {
        #region Properties
        private SeriesCollection FlowSeriesCollection { get; set; }
        private SeriesCollection PressureSeriesCollection { get; set; }
        private List<string> PressureLineALabels { get; set; }
        private List<string> PressureLineBLabels { get; set; }
        private List<string> FlowLabels { get; set; }
        private List<string> StrokeLengthLabels { get; set; }
        public static double NoofCyclesCompleted { get; set; }
        private Func<double, string> YFormatter { get; set; }
        LineSeries flowLineSeries { get; set; }
        LineSeries strokeLengthSeries { get; set; }
        LineSeries pressureLineASeries { get; set; }
        LineSeries pressureLineBSeries { get; set; }
        private DispatcherTimer dispatcherTimer { get; set; }
        private string PressureLineATagAddress { get; set; }
        private string PressureLineBTagAddress { get; set; }
        private string FlowTagAddress { get; set; }
        private string PressureLineAInputTagAddress { get; set; }
        private string PressureLineBInputTagAddress { get; set; }
        private string NumberofCyclesTagAddress { get; set; }
        private string CyclesCompletedTagAddress { get; set; }
        private StrokeTestInformation strokeTestInfo { get; set; }
        private byte slaveId { get; set; }
        ObservableCollection<Elpis.Windows.OPC.Server.Tag> TagsCollection { get; set; }
        private Dictionary<string, Tuple<LibplctagWrapper.Tag, Elpis.Windows.OPC.Server.DataType>> MappedTagList { get; set; }
        public string PlcTriggerTagAddress { get; set; }
        public string StrokeLengthTagAddress { get; set; }

        int startAddressPos = 0;
        int retryCount = 0;
        BitmapImage starticon = new BitmapImage();
        BitmapImage stopicon = new BitmapImage();
       
        #endregion

        #region Constructor & Loading
        public StrokeTestWindow()
        {
            InitializeComponent();
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            //UpdateDataReadInterval();
            if (HomePage.strokeTestInfo == null)
                HomePage.strokeTestInfo = new StrokeTestInformation();
            HomePage.strokeTestInfo.TestName = TestType.StrokeTest;
            strokeTestInfo = HomePage.strokeTestInfo;
            this.gridCeritificateInfo.DataContext = HomePage.strokeTestInfo;
            NoofCyclesCompleted = 0;
            //  txtJobNumber.Text = "";
            txtNoofCyclesCompleted.Text = NoofCyclesCompleted.ToString();
            chartFlow.AxisY[0].LabelFormatter = chartPressure.AxisY[0].LabelFormatter = value => value.ToString("N2");
            slaveId = 1;

            DataContext = this;
            starticon.BeginInit();
            starticon.UriSource = new Uri("/ElpisOpcServer;component/Images/starticon.png", UriKind.Relative);
            starticon.EndInit();
            stopicon.BeginInit();
            stopicon.UriSource = new Uri("/ElpisOpcServer;component/Images/stopicon.png", UriKind.Relative);
            stopicon.EndInit();
        }

        private void UpdateDataReadInterval()
        {
            try
            {
                Regex expr = new Regex("^[0-9]{1,2}:[0-9]{1,2}:[0-9]{1,2}:[0-9]{1,3}$");
                string cycleDuration = ConfigurationManager.AppSettings["StrokeTestDataReadInterval"].ToString();
                if (!string.IsNullOrEmpty(cycleDuration) && !expr.IsMatch(cycleDuration))
                {
                    MessageBox.Show("Configuration file have invalid or no value for Cycle Duration. It set to default value 00:00:02:000");
                    cycleDuration = "00:00:02:00";
                }
                string[] CycleDurationTime = cycleDuration.Split(':');
                dispatcherTimer.Interval = new TimeSpan(0, int.Parse(CycleDurationTime[0]), int.Parse(CycleDurationTime[1]), int.Parse(CycleDurationTime[2]), int.Parse(CycleDurationTime[3]));
                //txtTimer.Text = CycleDurationTime[0];
                //txtTimerMin.Text = CycleDurationTime[1];
                //txtTimerSec.Text = CycleDurationTime[2];
                //txtTimerMilliSec.Text = CycleDurationTime[3];
            }
            catch(Exception e)
            {

            }
        }

        private void StrokeTestUI_Loaded(object sender, RoutedEventArgs e)
        {
            string reportNumber = HomePage.strokeTestInfo.ReportNumber;
            this.gridCeritificateInfo.DataContext = null;
            HomePage.strokeTestInfo.ReportNumber = reportNumber;
            HomePage.strokeTestInfo.IsTestStarted = false;
            this.gridCeritificateInfo.DataContext = HomePage.strokeTestInfo;
            HomePage.strokeTestInfo.IsTestStarted = true;
            SaveLoadedData();
            UpdateDataReadInterval();
            DataContext = this;
        }

        private void SaveLoadedData()
        {
            UpdateConfigKey("StrokeTestDataReadInterval", string.Format("{0}:{1}:{2}:{3}", txtTimer.Text, txtTimerMin.Text, txtTimerSec.Text, txtTimerMilliSec.Text));
            //btnDataReadIntervalEdit.IsEnabled = true;
            //DataIntervalPanal.IsEnabled = true;
            //btnDataReadIntervalSave.IsEnabled = false;
        }
        #endregion

        #region UI Events
        /// <summary>
        /// Start/Stop button click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStartStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lblStartStop.Content.ToString() == "Start Record")
                {
                    bool isValid = ValidateInputs();
                    if (isValid)
                    {
                        if (FlowSeriesCollection == null && PressureSeriesCollection == null && isValid) //& (pressureLineASeries.Values.Count > 0 && pressureLineBSeries.Values.Count > 0 && flowLineSeries.Values.Count > 0))
                        {
                            string connectorName = HomePage.SelectedConnector;
                            string deviceName = HomePage.SelectedDevice;
                            if (!string.IsNullOrEmpty(connectorName) && !string.IsNullOrEmpty(deviceName))
                            {
                                this.IsHitTestVisible = false;
                                this.Cursor = Cursors.Wait;

                                lblStartStop.Content = "Stop Record";
                                imgStartStop.Source=stopicon;
                                btnStartStop.ToolTip = "Stop Record";
                                //HomePage.strokeTestInfo.TriggerStatusStroke = "Start";
                                tbxDeviceStatus.Text = "";
                                TagsCollection = Helper.GetTagsCollection(TestType.StrokeTest, connectorName, deviceName);
                                List<Tuple<string, bool>> tagStatus = new List<Tuple<string, bool>>();
                                if (TagsCollection != null && TagsCollection.Count > 0)
                                {
                                    foreach (var tag in TagsCollection)
                                    {
                                        if (tag.TagName.ToLower().Contains("pressurelinea"))
                                        {
                                            PressureLineATagAddress = (tag.Address);
                                            tagStatus.Add(new Tuple<string, bool>("PresssureLineA", true));
                                        }
                                        else if (tag.TagName.ToLower().Contains("pressurelineb"))
                                        {
                                            PressureLineBTagAddress = (tag.Address);
                                            tagStatus.Add(new Tuple<string, bool>("PresssureLineB", true));
                                        }
                                        else if (tag.TagName.ToLower().Contains("flow"))
                                        {
                                            FlowTagAddress = (tag.Address);
                                            tagStatus.Add(new Tuple<string, bool>("Flow", true));
                                        }
                                        else if (tag.TagName.ToLower().Contains("pressureallowablelinea"))
                                        {
                                            PressureLineAInputTagAddress = (tag.Address);
                                            tagStatus.Add(new Tuple<string, bool>("AllowablePressureLineA", true));
                                        }
                                        else if (tag.TagName.ToLower().Contains("pressureallowablelineb"))
                                        {
                                            PressureLineBInputTagAddress = (tag.Address);
                                            tagStatus.Add(new Tuple<string, bool>("AllowablePressurelineB", true));
                                        }
                                        else if (tag.TagName.ToLower().Contains("numberofcycles"))
                                        {
                                            NumberofCyclesTagAddress = (tag.Address);
                                            tagStatus.Add(new Tuple<string, bool>("NumberofCycles", true));
                                        }
                                        else if (tag.TagName.ToLower().Contains("cyclescompleted"))
                                        {
                                            CyclesCompletedTagAddress = (tag.Address);
                                            tagStatus.Add(new Tuple<string, bool>("CyclesCompleted", true));
                                        }
                                        else if (tag.TagName.ToLower().Contains("plctrigger"))
                                        {
                                            PlcTriggerTagAddress = (tag.Address);
                                            tagStatus.Add(new Tuple<string, bool>("PlcTrigger", true));
                                        }

                                        else if (tag.TagName.ToLower().Contains("strokelength"))
                                        {
                                            StrokeLengthTagAddress = (tag.Address);
                                            tagStatus.Add(new Tuple<string, bool>("StrokeLength", true));
                                        }

                                    }

                                    if (tagStatus.Count == 9)
                                    {
                                        ElpisServer.Addlogs("All", "SPG Reporting Tool - Stroke Test", string.Format("PressureLineA Address:{0} PressureLineB Address:{1} Flow Address:{2} Number of Cycles:{3} Cycles Completed: {4} AllowablePressure LineA:{5} AllowablePressure LineB:{6} PlcTrigger Address:{7} Stroke Length:{8}", PressureLineATagAddress, PressureLineBTagAddress, FlowTagAddress, NumberofCyclesTagAddress, CyclesCompletedTagAddress, PressureLineAInputTagAddress, PressureLineBInputTagAddress, PlcTriggerTagAddress,StrokeLengthTagAddress), LogStatus.Information);
                                        //Call for Connecting to Device/PLC
                                        ConnectDevice();
                                        this.IsHitTestVisible = true;
                                        txtNoofCyclesCompleted.Text = "0";
                                        NoofCyclesCompleted = 0;
                                        spDeviceStatus.Visibility = Visibility.Visible;
                                        this.IsHitTestVisible = true;

                                        if (tbxDeviceStatus.Text == "Connected")
                                        {
                                            ElpisOPCServerMainWindow.homePage.DisableInputs(true);
                                            // ElpisOPCServerMainWindow.homePage.btnReset.IsEnabled = false;
                                            ElpisOPCServerMainWindow.pump_Test.btnReset.IsEnabled = true;
                                            //ElpisOPCServerMainWindow.homePage.panelStatusMessage.Visibility = Visibility.Hidden;
                                            ElpisOPCServerMainWindow.pump_Test.panelStatusMessage.Visibility = Visibility.Hidden;
                                            ElpisOPCServerMainWindow.pump_Test.btnGenerateReport.IsEnabled = false;
                                            ElpisOPCServerMainWindow.homePage.ReportTab.IsEnabled = true;
                                            ElpisOPCServerMainWindow.homePage.txtFilePath.IsEnabled = false;
                                            DataIntervalPanal.IsEnabled = false;
                                            txtTimer.Background = Brushes.Transparent;
                                            txtTimerMin.Background = Brushes.Transparent;
                                            txtTimerSec.Background = Brushes.Transparent;
                                            txtTimerMilliSec.Background = Brushes.Transparent;

                                            flowLineSeries = new LineSeries
                                            {
                                                Title = "Flow",
                                                Values = new ChartValues<double>(),
                                                Stroke = Brushes.DarkOrange,
                                                PointGeometrySize = 5,
                                                ScalesYAt = 0

                                            };
                                            strokeLengthSeries = new LineSeries
                                            {
                                                Title = "StrokeLengthValue",
                                                Values = new ChartValues<double>(),
                                                Stroke = Brushes.DarkGreen,
                                                PointGeometrySize = 5,
                                                ScalesYAt = 1
                                            };
                                            pressureLineASeries = new LineSeries
                                            {
                                                Title = "PressureLineA",
                                                Values = new ChartValues<double>(),
                                                Stroke = Brushes.Red,
                                                PointGeometrySize = 5,
                                                ScalesYAt=0
                                            };

                                            pressureLineBSeries = new LineSeries
                                            {
                                                Title = "PressureLineB",
                                                Values = new ChartValues<double>(),
                                                Stroke = Brushes.Blue,
                                                PointGeometrySize = 5,
                                                ScalesYAt=1
                                               
                                            };

                                            PressureSeriesCollection = new SeriesCollection { pressureLineASeries, pressureLineBSeries};
                                            FlowSeriesCollection = new SeriesCollection { flowLineSeries,strokeLengthSeries };

                                            chartFlow.Series = FlowSeriesCollection;
                                            chartPressure.Series = PressureSeriesCollection;

                                            // PressureLabels = new List<string>();
                                            PressureLineALabels = new List<string>();
                                            PressureLineBLabels = new List<string>();
                                            FlowLabels = new List<string>();
                                            StrokeLengthLabels = new List<string>();
                                            YFormatter = value => (value + 100.00).ToString("N2");
                                            flowXAxis.Labels = FlowLabels;
                                            pressureXAxis.Labels = PressureLineBLabels;
                                            DataContext = this;

                                            SunPowerGenMainPage.isTestRunning = true;
                                            TriggerPLC(true);
                                            string pILA = ReadTag(SunPowerGenMainPage.DeviceObject.DeviceType, PressureLineAInputTagAddress, "pressureallowablelinea", Elpis.Windows.OPC.Server.DataType.Short);
                                            if (pILA == null)
                                            {
                                                StopTest();
                                                return;
                                            }
                                            else
                                            {
                                                txtLineAPressureInput.Text = pILA;
                                                txtLineBPressureInput.Text = ReadTag(SunPowerGenMainPage.DeviceObject.DeviceType, PressureLineBInputTagAddress, "pressureallowablelineb", Elpis.Windows.OPC.Server.DataType.Short);
                                                txtNoofCycles.Text = double.Parse(ReadTag(SunPowerGenMainPage.DeviceObject.DeviceType, NumberofCyclesTagAddress, "numberofcycles", Elpis.Windows.OPC.Server.DataType.Short)).ToString();
                                                NoofCyclesCompleted = double.Parse(ReadTag(SunPowerGenMainPage.DeviceObject.DeviceType, CyclesCompletedTagAddress, "cyclescompleted", Elpis.Windows.OPC.Server.DataType.Short));
                                                txtNoofCyclesCompleted.Text = NoofCyclesCompleted.ToString();
                                            }

                                            dispatcherTimer.Start();
                                            
                                            ReadDeviceData();
                                            

                                            tbxDateTine.Text = DateTime.Now.ToString();
                                            lblDateTime.Visibility = Visibility.Visible;
                                        }
                                        else
                                        {
                                            lblStartStop.Content = "Start Record";
                                            imgStartStop.Source = starticon;
                                            btnStartStop.ToolTip = "Start Record";
                                            
                                        }
                                    }
                                    else
                                    {
                                        StopTest();
                                        MessageBox.Show("Configuration file having the invalid Tag Names, please check it.\nConfigure Tag Names Like as follows:\n  Flow\n  PressureLineA\n  PressureLineB\n PressureAllowableLineA\n PressureAllowableLineB\n CyclesCompleted\n NumberofCycles", "SPG Reporting Tool", MessageBoxButton.OK, MessageBoxImage.Warning);

                                    }
                                }
                                else
                                {

                                    StopTest();
                                    //MessageBox.Show("Please create tags in configuration section.", "SPG Reporting Tool", MessageBoxButton.OK, MessageBoxImage.Warning);

                                }
                            }
                            else
                            {
                                StopTest();
                                MessageBox.Show("Please configure connector,device and tags in configuration section.", "SPG Reporting Tool", MessageBoxButton.OK, MessageBoxImage.Warning);

                            }
                        }
                        else
                        {
                            MessageBoxResult messageOption = MessageBox.Show("Please reset all fields by clicking reset button, and start new Data Recording.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information);
                            ElpisOPCServerMainWindow.homePage.expanderCertificate.IsExpanded = true;
                        }
                    }
                }
                else
                {
                    MessageBoxResult messageOption = MessageBox.Show("Do you want to stop the recording data?", "SPG Report Tool", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (messageOption == MessageBoxResult.Yes)
                    {
                        StopTest();
                    }
                }
                this.Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                // btnReset.IsEnabled = true;
                this.Cursor = Cursors.Arrow;
                ElpisServer.Addlogs("Report Tool", "Start Button- Stroke Test", ex.Message, LogStatus.Error);
                StopTest();
            }

        }
        private void TriggerPlc_OLD()
        {
            try
            {
                #region ABMicroLogicxEthernet
                if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ABMicroLogixEthernet)
                {
                    if (PlcTriggerTagAddress != null)
                    {
                        //ABMicrologixEthernetDevice abDevice = SunPowerGenMainPage.DeviceObject as ABMicrologixEthernetDevice;
                        //CpuType cpuType = Helper.GetABDeviceModelCPUType(abDevice.DeviceModelType);

                        foreach (var item in MappedTagList)
                        {
                            if (item.Key.ToLower().Contains("plctrigger"))
                            {
                                if (item.Value.Item1.Name.StartsWith("B3"))
                                {
                                    HomePage.strokeTestInfo.OffSetValue = Convert.ToInt16(item.Value.Item1.Name.Split('/')[1]);
                                }
                                var a= Helper.WriteEthernetIPDevice1(item.Value.Item1, HomePage.strokeTestInfo.OffSetValue,true);
                                ElpisServer.Addlogs("Report Tool", "plc stroketest values", string.Format("tag details:{0} tag datatype:{1}", item.Value.Item1.UniqueKey, item.Value.Item2), LogStatus.Information);

                            }
                        }
                        if (PlcTriggerTagAddress == HomePage.strokeTestInfo.TriggerTestAddress)
                        {
                            HomePage.strokeTestInfo.TriggerStatusStrokeTest = HomePage.strokeTestInfo.TriggerStatus;
                            ElpisServer.Addlogs("Report Tool/WriteTag", "return trigger status information", string.Format("Trigger status :{0}", HomePage.strokeTestInfo.TriggerStatusStrokeTest), LogStatus.Information);
                            if (HomePage.strokeTestInfo.TriggerStatusStrokeTest == "ON")
                            {
                                blbStateON.Visibility = Visibility.Visible;
                                blbStateOFF.Visibility = Visibility.Hidden;
                            }
                            else
                            {
                                blbStateON.Visibility = Visibility.Hidden;
                                blbStateOFF.Visibility = Visibility.Visible;
                            }
                        }
                    }
                }
                #endregion

                #region ModBus Ethernet
                else if (SunPowerGenMainPage.DeviceObject.DeviceType==DeviceType.ModbusEthernet)
                {

                }
                #endregion

                #region ModBus Serial
                else if (SunPowerGenMainPage.DeviceObject.DeviceType==DeviceType.ModbusSerial)
                {

                }
                #endregion

            }
            catch (Exception e)
            {
                ElpisServer.Addlogs("Report tool", "Plc trigger in stroke test", e.Message, LogStatus.Warning);

                //StopTest();
            }
        }
        private void TriggerPLC(bool value)
        {
            try
            {
                int writeStatus = -1;
                
                //string tagName = MappedTagList.Where(p => p.Key == "Plctrigger").Select(p => p.Value.Item1.Name).First().ToString();
                //Helper.TriggerPLC(tag,)
                #region ABMicroLogicxEthernet
                if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ABMicroLogixEthernet)
                {
                    int offSetValue=0;
                    LibplctagWrapper.Tag item = MappedTagList.Where(p => p.Key == "PlcTrigger").Select(p => p.Value.Item1).First();
                    if (PlcTriggerTagAddress != null)
                    {
                        if (PlcTriggerTagAddress.Split('/').Count() > 0)
                        {
                            offSetValue = Convert.ToInt16(PlcTriggerTagAddress.Split('/')[1]);
                            writeStatus = Helper.WriteEthernetIPDevice1(item, offSetValue, value);
                            ElpisServer.Addlogs("Report Tool", "PLC stroketest Trigger PLC", string.Format("tag details:{0} tag name:{1} triggerToStart:{2}", item.UniqueKey, item.Name,value), LogStatus.Information);
                        }
                        SetTestRunning(writeStatus==0 && value);

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
        }

        private void SetTestRunning(bool state)
        {
            if (state)
            {
                blbStateON.Visibility = Visibility.Visible;
                blbStateOFF.Visibility = Visibility.Hidden;
            }
            else
            {
                blbStateON.Visibility = Visibility.Hidden;
                blbStateOFF.Visibility = Visibility.Visible;
            }
        }
        private void StopTriggerPlc_Not_Used()
        {
            try
            {
                #region ABMicroLogicxEthernet
                if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ABMicroLogixEthernet)
                {
                    if (PlcTriggerTagAddress != null)
                    {
                        //ABMicrologixEthernetDevice abDevice = SunPowerGenMainPage.DeviceObject as ABMicrologixEthernetDevice;
                        //CpuType cpuType = Helper.GetABDeviceModelCPUType(abDevice.DeviceModelType);

                        foreach (var item in MappedTagList)
                        {
                            if (item.Key.ToLower().Contains("plctrigger"))
                            {
                                if (item.Value.Item1.Name.StartsWith("B3"))
                                {
                                    HomePage.strokeTestInfo.OffSetValue = Convert.ToInt16(item.Value.Item1.Name.Split('/')[1]);
                                }
                                Helper.WriteEthernetIPDeviceStop(item.Value.Item1, item.Value.Item2);
                                ElpisServer.Addlogs("Report Tool", "plc stroketest values", string.Format("tag details:{0} tag datatype:{1}", item.Value.Item1.UniqueKey, item.Value.Item2), LogStatus.Information);

                            }
                        }
                        if (PlcTriggerTagAddress == HomePage.strokeTestInfo.TriggerTestAddress)
                        {
                            HomePage.strokeTestInfo.TriggerStatusStrokeTest = HomePage.strokeTestInfo.TriggerStatus;
                            ElpisServer.Addlogs("Report Tool/WriteTag", "return trigger status information", string.Format("Trigger status :{0}", HomePage.strokeTestInfo.TriggerStatusStrokeTest), LogStatus.Information);
                            if (HomePage.strokeTestInfo.TriggerStatusStrokeTest == "OFF")
                            {
                                blbStateON.Visibility = Visibility.Hidden;
                                blbStateOFF.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                blbStateON.Visibility = Visibility.Visible;
                                blbStateOFF.Visibility = Visibility.Hidden;
                            }
                        }
                    }
                }
                #endregion

                #region ModBus Ethernet
                else if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ModbusEthernet)
                {

                }
                #endregion

                #region ModBus Serial
                else if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ModbusSerial)
                {

                }
                #endregion

            }
            catch (Exception e)
            {
                ElpisServer.Addlogs("Report tool", "Plc trigger in stroke test", e.Message, LogStatus.Warning);

                //StopTest();
            }
        }

        private string ReadTag(DeviceType deviceType, string tagAddress, string tagName, Elpis.Windows.OPC.Server.DataType dataType)
        {
            string result = string.Empty; ;
            if (deviceType == DeviceType.ModbusEthernet)
            {
                if (tagAddress.ToString().Length >= 5)
                {
                    if (PressureLineATagAddress.ToString()[0].ToString() == "3")
                        result = Helper.ReadDeviceInputRegisterValue(ushort.Parse(tagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusEthernet, startAddressPos, slaveId);
                    else if (PressureLineATagAddress.ToString()[0].ToString() == "4")
                        result = Helper.ReadDeviceHoldingRegisterValue(ushort.Parse(tagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusEthernet, startAddressPos, slaveId);
                }
                else
                {
                    result = SunPowerGenMainPage.ModbusTcpMaster.ReadHoldingRegisters(ushort.Parse(tagAddress), 1)[0].ToString();
                }
            }
            else if (deviceType == DeviceType.ModbusSerial)
            {
                if (tagAddress.ToString().Length >= 5)
                {
                    if (FlowTagAddress.ToString()[0].ToString() == "3")
                        result = Helper.ReadDeviceInputRegisterValue(ushort.Parse(tagAddress), dataType, DeviceType.ModbusSerial, 0, slaveId);
                    else if (FlowTagAddress.ToString()[0].ToString() == "4")
                        result = Helper.ReadDeviceHoldingRegisterValue(ushort.Parse(tagAddress), dataType, DeviceType.ModbusSerial, 0, slaveId);
                }
                else
                {
                    result = SunPowerGenMainPage.ModbusSerialPortMaster.ReadHoldingRegisters(slaveId, ushort.Parse(tagAddress), 1)[0].ToString();
                }
            }
            else if (deviceType == DeviceType.ABMicroLogixEthernet)
            {

                foreach (var item in MappedTagList)
                {
                    if (item.Key.ToLower().Contains(tagName.ToLower()))
                        result = Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                }
            }
            return result;
        }

        private bool ValidateInputs()
        {
            bool isValid = false;
            try
            {
                isValid = true;
                if (string.IsNullOrWhiteSpace(HomePage.strokeTestInfo.JobNumber) || string.IsNullOrEmpty(HomePage.strokeTestInfo.JobNumber))
                    isValid = isValid && false;
                else if (HomePage.strokeTestInfo.JobNumber.Length > 0)
                {
                    Regex expr = new Regex("^[0-9]{10}$");
                    if (!expr.IsMatch(HomePage.strokeTestInfo.JobNumber))
                        isValid = isValid && false;
                }

                if (string.IsNullOrWhiteSpace(HomePage.strokeTestInfo.CustomerName) || string.IsNullOrEmpty(HomePage.strokeTestInfo.CustomerName))
                    isValid = isValid && false;

                //if (strokeTestInfo.NoofCycles <= ushort.MinValue || strokeTestInfo.NoofCycles >= ushort.MaxValue)
                //    isValid = isValid && false;

                //if (string.IsNullOrEmpty(strokeTestInfo.LineAPressureInput) || float.Parse(strokeTestInfo.LineAPressureInput) <= float.MinValue || float.Parse(strokeTestInfo.LineAPressureInput) >= float.MaxValue)
                //    isValid = isValid && false;

                //if (string.IsNullOrEmpty(strokeTestInfo.LineBPressureInput) || float.Parse(strokeTestInfo.LineBPressureInput) <= float.MinValue || float.Parse(strokeTestInfo.LineBPressureInput) >= float.MaxValue)
                //    isValid = isValid && false;

                if (HomePage.strokeTestInfo.BoreSize <= uint.MinValue || HomePage.strokeTestInfo.BoreSize >= uint.MaxValue)
                    isValid = isValid && false;

                if (HomePage.strokeTestInfo.RodSize <= uint.MinValue || HomePage.strokeTestInfo.RodSize >= uint.MaxValue)
                    isValid = isValid && false;

                if (HomePage.strokeTestInfo.StrokeLength <= uint.MinValue || HomePage.strokeTestInfo.StrokeLength >= uint.MaxValue)
                    isValid = isValid && false;

                if (string.IsNullOrWhiteSpace(HomePage.strokeTestInfo.CylinderNumber) || string.IsNullOrEmpty(HomePage.strokeTestInfo.CylinderNumber))
                    isValid = isValid && false;

                //if (string.IsNullOrEmpty(txtCustName.Text))
                //{
                //    strokeTestInfo.CustomerName = txtCustName.Text;
                //    isValid = false;
                //}
                //else
                //{
                //    isValid = true;
                //}

            }
            catch (Exception ex)
            {
                isValid = false;
            }
            finally
            {
                string reportNumber = HomePage.strokeTestInfo.ReportNumber;
                this.gridCeritificateInfo.DataContext = null;
                HomePage.strokeTestInfo.ReportNumber = reportNumber;
                this.gridCeritificateInfo.DataContext = HomePage.strokeTestInfo;
                ElpisOPCServerMainWindow.homePage.gridMain.DataContext = null;
                ElpisOPCServerMainWindow.homePage.gridMain.DataContext = HomePage.strokeTestInfo;
            }
            if (isValid)
                ElpisOPCServerMainWindow.homePage.expanderCertificate.IsExpanded = false;
            else
                ElpisOPCServerMainWindow.homePage.expanderCertificate.IsExpanded = true;
            return isValid;
        }

        /// <summary>
        /// Dispatcher Tick Event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            ReadDeviceData();
        }

        private void ReadDeviceData()
        {
            try
            {
                if (SunPowerGenMainPage.ModbusTcpMaster != null || SunPowerGenMainPage.ModbusSerialPortMaster != null || SunPowerGenMainPage.ABEthernetClient != null && retryCount <= 3)
                {
                    if (NoofCyclesCompleted < double.Parse(txtNoofCycles.Text))
                    {
                        #region Modbus Ethernet
                        if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ModbusEthernet)
                        {
                            if (FlowTagAddress.ToString().Length >= 5 && PressureLineATagAddress.ToString().Length >= 5 && PressureLineBTagAddress.ToString().Length >= 5)
                            {
                                try
                                {
                                    if ((FlowTagAddress.ToString()[0]).ToString() == "3")
                                        txtFlow.Text = Helper.ReadDeviceInputRegisterValue(ushort.Parse(FlowTagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusEthernet, startAddressPos, slaveId);
                                    else if ((FlowTagAddress.ToString()[0]).ToString() == "4")
                                        txtFlow.Text = Helper.ReadDeviceHoldingRegisterValue(ushort.Parse(FlowTagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusEthernet, startAddressPos, slaveId);

                                    if ((StrokeLengthTagAddress.ToString()[0]).ToString() == "3")
                                      HomePage.strokeTestInfo.StrokeLengthValue = Helper.ReadDeviceInputRegisterValue(ushort.Parse(StrokeLengthTagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusEthernet, startAddressPos, slaveId);
                                    else if ((StrokeLengthTagAddress.ToString()[0]).ToString() == "4")
                                        HomePage.strokeTestInfo.StrokeLengthValue = Helper.ReadDeviceHoldingRegisterValue(ushort.Parse(StrokeLengthTagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusEthernet, startAddressPos, slaveId);


                                    if (PressureLineATagAddress.ToString()[0].ToString() == "3")
                                        txtPressureLineA.Text = Helper.ReadDeviceInputRegisterValue(ushort.Parse(PressureLineATagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusEthernet, startAddressPos, slaveId);
                                    else if (PressureLineATagAddress.ToString()[0].ToString() == "4")
                                        txtPressureLineA.Text = Helper.ReadDeviceHoldingRegisterValue(ushort.Parse(PressureLineATagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusEthernet, startAddressPos, slaveId);

                                    if (PressureLineBTagAddress.ToString()[0].ToString() == "3")
                                        txtPressureLineB.Text = Helper.ReadDeviceInputRegisterValue(ushort.Parse(PressureLineBTagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusEthernet, startAddressPos, slaveId);
                                    else if (PressureLineBTagAddress.ToString()[0].ToString() == "4")
                                        txtPressureLineB.Text = Helper.ReadDeviceHoldingRegisterValue(ushort.Parse(PressureLineBTagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusEthernet, startAddressPos, slaveId);
                                }
                                catch (SlaveException)
                                {
                                    startAddressPos = 1;
                                    retryCount++;
                                }
                                catch (Exception ex)
                                {
                                    ElpisServer.Addlogs("All", "SPG Reporting Tool", ex.Message, LogStatus.Information);
                                    StopTest();
                                }
                            }
                            else
                            {
                                txtFlow.Text = SunPowerGenMainPage.ModbusTcpMaster.ReadHoldingRegisters(ushort.Parse(FlowTagAddress), 1)[0].ToString();
                                HomePage.strokeTestInfo.StrokeLengthValue = SunPowerGenMainPage.ModbusTcpMaster.ReadHoldingRegisters(ushort.Parse(StrokeLengthTagAddress), 1)[0].ToString();
                                txtPressureLineA.Text = SunPowerGenMainPage.ModbusTcpMaster.ReadHoldingRegisters(ushort.Parse(PressureLineATagAddress), 1)[0].ToString();
                                txtPressureLineB.Text = SunPowerGenMainPage.ModbusTcpMaster.ReadHoldingRegisters(ushort.Parse(PressureLineBTagAddress), 1)[0].ToString();
                                NoofCyclesCompleted = int.Parse(SunPowerGenMainPage.ModbusTcpMaster.ReadHoldingRegisters(ushort.Parse(CyclesCompletedTagAddress), 1)[0].ToString());

                            }
                        }
                        #endregion Modbus Ethernet

                        #region Serial Device
                        else if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ModbusSerial)
                        {

                            if (FlowTagAddress.ToString().Length >= 5 && PressureLineATagAddress.ToString().Length >= 5 && PressureLineBTagAddress.ToString().Length >= 5)
                            {
                                if (FlowTagAddress.ToString()[0].ToString() == "1")
                                    txtFlow.Text = Helper.ReadDeviceCoilsRegisterValue(ushort.Parse(FlowTagAddress), DeviceType.ModbusSerial, slaveId);
                                else if (FlowTagAddress.ToString()[0].ToString() == "2")
                                    txtFlow.Text = Helper.ReadDeviceDiscreteInputRegisterValue(ushort.Parse(FlowTagAddress), DeviceType.ModbusSerial, slaveId);
                                else if (FlowTagAddress.ToString()[0].ToString() == "3")
                                    txtFlow.Text = Helper.ReadDeviceInputRegisterValue(ushort.Parse(FlowTagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusSerial, 0, slaveId);
                                else if (FlowTagAddress.ToString()[0].ToString() == "4")
                                    txtFlow.Text = Helper.ReadDeviceHoldingRegisterValue(ushort.Parse(FlowTagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusSerial, 0, slaveId);

                                if (StrokeLengthTagAddress.ToString()[0].ToString() == "1")
                                    HomePage.strokeTestInfo.StrokeLengthValue = Helper.ReadDeviceCoilsRegisterValue(ushort.Parse(StrokeLengthTagAddress), DeviceType.ModbusSerial, slaveId);
                                else if (StrokeLengthTagAddress.ToString()[0].ToString() == "2")
                                    HomePage.strokeTestInfo.StrokeLengthValue = Helper.ReadDeviceDiscreteInputRegisterValue(ushort.Parse(StrokeLengthTagAddress), DeviceType.ModbusSerial, slaveId);
                                else if (StrokeLengthTagAddress.ToString()[0].ToString() == "3")
                                    HomePage.strokeTestInfo.StrokeLengthValue = Helper.ReadDeviceInputRegisterValue(ushort.Parse(StrokeLengthTagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusSerial, 0, slaveId);
                                else if (StrokeLengthTagAddress.ToString()[0].ToString() == "4")
                                    HomePage.strokeTestInfo.StrokeLengthValue = Helper.ReadDeviceHoldingRegisterValue(ushort.Parse(StrokeLengthTagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusSerial, 0, slaveId);


                                if (PressureLineATagAddress.ToString()[0].ToString() == "1")
                                    txtPressureLineA.Text = Helper.ReadDeviceCoilsRegisterValue(ushort.Parse(PressureLineATagAddress), DeviceType.ModbusSerial, slaveId);
                                else if (PressureLineATagAddress.ToString()[0].ToString() == "2")
                                    txtPressureLineA.Text = Helper.ReadDeviceDiscreteInputRegisterValue(ushort.Parse(PressureLineATagAddress), DeviceType.ModbusSerial, slaveId);
                                else if (PressureLineATagAddress.ToString()[0].ToString() == "3")
                                    txtPressureLineA.Text = Helper.ReadDeviceInputRegisterValue(ushort.Parse(PressureLineATagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusSerial, 0, slaveId);
                                else if (PressureLineATagAddress.ToString()[0].ToString() == "4")
                                    txtPressureLineA.Text = Helper.ReadDeviceHoldingRegisterValue(ushort.Parse(PressureLineATagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusSerial, 0, slaveId);

                                if (PressureLineBTagAddress.ToString()[0].ToString() == "1")
                                    txtPressureLineB.Text = Helper.ReadDeviceCoilsRegisterValue(ushort.Parse(PressureLineBTagAddress), DeviceType.ModbusSerial, slaveId);
                                else if (PressureLineBTagAddress.ToString()[0].ToString() == "2")
                                    txtPressureLineB.Text = Helper.ReadDeviceDiscreteInputRegisterValue(ushort.Parse(PressureLineBTagAddress), DeviceType.ModbusSerial, slaveId);
                                else if (PressureLineBTagAddress.ToString()[0].ToString() == "3")
                                    txtPressureLineB.Text = Helper.ReadDeviceInputRegisterValue(ushort.Parse(PressureLineBTagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusSerial, 0, slaveId);
                                else if (PressureLineBTagAddress.ToString()[0].ToString() == "4")
                                    txtPressureLineB.Text = Helper.ReadDeviceHoldingRegisterValue(ushort.Parse(PressureLineBTagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusSerial, 0, slaveId);
                            }
                            else
                            {
                                txtFlow.Text = SunPowerGenMainPage.ModbusSerialPortMaster.ReadHoldingRegisters(slaveId, ushort.Parse(FlowTagAddress), 1)[0].ToString();
                                HomePage.strokeTestInfo.StrokeLengthValue = SunPowerGenMainPage.ModbusSerialPortMaster.ReadHoldingRegisters(slaveId, ushort.Parse(StrokeLengthTagAddress), 1)[0].ToString();
                                txtPressureLineA.Text = SunPowerGenMainPage.ModbusSerialPortMaster.ReadHoldingRegisters(slaveId, ushort.Parse(PressureLineATagAddress), 1)[0].ToString();
                                txtPressureLineB.Text = SunPowerGenMainPage.ModbusSerialPortMaster.ReadHoldingRegisters(slaveId, ushort.Parse(PressureLineBTagAddress), 1)[0].ToString();
                                NoofCyclesCompleted = int.Parse(SunPowerGenMainPage.ModbusSerialPortMaster.ReadHoldingRegisters(slaveId, ushort.Parse(CyclesCompletedTagAddress), 1)[0].ToString());
                            }
                            //txtFlow.Text = ModbusSerialPortMaster.ReadHoldingRegisters(slaveId, FlowTagAddress, 1)[0].ToString();
                            //txtPressure.Text = ModbusSerialPortMaster.ReadHoldingRegisters(slaveId, PressureTagAddress, 1)[0].ToString();
                        }
                        #endregion Serial Device

                        #region AB Micrologix Ethernet
                        else if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ABMicroLogixEthernet)
                        {
                            foreach (var item in MappedTagList)
                            {
                                if (!string.IsNullOrEmpty(HomePage.strokeTestInfo.Flow) && !string.IsNullOrEmpty(HomePage.strokeTestInfo.PressureLineA) && !string.IsNullOrEmpty(HomePage.strokeTestInfo.PressureLineA))
                                {
                                    if (item.Key.ToLower().Contains("flow"))
                                        HomePage.strokeTestInfo.Flow = Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                                    if (HomePage.strokeTestInfo.Flow == null)
                                    {
                                        SunPowerGenMainPage.ABEthernetClient.Dispose();
                                        tbxDeviceStatus.Text = "Not Connected";
                                        tbxDeviceStatus.Foreground = Brushes.Red;
                                        StopTest();
                                        MessageBox.Show("Please, check Stroke test device connection. One of the following are reason:\n1.Device is disconnected.\n2.Invalid Tag Address.\n3.Mismatch the Tag DataType.\n4.Processor Selection Mismatch.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                                        return;
                                    }
                                    if (item.Key.ToLower().Contains("strokelength"))
                                        HomePage.strokeTestInfo.StrokeLengthValue = Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                                    if (HomePage.strokeTestInfo.StrokeLengthValue == null)
                                    {
                                        SunPowerGenMainPage.ABEthernetClient.Dispose();
                                        tbxDeviceStatus.Text = "Not Connected";
                                        tbxDeviceStatus.Foreground = Brushes.Red;
                                        StopTest();
                                        MessageBox.Show("Please, check Stroke test device connection. One of the following are reason:\n1.Device is disconnected.\n2.Invalid Tag Address.\n3.Mismatch the Tag DataType.\n4.Processor Selection Mismatch.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                                        return;
                                    }
                                    else if (item.Key.ToLower().Contains("pressurelinea"))
                                    {
                                        HomePage.strokeTestInfo.PressureLineA = Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                                        if (HomePage.strokeTestInfo.PressureLineA == null)
                                        {
                                            SunPowerGenMainPage.ABEthernetClient.Dispose();
                                            tbxDeviceStatus.Text = "Not Connected";
                                            tbxDeviceStatus.Foreground = Brushes.Red;
                                            StopTest();
                                            MessageBox.Show("Please, check Stroke test device connection. One of the following are reason:\n1.Device is disconnected.\n2.Invalid Tag Address.\n3.Mismatch the Tag DataType.\n4.Processor Selection Mismatch.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                                            return;
                                        }
                                    }
                                    else if (item.Key.ToLower().Contains("pressurelineb"))
                                    {
                                        HomePage.strokeTestInfo.PressureLineB = Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                                        if (HomePage.strokeTestInfo.PressureLineB == null)
                                        {
                                            SunPowerGenMainPage.ABEthernetClient.Dispose();
                                            tbxDeviceStatus.Text = "Not Connected";
                                            tbxDeviceStatus.Foreground = Brushes.Red;
                                            StopTest();
                                            MessageBox.Show("Please, check Stroke test device connection. One of the following are reason:\n1.Device is disconnected.\n2.Invalid Tag Address.\n3.Mismatch the Tag DataType.\n4.Processor Selection Mismatch.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                                            return;
                                        }
                                    }
                                    else if (item.Key.ToLower().Contains("cyclescompleted"))
                                    {
                                        NoofCyclesCompleted = int.Parse(Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2));
                                       
                                    }
                                }
                                //switch (item.Key.ToLower())
                                //{
                                //    case  (item.Key.Contains("flow")?item.Key.ToLower():"flow") :
                                //        strokeTestInfo.Flow = Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2);

                                //        break;
                                //    case "pressurelinea":
                                //        strokeTestInfo.PressureLineA = Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                                //        break;
                                //    case "pressurelineb":
                                //        strokeTestInfo.PressureLineB = Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                                //        break;
                                //    default:
                                //        break;
                                //}
                                if (string.IsNullOrEmpty(txtFlow.Text) || string.IsNullOrEmpty(txtPressureLineA.Text) || string.IsNullOrEmpty(txtPressureLineB.Text))
                                {
                                    if (pressureLineASeries.Values.Count > 0 && flowLineSeries.Values.Count > 0)
                                    {
                                        HomePage.strokeTestInfo.PressureLineA = pressureLineASeries.Values[pressureLineASeries.Values.Count - 1].ToString();
                                        HomePage.strokeTestInfo.PressureLineB = pressureLineBSeries.Values[pressureLineBSeries.Values.Count - 1].ToString();
                                        HomePage.strokeTestInfo.Flow = flowLineSeries.Values[flowLineSeries.Values.Count - 1].ToString();
                                        HomePage.strokeTestInfo.StrokeLengthValue = strokeLengthSeries.Values[strokeLengthSeries.Values.Count - 1].ToString();
                                    }
                                    else
                                    {
                                        HomePage.strokeTestInfo.PressureLineA = "0";
                                        HomePage.strokeTestInfo.PressureLineB = "0";
                                        HomePage.strokeTestInfo.Flow = "0";
                                        HomePage.strokeTestInfo.StrokeLengthValue = "0";
                                    }
                                    //SunPowerGenMainPage.ABEthernetClient.Dispose();
                                    //tbxDeviceStatus.Text = "Not Connected";
                                    //tbxDeviceStatus.Foreground = Brushes.Red;
                                    //StopTest();
                                    //MessageBox.Show("Please, check Stroke test device connection. One of the following are reason:\n1.Device is disconnected.\n2.Invalid Tag Address.\n3.Mismatch the Tag DataType.\n4.Processor Selection Mismatch.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                                    //return;
                                }

                            }
                            //txtFlow.Text = Helper.ReadEthernetIPDevice(SunPowerGenMainPage.EIPTags.FirstOrDefault(t=>t.Name.ToLower()== FlowTagAddress), TagsCollection.FirstOrDefault(t=>t.TagName.ToLower()==FlowTagAddress).DataType);
                            // txtPressureLineA.Text = Helper.ReadEthernetIPDevice(SunPowerGenMainPage.EIPTags.FirstOrDefault(t => t.Name.ToLower() == PressureLineATagAddress), TagsCollection.FirstOrDefault(t => t.Address.ToLower() == PressureLineATagAddress).DataType);//ipAddress,PressureLineATagAddress, DataType.Integer
                            // txtPressureLineB.Text = Helper.ReadEthernetIPDevice(SunPowerGenMainPage.EIPTags.FirstOrDefault(t => t.Name.ToLower() == PressureLineBTagAddress), TagsCollection.FirstOrDefault(t => t.Address.ToLower() == PressureLineBTagAddress).DataType);
                            if (string.IsNullOrEmpty(txtFlow.Text) || string.IsNullOrEmpty(txtPressureLineA.Text) || string.IsNullOrEmpty(txtPressureLineB.Text))
                            {
                                //SunPowerGenMainPage.ABEthernetClient.Dispose();
                                StopTest();

                            }
                        }
                        #endregion AB Micrologix Ethernet

                        //if (txtLineAPressureInput.Text.TrimStart('0') == txtPressureLineA.Text || txtLineBPressureInput.Text.TrimStart('0') == txtPressureLineB.Text)
                        //{
                        //    NoofCyclesCompleted++;
                        //    txtNoofCyclesCompleted.Text = NoofCyclesCompleted.ToString();
                        //}

                        txtNoofCyclesCompleted.Text = NoofCyclesCompleted.ToString();
                        flowLineSeries.Values.Add(double.Parse(txtFlow.Text.ToString()));
                        FlowLabels.Add(DateTime.Now.ToString("h:mm:ss.ff"));
                        strokeLengthSeries.Values.Add(double.Parse(HomePage.strokeTestInfo.StrokeLengthValue));
                        StrokeLengthLabels.Add(DateTime.Now.ToString("h:mm:ss.ff"));
                        //pressureLineSeries.Values.Add(double.Parse(txtPressureLineA.Text.ToString()));
                        // PressureLabels.Add(DateTime.Now.ToString("h:mm:ss.ff"));
                        pressureLineASeries.Values.Add(double.Parse(txtPressureLineA.Text.ToString()));
                        PressureLineALabels.Add(DateTime.Now.ToString("h:mm:ss.ff"));
                        pressureLineBSeries.Values.Add(double.Parse(txtPressureLineB.Text.ToString()));
                        PressureLineBLabels.Add(DateTime.Now.ToString("h:mm:ss.ff"));
                        flowXAxis.Labels = FlowLabels;
                        pressureXAxis.Labels = PressureLineBLabels;
                        DataContext = this;
                    }

                    else
                    {
                        StopTest();
                        MessageBox.Show(string.Format("Cycles Completed:{0}.\nRecording data was stopped.", txtNoofCyclesCompleted.Text), "SPG Reporting Tool", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    StopTest();
                    ElpisServer.Addlogs("All", "SPG Reporting Tool", string.Format("Retry Count:{0}", retryCount), LogStatus.Information);
                    MessageBox.Show("Problem in connecting device, please check it.", "SPG Reporting Tool", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

            }
            catch (Exception exe)
            {
                ElpisServer.Addlogs("All", "SPG Reporting Tool", string.Format("Error in Read value.{0}.", exe.Message), LogStatus.Information);
                StopTest();
                //ConnectDevice();
            }
        }



        internal bool GenereateStrokeTestReport()
        {
            string ReportLocation = ConfigurationManager.AppSettings["ReportLocation"].ToString();
            if (string.IsNullOrEmpty(ReportLocation) || !(Directory.Exists(ReportLocation)))
            {

                ReportLocation = string.Format("{0}\\Reports", Directory.GetCurrentDirectory());
                if (!Directory.Exists(ReportLocation))
                    Directory.CreateDirectory(string.Format(@"{0}", ReportLocation));
            }
            string filePath = string.Empty;
            filePath = string.Format(@"{0}\{1}\{2}", ReportLocation, HomePage.strokeTestInfo.JobNumber, HomePage.strokeTestInfo.CylinderNumber);
            bool isNew = false;
            if (Directory.Exists(filePath))
            {
                string[] files = Directory.GetFiles(filePath);
                GenerateReportMessageBox messageBox = new GenerateReportMessageBox();
                if (files.Length >= 2)
                {
                    if (btnStartStop.IsEnabled && FlowSeriesCollection != null && PressureSeriesCollection != null)
                    {
                        messageBox.spNewOverride.Visibility = Visibility.Visible;
                        messageBox.panelMessage.Visibility = Visibility.Hidden;

                        messageBox.spImport.Visibility = Visibility.Hidden;
                        messageBox.panelNewOVerride.Visibility = Visibility.Visible;
                        messageBox.btnImport.Visibility = Visibility.Hidden;
                        messageBox.btnOK.Visibility = Visibility.Hidden;
                        
                    }
                    else
                    {
                        messageBox.spNewOverride.Visibility = Visibility.Hidden;
                        messageBox.btnNew.Visibility = Visibility.Hidden;
                        messageBox.btnOverride.Visibility = Visibility.Hidden;

                        messageBox.panelMessage.Visibility = Visibility.Visible;
                        messageBox.btnImport.Visibility = Visibility.Hidden;
                        messageBox.btnOK.Visibility = Visibility.Visible;

                    }
                }
                else if (FlowSeriesCollection == null && PressureSeriesCollection == null)
                {
                    messageBox.btnNew.Visibility = Visibility.Hidden;
                    messageBox.btnOverride.Visibility = Visibility.Hidden;
                    messageBox.spNewOverride.Visibility = Visibility.Hidden;

                    messageBox.btnImport.Visibility = Visibility.Hidden;
                    messageBox.btnOK.Visibility = Visibility.Visible;
                    messageBox.panelMessage.Visibility = Visibility.Visible;
                }

                messageBox.ShowDialog();
                if (string.IsNullOrEmpty(messageBox.SelectedOpeeration))
                    return false;
                else if (messageBox.SelectedOpeeration == "Import")
                {

                    // ImportAndUpdateUI();
                    return false;
                }
                else
                {
                    if (messageBox.SelectedOpeeration == "New")
                        isNew = true;
                }
            }

            if (btnStartStop.IsEnabled)
            {
                try
                {
                    if (pressureLineASeries != null || pressureLineBSeries != null || flowLineSeries != null)
                    {
                        //btnGenerateReport.IsEnabled = false;
                        ReportGeneration reportGenerator = new ReportGeneration();
                        List<LineSeries> lineSeriesCollection = new List<LineSeries>() { };
                        // reportGenerator.GenerateCSVFile(TestType.StrokeTest, strokeTestInfo, txtNoofCyclesCompleted.Text, flowLineSeries.Values, pressureLineSeries.Values, flowLineSeries.Title, pressureLineSeries.Title);
                        LineSeries series1 = new LineSeries() { Values = flowLineSeries.Values, Title = flowLineSeries.Title, Stroke = flowLineSeries.Stroke, Foreground = Brushes.Black };
                        LineSeries series2 = new LineSeries() { Values = strokeLengthSeries.Values, Title = strokeLengthSeries.Title, Stroke = strokeLengthSeries.Stroke, Foreground = Brushes.Black };
                        LineSeries series3 = new LineSeries() { Values = pressureLineASeries.Values, Title = pressureLineASeries.Title, Stroke = pressureLineASeries.Stroke, Foreground = Brushes.Black };
                        LineSeries series4 = new LineSeries() { Values = pressureLineBSeries.Values, Title = pressureLineBSeries.Title, Stroke = pressureLineBSeries.Stroke, Foreground = Brushes.Black };
                        ObservableCollection<SeriesCollection> seriesCollection = new ObservableCollection<SeriesCollection>() { new SeriesCollection() { series1 ,series2}, new SeriesCollection() { series3, series4 } };
                        ObservableCollection<LineSeries> lineSeriesList = new ObservableCollection<LineSeries>() { series1,series2, series3, series4 };
                        ObservableCollection<List<string>> labelCollection = new ObservableCollection<List<string>>() { FlowLabels,StrokeLengthLabels, PressureLineALabels, PressureLineBLabels };
                        // if (chkGenerateReport.IsChecked == false)

                        reportGenerator.GenerateCSVFile(TestType.StrokeTest, HomePage.strokeTestInfo, lineSeriesList, labelCollection, isNew, txtNoofCyclesCompleted.Text);
                        reportGenerator.GeneratePDFReport(TestType.StrokeTest, HomePage.strokeTestInfo, seriesCollection, labelCollection, isNew, NoofCyclesCompleted.ToString());
                        
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("No Recorded data found. Please, Start Recording data and then generate reports", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information);
                        return false;
                    }
                }
                catch (IOException ioe)
                {
                    MessageBox.Show(string.Format("{0}\nPlease close the file and generate the file.", ioe.Message), "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
                catch (Exception ex)
                {
                    ElpisServer.Addlogs("Stroke Test", "Report Data Generation", ex.Message, LogStatus.Error);
                }

            }
            else
            {
                return false;
            }

            return false;
        }


        internal void ResetStrokeTest()
        {
            HomePage.strokeTestInfo = null;
            HomePage.strokeTestInfo = new StrokeTestInformation();
            this.gridCeritificateInfo.DataContext = HomePage.strokeTestInfo;
            ElpisOPCServerMainWindow.homePage.gridMain.DataContext = HomePage.strokeTestInfo;
            HomePage.strokeTestInfo.IsTestStarted = true;
            txtNoofCyclesCompleted.Text = "0";
            tbxDeviceStatus.Text = "";
            blbStateOFF.Visibility = Visibility.Visible;
            blbStateON.Visibility = Visibility.Hidden;
            txtTimer.Text = "00";
            txtTimerMin.Text = "00";
            txtTimerSec.Text = "02";
            txtTimerMilliSec.Text = "000";
            if (lblStartStop.Content.ToString() == "Stop Record")
                StopTest();
            //chkGenerateReport.IsChecked = false;
            //btnGenerateReport.IsEnabled = true;
            //btnTimer.IsEnabled = true;
            //if (txtCustName.IsReadOnly == true)
            //    DisableInputs();
            //  DisableInputs();
            if (FlowSeriesCollection != null)
                FlowSeriesCollection.Clear();
            FlowSeriesCollection = null;
            if (PressureSeriesCollection != null)
                PressureSeriesCollection.Clear();
            PressureSeriesCollection = null;
            if (FlowLabels != null)
                FlowLabels.Clear();

            if (PressureLineBLabels != null)
                PressureLineBLabels.Clear();
            this.DataContext = this;
        }


        private void ImportAndUpdateUI(string filePath)
        {
            try
            {
                //string ReportLocation = ConfigurationManager.AppSettings["ReportLocation"].ToString();
                //if (string.IsNullOrEmpty(ReportLocation) || !(Directory.Exists(ReportLocation)))
                //    ReportLocation = string.Format("{0}\\Reports", Directory.GetCurrentDirectory());
                //string filePath = string.Empty;
                ////MessageBox.Show("Please select file.", "SPG Reporting Tool", MessageBoxButton.OK, MessageBoxImage.Error);
                //if (jobNumber != "" && jobNumber.Length == 16)
                //{
                //    string folderPath = string.Format(@"{0}\{1}\{2}", ReportLocation, jobNumber, cylinderNumber);
                //    if (Directory.Exists(folderPath))
                //    {
                //        filePath =  // Helper.BrowseFile(folderPath);
                //    }
                //    else
                //    {
                //        filePath = Helper.BrowseFile(ReportLocation);
                //    }
                //}
                //else
                //{

                //    filePath = Helper.BrowseFile(ReportLocation);
                //}
                if (!string.IsNullOrEmpty(filePath))
                    UpdateUIData(filePath);
            }
            catch (Exception ex)
            {
                ElpisServer.Addlogs("Report Tool", "Import Stroke Test Data.", ex.Message, LogStatus.Error);
            }
        }

        #endregion


        #region Methods


        /// <summary>
        /// Connect the device.
        /// </summary>
        private void ConnectDevice()
        {
            try
            {
                if (SunPowerGenMainPage.DeviceObject != null)
                {
                    DeviceType deviceType = SunPowerGenMainPage.DeviceObject.DeviceType;
                    bool isConnected = false;
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
                        //string data = SunPowerGenMainPage.ModbusSerialPortMaster.ReadHoldingRegisters(slaveId, ushort.Parse(FlowTagAddress), 1)[0].ToString();
                        isConnected = true;
                    }
                    else if (deviceType == DeviceType.ABMicroLogixEthernet)
                    {
                        SunPowerGenMainPage.DeviceTcpClient = Helper.CreateTcpClient(((ABMicrologixEthernetDevice)SunPowerGenMainPage.DeviceObject).IPAddress, ((ABMicrologixEthernetDevice)SunPowerGenMainPage.DeviceObject).Port);
                        if (SunPowerGenMainPage.DeviceTcpClient != null && SunPowerGenMainPage.DeviceTcpClient.Connected)
                        {
                            //ipAddress = SunPowerGenMainPage.DeviceTcpClient.Client.LocalEndPoint.ToString();
                            // CreateEIPTags(ipAddress);
                            //if list having the previous test data, clear it.
                            if (MappedTagList != null)
                                MappedTagList.Clear();
                            //If list is null it creates a new list.
                            else
                                MappedTagList = new Dictionary<string, Tuple<LibplctagWrapper.Tag, Elpis.Windows.OPC.Server.DataType>>();
                            CreateMappedTagList();
                            if (SunPowerGenMainPage.ABEthernetClient != null)
                                SunPowerGenMainPage.ABEthernetClient.Dispose();
                            SunPowerGenMainPage.ABEthernetClient = new LibplctagWrapper.Libplctag();
                            isConnected = true;
                        }
                    }
                    //Update Device Status
                    if (isConnected)
                    {
                        tbxDeviceStatus.Text = "Connected";
                        tbxDeviceStatus.Foreground = Brushes.DarkGreen;
                        startAddressPos = 0;
                        retryCount = 0;
                    }
                    else
                    {
                        tbxDeviceStatus.Text = "Not Connected";
                        tbxDeviceStatus.Foreground = Brushes.Red;
                    }
                }
            }

            catch (Exception)
            {
                //MessageBox.Show(ex.Message);
                tbxDeviceStatus.Text = "Not Connected";
                tbxDeviceStatus.Foreground = Brushes.Red;
            }
        }

        /// <summary>
        /// Create a Ethernet/IP Tags mapped list, list is available until test is complete.
        /// </summary>
        private void CreateMappedTagList()
        {
            try
            {
                ABMicrologixEthernetDevice abDevice = SunPowerGenMainPage.DeviceObject as ABMicrologixEthernetDevice;
                CpuType cpuType = Helper.GetABDeviceModelCPUType(abDevice.DeviceModelType);
                foreach (var tag in TagsCollection)
                {
                    string key = tag.TagName;
                    int elementSize = Helper.GetElementSize(tag.DataType);
                    LibplctagWrapper.Tag eipTag = new LibplctagWrapper.Tag(abDevice.IPAddress, (LibplctagWrapper.CpuType)cpuType, tag.Address, elementSize, 1,0);
                    Tuple<LibplctagWrapper.Tag, Elpis.Windows.OPC.Server.DataType> value = new Tuple<LibplctagWrapper.Tag, Elpis.Windows.OPC.Server.DataType>(eipTag, tag.DataType);
                    MappedTagList.Add(key, value);

                }
                //SunPowerGenMainPage.EIPTags = eipTagsCollection;
            }
            catch (Exception e)
            {
                ElpisServer.Addlogs("All", "SPG Reporting Tool", string.Format("Error in creating MappedTagList.{0}.", e.Message), LogStatus.Information);
            }
        }


        /// <summary>
        /// Stop the test.
        /// </summary>
        private void StopTest()
        {
            lblStartStop.Content = "Start Record";
          
            imgStartStop.Source = starticon;
            btnStartStop.ToolTip = "Start Record";
            dispatcherTimer.Stop();
            //StopTriggerPlc();
            DataIntervalPanal.IsEnabled = true;
            txtTimer.Background = Brushes.White;
            txtTimerMin.Background = Brushes.White;
            txtTimerSec.Background = Brushes.White;
            txtTimerMilliSec.Background = Brushes.White;
            SunPowerGenMainPage.isTestRunning = false;
            this.IsHitTestVisible = true;
            spDeviceStatus.Visibility = Visibility.Visible;
            //ElpisOPCServerMainWindow.homePage.btnReset.IsEnabled = true;
            ElpisOPCServerMainWindow.pump_Test.btnReset.IsEnabled = true;

            //ElpisOPCServerMainWindow.homePage.btnReset.IsEnabled = true;
            ElpisOPCServerMainWindow.pump_Test.btnGenerateReport.IsEnabled = true;
            ElpisOPCServerMainWindow.homePage.expanderCertificate.IsExpanded = true;

            TriggerPLC(false);
            GenereateCSVDataFile();
        }

        /// <summary>
        /// This Method Stores a test data in file.
        /// </summary>
        private void GenereateCSVDataFile()
        {
            try
            {
                if (pressureLineASeries != null || pressureLineBSeries != null || flowLineSeries != null)
                {
                    ReportGeneration reportGenerator = new ReportGeneration();
                    LineSeries series1 = new LineSeries() { Values = flowLineSeries.Values, Title = flowLineSeries.Title, Stroke = flowLineSeries.Stroke, Foreground = Brushes.Black };
                    LineSeries series2 = new LineSeries() { Values = strokeLengthSeries.Values, Title = strokeLengthSeries.Title, Stroke = strokeLengthSeries.Stroke, Foreground = Brushes.Black };
                    LineSeries series3 = new LineSeries() { Values = pressureLineASeries.Values, Title = pressureLineASeries.Title, Stroke = pressureLineASeries.Stroke, Foreground = Brushes.Black };
                    LineSeries series4 = new LineSeries() { Values = pressureLineBSeries.Values, Title = pressureLineBSeries.Title, Stroke = pressureLineBSeries.Stroke, Foreground = Brushes.Black };
                    ObservableCollection<SeriesCollection> seriesCollection = new ObservableCollection<SeriesCollection>() { new SeriesCollection() { series1,series2 }, new SeriesCollection() { series3, series4 } };
                    ObservableCollection<LineSeries> lineSeriesList = new ObservableCollection<LineSeries>() { series1,series2, series3, series4 };
                    ObservableCollection<List<string>> labelCollection = new ObservableCollection<List<string>>() { FlowLabels,StrokeLengthLabels, PressureLineALabels, PressureLineBLabels };
                    reportGenerator.GenerateCSVFile(TestType.StrokeTest, HomePage.strokeTestInfo, lineSeriesList, labelCollection, false, txtNoofCyclesCompleted.Text);

                }
                else
                {
                   // MessageBox.Show("No Recorded data found. Please, Start Recording data and then generate reports", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch(Exception ex)
            {

            }
        }

        /// <summary>
        /// Read the data from the CSV file and update in UI.
        /// </summary>
        /// <param name="fileName"></param>
        private void UpdateUIData(string fileName)
        {
            ObservableCollection<LineSeries> lineSeriesCollection = Helper.GetSeriesCollection(fileName, 3,TestType.StrokeTest);
            ObservableCollection<List<string>> labelCollection = Helper.GetLabelCollection(fileName, 3);
            //ObservableCollection<SeriesCollection> seriesCollection = null;
            StrokeTestInformation testInformation = Helper.GetTestInformation(fileName, TestType.StrokeTest) as StrokeTestInformation;
            if (lineSeriesCollection != null && lineSeriesCollection.Count == 3)
            {
                lineSeriesCollection[0].Title = "Flow";
                lineSeriesCollection[0].Stroke = Brushes.DarkOrange;
                lineSeriesCollection[0].PointGeometrySize = 5;
                lineSeriesCollection[0].StrokeThickness = 1;
                flowLineSeries = lineSeriesCollection[0];

                lineSeriesCollection[3].Title = "StrokeLength";
                lineSeriesCollection[3].Stroke = Brushes.DarkGreen;
                lineSeriesCollection[3].PointGeometrySize = 5;
                lineSeriesCollection[3].StrokeThickness = 1;
                strokeLengthSeries = lineSeriesCollection[3];
                FlowSeriesCollection = new SeriesCollection() { lineSeriesCollection[0] ,lineSeriesCollection[3]};

                lineSeriesCollection[1].Title = "PressureLineA";
                lineSeriesCollection[1].Stroke = Brushes.Blue;
                lineSeriesCollection[1].PointGeometrySize = 5;
                lineSeriesCollection[1].StrokeThickness = 1;

                lineSeriesCollection[2].Title = "PressureLineB";
                lineSeriesCollection[2].Stroke = Brushes.SandyBrown;
                lineSeriesCollection[2].PointGeometrySize = 5;
                lineSeriesCollection[2].StrokeThickness = 1;
                pressureLineASeries = lineSeriesCollection[1];
                pressureLineBSeries = lineSeriesCollection[2];
                PressureSeriesCollection = new SeriesCollection() { lineSeriesCollection[1], lineSeriesCollection[2] };


                // HomePage.strokeTestInfo = null;
                //  HomePage.strokeTestInfo = testInformation;
                // this.gridCeritificateInfo.DataContext = HomePage.strokeTestInfo;
                txtNoofCyclesCompleted.Text = testInformation.NoofCyclesCompleted.ToString(); //NoofCyclesCompleted.ToString();

                FlowLabels = new List<string>();
                foreach (var item in labelCollection[0])
                {
                    FlowLabels.Add(item);
                }
                PressureLineBLabels = new List<string>();
                foreach (var item in labelCollection[2])
                {
                    PressureLineBLabels.Add(item);
                }

                //PressureLineALabels = labelCollection[1];
                // flowXAxis.Labels = FlowLabels;
                // pressureXAxis.Labels = PressureLineBLabels;
                // chartFlow.Series = FlowSeriesCollection;
                //chartPressure.Series = PressureSeriesCollection;
                // DataContext = this;

                ReportGeneration.seriesCollection = new ObservableCollection<SeriesCollection>() { FlowSeriesCollection, PressureSeriesCollection };
                ReportGeneration.labelCollection = new ObservableCollection<List<string>>() { FlowLabels, PressureLineBLabels };
            }
        }

        private void chartFlow_DataHover(object sender, ChartPoint chartPoint)
        {
            //LiveCharts.Wpf.DefaultTooltip t = ((LiveCharts.Wpf.Charts.Base.Chart)chartPoint.ChartView).DataTooltip as DefaultTooltip;
            //t.ShowSeries = true;
            ////t.DataContext = "dds";
            //t.ShowTitle = true;
            //t.SelectionMode = TooltipSelectionMode.OnlySender;
        }

        #endregion


        private void btnSaveOperation_Click(object sender, RoutedEventArgs e)
        {
            if (txtTimer.Text == "00" && txtTimerMin.Text == "00" && txtTimerSec.Text == "00" && txtTimerMilliSec.Text == "000")
            {
                //AddErrorInfo("Key, < DataReadInterval > has invalid value, updated with default values(00:00:05:000).");
                txtTimer.Text = "00";
                txtTimerMin.Text = "00";
                txtTimerSec.Text = "02";
                txtTimerMilliSec.Text = "000";
            }
            //btnDataReadIntervalEdit.IsEnabled = true;
            //btnDataReadIntervalSave.IsEnabled = false;
            //btnDataReadIntervalSave.IsEnabled = false;
            //panelDataRead.IsEnabled = false;
            //btnDataReadIntervalEdit.IsEnabled = true;
            //DataIntervalPanal.IsEnabled = false;
            //btnDataReadIntervalSave.IsEnabled = false;

            UpdateConfigKey("StrokeTestDataReadInterval", string.Format("{0}:{1}:{2}:{3}", txtTimer.Text, txtTimerMin.Text, txtTimerSec.Text, txtTimerMilliSec.Text));
            UpdateDataReadInterval();
            //HomePage.slipStickTestInformation.TimeInterval = txtTimer.Text +":"+ txtTimerMin.Text + ":" + txtTimerSec.Text + ":" + txtTimerMilliSec.Text;




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
                //AddErrorInfo(ex.Message);
            }
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
                Regex expr = new Regex("^[0-9]{1,3}$");
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

        private void btnTimer_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

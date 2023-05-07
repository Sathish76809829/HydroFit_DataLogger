using Elpis.Windows.OPC.Server;
using LiveCharts;
using LibplctagWrapper;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using OPCEngine.Connectors.Allen_Bradley;
using Modbus.Device;
using Modbus;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Timers;

namespace ElpisOpcServer.SunPowerGen
{
    /// <summary>
    /// Interaction logic for Hold_MidPositionTestUI.xaml
    /// </summary>
    public partial class Hold_MidPositionTestUI : UserControl
    {
        private int retryCount;
        private int startAddressPos;
        private byte slaveId;

        private Hold_MidPositionLineATestInformation holdTestInformation { get; set; }

        private SeriesCollection PressureLineASeriesCollection { get; set; }
        private SeriesCollection PressureLineBSeriesCollection { get; set; }
        LineSeries PressureLineALineSeries { get; set; }
        LineSeries PressureLineBLineSeries { get; set; }
        LineSeries CylinderMovementLineALineSeries { get; set; }
        LineSeries CylinderMovementLineBLineSeries { get; set; }
        private DispatcherTimer timer { get; set; }
        
        private string HoldingTimeATagAddress { get; set; }
        private string HoldingTimeBTagAddress { get; set; }
        private string AllowablePressureDropTagAddress { get; set; }
        private string PressureLineATagAddress { get; set; }
        private string PressureLineBTagAddress { get; set; }
        private string CylinderMovementTagAddress { get; set; }
       
        private string InitialPressureLineA { get; set; }
        

        private List<string> PressureLabels { get; set; }
        private List<int> ElapsedTimeValue { get; set; }
        private List<string> PressureLineBLabels { get; set; }
        private Func<double, string> YFormatter { get; set; }
        private ObservableCollection<Elpis.Windows.OPC.Server.Tag> TagsCollection { get; set; }
        public Dictionary<string, Tuple<LibplctagWrapper.Tag, Elpis.Windows.OPC.Server.DataType>> MappedTagList { get; private set; }
        private bool isHoldingTimeOver { get; set; }
        Int64 Count;
        private string HoldingTimeFormat { get; set; }
        public string PlcTriggerLineATagAddress { get; private set; }
        public string PlcTriggerLineBTagAddress { get; private set; }
        public Int16 TimerValue { get; private set; }
        private static System.Timers.Timer aTimer;
        private static TimeSpan ticksDownHoldingTime;
        BitmapImage starticon = new BitmapImage();
        BitmapImage stopicon = new BitmapImage();
        public Hold_MidPositionTestUI()
        {
            
            InitializeComponent();
            
            if (HomePage.holdMidPositionLineAInfo == null)
                HomePage.holdMidPositionLineAInfo = new Hold_MidPositionLineATestInformation();
            HomePage.holdMidPositionLineAInfo.TestName = TestType.HoldMidPositionTest;

            if (HomePage.holdMidPositionLineBinfo == null)
                HomePage.holdMidPositionLineBinfo = new Hold_MidPositionLineBTestInformation();

            HomePage.holdMidPositionLineBinfo.TestName = TestType.HoldMidPositionLineBTest;

            gridMain.DataContext = HomePage.holdMidPositionLineAInfo;
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick; 
            //timer.Interval = new TimeSpan(0,0, 0, 1,0);
            HoldingTimeFormat = ConfigurationManager.AppSettings["HoldingTimeFormat"].ToString();

            //chartPressure.AxisY[0].LabelFormatter = value => value.ToString("N2");

            //chartPressureLineB.AxisY[0].LabelFormatter = value => value.ToString("N2");
            starticon.BeginInit();
            starticon.UriSource = new Uri("/ElpisOpcServer;component/Images/starticon.png", UriKind.Relative);
            starticon.EndInit();
            stopicon.BeginInit();
            stopicon.UriSource = new Uri("/ElpisOpcServer;component/Images/stopicon.png", UriKind.Relative);
            stopicon.EndInit();

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            ticksDownHoldingTime = ticksDownHoldingTime.Subtract(timer.Interval);
            ElpisServer.Addlogs("All", "JJJ Hold mid position test - ticksDownHoldingTime", string.Format("ticksDownHoldingTime:{0} ", ticksDownHoldingTime), LogStatus.Information);
            if (ticksDownHoldingTime.TotalSeconds == 0)
            {
            ElpisServer.Addlogs("All", "JJJ Hold mid position test- iiiiiiiif (ticksDownHoldingTime.Seconds == 0) - ticksDownHoldingTime", string.Format("ticksDownHoldingTime:{0} ", ticksDownHoldingTime), LogStatus.Information);

                StopTest();
                return;
            }

                if (rb_LineA.IsChecked == true)
                {
                    ReadDeviceData(PressureLineALineSeries, CylinderMovementLineALineSeries, HomePage.holdMidPositionLineAInfo/*, PressureLineBLineSeries*/);
                }
                else if (rb_LineB.IsChecked == true)
                {
                    ReadDeviceData(PressureLineBLineSeries, CylinderMovementLineBLineSeries, HomePage.holdMidPositionLineBinfo/*, PressureLineALineSeries*/);
                }
        }
        private void Stop_Time(object sender , EventArgs e)
        {

        }
        private void ReadDeviceData(LineSeries pressureLineSeriesA, LineSeries cylinderLineSeries, dynamic testInformation/*,LineSeries pressureLineSeriesB*/)
        {
            string runningHoldingTime = "";
            try
            {
                if (pressureLineSeriesA != null && cylinderLineSeries != null /*&& pressureLineSeriesB!=null*/)
                {
                    if (SunPowerGenMainPage.ModbusTcpMaster != null || SunPowerGenMainPage.ModbusSerialPortMaster != null || SunPowerGenMainPage.ABEthernetClient != null && retryCount <= 3)
                    {
                        if (!isHoldingTimeOver)
                        {
                            ElpisServer.Addlogs("All", "JJJ Hold mid position test - ticksDownHoldingTime state", string.Format("ticksDownHoldingTime State:{0} ", isHoldingTimeOver), LogStatus.Information);

                            #region Modbus Ethernet
                            if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ModbusEthernet)
                            {
                                testInformation.HoldingPressureLineA = txtHoldingPressureA.Text = txtHoldingPressureLineA.Text = SunPowerGenMainPage.ModbusTcpMaster.ReadHoldingRegisters(ushort.Parse(PressureLineATagAddress), 1)[0].ToString();  //txtHoldingPressureA.Text= txtHoldingPressureLineA.Text =
                                testInformation.HoldingPressureLineB = txtHoldingPressureB.Text = txtHoldingPressureLineB.Text = SunPowerGenMainPage.ModbusTcpMaster.ReadHoldingRegisters(ushort.Parse(PressureLineBTagAddress), 1)[0].ToString();//= txtHoldingPressureB.Text= txtHoldingPressureB.Text
                                testInformation.CylinderMovement = SunPowerGenMainPage.ModbusTcpMaster.ReadHoldingRegisters(ushort.Parse(CylinderMovementTagAddress), 1)[0].ToString();

                                    if (rb_LineA.IsChecked == true)
                                {
                                    runningHoldingTime= SunPowerGenMainPage.ModbusTcpMaster.ReadHoldingRegisters(ushort.Parse(HoldingTimeATagAddress), 1)[0].ToString();
                                    HomePage.holdMidPositionLineAInfo.RunningHoldingTimeLineA = runningHoldingTime;
                                    if (int.Parse(runningHoldingTime) == 0) isHoldingTimeOver = true;
                                }
                                else if(rb_LineB.IsChecked==true)
                                {
                                    runningHoldingTime = SunPowerGenMainPage.ModbusTcpMaster.ReadHoldingRegisters(ushort.Parse(HoldingTimeBTagAddress), 1)[0].ToString();
                                    HomePage.holdMidPositionLineBinfo.RunningHoldingTimeLineB = runningHoldingTime; //SunPowerGenMainPage.ModbusTcpMaster.ReadHoldingRegisters(ushort.Parse(HoldingTimeBTagAddress), 1)[0].ToString();
                                    if (int.Parse(runningHoldingTime) == 0) isHoldingTimeOver = true;

                                }

                            }
                            #endregion Modbus Ethernet

                            #region Serial Device
                            else if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ModbusSerial)
                            {
                                testInformation.HoldingPressureLineB = txtHoldingPressureB.Text = txtHoldingPressureLineB.Text = SunPowerGenMainPage.ModbusSerialPortMaster.ReadHoldingRegisters(slaveId, ushort.Parse(PressureLineBTagAddress), 1)[0].ToString();
                                testInformation.HoldingPressureLineA = txtHoldingPressureA.Text = txtHoldingPressureLineA.Text = SunPowerGenMainPage.ModbusSerialPortMaster.ReadHoldingRegisters(slaveId, ushort.Parse(PressureLineATagAddress), 1)[0].ToString();
                                testInformation.CylinderMovement = txtCylinderMovement.Text = SunPowerGenMainPage.ModbusSerialPortMaster.ReadHoldingRegisters(slaveId, ushort.Parse(CylinderMovementTagAddress), 1)[0].ToString();
                                if (rb_LineA.IsChecked == true)
                                {
                                    HomePage.holdMidPositionLineAInfo.RunningHoldingTimeLineA = SunPowerGenMainPage.ModbusSerialPortMaster.ReadHoldingRegisters(slaveId, ushort.Parse(HoldingTimeATagAddress), 1)[0].ToString();
                                }
                                else if (rb_LineB.IsChecked == true)
                                {
                                    HomePage.holdMidPositionLineBinfo.RunningHoldingTimeLineB = SunPowerGenMainPage.ModbusSerialPortMaster.ReadHoldingRegisters(slaveId, ushort.Parse(HoldingTimeBTagAddress), 1)[0].ToString();

                                }
                            }
                            #endregion Serial Device

                            #region AB Micrologix Ethernet
                            else if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ABMicroLogixEthernet)
                            {
                                foreach (var item in Helper.MappedTagList)
                                {
                                    if (!string.IsNullOrEmpty(HomePage.holdMidPositionLineAInfo.AllowablePressureDrop) && !string.IsNullOrEmpty(HomePage.holdMidPositionLineAInfo.HoldingPressureLineA) && !string.IsNullOrEmpty(HomePage.holdMidPositionLineAInfo.CylinderMovement))
                                    {
                                        //if (item.Key.ToLower().Contains("allowablepressuredrop"))
                                        //    HomePage.holdMidPositionLineAInfo.AllowablePressureDrop = Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                                        //else 
                                        if (item.Key.ToLower().Contains("pressurelinea"))
                                        {
                                            testInformation.HoldingPressureLineA = txtHoldingPressureA.Text = txtHoldingPressureLineA.Text = Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                                            if (testInformation.HoldingPressureLineA == null)
                                            {
                                                SunPowerGenMainPage.ABEthernetClient.Dispose();
                                                tbxDeviceStatus.Text = "Not Connected";
                                                tbxDeviceStatus.Foreground = Brushes.Red;
                                                StopTest();

                                                MessageBox.Show("Please, check hold mid position test device connection. One of the following are reason:\n1.Device is disconnected.\n2.Invalid Tag Address.\n3.Mismatch the Tag DataType.\n4.Processor Selection Mismatch.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                                                return;

                                            }

                                        }
                                        else if (item.Key.ToLower().Contains("pressurelineb"))
                                        {
                                            testInformation.HoldingPressureLineB = txtHoldingPressureB.Text = txtHoldingPressureLineB.Text = Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                                            if (testInformation.HoldingPressureLineB == null)
                                            {
                                                SunPowerGenMainPage.ABEthernetClient.Dispose();
                                                tbxDeviceStatus.Text = "Not Connected";
                                                tbxDeviceStatus.Foreground = Brushes.Red;
                                                StopTest();

                                                MessageBox.Show("Please, check hold mid position test device connection. One of the following are reason:\n1.Device is disconnected.\n2.Invalid Tag Address.\n3.Mismatch the Tag DataType.\n4.Processor Selection Mismatch.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                                                return;

                                            }
                                        }
                                        else if (item.Key.ToLower().Contains("cylindermovement"))
                                        {
                                            testInformation.CylinderMovement = txtCylinderMovement.Text = Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                                            if (testInformation.CylinderMovement == null)
                                            {
                                                SunPowerGenMainPage.ABEthernetClient.Dispose();
                                                tbxDeviceStatus.Text = "Not Connected";
                                                tbxDeviceStatus.Foreground = Brushes.Red;
                                                StopTest();

                                                MessageBox.Show("Please, check hold mid position test device connection. One of the following are reason:\n1.Device is disconnected.\n2.Invalid Tag Address.\n3.Mismatch the Tag DataType.\n4.Processor Selection Mismatch.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                                                return;

                                            }
                                        }
                                        if (rb_LineA.IsChecked == true)
                                        { 
                                          if (item.Key.ToLower().Contains("holdingtimelinea"))
                                            {
                                                runningHoldingTime =Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                                                HomePage.holdMidPositionLineAInfo.RunningHoldingTimeLineA =runningHoldingTime;// Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                                            }
                                        }

                                        if (rb_LineB.IsChecked == true)
                                        {
                                            if (item.Key.ToLower().Contains("holdingtimelineb"))
                                            {
                                                runningHoldingTime = Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                                                HomePage.holdMidPositionLineBinfo.RunningHoldingTimeLineB = runningHoldingTime;//Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                                            }
                                        }
                                        

                                    }

                                    if (string.IsNullOrEmpty(txtAllowablePressureA.Text) || string.IsNullOrEmpty(txtHoldingPressureA.Text) || string.IsNullOrEmpty(txtCylinderMovement.Text))
                                    {
                                        if (rb_LineA.IsChecked == true)
                                        {
                                            if (PressureLineALineSeries.Values.Count > 0 && CylinderMovementLineALineSeries.Values.Count > 0)
                                            {
                                                HomePage.holdMidPositionLineAInfo.HoldingPressureLineA = PressureLineALineSeries.Values[PressureLineALineSeries.Values.Count - 1].ToString();
                                                HomePage.holdMidPositionLineAInfo.CylinderMovement = CylinderMovementLineALineSeries.Values[CylinderMovementLineALineSeries.Values.Count - 1].ToString();
                                            }
                                            else
                                            {
                                                HomePage.holdMidPositionLineAInfo.AllowablePressureDrop = "0";
                                                HomePage.holdMidPositionLineAInfo.HoldingPressureLineA = "0";
                                                HomePage.holdMidPositionLineAInfo.CylinderMovement = "0";
                                            }
                                            //SunPowerGenMainPage.ABEthernetClient.Dispose();
                                            //tbxDeviceStatus.Text = "Not Connected";
                                            //tbxDeviceStatus.Foreground = Brushes.Red;
                                            //StopTest();
                                            
                                            //MessageBox.Show("Please, check hold mid position test device connection. One of the following are reason:\n1.Device is disconnected.\n2.Invalid Tag Address.\n3.Mismatch the Tag DataType.\n4.Processor Selection Mismatch.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                                            
                                            //return;
                                        }
                                        else if (rb_LineB.IsChecked == true)
                                        {
                                            if (PressureLineBLineSeries.Values.Count > 0 && CylinderMovementLineBLineSeries.Values.Count > 0)
                                            {
                                                HomePage.holdMidPositionLineBinfo.HoldingPressureLineA = PressureLineBLineSeries.Values[PressureLineBLineSeries.Values.Count - 1].ToString();
                                                HomePage.holdMidPositionLineBinfo.CylinderMovement = CylinderMovementLineBLineSeries.Values[CylinderMovementLineBLineSeries.Values.Count - 1].ToString();
                                            }
                                            else
                                            {
                                                HomePage.holdMidPositionLineBinfo.AllowablePressureDrop = "0";
                                                HomePage.holdMidPositionLineBinfo.HoldingPressureLineA = "0";
                                                HomePage.holdMidPositionLineBinfo.CylinderMovement = "0";
                                            }
                                            //SunPowerGenMainPage.ABEthernetClient.Dispose();
                                            //StopTest();
                                            //tbxDeviceStatus.Text = "Not Connected";
                                            //tbxDeviceStatus.Foreground = Brushes.Red;
                                            //MessageBox.Show("Please, check hold mid position test device connection. One of the following are reason:\n1.Device is disconnected.\n2.Invalid Tag Address.\n3.Mismatch the Tag DataType.\n4.Processor Selection Mismatch.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);

                                            //return;
                                        }

                                    }

                                }

                                ElpisServer.Addlogs("All", "JJJ Hold mid position test - running", string.Format("Holding TimeA Running:{0} ", runningHoldingTime), LogStatus.Information);

                                if (decimal.Parse(runningHoldingTime)==0 ||  string.IsNullOrEmpty(txtHoldingPressureA.Text) || string.IsNullOrEmpty(txtAllowablePressureA.Text) || string.IsNullOrEmpty(txtCylinderMovement.Text))
                                {
                                    
                                    StopTest();
                                }
                            }
                            #endregion AB Micrologix Ethernet

                            if (rb_LineA.IsChecked == true)
                            {
                                pressureLineSeriesA.Values.Add(double.Parse(txtHoldingPressureA.Text));
                                //PressureLineBLineSeries.Values.Add(double.Parse(txtHoldingPressureB.Text));
                                ////25_sep_2018
                                //pressureLineSeriesB.Values.Add(double.Parse(txtHoldingPressureB.Text));
                                ////25_sep_2018
                                PressureLabels.Add(DateTime.Now.ToString("h:mm:ss.ff"));
                                pressureXAxis.Labels = PressureLabels;
                                



                            }
                            else if (rb_LineB.IsChecked == true)
                            {
                                PressureLineBLineSeries.Values.Add(double.Parse(txtHoldingPressureB.Text));
                                ////25_sep_2018
                                //pressureLineSeriesA.Values.Add(double.Parse(txtHoldingPressureA.Text));
                                ////25_sep_2018
                                PressureLineBLabels.Add(DateTime.Now.ToString("h:mm:ss.ff"));
                                pressureLineBXAxis.Labels = PressureLineBLabels;
                            }

                            cylinderLineSeries.Values.Add(double.Parse(txtCylinderMovement.Text));           
                            this.DataContext = this;
                        }

                        else
                        {
                            StopTest();
                            MessageBox.Show(string.Format("Holding Time for Pressure LineA Completed:{0}.\nRecording data was stopped.", txtHoldingTimeA.Text), "SPG Reporting Tool", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        StopTest();
                        ElpisServer.Addlogs("All", "SPG Reporting Tool-Hold Mid Position Test", string.Format("Retry Count:{0}", retryCount), LogStatus.Information);
                        MessageBox.Show("Problem in connecting device, please check it.", "SPG Reporting Tool", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }

            }
            catch (Exception exe)
            {
                ElpisServer.Addlogs("All", "SPG Reporting Tool-Hold Mid Position Test", string.Format("Error in Read value.{0}.", exe.Message), LogStatus.Information);
                StopTest();
                //ConnectDevice();
            }
        }


        private void btnStartStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lblStartStop.Content.ToString() == "Start Record")
                {
                    bool isValid = false;
                    
                    if (rb_LineA.IsChecked == true)
                        isValid = ValidateInputsLineA();
                    else if (rb_LineB.IsChecked == true)
                        isValid = ValidateInputsLineB();
                    if (isValid)
                    {
                        if (rb_LineA.IsChecked == true ? (PressureLineASeriesCollection == null) : true && rb_LineB.IsChecked == true ? (PressureLineBSeriesCollection == null) : true)
                        {
                            string connectorName = HomePage.SelectedConnector;
                            string deviceName = HomePage.SelectedDevice;
                            if (!string.IsNullOrEmpty(connectorName) && !string.IsNullOrEmpty(deviceName))
                            {
                                TagsCollection = Helper.GetTagsCollection(TestType.HoldMidPositionTest, connectorName, deviceName);
                                int tagMatchCount = 0;
                                //connectorSelection.Close();
                                if (TagsCollection != null)
                                {
                                    foreach (var item in TagsCollection)
                                    {
                                        if (item.TagName.ToLower().Contains("allowablepressuredrop"))
                                        {
                                            AllowablePressureDropTagAddress = (item.Address);
                                            tagMatchCount++;
                                        }
                                        else if (item.TagName.ToLower().Contains("holdingtimelinea"))
                                        {
                                            HoldingTimeATagAddress = (item.Address);
                                            tagMatchCount++;
                                        }

                                        else if (item.TagName.ToLower().Contains("holdingtimelineb"))
                                        {
                                            HoldingTimeBTagAddress = (item.Address);
                                            tagMatchCount++;
                                        }
                                        else if (item.TagName.ToLower().Contains("pressurelinea"))
                                        {
                                            PressureLineATagAddress = (item.Address);
                                            tagMatchCount++;
                                        }
                                        else if (item.TagName.ToLower().Contains("pressurelineb"))
                                        {
                                            PressureLineBTagAddress = (item.Address);
                                            tagMatchCount++;
                                        }
                                        else if (item.TagName.ToLower().Contains("cylindermovement"))
                                        {
                                            CylinderMovementTagAddress = (item.Address);
                                            tagMatchCount++;
                                        }
                                        else if (item.TagName.ToLower().Contains("plctriggerlinea"))
                                        {
                                            PlcTriggerLineATagAddress = (item.Address);
                                            tagMatchCount++;
                                        }
                                        else if (item.TagName.ToLower().Contains("plctriggerlineb"))
                                        {
                                            PlcTriggerLineBTagAddress = (item.Address);
                                            tagMatchCount++;
                                        }

                                    }

                                    if (TagsCollection.Count == 8 && tagMatchCount == 8)
                                    {
                                        ElpisServer.Addlogs("All", "SPG Reporting Tool-Hold mid position test", string.Format("Holding TimeA Address:{0} Allowable Pressure Drop Address:{1} Holding Pressure A Address:{2}  Holding Pressure B Address:{3},Holding TimeB Address:{4},Cylinder Movement Address:{5},Plc Trigger Line A:{6},Plc trigger :{7} ", HoldingTimeATagAddress, AllowablePressureDropTagAddress, PressureLineATagAddress, PressureLineBTagAddress, HoldingTimeBTagAddress, CylinderMovementTagAddress, PlcTriggerLineATagAddress, PlcTriggerLineBTagAddress), LogStatus.Information);
                                        
                                       
                                        spDeviceStatus.Visibility = Visibility.Visible;
                                        ConnectDevice();
                                        
                                        if (tbxDeviceStatus.Text == "Connected")
                                        {
                                            ElpisOPCServerMainWindow.homePage.DisableInputs(true);

                                            //ElpisOPCServerMainWindow.homePage.btnReset.IsEnabled = false;
                                            ElpisOPCServerMainWindow.pump_Test.btnReset.IsEnabled = true;
                                            ElpisOPCServerMainWindow.pump_Test.btnGenerateReport.IsEnabled = false;
                                            ElpisOPCServerMainWindow.homePage.ReportTab.IsEnabled = true;
                                            ElpisOPCServerMainWindow.homePage.txtFilePath.IsEnabled = false;
                                            DataIntervalPanal.IsEnabled = false;
                                            txtTimer.Background = Brushes.Transparent;
                                            txtTimerMin.Background = Brushes.Transparent;
                                            txtTimerSec.Background = Brushes.Transparent;
                                            txtTimerMilliSec.Background = Brushes.Transparent;
                                            if (rb_LineA.IsChecked == true)
                                            {
                                                txtTestStatusA.Text = "";
                                                PressureLineALineSeries = new LineSeries
                                                {
                                                    Title = "Pressure LineA",
                                                    Values = new ChartValues<double>(),
                                                    Stroke = Brushes.DarkGreen,
                                                    PointGeometrySize = 5,
                                                    ScalesYAt = 0

                                                };

                                                CylinderMovementLineALineSeries = new LineSeries
                                                {
                                                    Title = "Cylinder Movement",
                                                    Values = new ChartValues<double>(),
                                                    Stroke = Brushes.DarkBlue,
                                                    PointGeometrySize = 5,
                                                    ScalesYAt = 1

                                                };
                                                ////25_SEP_2018
                                                //PressureLineBLineSeries = new LineSeries
                                                //{
                                                //    Title = "Pressure LineB",
                                                //    Values = new ChartValues<double>(),
                                                //    Stroke = Brushes.DarkOrange,
                                                //    PointGeometrySize = 5,
                                                //    ScalesYAt=0
                                                //};
                                                ////25_SEP_2018
                                                PressureLineASeriesCollection = new SeriesCollection() { PressureLineALineSeries, CylinderMovementLineALineSeries/*, PressureLineBLineSeries */};
                                                chartPressure.Series = PressureLineASeriesCollection;
                                                PressureLabels = new List<string>();
                                                
                                            }
                                            else if (rb_LineB.IsChecked == true)
                                            {
                                                txtTestStatusB.Text = "";
                                                
                                                PressureLineBLineSeries = new LineSeries
                                                {
                                                    Title = "Pressure LineB",
                                                    Values = new ChartValues<double>(),
                                                    Stroke = Brushes.DarkGreen,
                                                    PointGeometrySize = 5,
                                                    ScalesYAt = 0
                                                };

                                                CylinderMovementLineBLineSeries = new LineSeries
                                                {
                                                    Title = "Cylinder Movement",
                                                    Values = new ChartValues<double>(),
                                                    Stroke = Brushes.DarkBlue,
                                                    PointGeometrySize = 5,
                                                    ScalesYAt = 1,


                                                };


                                                ////25_SEP_2018
                                                //PressureLineALineSeries = new LineSeries
                                                //{
                                                //    Title = "Pressure LineA",
                                                //    Values = new ChartValues<double>(),
                                                //    Stroke = Brushes.DarkOrange,
                                                //    PointGeometrySize = 5,
                                                //    ScalesYAt=0
                                                    
                                                //};
                                                //25_SEP_2018

                                                PressureLineBSeriesCollection = new SeriesCollection() {PressureLineBLineSeries, CylinderMovementLineBLineSeries/*, PressureLineALineSeries */};
                                                chartPressureLineB.Series = PressureLineBSeriesCollection;
                                                PressureLineBLabels = new List<string>();
                                                
                                            }


                                            YFormatter = value => (value + 100.00).ToString();
                                            DataContext = this;
                                        
                                            retryCount = 3;
                                            //HomePage.holdMidPositionLineAInfo.TriggerPLCHoldMid = "Start";
                                            lblStartStop.Content = "Stop Record";
                                            imgStartStop.Source = stopicon;
                                            btnStartStop.ToolTip = "Stop Record";
                                            panelPressureLineSelection.IsEnabled = false;
                                            isHoldingTimeOver = false;
                                            timer.Start();
                                            




                                            tbxDateTime.Text = DateTime.Now.ToString();
                                            TriggerPLC(true);
                                            String HoldingTime = "";
                                            if (rb_LineA.IsChecked == true)
                                            {
                                               
                                                ReadDeviceData(PressureLineALineSeries, CylinderMovementLineALineSeries, HomePage.holdMidPositionLineAInfo/*, PressureLineBLineSeries*/);
                                               txtAllowablePressureA.Text = ReadTag(SunPowerGenMainPage.DeviceObject.DeviceType, AllowablePressureDropTagAddress, "allowablepressuredrop", Elpis.Windows.OPC.Server.DataType.Short);
                                                HomePage.holdMidPositionLineAInfo.HoldingLineAInitialPressure = PressureLineALineSeries.Values[0].ToString();
                                                HomePage.holdMidPositionLineAInfo.InitialCylinderMovement = CylinderMovementLineALineSeries.Values[0].ToString();
                                                txtInitialPressureLineA.Text = HomePage.holdMidPositionLineAInfo.HoldingLineAInitialPressure;
                                                 HoldingTime = ReadTag(SunPowerGenMainPage.DeviceObject.DeviceType, HoldingTimeATagAddress, "holdingtimelinea", Elpis.Windows.OPC.Server.DataType.Short);

                                                txtHoldingTimeA.Text = HoldingTime;
                                                HomePage.holdMidPositionLineAInfo.HoldingTimeValue = Convert.ToInt64(float.Parse(HoldingTime));
                                                if (HoldingTimeFormat == "Seconds")
                                                {
                                                    //String HoldingTimeA = ReadTag(SunPowerGenMainPage.DeviceObject.DeviceType, HoldingTimeATagAddress, "holdingtimelinea", Elpis.Windows.OPC.Server.DataType.Short);
                                                    //HomePage.holdMidPositionLineAInfo.HoldingTimeValue = Convert.ToInt64(float.Parse(HoldingTimeA));
                                                    txtHoldingTimeA.Text = Convert.ToString((HomePage.holdMidPositionLineAInfo.HoldingTimeValue) * (60000));
                                                }
                                                ticksDownHoldingTime = TimeSpan.FromMinutes(double.Parse(HoldingTime));
                                               // ////14/09/2018 changes
                                               //txtHoldingPressureA.Text = ReadTag(SunPowerGenMainPage.DeviceObject.DeviceType, AllowablePressureDropTagAddress, "pressurelinea", Elpis.Windows.OPC.Server.DataType.Short);
                                               //txtCylinderMovement.Text = ReadTag(SunPowerGenMainPage.DeviceObject.DeviceType, AllowablePressureDropTagAddress, "cylindermovement", Elpis.Windows.OPC.Server.DataType.Short);
                                               //txtHoldingPressureLineB.Text = ReadTag(SunPowerGenMainPage.DeviceObject.DeviceType, AllowablePressureDropTagAddress, "pressurelineb", Elpis.Windows.OPC.Server.DataType.Short);
                                               // ////14/09/2018 changes

                                                if (txtAllowablePressureA.Text == null || txtHoldingTimeA.Text == null)
                                                    StopTest();

                                                ElpisServer.Addlogs("All", "SPG Reporting Tool-Hold Mid Position Test", string.Format("Holding TimeA Address:{0} Holding TimeA Value{1}", HoldingTimeATagAddress, txtHoldingTimeA.Text), LogStatus.Information);
                                                //HomePage.holdMidPositionLineAInfo.HoldingTimeLineA = "";
                                                //HomePage.holdMidPositionLineAInfo.AllowablePressureDrop = "";
                                            }
                                            else if (rb_LineB.IsChecked == true)
                                            {
                                                
                                                ReadDeviceData(PressureLineBLineSeries, CylinderMovementLineBLineSeries, HomePage.holdMidPositionLineBinfo/*, PressureLineALineSeries*/);
                                                HomePage.holdMidPositionLineBinfo.TestDateTime = tbxDateTime.Text;
                                                txtAllowablePressureA.Text = ReadTag(SunPowerGenMainPage.DeviceObject.DeviceType, AllowablePressureDropTagAddress, "allowablepressuredrop", Elpis.Windows.OPC.Server.DataType.Short);
                                                txtHoldingTimeA.Text = ReadTag(SunPowerGenMainPage.DeviceObject.DeviceType, AllowablePressureDropTagAddress, "holdingtimelinea", Elpis.Windows.OPC.Server.DataType.Short);
                                                HomePage.holdMidPositionLineBinfo.InitialCylinderMovement = CylinderMovementLineBLineSeries.Values[0].ToString();
                                                HomePage.holdMidPositionLineBinfo.InitialPressureLineB = PressureLineBLineSeries.Values[0].ToString();
                                                txtInitialPressureLineB.Text = HomePage.holdMidPositionLineBinfo.InitialPressureLineB;
                                                if (txtAllowablePressureA.Text == null)
                                                {
                                                    StopTest();
                                                    return;
                                                }
                                               
                                                if (HoldingTimeFormat == "Minutes")
                                                {

                                                    txtHoldingTimeB.Text = ReadTag(SunPowerGenMainPage.DeviceObject.DeviceType, HoldingTimeBTagAddress, "holdingtimelineb", Elpis.Windows.OPC.Server.DataType.Short);
                                                    HomePage.holdMidPositionLineAInfo.HoldingTimeValue = Convert.ToInt16(float.Parse(txtHoldingTimeB.Text));
                                                    //var time = txtHoldingTimeB.Text;
                                                    ticksDownHoldingTime = TimeSpan.FromMinutes(double.Parse(txtHoldingTimeB.Text));
                                                }

                                                if (HoldingTimeFormat == "Seconds")
                                                {
                                                    String HoldingTimeB = ReadTag(SunPowerGenMainPage.DeviceObject.DeviceType, HoldingTimeBTagAddress, "holdingtimelineb", Elpis.Windows.OPC.Server.DataType.Short);
                                                    HomePage.holdMidPositionLineAInfo.HoldingTimeValue = Convert.ToInt64(float.Parse(HoldingTimeB)) ;
                                                    txtHoldingTimeB.Text = Convert.ToString((HomePage.holdMidPositionLineAInfo.HoldingTimeValue) * (60000));

                                                }
                                              


                                                ////14/09/2018 changes
                                                //txtHoldingPressureB.Text = ReadTag(SunPowerGenMainPage.DeviceObject.DeviceType, AllowablePressureDropTagAddress, "pressurelineb", Elpis.Windows.OPC.Server.DataType.Short);
                                                //txtCylinderMovement.Text = ReadTag(SunPowerGenMainPage.DeviceObject.DeviceType, AllowablePressureDropTagAddress, "cylindermovement", Elpis.Windows.OPC.Server.DataType.Short);
                                                //txtHoldingPressureLineA.Text = ReadTag(SunPowerGenMainPage.DeviceObject.DeviceType, AllowablePressureDropTagAddress, "pressurelinea", Elpis.Windows.OPC.Server.DataType.Short);
                                                ////14/09/2018 changes

                                                //txtHoldingTimeB.Text = ReadTag(SunPowerGenMainPage.DeviceObject.DeviceType, AllowablePressureDropTagAddress, "holdingtimelineb", Elpis.Windows.OPC.Server.DataType.Short);
                                                ElpisServer.Addlogs("All", "SPG Reporting Tool-Hold Mid Position Test", string.Format("Holding TimeB Address:{0} Holding TimeB Value{1}", HoldingTimeBTagAddress, txtHoldingTimeB.Text), LogStatus.Information);
                                            }
                                            //SetTimer();
                                            DataContext = this;
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
                                        MessageBox.Show("Configuration file having the invalid Tag Names, please check it.\nConfigure Tag Names Like as follows:\n  AllowablePressureDrop\n  PressureLineA\n  PressureLineB\n HoldingTimeLineA\n HoldingTimeLineA\n CylinderMovement\n PlcTriggerLineA\n PlcTriggerLineB", "SPG Reporting Tool", MessageBoxButton.OK, MessageBoxImage.Warning);

                                    }
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
                            //ElpisOPCServerMainWindow.homePage.expanderCertificate.IsExpanded = true;
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

            }
            catch (Exception ex)
            {
                panelPressureLineSelection.IsEnabled = true;
                ElpisServer.Addlogs("Report Tool", "Start Button-Hold Mid Position Test", ex.Message, LogStatus.Error);
            }
        }

        private void TriggerPLC_old()
        {
            try
            {
                #region ABMicroLogixEthernet
                if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ABMicroLogixEthernet)
                {
                    if (PlcTriggerLineATagAddress != null || PlcTriggerLineBTagAddress != null)
                    {
                        ABMicrologixEthernetDevice abDevice = SunPowerGenMainPage.DeviceObject as ABMicrologixEthernetDevice;
                        CpuType cpuType = Helper.GetABDeviceModelCPUType(abDevice.DeviceModelType);
                        if (rb_LineA.IsChecked == true)
                        {
                            foreach (var item in Helper.MappedTagList)
                            {
                                if (item.Key.ToLower().Contains("plctriggerlinea"))
                                {
                                    if (item.Value.Item1.Name.StartsWith("B3"))
                                    {//jey changed offset val ref 
                                        HomePage.holdMidPositionLineAInfo.OffSetValue = Convert.ToInt16(item.Value.Item1.Name.Split('/')[1]);
                                    }
                                    Helper.WriteEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                                    ElpisServer.Addlogs("Report Tool", "plc hold mid position lineA values", string.Format("tag details:{0} tag datatype:{1}", item.Value.Item1.UniqueKey, item.Value.Item2), LogStatus.Information);

                                }
                            }
                            if (PlcTriggerLineATagAddress == HomePage.holdMidPositionLineAInfo.TriggerTestAddress)
                                    {
                                        HomePage.holdMidPositionLineAInfo.TriggerPLCHoldMid = HomePage.holdMidPositionLineAInfo.TriggerStatus;
                                        ElpisServer.Addlogs("Report Tool/WriteTag", "return trigger status information", string.Format("Trigger status of hold mid position lineA:{0}", HomePage.holdMidPositionLineAInfo.TriggerPLCHoldMid), LogStatus.Information);
                                        if (HomePage.holdMidPositionLineAInfo.TriggerPLCHoldMid == "ON")
                                        {

                                            blbStateOFFLineA.Visibility = Visibility.Hidden;
                                            blbStateONLineA.Visibility = Visibility.Visible;




                                        }
                                        else
                                        {

                                            blbStateONLineA.Visibility = Visibility.Hidden;
                                            blbStateOFFLineA.Visibility = Visibility.Visible;
                                        }
                                    }

                                }
                                
                      

                        if (rb_LineB.IsChecked == true)
                        {
                            foreach (var item in Helper.MappedTagList)
                            {

                                if (item.Key.ToLower().Contains("plctriggerlineb"))
                                {
                                    if (item.Value.Item1.Name.StartsWith("B3"))
                                    {
                                        HomePage.holdMidPositionLineBinfo.OffSetValue = Convert.ToInt16(item.Value.Item1.Name.Split('/')[1]);
                                    }
                                    Helper.WriteEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                                    ElpisServer.Addlogs("Report Tool", "plc hold mid position lineB values", string.Format("tag details:{0} tag datatype:{1}", item.Value.Item1.UniqueKey, item.Value.Item2), LogStatus.Information);

                                }
                            }
                   
                            if (PlcTriggerLineBTagAddress == HomePage.holdMidPositionLineBinfo.TriggerTestAddress)
                        {
                            HomePage.holdMidPositionLineBinfo.TriggerPLCHoldMidB = HomePage.holdMidPositionLineBinfo.TriggerStatus;
                            ElpisServer.Addlogs("Report Tool/WriteTag", "return trigger status information", string.Format("Trigger status of hold mid position lineB:{0}", HomePage.holdMidPositionLineBinfo.TriggerPLCHoldMidB), LogStatus.Information);
                            if (HomePage.holdMidPositionLineBinfo.TriggerPLCHoldMidB == "ON")
                            {
                                blbStateOFFLineB.Visibility = Visibility.Hidden;
                                blbStateONLineB.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                blbStateOFFLineB.Visibility = Visibility.Visible;
                                blbStateONLineB.Visibility = Visibility.Hidden;
                            }
                        }

                    
                       }

                    }
                }
                #endregion

                #region ModBusEthernet
                else if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ModbusEthernet)
                {

                }
                #endregion ModBusEthernet

                #region ModBusSerial
                else if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ModbusSerial)
                {

                }
                #endregion ModBusSerial

            }
            catch (Exception e)
            {
                ElpisServer.Addlogs("Report tool", "PLC trigger in holdmid position test not communicated problem", e.Message, LogStatus.Warning);
                //StopTest();
            }
        }

        private void TriggerPLC(bool value)
        {
            int writeStatus = -1;
            int offSetValue = -1;
            LibplctagWrapper.Tag item = null;
            try
            {
                #region ABMicroLogixEthernet
                if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ABMicroLogixEthernet)
                {
                    if (rb_LineA.IsChecked == true && PlcTriggerLineATagAddress != null)
                    {
                        item = Helper.MappedTagList.Where(p => p.Key.ToLower() == "plctriggerlinea").Select(p => p.Value.Item1).First();
                        if (PlcTriggerLineATagAddress.Split('/').Count() > 0)
                            offSetValue = Convert.ToInt16(PlcTriggerLineATagAddress.Split('/')[1]);

                    }
                    else if (rb_LineB.IsChecked == true && PlcTriggerLineBTagAddress != null)
                    {
                        item = Helper.MappedTagList.Where(p => p.Key.ToLower() == "plctriggerlineb").Select(p => p.Value.Item1).First();

                        if (PlcTriggerLineBTagAddress.Split('/').Count() > 0)
                            offSetValue = Convert.ToInt16(PlcTriggerLineBTagAddress.Split('/')[1]);
                    }
                    if (offSetValue != -1)
                    {
                        writeStatus = Helper.WriteEthernetIPDevice1(item, offSetValue, value);
                        ElpisServer.Addlogs("Report Tool", "PLC Line A Hold Test Trigger PLC", string.Format("tag details:{0} tag name:{1} triggerToStart:{2}", item.UniqueKey, item.Name, value), LogStatus.Information);
                    }
                    SetTestRunning(writeStatus == 0 && value);
                    //ABMicrologixEthernetDevice abDevice = SunPowerGenMainPage.DeviceObject as ABMicrologixEthernetDevice;
                    //CpuType cpuType = Helper.GetABDeviceModelCPUType(abDevice.DeviceModelType);
                   
                    
                }
                #endregion

                #region ModBusEthernet
                else if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ModbusEthernet)
                {

                }
                #endregion ModBusEthernet

                #region ModBusSerial
                else if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ModbusSerial)
                {

                }
                #endregion ModBusSerial

            }
            catch (Exception e)
            {
                ElpisServer.Addlogs("Report tool", "PLC trigger in holdmid position test not communicated problem", e.Message, LogStatus.Warning);
                //StopTest();
            }
        }

        

        private void SetTestRunning(bool state)
        {
            if (state)
            {
                blbStateONLineA.Visibility = Visibility.Visible;
                blbStateOFFLineA.Visibility = Visibility.Hidden;
            }
            else
            {
                blbStateONLineA.Visibility = Visibility.Hidden;
                blbStateOFFLineA.Visibility = Visibility.Visible;
            }
        }
        private void StopTriggerPLC()
        {
            try
            {
                #region ABMicroLogixEthernet
                if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ABMicroLogixEthernet)
                {
                    if (PlcTriggerLineATagAddress != null || PlcTriggerLineBTagAddress != null)
                    {
                        ABMicrologixEthernetDevice abDevice = SunPowerGenMainPage.DeviceObject as ABMicrologixEthernetDevice;
                        CpuType cpuType = Helper.GetABDeviceModelCPUType(abDevice.DeviceModelType);
                        if (rb_LineA.IsChecked == true)
                        {
                            foreach (var item in Helper.MappedTagList)
                            {
                                if (item.Key.ToLower().Contains("plctriggerlinea"))
                                {
                                    if (item.Value.Item1.Name.StartsWith("B3"))
                                    {
                                        HomePage.slipStickTestInformation.OffSetValue = Convert.ToInt16(item.Value.Item1.Name.Split('/')[1]);
                                    }
                                    Helper.WriteEthernetIPDeviceStop(item.Value.Item1, item.Value.Item2);
                                    ElpisServer.Addlogs("Report Tool", "plc hold mid position lineA values", string.Format("tag details:{0} tag datatype:{1}", item.Value.Item1.UniqueKey, item.Value.Item2), LogStatus.Information);

                                }
                            }
                            if (PlcTriggerLineATagAddress == HomePage.strokeTestInfo.TriggerTestAddress)
                            {
                                HomePage.holdMidPositionLineAInfo.TriggerPLCHoldMid = HomePage.strokeTestInfo.TriggerStatus;
                                ElpisServer.Addlogs("Report Tool/WriteTag", "return trigger status information", string.Format("Trigger status of hold mid position lineA:{0}", HomePage.holdMidPositionLineAInfo.TriggerPLCHoldMid), LogStatus.Information);
                                if (HomePage.holdMidPositionLineAInfo.TriggerPLCHoldMid == "OFF")
                                {

                                    blbStateOFFLineA.Visibility = Visibility.Visible;
                                    blbStateONLineA.Visibility = Visibility.Hidden;




                                }
                                else
                                {

                                    blbStateONLineA.Visibility = Visibility.Visible;
                                    blbStateOFFLineA.Visibility = Visibility.Hidden;
                                }
                            }

                        }



                        if (rb_LineB.IsChecked == true)
                        {
                            foreach (var item in Helper.MappedTagList)
                            {

                                if (item.Key.ToLower().Contains("plctriggerlineb"))
                                {
                                    if (item.Value.Item1.Name.StartsWith("B3"))
                                    {
                                        HomePage.slipStickTestInformation.OffSetValue = Convert.ToInt16(item.Value.Item1.Name.Split('/')[1]);
                                    }
                                    Helper.WriteEthernetIPDeviceStop(item.Value.Item1, item.Value.Item2);
                                    ElpisServer.Addlogs("Report Tool", "plc hold mid position lineB values", string.Format("tag details:{0} tag datatype:{1}", item.Value.Item1.UniqueKey, item.Value.Item2), LogStatus.Information);

                                }
                            }

                            if (PlcTriggerLineBTagAddress == HomePage.strokeTestInfo.TriggerTestAddress)
                            {
                                HomePage.holdMidPositionLineBinfo.TriggerPLCHoldMidB = HomePage.strokeTestInfo.TriggerStatus;
                                ElpisServer.Addlogs("Report Tool/WriteTag", "return trigger status information", string.Format("Trigger status of hold mid position lineB:{0}", HomePage.holdMidPositionLineBinfo.TriggerPLCHoldMidB), LogStatus.Information);
                                if (HomePage.holdMidPositionLineBinfo.TriggerPLCHoldMidB == "OFF")
                                {
                                    blbStateOFFLineB.Visibility = Visibility.Visible;
                                    blbStateONLineB.Visibility = Visibility.Hidden;
                                }
                                else
                                {
                                    blbStateOFFLineB.Visibility = Visibility.Hidden;
                                    blbStateONLineB.Visibility = Visibility.Visible;
                                }
                            }


                        }

                    }
                }
                #endregion

                #region ModBusEthernet
                else if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ModbusEthernet)
                {

                }
                #endregion ModBusEthernet

                #region ModBusSerial
                else if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ModbusSerial)
                {

                }
                #endregion ModBusSerial

            }
            catch (Exception e)
            {
                ElpisServer.Addlogs("Report tool", "PLC trigger in holdmid position test not communicated problem", e.Message, LogStatus.Warning);
                //StopTest();
            }
        }


        private bool ValidateInputsLineA()
        {
            bool isValid = false;
            try
            {
                isValid = true;
                if (string.IsNullOrWhiteSpace(HomePage.holdMidPositionLineAInfo.JobNumber) || string.IsNullOrEmpty(HomePage.holdMidPositionLineAInfo.JobNumber))
                    isValid = isValid && false;
                else if (HomePage.holdMidPositionLineAInfo.JobNumber.Length > 0)
                {
                    Regex expr = new Regex("^[0-9]{10}$");
                    if (!expr.IsMatch(HomePage.holdMidPositionLineAInfo.JobNumber))
                        isValid = isValid && false;
                }

                if (string.IsNullOrWhiteSpace(HomePage.holdMidPositionLineAInfo.CustomerName) || string.IsNullOrEmpty(HomePage.holdMidPositionLineAInfo.CustomerName))
                    isValid = isValid && false;

                if (HomePage.holdMidPositionLineAInfo.BoreSize <= uint.MinValue || HomePage.holdMidPositionLineAInfo.BoreSize >= uint.MaxValue)
                    isValid = isValid && false;

                if (HomePage.holdMidPositionLineAInfo.RodSize <= uint.MinValue || HomePage.holdMidPositionLineAInfo.RodSize >= uint.MaxValue)
                    isValid = isValid && false;

                if (HomePage.holdMidPositionLineAInfo.StrokeLength <= uint.MinValue || HomePage.holdMidPositionLineAInfo.StrokeLength >= uint.MaxValue)
                    isValid = isValid && false;

                if (string.IsNullOrWhiteSpace(HomePage.holdMidPositionLineAInfo.CylinderNumber) || string.IsNullOrEmpty(HomePage.holdMidPositionLineAInfo.CylinderNumber))
                    isValid = isValid && false;

            }
            catch (Exception ex)
            {
                isValid = false;
            }
            finally
            {
                string reportNumber = HomePage.holdMidPositionLineAInfo.ReportNumber;
                this.gridMain.DataContext = null;
                ElpisOPCServerMainWindow.homePage.gridMain.DataContext = null;
                HomePage.holdMidPositionLineAInfo.ReportNumber = reportNumber;
                this.gridMain.DataContext = HomePage.holdMidPositionLineAInfo;
                ElpisOPCServerMainWindow.homePage.gridMain.DataContext = HomePage.holdMidPositionLineAInfo;
            }
            if (isValid)
                ElpisOPCServerMainWindow.homePage.expanderCertificate.IsExpanded = false;
            return isValid;
        }

        private bool ValidateInputsLineB()
        {
            bool isValid = false;
            try
            {
                isValid = true;
                if (string.IsNullOrWhiteSpace(HomePage.holdMidPositionLineBinfo.JobNumber) || string.IsNullOrEmpty(HomePage.holdMidPositionLineBinfo.JobNumber))
                    isValid = isValid && false;
                else if (HomePage.holdMidPositionLineBinfo.JobNumber.Length > 0)
                {
                    Regex expr = new Regex("^[0-9]{10}$");
                    if (!expr.IsMatch(HomePage.holdMidPositionLineBinfo.JobNumber))
                        isValid = isValid && false;
                }

                if (string.IsNullOrWhiteSpace(HomePage.holdMidPositionLineBinfo.CustomerName) || string.IsNullOrEmpty(HomePage.holdMidPositionLineBinfo.CustomerName))
                    isValid = isValid && false;

                if (HomePage.holdMidPositionLineBinfo.BoreSize <= uint.MinValue || HomePage.holdMidPositionLineBinfo.BoreSize >= uint.MaxValue)
                    isValid = isValid && false;

                if (HomePage.holdMidPositionLineBinfo.RodSize <= uint.MinValue || HomePage.holdMidPositionLineBinfo.RodSize >= uint.MaxValue)
                    isValid = isValid && false;

                if (HomePage.holdMidPositionLineBinfo.StrokeLength <= uint.MinValue || HomePage.holdMidPositionLineBinfo.StrokeLength >= uint.MaxValue)
                    isValid = isValid && false;

                if (string.IsNullOrWhiteSpace(HomePage.holdMidPositionLineBinfo.CylinderNumber) || string.IsNullOrEmpty(HomePage.holdMidPositionLineBinfo.CylinderNumber))
                    isValid = isValid && false;

            }
            catch (Exception ex)
            {
                isValid = false;
            }
            finally
            {
                string reportNumber = HomePage.holdMidPositionLineBinfo.ReportNumber;
                this.gridMain.DataContext = null;
                ElpisOPCServerMainWindow.homePage.gridMain.DataContext = null;
                HomePage.holdMidPositionLineBinfo.ReportNumber = reportNumber;
                this.gridMain.DataContext = HomePage.holdMidPositionLineBinfo;
                ElpisOPCServerMainWindow.homePage.gridMain.DataContext = HomePage.holdMidPositionLineBinfo;
            }
            if (isValid)
                ElpisOPCServerMainWindow.homePage.expanderCertificate.IsExpanded = false;
            return isValid;
        }

        private string ReadTag(DeviceType deviceType, string tagAddress, string tagName, Elpis.Windows.OPC.Server.DataType dataType)
        {
            string result = string.Empty;
            try
            {
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
                        if (tagAddress.ToString()[0].ToString() == "3")
                            result = Helper.ReadDeviceInputRegisterValue(ushort.Parse(tagAddress), dataType, DeviceType.ModbusSerial, 0, slaveId);
                        else if (tagAddress.ToString()[0].ToString() == "4")
                            result = Helper.ReadDeviceHoldingRegisterValue(ushort.Parse(tagAddress), dataType, DeviceType.ModbusSerial, 0, slaveId);
                    }
                    else
                    {
                        result = SunPowerGenMainPage.ModbusSerialPortMaster.ReadHoldingRegisters(slaveId, ushort.Parse(tagAddress), 1)[0].ToString();
                    }
                }
                else if (deviceType == DeviceType.ABMicroLogixEthernet)
                {
                    if (Helper.MappedTagList != null)
                    {
                        foreach (var item in Helper.MappedTagList)
                        {
                            if (item.Key.ToLower().Contains(tagName.ToLower()))
                                result = Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ElpisServer.Addlogs("Report Tool", "Read Tag-Hold Mid Position Test", e.Message, LogStatus.Information);
            }
            return result;
        }

        internal void GenerateReport()
        {
            ReportGeneration reportGeneration = new ReportGeneration();

            //reportGeneration.GenerateCSVFile(TestType.HoldMidPositionTest,)
        }

        private void StopTest()
        {
            isHoldingTimeOver = true;
            
            lblStartStop.Content = "Start Record";
            imgStartStop.Source = starticon;
            btnStartStop.ToolTip = "Start Record";
            //HomePage.holdMidPositionLineAInfo.TriggerPLCHoldMid = "Stop";
            DataIntervalPanal.IsEnabled = true;
            txtTimer.Background = Brushes.White;
            txtTimerMin.Background = Brushes.White;
            txtTimerSec.Background = Brushes.White;
            txtTimerMilliSec.Background = Brushes.White;
            timer.Stop();
            
            CheckTestStatus();
            //StopTriggerPLC();
            TriggerPLC(false);
            if (rb_LineA.IsChecked == true) { txtTestStatusA.Text = HomePage.holdMidPositionLineAInfo.TestStatusA; }
            if (rb_LineB.IsChecked == true) { txtTestStatusB.Text = HomePage.holdMidPositionLineBinfo.TestStatusB; }
            panelPressureLineSelection.IsEnabled = true;
            //Minutes.IsEnabled = true;
           // Seconds.IsEnabled = true;
            spDeviceStatus.Visibility = Visibility.Visible;
            //ElpisOPCServerMainWindow.homePage.btnReset.IsEnabled = true;
            ElpisOPCServerMainWindow.pump_Test.btnReset.IsEnabled = true;
            ElpisOPCServerMainWindow.pump_Test.btnGenerateReport.IsEnabled = true;
            ElpisOPCServerMainWindow.homePage.expanderCertificate.IsExpanded = true;
            
            GenerateCSVFile();
            Count = 0;
        }

        private void CheckTestStatus()
        {
            if (rb_LineA.IsChecked == true && PressureLineALineSeries.Values.Count > 0)
            {
                if ((double.Parse(PressureLineALineSeries.Values[PressureLineALineSeries.Values.Count - 1].ToString()))> ((double.Parse(PressureLineALineSeries.Values[0].ToString()))- (double.Parse(HomePage.holdMidPositionLineAInfo.AllowablePressureDrop))))
                {
                    HomePage.holdMidPositionLineAInfo.TestStatusA = "Pass";
                    //txtTestStatus.Text = "Pass";
                }
                else
                {
                    HomePage.holdMidPositionLineAInfo.TestStatusA = "Fail";
                   // txtTestStatus.Text = "Fail";
                }
            }
            else if (rb_LineB.IsChecked == true && PressureLineBLineSeries.Values.Count > 0)
            {
                if ((double.Parse(PressureLineBLineSeries.Values[PressureLineBLineSeries.Values.Count - 1].ToString())) >((double.Parse(PressureLineBLineSeries.Values[0].ToString()))- (double.Parse(HomePage.holdMidPositionLineBinfo.AllowablePressureDrop))))
                {
                    HomePage.holdMidPositionLineBinfo.TestStatusB = "Pass";
                    //txtTestStatus.Text = "Pass";
                }
                else
                {
                    HomePage.holdMidPositionLineBinfo.TestStatusB = "Fail";
                   // txtTestStatus.Text = "Fail";
                }
            }
        }

        private void GenerateCSVFile()
        {
            ReportGeneration reportGeneration = new ReportGeneration();
            ObservableCollection<LineSeries> lineSeriesList = null;
            ObservableCollection<List<string>> labelCollection = null;
            dynamic testData = null;
            if (rb_LineA.IsChecked == true)
            {
                testData = HomePage.holdMidPositionLineAInfo;
                LineSeries series1 = new LineSeries() { Values = PressureLineALineSeries.Values, Title = PressureLineALineSeries.Title, Stroke = PressureLineALineSeries.Stroke,/*S/*calesYAt=PressureLineALineSeries.ScalesYAt,*/ Foreground = Brushes.Black };
                LineSeries series2 = new LineSeries() { Values = CylinderMovementLineALineSeries.Values, Title = CylinderMovementLineALineSeries.Title, Stroke = CylinderMovementLineALineSeries.Stroke/*,ScalesYAt=CylinderMovementLineALineSeries.ScalesYAt*/, Foreground = Brushes.Black };
                //LineSeries series3 = new LineSeries() { Values = PressureLineBLineSeries.Values, Title = PressureLineBLineSeries.Title, Stroke = PressureLineBLineSeries.Stroke, Foreground = Brushes.Black };
                lineSeriesList = new ObservableCollection<LineSeries>() { series1, series2/*, series3*/ };
                labelCollection = new ObservableCollection<List<string>>() { PressureLabels, PressureLabels /*,PressureLabels*/};
                reportGeneration.GenerateCSVFile(TestType.HoldMidPositionTest, testData, lineSeriesList, labelCollection);
            }

            else if (rb_LineB.IsChecked == true)
            {
                testData = HomePage.holdMidPositionLineBinfo;
                LineSeries series1 = new LineSeries() { Values = PressureLineBLineSeries.Values, Title = PressureLineBLineSeries.Title, Stroke = PressureLineBLineSeries.Stroke, Foreground = Brushes.Black };
                LineSeries series2 = new LineSeries() { Values = CylinderMovementLineBLineSeries.Values, Title = CylinderMovementLineBLineSeries.Title, Stroke = CylinderMovementLineBLineSeries.Stroke, Foreground = Brushes.Black };
                //LineSeries series3 = new LineSeries() { Values = PressureLineALineSeries.Values, Title = PressureLineALineSeries.Title, Stroke = PressureLineALineSeries.Stroke, Foreground = Brushes.Black };
                lineSeriesList = new ObservableCollection<LineSeries>() { series1, series2/*, series3*/ };
                labelCollection = new ObservableCollection<List<string>>() { PressureLineBLabels, PressureLineBLabels/*, PressureLineBLabels */};
                reportGeneration.GenerateCSVFile(TestType.HoldMidPositionLineBTest, testData, lineSeriesList, labelCollection);
               
            }

        }

        private void ConnectDevice()
        {
            try
            {
                //17/9/2018 changes
                if (SunPowerGenMainPage.DeviceObject != null)
                {
                    DeviceType deviceType = SunPowerGenMainPage.DeviceObject.DeviceType;
                    bool isConnected = false;
                    //if (deviceType == DeviceType.ABMicroLogixEthernet)
                    //{
                    //    SunPowerGenMainPage.DeviceTcpClient = Helper.CreateTcpClient(((ABMicrologixEthernetDevice)SunPowerGenMainPage.DeviceObject).IPAddress, ((ABMicrologixEthernetDevice)SunPowerGenMainPage.DeviceObject).Port);
                    //    if (SunPowerGenMainPage.DeviceTcpClient != null && SunPowerGenMainPage.DeviceTcpClient.Connected)
                    //    {
                    //        //ipAddress = SunPowerGenMainPage.DeviceTcpClient.Client.LocalEndPoint.ToString();
                    //        // CreateEIPTags(ipAddress);
                    //        //if list having the previous test data, clear it.
                    //        if (MappedTagList != null)
                    //            MappedTagList.Clear();
                    //        //If list is null it creates a new list.
                    //        else
                    //            MappedTagList = new Dictionary<string, Tuple<LibplctagWrapper.Tag, Elpis.Windows.OPC.Server.DataType>>();
                    //        CreateMappedTagList();
                    //        if (SunPowerGenMainPage.ABEthernetClient != null)
                    //            SunPowerGenMainPage.ABEthernetClient.Dispose();
                    //        SunPowerGenMainPage.ABEthernetClient = new LibplctagWrapper.Libplctag();
                    //        isConnected = true;
                    //    }
                    //}
                    //17/9/2018 chnages

                    //bool isConnected = false;
                    isConnected = Helper.ConnectingDevice(isConnected, TagsCollection);
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
                tbxDeviceStatus.Text = "Not Connected";
                tbxDeviceStatus.Foreground = Brushes.Red;
            }
        }

        private void CreateMappedTagList()
        {
            try
            {
                //17/9/2018 changes
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
                ElpisServer.Addlogs("All", "SPG Reporting Tool-Hold Mid Position Test", string.Format("Error in creating MappedTagList.{0}.", e.Message), LogStatus.Information);
            }
            //17/9/2018 changes

           // throw new NotImplementedException();
        }



        private void GenerateHoldMidLineAReport()
        {
            ReportGeneration reportGenerator = new ReportGeneration();
            List<LineSeries> lineSeriesCollection = new List<LineSeries>() { };
            LineSeries series1 = new LineSeries() { Values = PressureLineALineSeries.Values, Title = PressureLineALineSeries.Title, Stroke = PressureLineALineSeries.Stroke, Foreground = Brushes.Black };

            LineSeries series2 = new LineSeries() { Values = CylinderMovementLineALineSeries.Values, Title = CylinderMovementLineALineSeries.Title, Stroke = CylinderMovementLineALineSeries.Stroke, Foreground = Brushes.Black };


            ObservableCollection<SeriesCollection> seriesCollection = new ObservableCollection<SeriesCollection>() { new SeriesCollection() { series1, series2 } };
            ObservableCollection<LineSeries> lineSeriesList = new ObservableCollection<LineSeries>() { series1, series2 };
            ObservableCollection<List<string>> labelCollection = new ObservableCollection<List<string>>() { PressureLabels };

            reportGenerator.GenerateCSVFile(TestType.HoldMidPositionTest, holdTestInformation, lineSeriesList, labelCollection, false, txtTestStatusA.Text);
            reportGenerator.GeneratePDFReport(TestType.HoldMidPositionTest, holdTestInformation, seriesCollection, labelCollection, false, txtTestStatusA.ToString());
            //MessageBox.Show("Report are generated successfully.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        internal void ResetHoldMidPosition()
        {
            HomePage.holdMidPositionLineAInfo = null;
            HomePage.holdMidPositionLineAInfo = new Hold_MidPositionLineATestInformation();
            tbxDeviceStatus.Text = "";
            txtTestStatusA.Text = "";
            txtTestStatusB.Text = "";
            txtTimer.Text = "00";
            txtTimerMin.Text = "00";
            txtTimerSec.Text = "02";
            txtTimerMilliSec.Text = "000";

            blbStateONLineA.Visibility = Visibility.Hidden;
            blbStateONLineB.Visibility = Visibility.Hidden;
            blbStateOFFLineB.Visibility = Visibility.Visible;
            blbStateOFFLineA.Visibility = Visibility.Visible;
            

            txtInitialPressureLineA.Text = "0";
            txtInitialPressureLineB.Text = "0";
            if (rb_LineA.IsChecked == true)
            {
                if (btnStartStop.Content.ToString() == "Stop Record")
                    StopTest();
                HomePage.holdMidPositionLineAInfo.IsTestStarted = false;
                this.gridMain.DataContext = HomePage.holdMidPositionLineAInfo;
                HomePage.holdMidPositionLineAInfo.IsTestStarted = true;
                txtTestStatusA.Text = "";

            }

            HomePage.holdMidPositionLineBinfo = null;
            HomePage.holdMidPositionLineBinfo = new Hold_MidPositionLineBTestInformation();
            if (rb_LineB.IsChecked == true)
            {
                if (btnStartStop.Content.ToString() == "Stop Record")
                    StopTest();
                HomePage.holdMidPositionLineBinfo.IsTestStarted = false;
                this.gridMain.DataContext = HomePage.holdMidPositionLineBinfo;
                HomePage.holdMidPositionLineBinfo.IsTestStarted = true;
                txtTestStatusB.Text = "";


            }
                

            if (PressureLineASeriesCollection != null)
                PressureLineASeriesCollection.Clear();
            PressureLineASeriesCollection = null;

            if (PressureLineBSeriesCollection != null)
                PressureLineBSeriesCollection.Clear();
            PressureLineBSeriesCollection = null;

            if (PressureLabels != null)
                PressureLabels.Clear();
            PressureLabels = null;

            if (PressureLineBLabels != null)
                PressureLineBLabels.Clear();
            PressureLineBLabels = null;

            
                


            DataContext = this;
        }



        private void UpdateUIData(string fileName)
        {
            if (rb_LineA.IsChecked == true)
            {
                ObservableCollection<LineSeries> seriesCollection = Helper.GetSeriesCollection(fileName, 1,TestType.HoldMidPositionTest);
                ObservableCollection<List<string>> labelCollection = Helper.GetLabelCollection(fileName, 1);
                Hold_MidPositionLineATestInformation testInformation = Helper.GetTestInformation(fileName, TestType.HoldMidPositionTest) as Hold_MidPositionLineATestInformation;
                if (seriesCollection != null && seriesCollection.Count == 1)
                {
                    seriesCollection[0].Title = "Pressure LineA";
                    seriesCollection[0].Stroke = Brushes.DarkOrange;
                    seriesCollection[0].PointGeometrySize = 5;
                    seriesCollection[0].StrokeThickness = 1;
                    PressureLineALineSeries = seriesCollection[0];

                    seriesCollection[1].Title = "Cylinder Movement";
                    seriesCollection[1].Stroke = Brushes.DarkBlue;
                    seriesCollection[1].PointGeometrySize = 5;
                    seriesCollection[1].StrokeThickness = 1;
                    CylinderMovementLineALineSeries = seriesCollection[1];
                    ////25_SEP_2018
                    //seriesCollection[2].Title = "Pressure LineB";
                    //seriesCollection[2].Stroke = Brushes.DarkGreen;
                    //seriesCollection[2].PointGeometrySize = 5;
                    //seriesCollection[2].StrokeThickness = 1;
                    //PressureLineBLineSeries = seriesCollection[2];
                    ////25_SEP_2018
                    PressureLineASeriesCollection = new SeriesCollection() { seriesCollection[0], seriesCollection[1] /*, seriesCollection[2]*/ };


                    HomePage.holdMidPositionLineAInfo = null;
                    HomePage.holdMidPositionLineAInfo = testInformation;
                    this.gridMain.DataContext = HomePage.holdMidPositionLineAInfo;

                    PressureLabels = new List<string>();
                    foreach (var item in labelCollection[0])
                    {
                        PressureLabels.Add(item);
                    }

                    pressureXAxis.Labels = PressureLabels;

                    chartPressure.Series = PressureLineASeriesCollection;
                    DataContext = this;
                }
            }
            else
            {
                ObservableCollection<LineSeries> seriesCollection = Helper.GetSeriesCollection(fileName, 1,TestType.HoldMidPositionLineBTest);
                ObservableCollection<List<string>> labelCollection = Helper.GetLabelCollection(fileName, 1);
                Hold_MidPositionLineBTestInformation testInformation = Helper.GetTestInformation(fileName, TestType.HoldMidPositionLineBTest) as Hold_MidPositionLineBTestInformation;
                if (seriesCollection != null && seriesCollection.Count == 1)
                {
                    ////25_SEP_2018
                    //seriesCollection[0].Title = "Pressure LineA";
                    //seriesCollection[0].Stroke = Brushes.DarkOrange;
                    //seriesCollection[0].PointGeometrySize = 5;
                    //seriesCollection[0].StrokeThickness = 1;

                    ////25_SEP_2018
                  
                    seriesCollection[1].Title = "Cylinder Movement";
                    seriesCollection[1].Stroke = Brushes.DarkBlue;
                    seriesCollection[1].PointGeometrySize = 5;
                    seriesCollection[1].StrokeThickness = 1;

                    seriesCollection[2].Title = "Pressure LineB";
                    seriesCollection[2].Stroke = Brushes.DarkGreen;
                    seriesCollection[2].PointGeometrySize = 5;
                    seriesCollection[2].StrokeThickness = 1;


                    PressureLineBLineSeries = seriesCollection[2];
                    CylinderMovementLineBLineSeries = seriesCollection[1];
                    PressureLineALineSeries = seriesCollection[0];
                    PressureLineBSeriesCollection = new SeriesCollection() { seriesCollection[0], seriesCollection[1]/* , seriesCollection[2] */};


                    HomePage.holdMidPositionLineBinfo = null;
                    HomePage.holdMidPositionLineBinfo = testInformation;
                    this.gridMain.DataContext = HomePage.holdMidPositionLineBinfo;

                    PressureLineBLabels = new List<string>();
                    foreach (var item in labelCollection[0])
                    {
                        PressureLineBLabels.Add(item);
                    }

                    pressureLineBXAxis.Labels = PressureLineBLabels;

                    chartPressure.Series = PressureLineBSeriesCollection;
                    DataContext = this;
                }
            }
        }


        private void HoldMidPosition_Loaded(object sender, RoutedEventArgs e)
        {
            if (rb_LineA.IsChecked == true)
                LoadHoldMidPositionLineA();
            else
                LoadHoldMidPositionLineB();
            SaveLoadedData();
            UpdateDataReadInterval();
        }
        private void SaveLoadedData()
        {
            UpdateConfigKey("HoldMidTestDataReadInterval", string.Format("{0}:{1}:{2}:{3}", txtTimer.Text, txtTimerMin.Text, txtTimerSec.Text, txtTimerMilliSec.Text));
            ////btnDataReadIntervalEdit.IsEnabled = true;
            //DataIntervalPanal.IsEnabled = false;
            //btnDataReadIntervalSave.IsEnabled = false;
        }

        private void LoadHoldMidPositionLineA()
        {

            if (HomePage.holdMidPositionLineAInfo == null)
                HomePage.holdMidPositionLineAInfo = new Hold_MidPositionLineATestInformation();
            string reportNumber = HomePage.holdMidPositionLineAInfo.ReportNumber;
            this.gridMain.DataContext = null;
            HomePage.holdMidPositionLineAInfo.ReportNumber = reportNumber;
            HomePage.holdMidPositionLineAInfo.IsTestStarted = false;
            HomePage.holdMidPositionLineAInfo.CustomerName = ElpisOPCServerMainWindow.homePage.txtCustName.Text;
            HomePage.holdMidPositionLineAInfo.JobNumber = ElpisOPCServerMainWindow.homePage.txtJobNumber.Text;
            HomePage.holdMidPositionLineAInfo.CylinderNumber = ElpisOPCServerMainWindow.homePage.txtCylinderNumber.Text;
            HomePage.holdMidPositionLineAInfo.RodSize = uint.Parse(ElpisOPCServerMainWindow.homePage.txtRodSize.Text);
            HomePage.holdMidPositionLineAInfo.BoreSize = uint.Parse(ElpisOPCServerMainWindow.homePage.txtBoreSize.Text);
            HomePage.holdMidPositionLineAInfo.StrokeLength = uint.Parse(ElpisOPCServerMainWindow.homePage.txtStrokeLength.Text);
            this.gridMain.DataContext = null;
            this.gridMain.DataContext = HomePage.holdMidPositionLineAInfo;
            ElpisOPCServerMainWindow.homePage.gridMain.DataContext = HomePage.holdMidPositionLineAInfo;
            HomePage.holdMidPositionLineAInfo.IsTestStarted = true;
            //ElpisOPCServerMainWindow.homePage.gridMain.DataContext = null;
            


            panelHoldingPresssureLineA.Visibility = Visibility.Hidden;
            panelInitialPresssureLineA.Visibility = Visibility.Visible;
            panelInitialPresssureLineB.Visibility = Visibility.Hidden;
            panelHoldingPresssureLineB.Visibility = Visibility.Visible;
            borderHoldingPressureLineB.Visibility = Visibility.Hidden;
            borderHoldingPressureLineA.Visibility = Visibility.Visible;
            borderHoldingTimeLineA.Visibility = Visibility.Visible;
            borderHoldingTimeLineB.Visibility = Visibility.Hidden;
            spChartPressureLineA.Visibility = Visibility.Visible;
            spChartPressureLineB.Visibility = Visibility.Hidden;
            txtTestStatusB.Visibility = Visibility.Hidden;
            txtTestStatusA.Visibility = Visibility.Visible;
            linA_status.Visibility = Visibility.Visible;
            LineB_Status.Visibility = Visibility.Hidden;
    
            //PressureLineABlock.Visibility = Visibility.Hidden;
            // PressureLineBBlock.Visibility = Visibility.Visible;
            DataContext = this;
        }

        private void LoadHoldMidPositionLineB()
        {
            if (HomePage.holdMidPositionLineBinfo == null)
                HomePage.holdMidPositionLineBinfo = new Hold_MidPositionLineBTestInformation();
            string reportNumber = HomePage.holdMidPositionLineBinfo.ReportNumber;
            this.gridMain.DataContext = null;
            HomePage.holdMidPositionLineBinfo.ReportNumber = reportNumber;
            HomePage.holdMidPositionLineBinfo.IsTestStarted = false;
            HomePage.holdMidPositionLineBinfo.CustomerName = ElpisOPCServerMainWindow.homePage.txtCustName.Text;
            HomePage.holdMidPositionLineBinfo.JobNumber = ElpisOPCServerMainWindow.homePage.txtJobNumber.Text;
            HomePage.holdMidPositionLineBinfo.CylinderNumber = ElpisOPCServerMainWindow.homePage.txtCylinderNumber.Text;
            HomePage.holdMidPositionLineBinfo.RodSize = uint.Parse(ElpisOPCServerMainWindow.homePage.txtRodSize.Text);
            HomePage.holdMidPositionLineBinfo.BoreSize = uint.Parse(ElpisOPCServerMainWindow.homePage.txtBoreSize.Text);
            HomePage.holdMidPositionLineBinfo.StrokeLength = uint.Parse(ElpisOPCServerMainWindow.homePage.txtStrokeLength.Text);
            this.gridMain.DataContext = null;
            this.gridMain.DataContext = HomePage.holdMidPositionLineBinfo;
            ElpisOPCServerMainWindow.homePage.gridMain.DataContext = HomePage.holdMidPositionLineBinfo;

            HomePage.holdMidPositionLineBinfo.IsTestStarted = true;


            panelHoldingPresssureLineA.Visibility = Visibility.Visible;
            panelInitialPresssureLineB.Visibility = Visibility.Visible;
            panelInitialPresssureLineA.Visibility = Visibility.Hidden;
            panelHoldingPresssureLineB.Visibility = Visibility.Hidden;
            borderHoldingPressureLineB.Visibility = Visibility.Visible;
            borderHoldingPressureLineA.Visibility = Visibility.Hidden;
            borderHoldingTimeLineA.Visibility = Visibility.Hidden;
            borderHoldingTimeLineB.Visibility = Visibility.Visible;
            spChartPressureLineA.Visibility = Visibility.Hidden;
            spChartPressureLineB.Visibility = Visibility.Visible;
            txtTestStatusA.Visibility = Visibility.Hidden;
            txtTestStatusB.Visibility = Visibility.Visible;
            linA_status.Visibility = Visibility.Hidden;
            LineB_Status.Visibility = Visibility.Visible;

            //lbTriggerLineB.Visibility = Visibility.Visible;
            //blbStateOFFLineB.Visibility = Visibility.Visible;
            //blbStateONLineB.Visibility = Visibility.Hidden;
            //lbTriggerLineA.Visibility = Visibility.Hidden;
            //blbStateOFFLineA.Visibility = Visibility.Hidden;
            //blbStateONLineA.Visibility = Visibility.Hidden;

            //PressureLineABlock.Visibility = Visibility.Visible;
            // PressureLineBBlock.Visibility = Visibility.Hidden;
            DataContext = this;
        }

        private void UpdateDataReadInterval()
        {
            try
            {
                Regex expr = new Regex("^[0-9]{1,2}:[0-9]{1,2}:[0-9]{1,2}:[0-9]{1,3}$");
                string cycleDuration = ConfigurationManager.AppSettings["HoldMidTestDataReadInterval"].ToString();
                if (!string.IsNullOrEmpty(cycleDuration) && !expr.IsMatch(cycleDuration))
                {
                    MessageBox.Show("Configuration file have invalid or no value for Cycle Duration. It set to default value 00:00:02:000");
                    cycleDuration = "00:00:02:000";
                }
                string[] CycleDurationTime = cycleDuration.Split(':');
                timer.Interval = new TimeSpan(0, int.Parse(CycleDurationTime[0]), int.Parse(CycleDurationTime[1]), int.Parse(CycleDurationTime[2]), int.Parse(CycleDurationTime[3]));
                //txtTimer.Text = CycleDurationTime[0];
                //txtTimerMin.Text = CycleDurationTime[1];
                //txtTimerSec.Text = CycleDurationTime[2];
                //txtTimerMilliSec.Text = CycleDurationTime[3];
            }
            catch(Exception e) { }
        }

        private void cmbLineSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private  void SetTimer()
        {
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(HomePage.holdMidPositionLineAInfo.HoldingTimeValue*60000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += new ElapsedEventHandler( OnTimedEvent);
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        { 
    
            
              Dispatcher.Invoke(delegate
              {
                  StopTest();
                  aTimer.Stop();
                  aTimer.Dispose();
              });
            
           
        }


        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
            {
                RadioButton rb = sender as RadioButton;
                if (rb != null && rb.Content.ToString() == "LineA")
                {
                    
                    LoadHoldMidPositionLineA();
                    
                   ElpisOPCServerMainWindow.homePage.txtCommentBox.Text = HomePage.holdMidPositionLineAInfo.Comment;
                    ElpisOPCServerMainWindow.homePage.txtCommnetUpdateMessage.Text = HomePage.holdMidPositionLineAInfo.CommentMessage;
                  
                  
                }
                else if (rb != null && rb.Content.ToString() == "LineB")
                {
                   
                    LoadHoldMidPositionLineB();
                    ElpisOPCServerMainWindow.homePage.txtCommentBox.Text = HomePage.holdMidPositionLineBinfo.Comment;
                    ElpisOPCServerMainWindow.homePage.txtCommnetUpdateMessage.Text = HomePage.holdMidPositionLineBinfo.CommentMessage;
                }
            }
        }

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
        
            //DataIntervalPanal.IsEnabled = false;
            //btnDataReadIntervalEdit.IsEnabled = true;
            //btnDataReadIntervalSave.IsEnabled = false;
            //txtTimer.Background = Brushes.Transparent;
            //txtTimerMin.Background = Brushes.Transparent;
            //txtTimerSec.Background = Brushes.Transparent;
            //txtTimerMilliSec.Background = Brushes.Transparent;

            UpdateConfigKey("HoldMidTestDataReadInterval", string.Format("{0}:{1}:{2}:{3}", txtTimer.Text, txtTimerMin.Text, txtTimerSec.Text, txtTimerMilliSec.Text));
            UpdateDataReadInterval();
          




          

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

        //private void btnEditOperation_Click(object sender, RoutedEventArgs e)
        //{
        //     DataIntervalPanal.IsEnabled = true;
        //     btnDataReadIntervalEdit.IsEnabled = false;
        //     btnDataReadIntervalSave.IsEnabled = true;
        //    txtTimer.Background = Brushes.White;
        //    txtTimerMin.Background = Brushes.White;
        //    txtTimerSec.Background = Brushes.White;
        //    txtTimerMilliSec.Background = Brushes.White;
        //}

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


    }
}

using Elpis.Windows.OPC.Server;
using LiveCharts;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using LiveCharts.Wpf;
using System.Windows.Threading;
using OPCEngine.Connectors.Allen_Bradley;
using Modbus.Device;
using LibplctagWrapper;
using Modbus;
using System.Configuration;
using System.IO;

namespace ElpisOpcServer.SunPowerGen
{
    /// <summary>
    /// Interaction logic for SlipStickTestUI.xaml
    /// </summary>
    public partial class SlipStickTestUI : UserControl
    {
        private int startAddressPos { get; set; }
        private int retryCount { get; set; }
        private byte slaveId { get; set; }
        private bool isInitialPressureSet { get; set; }
        public double PressureReadTime { get; }
        private DispatcherTimer dispatcherTimer { get; set; }
        public SeriesCollection PressureSeriesCollection { get; private set; }
        public ObservableCollection<Elpis.Windows.OPC.Server.Tag> TagsCollection { get; private set; }
        public string PressureTagAddress { get; private set; }
        public string FlowTagAddress { get; private set; }
        public string TimeInterval { get; private set; }
        
        public string CylinderMovementTagAddress { get; private set; }
        public LineSeries PressureSeries { get; private set; }
        public List<string> CylinderMovementLabels { get; private set; }
        public Func<double, string> YFormatter { get; set; }
        public string PlcTriggerTagAddress { get; private set; }
        private static System.Timers.Timer aTimer;
        BitmapImage starticon = new BitmapImage();
        BitmapImage stopicon = new BitmapImage();

        public SlipStickTestUI()
        {
            InitializeComponent();
            if (HomePage.slipStickTestInformation == null)
                HomePage.slipStickTestInformation = new Slip_StickTestInformation();
            HomePage.slipStickTestInformation.TestName = TestType.SlipStickTest;
            this.gridCeritificateInfo.DataContext = HomePage.slipStickTestInformation;
            
            chartPressure.AxisY[0].LabelFormatter = value => value.ToString("N2");
            PressureReadTime =double.Parse( ConfigurationManager.AppSettings["PressureReadInterval"].ToString());
            starticon.BeginInit();
            starticon.UriSource = new Uri("/ElpisOpcServer;component/Images/starticon.png", UriKind.Relative);
            starticon.EndInit();
            stopicon.BeginInit();
            stopicon.UriSource = new Uri("/ElpisOpcServer;component/Images/stopicon.png", UriKind.Relative);
            stopicon.EndInit();
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
        }



        private void btnStartStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lblStartStop.Content.ToString() == "Start Record")
                {
                    bool isValid = ValidateInputs();
                    if (isValid)
                    {
                        if (PressureSeriesCollection == null && isValid)
                        {   
                            string connectorName = HomePage.SelectedConnector;
                            string deviceName = HomePage.SelectedDevice;
                            if (!string.IsNullOrEmpty(connectorName) && !string.IsNullOrEmpty(deviceName))
                            {
                                this.IsHitTestVisible = false;
                                this.Cursor = Cursors.Wait;
                                //HomePage.slipStickTestInformation.TriggerStatus = "Start";
                                lblStartStop.Content = "Stop Record";
                                imgStartStop.Source = stopicon;
                                btnStartStop.ToolTip = "Stop Record";
                                tbxDeviceStatus.Text = "";                               TagsCollection = Helper.GetTagsCollection(TestType.SlipStickTest, connectorName, deviceName);
                                List<Tuple<string, bool>> tagStatus = new List<Tuple<string, bool>>();
                                if (TagsCollection != null && TagsCollection.Count > 0)
                                {
                                    foreach (var tag in TagsCollection)
                                    {
                                        if (tag.TagName.ToLower().Contains("pressure"))
                                        {
                                            PressureTagAddress = (tag.Address);
                                            tagStatus.Add(new Tuple<string, bool>("PresssureLineA", true));
                                        }

                                        else if (tag.TagName.ToLower().Contains("flow"))
                                        {
                                            FlowTagAddress = (tag.Address);
                                            tagStatus.Add(new Tuple<string, bool>("Flow", true));
                                        }
                                        else if (tag.TagName.ToLower().Contains("cylindermovement"))
                                        {
                                            CylinderMovementTagAddress = (tag.Address);
                                            tagStatus.Add(new Tuple<string, bool>("CylinderMovement", true));
                                        }
                                        else if (tag.TagName.ToLower().Contains("plctrigger"))
                                        {
                                            PlcTriggerTagAddress = (tag.Address);
                                            tagStatus.Add(new Tuple<string, bool>("PlcTrigger", true));
                                        }
                                    }

                                    if (tagStatus.Count == 4)
                                    {
                                        ElpisServer.Addlogs("All", "SPG Reporting Tool - slip stick test", string.Format("Pressure Address:{0} Flow Address:{1}  Cylinder Movement Address:{2} and PLCTrigger Address:{3}", PressureTagAddress, FlowTagAddress, CylinderMovementTagAddress, PlcTriggerTagAddress), LogStatus.Information);
                                        ConnectDevice();
                                        this.IsHitTestVisible = true;
                                        txtInitialPressure.Text = "0";
                                        spDeviceStatus.Visibility = Visibility.Visible;
                                        this.IsHitTestVisible = true;

                                        if (tbxDeviceStatus.Text == "Connected")
                                        {
                                            isInitialPressureSet = false;
                                            ElpisOPCServerMainWindow.homePage.DisableInputs(true);
                                            // ElpisOPCServerMainWindow.homePage.btnReset.IsEnabled = false;
                                            ElpisOPCServerMainWindow.pump_Test.btnReset.IsEnabled = true;
                                            ElpisOPCServerMainWindow.pump_Test.btnGenerateReport.IsEnabled = false;
                                            ElpisOPCServerMainWindow.homePage.ReportTab.IsEnabled = true;
                                            ElpisOPCServerMainWindow.homePage.txtFilePath.IsEnabled = false;
                                            //btnDataReadIntervalEdit.IsEnabled = false;
                                            DataIntervalPanal.IsEnabled = false;
                                            //btnDataReadIntervalSave.IsEnabled = false;
                                            txtTimer.Background = Brushes.Transparent;
                                            txtTimerMin.Background = Brushes.Transparent;
                                            txtTimerSec.Background = Brushes.Transparent;
                                            txtTimerMilliSec.Background = Brushes.Transparent;

                                            PressureSeries = new LineSeries
                                            {
                                                Title = "Pressure",
                                                Values = new ChartValues<double>(),
                                                Stroke = Brushes.Blue,
                                                PointGeometrySize = 5,
                                                //LabelPoint = Point => "" + Point.Y,
                                                //DataLabels = true,


                                            };

                                            PressureSeriesCollection = new SeriesCollection { PressureSeries  };
                                            
                                            chartPressure.Series = PressureSeriesCollection;
                                         
                                            CylinderMovementLabels = new List<string>();
                                            YFormatter = value => (value + 100.00).ToString("N2");
                                            pressureXAxis.Labels = CylinderMovementLabels;

                                            DataContext = this;

                                            SunPowerGenMainPage.isTestRunning = true;
                                            TriggerPLC(true);
                                            //string pressureValue = ReadTag(SunPowerGenMainPage.DeviceObject.DeviceType, PressureTagAddres, "pressure", Elpis.Windows.OPC.Server.DataType.Short);
                                            //if (pressureValue == null)
                                            //{
                                            //    StopTest();
                                            //    //btnGenerateReport.IsEnabled = true;
                                            //    //btnReset.IsEnabled = true;
                                            //}
                                            //else
                                            //{
                                            //    txtInitialPressure.Text = pressureValue;
                                            //}

                                            isInitialPressureSet = false;
                                            dispatcherTimer.Start();
                                           
                                            ReadDeviceData();
                                            tbxDateTine.Text = DateTime.Now.ToString();
                                            lblDateTime.Visibility = Visibility.Visible;
                                            blbStateOFF.Visibility = Visibility.Hidden;
                                            blbStateON.Visibility = Visibility.Visible;

                                        }
                                        else
                                        {
                                            lblStartStop.Content = "Start Record";
                                            imgStartStop.Source = starticon;
                                            btnStartStop.ToolTip = "Start Record";
                                            this.IsHitTestVisible = true;
                                            blbStateOFF.Visibility = Visibility.Visible;
                                            blbStateON.Visibility = Visibility.Hidden;
                                        }
                                    }
                                    else
                                    {
                                        StopTest();
                                        MessageBox.Show("Configuration file having the invalid Tag Names, please check it.\nConfigure Tag Names Like as follows:\n  Flow\n  Pressure\n CylinderMovement", "SPG Reporting Tool", MessageBoxButton.OK, MessageBoxImage.Warning);

                                    }
                                }
                                else
                                {

                                    StopTest();
                                    MessageBox.Show("Please create tags in configuration section.", "SPG Reporting Tool", MessageBoxButton.OK, MessageBoxImage.Warning);

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
                            MessageBoxResult messageOption = MessageBox.Show("Please reset all fields by clicking reset button, and start new Data Recording.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Question);
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(HomePage.slipStickTestInformation.PressureAfterFirstCylinderMovement))
                    {
                        MessageBoxResult Option = MessageBox.Show("Pressure is not captured Do you want to stop the recording data?", "SPG Report Tool", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (Option == MessageBoxResult.Yes)
                        {
                            StopTest();
                        }

                    }
                    else if (!string.IsNullOrEmpty(HomePage.slipStickTestInformation.PressureAfterFirstCylinderMovement))
                    {
                        MessageBoxResult messageOption = MessageBox.Show("Do you want to stop the recording data?", "SPG Report Tool", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (messageOption == MessageBoxResult.Yes)
                        {
                            StopTest();
                        }
                    }
                }
                this.Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Arrow;
                ElpisServer.Addlogs("Report Tool", "Start Button-Slip Stick Test", ex.Message, LogStatus.Error);
            }
        }


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
                    #region Modbus Ethernet
                    if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ModbusEthernet)
                    {
                        if (PressureTagAddress.ToString().Length >= 5 && FlowTagAddress.ToString().Length >= 5 && CylinderMovementTagAddress.ToString().Length >= 5)
                        {
                            try
                            {
                                if ((FlowTagAddress.ToString()[0]).ToString() == "3")
                                    txtFlow.Text = Helper.ReadDeviceInputRegisterValue(ushort.Parse(FlowTagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusEthernet, startAddressPos, slaveId);
                                else if ((FlowTagAddress.ToString()[0]).ToString() == "4")
                                    txtFlow.Text = Helper.ReadDeviceHoldingRegisterValue(ushort.Parse(FlowTagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusEthernet, startAddressPos, slaveId);

                                if (PressureTagAddress.ToString()[0].ToString() == "3")
                                    txtCurrentPressure.Text = Helper.ReadDeviceInputRegisterValue(ushort.Parse(PressureTagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusEthernet, startAddressPos, slaveId);
                                else if (PressureTagAddress.ToString()[0].ToString() == "4")
                                    txtCurrentPressure.Text = Helper.ReadDeviceHoldingRegisterValue(ushort.Parse(PressureTagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusEthernet, startAddressPos, slaveId);

                                if (CylinderMovementTagAddress.ToString()[0].ToString() == "3")
                                    txtCylinderMovement.Text = Helper.ReadDeviceInputRegisterValue(ushort.Parse(CylinderMovementTagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusEthernet, startAddressPos, slaveId);
                                else if (CylinderMovementTagAddress.ToString()[0].ToString() == "4")
                                    txtCylinderMovement.Text = Helper.ReadDeviceHoldingRegisterValue(ushort.Parse(CylinderMovementTagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusEthernet, startAddressPos, slaveId);
                            }
                            catch (SlaveException)
                            {
                                startAddressPos = 1;
                                retryCount++;
                            }
                            catch (Exception ex)
                            {
                                ElpisServer.Addlogs("All", "SPG Reporting Tool-Slip Stick Test", ex.Message, LogStatus.Information);
                                StopTest();
                            }
                        }
                        else
                        {
                            
                            txtFlow.Text = SunPowerGenMainPage.ModbusTcpMaster.ReadHoldingRegisters(ushort.Parse(FlowTagAddress), 1)[0].ToString();
                            txtCurrentPressure.Text = SunPowerGenMainPage.ModbusTcpMaster.ReadHoldingRegisters(ushort.Parse(PressureTagAddress), 1)[0].ToString();
                            if (!isInitialPressureSet)
                            {
                                txtInitialPressure.Text = txtCurrentPressure.Text;
                                isInitialPressureSet = true;
                            }
                            txtCylinderMovement.Text = SunPowerGenMainPage.ModbusTcpMaster.ReadHoldingRegisters(ushort.Parse(CylinderMovementTagAddress), 1)[0].ToString();

                        }
                    }
                    #endregion Modbus Ethernet

                    #region Serial Device
                    else if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ModbusSerial)
                    {

                        if (FlowTagAddress.ToString().Length >= 5 && PressureTagAddress.ToString().Length >= 5 && CylinderMovementTagAddress.ToString().Length >= 5)
                        {
                            if (FlowTagAddress.ToString()[0].ToString() == "1")
                                txtFlow.Text = Helper.ReadDeviceCoilsRegisterValue(ushort.Parse(FlowTagAddress), DeviceType.ModbusSerial, slaveId);
                            else if (FlowTagAddress.ToString()[0].ToString() == "2")
                                txtFlow.Text = Helper.ReadDeviceDiscreteInputRegisterValue(ushort.Parse(FlowTagAddress), DeviceType.ModbusSerial, slaveId);
                            else if (FlowTagAddress.ToString()[0].ToString() == "3")
                                txtFlow.Text = Helper.ReadDeviceInputRegisterValue(ushort.Parse(FlowTagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusSerial, 0, slaveId);
                            else if (FlowTagAddress.ToString()[0].ToString() == "4")
                                txtFlow.Text = Helper.ReadDeviceHoldingRegisterValue(ushort.Parse(FlowTagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusSerial, 0, slaveId);

                            if (PressureTagAddress.ToString()[0].ToString() == "1")
                                txtCurrentPressure.Text = Helper.ReadDeviceCoilsRegisterValue(ushort.Parse(PressureTagAddress), DeviceType.ModbusSerial, slaveId);
                            else if (PressureTagAddress.ToString()[0].ToString() == "2")
                                txtCurrentPressure.Text = Helper.ReadDeviceDiscreteInputRegisterValue(ushort.Parse(PressureTagAddress), DeviceType.ModbusSerial, slaveId);
                            else if (PressureTagAddress.ToString()[0].ToString() == "3")
                                txtCurrentPressure.Text = Helper.ReadDeviceInputRegisterValue(ushort.Parse(PressureTagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusSerial, 0, slaveId);
                            else if (PressureTagAddress.ToString()[0].ToString() == "4")
                                txtCurrentPressure.Text = Helper.ReadDeviceHoldingRegisterValue(ushort.Parse(PressureTagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusSerial, 0, slaveId);

                            if (CylinderMovementTagAddress.ToString()[0].ToString() == "1")
                                txtCylinderMovement.Text = Helper.ReadDeviceCoilsRegisterValue(ushort.Parse(CylinderMovementTagAddress), DeviceType.ModbusSerial, slaveId);
                            else if (CylinderMovementTagAddress.ToString()[0].ToString() == "2")
                                txtCylinderMovement.Text = Helper.ReadDeviceDiscreteInputRegisterValue(ushort.Parse(CylinderMovementTagAddress), DeviceType.ModbusSerial, slaveId);
                            else if (CylinderMovementTagAddress.ToString()[0].ToString() == "3")
                                txtCylinderMovement.Text = Helper.ReadDeviceInputRegisterValue(ushort.Parse(CylinderMovementTagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusSerial, 0, slaveId);
                            else if (CylinderMovementTagAddress.ToString()[0].ToString() == "4")
                                txtCylinderMovement.Text = Helper.ReadDeviceHoldingRegisterValue(ushort.Parse(CylinderMovementTagAddress), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusSerial, 0, slaveId);
                        }
                        else
                        {
                            txtFlow.Text = SunPowerGenMainPage.ModbusSerialPortMaster.ReadHoldingRegisters(slaveId, ushort.Parse(FlowTagAddress), 1)[0].ToString();
                            txtCurrentPressure.Text = SunPowerGenMainPage.ModbusSerialPortMaster.ReadHoldingRegisters(slaveId, ushort.Parse(PressureTagAddress), 1)[0].ToString();
                            if (!isInitialPressureSet)
                            {
                                txtInitialPressure.Text = txtCurrentPressure.Text;
                                isInitialPressureSet = true;
                            }
                            txtCylinderMovement.Text = SunPowerGenMainPage.ModbusSerialPortMaster.ReadHoldingRegisters(slaveId, ushort.Parse(CylinderMovementTagAddress), 1)[0].ToString();
                        }
                    }
                    #endregion Serial Device

                    #region AB Micrologix Ethernet
                    else if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ABMicroLogixEthernet)
                    {
                        if (Helper.MappedTagList != null)
                        {
                            foreach (var item in Helper.MappedTagList)
                            {
                                if (!string.IsNullOrEmpty(HomePage.slipStickTestInformation.Flow) && !string.IsNullOrEmpty(HomePage.slipStickTestInformation.Pressure) && !string.IsNullOrEmpty(HomePage.slipStickTestInformation.CylinderMovement))
                                {
                                    if (item.Key.ToLower().Contains("flow"))
                                        HomePage.slipStickTestInformation.Flow = Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                                    if (HomePage.slipStickTestInformation.Flow == null)
                                    {
                                        SunPowerGenMainPage.ABEthernetClient.Dispose();
                                        tbxDeviceStatus.Text = "Not Connected";
                                        tbxDeviceStatus.Foreground = Brushes.Red;
                                        StopTest();
                                        MessageBox.Show("Please, check slip stick test device connection. One of the following are reason:\n1.Device is disconnected.\n2.Invalid Tag Address.\n3.Mismatch the Tag DataType.\n4.Processor Selection Mismatch.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                                        return;

                                    }
                                    else if (item.Key.ToLower().Contains("pressure"))
                                    {
                                        if (HomePage.slipStickTestInformation.Pressure == null)
                                        {
                                            SunPowerGenMainPage.ABEthernetClient.Dispose();
                                            tbxDeviceStatus.Text = "Not Connected";
                                            tbxDeviceStatus.Foreground = Brushes.Red;
                                            StopTest();
                                            MessageBox.Show("Please, check slip stick test device connection. One of the following are reason:\n1.Device is disconnected.\n2.Invalid Tag Address.\n3.Mismatch the Tag DataType.\n4.Processor Selection Mismatch.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                                            return;

                                        }
                                        
                                        HomePage.slipStickTestInformation.Pressure = Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                                        if (!isInitialPressureSet)
                                        {
                                            txtInitialPressure.Text = txtCurrentPressure.Text;
                                            isInitialPressureSet = true;
                                        }
                                       

                                    }
                                    else if (item.Key.ToLower().Contains("cylindermovement"))
                                    {
                                        HomePage.slipStickTestInformation.CylinderMovement = Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                                        if (HomePage.slipStickTestInformation.CylinderMovement == null)
                                        {
                                            SunPowerGenMainPage.ABEthernetClient.Dispose();
                                            tbxDeviceStatus.Text = "Not Connected";
                                            tbxDeviceStatus.Foreground = Brushes.Red;
                                            StopTest();
                                            MessageBox.Show("Please, check slip stick test device connection. One of the following are reason:\n1.Device is disconnected.\n2.Invalid Tag Address.\n3.Mismatch the Tag DataType.\n4.Processor Selection Mismatch.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                                            return;

                                        }
                                    }
                                 
                                }


                                if (string.IsNullOrEmpty(txtFlow.Text) || string.IsNullOrEmpty(txtCurrentPressure.Text) || string.IsNullOrEmpty(txtCylinderMovement.Text))
                                {
                                    if (PressureSeries.Values.Count > 0)
                                    {
                                        HomePage.slipStickTestInformation.Pressure = PressureSeries.Values[PressureSeries.Values.Count - 1].ToString();

                                    }
                                    else
                                    {
                                        HomePage.slipStickTestInformation.Pressure = "0";
                                        HomePage.slipStickTestInformation.PressureAfterFirstCylinderMovement = "0";
                                        HomePage.slipStickTestInformation.CylinderMovement = "0";
                                        HomePage.slipStickTestInformation.InitialCylinderMovement = "0";
                                        HomePage.slipStickTestInformation.CylinderFirstMovement = "0";
                                        HomePage.slipStickTestInformation.Flow = "0";
                                    }
                                    //SunPowerGenMainPage.ABEthernetClient.Dispose();
                                    //tbxDeviceStatus.Text = "Not Connected";
                                    //tbxDeviceStatus.Foreground = Brushes.Red;
                                    //StopTest();
                                    //MessageBox.Show("Please, check slip stick test device connection. One of the following are reason:\n1.Device is disconnected.\n2.Invalid Tag Address.\n3.Mismatch the Tag DataType.\n4.Processor Selection Mismatch.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                                    //return;
                                }

                            }
                        }
                        //if (string.IsNullOrEmpty(txtFlow.Text) || string.IsNullOrEmpty(txtCylinderMovement.Text) || string.IsNullOrEmpty(txtCurrentPressure.Text))
                        //{
                        //    StopTest();

                        //}
                    }
                    #endregion AB Micrologix Ethernet                       

                    PressureSeries.Values.Add(double.Parse(txtCurrentPressure.Text.ToString()));
                    CylinderMovementLabels.Add(txtCylinderMovement.Text);
                    HomePage.slipStickTestInformation.InitialCylinderMovement = CylinderMovementLabels[0];
                    if (HomePage.slipStickTestInformation.InitialCylinderMovement!=txtCylinderMovement.Text&&string.IsNullOrEmpty(HomePage.slipStickTestInformation.CylinderFirstMovement))
                    {
                        HomePage.slipStickTestInformation.CylinderFirstMovement = txtCylinderMovement.Text;
                        TimerForPressureRead();
                        ElpisServer.Addlogs("All", "SPG Reporting Tool-Slip Stick Test", string.Format("First cylinder movement value:{0}", txtCylinderMovement.Text + "" + "Read timer started"), LogStatus.Information);
                    }

                    pressureXAxis.Labels = CylinderMovementLabels;                    
                    DataContext = this;

                }
                else
                {
                    StopTest();
                    ElpisServer.Addlogs("All", "SPG Reporting Tool-Slip Stick Test", string.Format("Retry Count:{0}", retryCount), LogStatus.Information);
                    MessageBox.Show("Problem in connecting device, please check it.", "SPG Reporting Tool", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

            }
            catch (Exception exe)
            {
                ElpisServer.Addlogs("All", "SPG Reporting Tool-Slip Stick Test", string.Format("Error in Read value.{0}.", exe.Message), LogStatus.Information);
                StopTest();
                //ConnectDevice();
            }
        }

        private void TimerForPressureRead()
        {   aTimer = new System.Timers.Timer();
            aTimer.Interval=PressureReadTime;
            aTimer.Elapsed += ATimer_Elapsed;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void ATimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(delegate { HomePage.slipStickTestInformation.PressureAfterFirstCylinderMovement = txtCurrentPressure.Text;
               aTimer.Stop(); });
        }

        private void ConnectDevice()
        {
            try
            {
                bool isConnected = false;
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

            catch (Exception)
            {
                tbxDeviceStatus.Text = "Not Connected";
                tbxDeviceStatus.Foreground = Brushes.Red;
            }
        }


        private void StopTest()
        {
            lblStartStop.Content = "Start Record";
            imgStartStop.Source = starticon;
            btnStartStop.ToolTip = "Start Record";
            //HomePage.slipStickTestInformation.TriggerStatus = "Stop";
            dispatcherTimer.Stop();
          
            SunPowerGenMainPage.isTestRunning = false;
            this.IsHitTestVisible = true;
            spDeviceStatus.Visibility = Visibility.Visible;
            //ElpisOPCServerMainWindow.homePage.btnReset.IsEnabled = true;
            ElpisOPCServerMainWindow.pump_Test.btnReset.IsEnabled = true;
            ElpisOPCServerMainWindow.pump_Test.btnGenerateReport.IsEnabled = true;
            ElpisOPCServerMainWindow.homePage.expanderCertificate.IsExpanded = true;
            //btnDataReadIntervalEdit.IsEnabled = true;
            DataIntervalPanal.IsEnabled = true;
            //btnDataReadIntervalSave.IsEnabled = false;
            txtTimer.Background = Brushes.White;
            txtTimerMin.Background = Brushes.White;
            txtTimerSec.Background = Brushes.White;
            txtTimerMilliSec.Background = Brushes.White;
            blbStateOFF.Visibility = Visibility.Visible;
            blbStateON.Visibility = Visibility.Hidden;
            //StopTrigger();
            if(aTimer!=null)
                aTimer.Stop();
            TriggerPLC(false);
            GenerateCSVDataFile();
        }

        private void GenerateCSVDataFile()
        {
            ReportGeneration reportGeneration = new ReportGeneration();
            ObservableCollection<LineSeries> lineSeriesList = null;
            ObservableCollection<List<string>> labelCollection = null;
            Slip_StickTestInformation testData = HomePage.slipStickTestInformation;
            if (PressureSeries != null)
            {
                LineSeries series1 = new LineSeries() { Values = PressureSeries.Values, Title = PressureSeries.Title, Stroke = PressureSeries.Stroke,LabelPoint=PressureSeries.LabelPoint, Foreground = Brushes.Black };
                // LineSeries series2 = new LineSeries() { Values = CylinderMovementSeries.Values, Title = CylinderMovementSeries.Title, Stroke = CylinderMovementSeries.Stroke, Foreground = Brushes.Black };
                lineSeriesList = new ObservableCollection<LineSeries>() { series1 };
                labelCollection = new ObservableCollection<List<string>>() { CylinderMovementLabels };
                reportGeneration.GenerateCSVFile(TestType.SlipStickTest, testData, lineSeriesList, labelCollection);
            }


        }

        private bool ValidateInputs()
        {
            bool isValid = false;
            try
            {
                isValid = true;
                if (string.IsNullOrWhiteSpace(HomePage.slipStickTestInformation.JobNumber) || string.IsNullOrEmpty(HomePage.slipStickTestInformation.JobNumber))
                    isValid = isValid && false;
                else if (HomePage.slipStickTestInformation.JobNumber.Length > 0)
                {
                    Regex expr = new Regex("^[0-9]{10}$");
                    if (!expr.IsMatch(HomePage.slipStickTestInformation.JobNumber))
                        isValid = isValid && false;
                }

                if (string.IsNullOrWhiteSpace(HomePage.slipStickTestInformation.CustomerName) || string.IsNullOrEmpty(HomePage.slipStickTestInformation.CustomerName))
                    isValid = isValid && false;

                if (HomePage.slipStickTestInformation.BoreSize <= uint.MinValue || HomePage.slipStickTestInformation.BoreSize >= uint.MaxValue)
                    isValid = isValid && false;

                if (HomePage.slipStickTestInformation.RodSize <= uint.MinValue || HomePage.slipStickTestInformation.RodSize >= uint.MaxValue)
                    isValid = isValid && false;

                if (HomePage.slipStickTestInformation.StrokeLength <= uint.MinValue || HomePage.slipStickTestInformation.StrokeLength >= uint.MaxValue)
                    isValid = isValid && false;

                if (string.IsNullOrWhiteSpace(HomePage.slipStickTestInformation.CylinderNumber) || string.IsNullOrEmpty(HomePage.slipStickTestInformation.CylinderNumber))
                    isValid = isValid && false;

            }
            catch (Exception ex)
            {
                isValid = false;
            }
            finally
            {
                string reportNumber = HomePage.slipStickTestInformation.ReportNumber;
                this.gridCeritificateInfo.DataContext = null;
                ElpisOPCServerMainWindow.homePage.gridMain.DataContext = null;
                HomePage.slipStickTestInformation.ReportNumber = reportNumber;
                this.gridCeritificateInfo.DataContext = HomePage.slipStickTestInformation;
                ElpisOPCServerMainWindow.homePage.gridMain.DataContext = HomePage.slipStickTestInformation;
            }
            if (isValid)
                ElpisOPCServerMainWindow.homePage.expanderCertificate.IsExpanded = false;
            return isValid;
        }


        //public void ResetSlipStickTestWindow()
        //{
        //    HomePage.slipStickTestInformation = null;
        //    HomePage.slipStickTestInformation = new Slip_StickTestInformation();
        //    HomePage.slipStickTestInformation.IsTestStarted = false;
        //    HomePage.slipStickTestInformation.TestName = TestType.SlipStickTest;
        //    this.gridMain.DataContext = null;
        //    this.gridMain.DataContext = HomePage.slipStickTestInformation;
        //}


        private void Slip_StickTest_Loaded(object sender, RoutedEventArgs e)
        {
            string reportNumber = HomePage.slipStickTestInformation.ReportNumber;
            this.gridCeritificateInfo.DataContext = null;
            HomePage.slipStickTestInformation.ReportNumber = reportNumber;
            HomePage.slipStickTestInformation.IsTestStarted = false;
            this.gridCeritificateInfo.DataContext = null;
            this.gridCeritificateInfo.DataContext = HomePage.slipStickTestInformation;
            HomePage.slipStickTestInformation.IsTestStarted = true;
           
            SaveLoadedData();
            UpdateDataReadInterval();
            DataContext = this;
        }

        private void SaveLoadedData()
        {
            UpdateConfigKey("SlipStickTestDataReadInterval", string.Format("{0}:{1}:{2}:{3}", txtTimer.Text, txtTimerMin.Text, txtTimerSec.Text, txtTimerMilliSec.Text));
            //btnDataReadIntervalEdit.IsEnabled = true;
           // DataIntervalPanal.IsEnabled = true;
            //btnDataReadIntervalSave.IsEnabled = false;
        }

        private void UpdateDataReadInterval()
        {
            try
            {
                Regex expr = new Regex("^[0-9]{1,2}:[0-9]{1,2}:[0-9]{1,2}:[0-9]{1,3}$");

                string cycleDuration = ConfigurationManager.AppSettings["SlipStickTestDataReadInterval"].ToString();
                if (!string.IsNullOrEmpty(cycleDuration) && !expr.IsMatch(cycleDuration))
                {
                    MessageBox.Show("Configuration file have invalid value for Cycle Duration. It set to default value 00:00:02:000");
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

        internal void GenerateReport()
        {
            try
            {
                string ReportLocation = ConfigurationManager.AppSettings["ReportLocation"].ToString();
                if (string.IsNullOrEmpty(ReportLocation) || !(Directory.Exists(ReportLocation)))
                {

                    ReportLocation = string.Format("{0}\\Reports", Directory.GetCurrentDirectory());
                    if (!Directory.Exists(ReportLocation))
                        Directory.CreateDirectory(string.Format(@"{0}", ReportLocation));
                }
                string filePath = string.Empty;
                filePath = string.Format(@"{0}\{1}\{2}", ReportLocation, HomePage.slipStickTestInformation.JobNumber, HomePage.slipStickTestInformation.CylinderNumber);
                bool isNew = false;
                if (Directory.Exists(filePath))
                {
                    string[] files = Directory.GetFiles(filePath);
                    GenerateReportMessageBox messageBox = new GenerateReportMessageBox();
                    if (files.Length >= 2)
                    {
                        // messageBox = new GenerateReportMessageBox();
                        if (btnStartStop.IsEnabled && PressureSeriesCollection != null)
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
                    else
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
                        return;
                    else if (messageBox.SelectedOpeeration == "Import")
                    {
                        //ImportAndUpdateUI(true);
                        return;
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
                        if (PressureSeries != null)
                        {
                            ReportGeneration reportGenerator = new ReportGeneration();
                            List<LineSeries> lineSeriesCollection = new List<LineSeries>() { };
                            LineSeries series1 = new LineSeries() { Values = PressureSeries.Values, Title = PressureSeries.Title, Stroke = PressureSeries.Stroke, Foreground = Brushes.Black};

                            ObservableCollection<SeriesCollection> seriesCollection = new ObservableCollection<SeriesCollection>() { new SeriesCollection() { series1 } };
                            ObservableCollection<LineSeries> lineSeriesList = new ObservableCollection<LineSeries>() { series1 };
                            ObservableCollection<List<string>> labelCollection = new ObservableCollection<List<string>>() { CylinderMovementLabels };

                            reportGenerator.GenerateCSVFile(TestType.SlipStickTest, HomePage.slipStickTestInformation, lineSeriesList, labelCollection, isNew);
                            reportGenerator.GeneratePDFReport(TestType.SlipStickTest, HomePage.slipStickTestInformation, seriesCollection, labelCollection, isNew);
                            MessageBox.Show(string.Format("Stroke Test Report are generated successfully in following location:\n{0}\\{1}\\{2}", ReportGeneration.ReportLocation, HomePage.strokeTestInfo.JobNumber, HomePage.strokeTestInfo.CylinderNumber), "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("No Recorded data found. Please, Start Recording data and then generate reports", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (IOException ioe)
                    {
                        MessageBox.Show(string.Format("{0}\nPlease close the file and generate the file.", ioe.Message), "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        //btnGenerateReport.IsEnabled = true;
                        // MessageBox.Show("no Data recorded, ", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                }
                else
                {
                    MessageBox.Show("No Recorded data found. Please, Start Recording data and then generate reports", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {

            }
           

        }

        internal void ResetSlipStickTest()
        {
            HomePage.slipStickTestInformation = null;
            HomePage.slipStickTestInformation = new Slip_StickTestInformation();
            this.gridCeritificateInfo.DataContext = null;
            this.gridCeritificateInfo.DataContext = HomePage.slipStickTestInformation;
            ElpisOPCServerMainWindow.homePage.gridMain.DataContext = null;
            ElpisOPCServerMainWindow.homePage.gridMain.DataContext = HomePage.slipStickTestInformation;
            HomePage.slipStickTestInformation.IsTestStarted = true;
            txtInitialPressure.Text = "0";
            tbxDeviceStatus.Text = "";
            txtTimer.Text = "00";
            txtTimerMin.Text = "00";
            txtTimerSec.Text = "02";
            txtTimerMilliSec.Text = "000";
            blbStateOFF.Visibility = Visibility.Visible;
            blbStateON.Visibility = Visibility.Hidden;
            if (btnStartStop.Content.ToString() == "Stop Record")
                StopTest();

            if (PressureSeriesCollection != null)
                PressureSeriesCollection.Clear();
            PressureSeriesCollection = null;
            if (CylinderMovementLabels != null)
                CylinderMovementLabels.Clear();
            CylinderMovementLabels = null;

            DataContext = this;
        }

        private void chartPressure_Loaded(object sender, RoutedEventArgs e)
        {
           
        }
        #region PLC triger old 
        private void Trigger_old()
        {
            try
            {

                #region AB Micrologix Ethernet
                if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ABMicroLogixEthernet)
                {
                    if (PlcTriggerTagAddress != null)
                    {
                        ABMicrologixEthernetDevice abDevice = SunPowerGenMainPage.DeviceObject as ABMicrologixEthernetDevice;
                        CpuType cpuType = Helper.GetABDeviceModelCPUType(abDevice.DeviceModelType);

                        foreach (var item in Helper.MappedTagList)
                        {
                            if (item.Key.ToLower().Contains("plctrigger"))
                            {
                                if (item.Value.Item1.Name.StartsWith("B3"))
                                {
                                    HomePage.slipStickTestInformation.OffSetValue = Convert.ToInt16(item.Value.Item1.Name.Split('/')[1]);
                                }

                                Helper.WriteEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                                ElpisServer.Addlogs("Report Tool", "plc slipstick values", string.Format("tag details:{0} tag datatype:{1}", item.Value.Item1.UniqueKey, item.Value.Item2), LogStatus.Information);


                            }
                        }
                        //jey commented
                        //if (PlcTriggerTagAddress == HomePage.strokeTestInfo.TriggerTestAddress) 
                        if (PlcTriggerTagAddress == HomePage.slipStickTestInformation.TriggerTestAddress)
                        {
                            HomePage.slipStickTestInformation.SlipStickTestStatus = HomePage.strokeTestInfo.TriggerStatus;
                            ElpisServer.Addlogs("Report Tool/WriteTag", "return trigger status information", string.Format("Trigger status of slip stick test:{0}", HomePage.slipStickTestInformation.SlipStickTestStatus), LogStatus.Information);
                            if (HomePage.slipStickTestInformation.SlipStickTestStatus == "ON")
                            {
                                //    txtTriggerStatus.Foreground = Brushes.DarkGreen;
                                blbStateOFF.Visibility = Visibility.Hidden;
                                blbStateON.Visibility = Visibility.Visible;

                            }
                            else
                            {
                                blbStateOFF.Visibility = Visibility.Visible;
                                blbStateON.Visibility = Visibility.Hidden;
                            }
                        }
                    }
                }
                #endregion AB Micrologix Ethernet

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
                ElpisServer.Addlogs("Report tool", "Plc trigger in Slip stick test", e.Message, LogStatus.Warning);
                
                //StopTest();
            }
        }

        private void StopTrigger_plc_not_used()
        {
            try
            {
                #region AB Micrologix Ethernet
                if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ABMicroLogixEthernet)
                {
                    if (PlcTriggerTagAddress != null)
                    {
                        ABMicrologixEthernetDevice abDevice = SunPowerGenMainPage.DeviceObject as ABMicrologixEthernetDevice;
                        CpuType cpuType = Helper.GetABDeviceModelCPUType(abDevice.DeviceModelType);

                        foreach (var item in Helper.MappedTagList)
                        {
                            if (item.Key.ToLower().Contains("plctrigger"))
                            {
                                if (item.Value.Item1.Name.StartsWith("B3"))
                                {
                                    HomePage.slipStickTestInformation.OffSetValue = Convert.ToInt16(item.Value.Item1.Name.Split('/')[1]);
                                }

                                Helper.WriteEthernetIPDeviceStop(item.Value.Item1, item.Value.Item2);
                                ElpisServer.Addlogs("Report Tool", "plc slipstick values", string.Format("tag details:{0} tag datatype:{1}", item.Value.Item1.UniqueKey, item.Value.Item2), LogStatus.Information);


                            }
                        }
                        HomePage.slipStickTestInformation.SlipStickTestStatus = HomePage.strokeTestInfo.TriggerStatus;
                        ElpisServer.Addlogs("Report Tool/WriteTag", "return trigger status information", string.Format("Trigger status of slip stick test:{0}", HomePage.slipStickTestInformation.SlipStickTestStatus), LogStatus.Information);
                        if (HomePage.slipStickTestInformation.SlipStickTestStatus == "OFF")
                        {
                            //    txtTriggerStatus.Foreground = Brushes.DarkGreen;
                            blbStateOFF.Visibility = Visibility.Visible;
                            blbStateON.Visibility = Visibility.Hidden;

                        }
                        else
                        {
                            blbStateOFF.Visibility = Visibility.Hidden;
                            blbStateON.Visibility = Visibility.Visible;
                        }
                    }
                }
                #endregion AB Micrologix Ethernet

                #region ModBusEthernet
                else if (SunPowerGenMainPage.DeviceObject.DeviceType==DeviceType.ModbusEthernet)
                {

                }
                #endregion ModBusEthernet

                #region ModBusSerial
                else if(SunPowerGenMainPage.DeviceObject.DeviceType==DeviceType.ModbusSerial)
                {

                }
                #endregion ModBusSerial
            }
            catch (Exception e)
            {
                ElpisServer.Addlogs("Report tool", "Plc trigger in Slip stick test", e.Message, LogStatus.Warning);

                //StopTest();
            }
        }
        #endregion
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
                    int offSetValue = 0;
                    LibplctagWrapper.Tag item = Helper.MappedTagList.Where(p => p.Key == "PlcTrigger").Select(p => p.Value.Item1).First();
                    if (PlcTriggerTagAddress != null)
                    {
                        if (PlcTriggerTagAddress.Split('/').Count() > 0)
                        {
                            offSetValue = Convert.ToInt16(PlcTriggerTagAddress.Split('/')[1]);
                            writeStatus = Helper.WriteEthernetIPDevice1(item, offSetValue, value);
                            ElpisServer.Addlogs("Report Tool", "PLC slip stick test Trigger PLC", string.Format("tag details:{0} tag name:{1} triggerToStart:{2}", item.UniqueKey, item.Name, value), LogStatus.Information);
                        }
                        SetTestRunning(writeStatus == 0 && value);

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
                ElpisServer.Addlogs("Report tool", "Plc trigger in slip stick test", e.Message, LogStatus.Warning);

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

            UpdateConfigKey("SlipStickTestDataReadInterval", string.Format("{0}:{1}:{2}:{3}", txtTimer.Text, txtTimerMin.Text, txtTimerSec.Text, txtTimerMilliSec.Text));
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

        //private void btnEditOperation_Click(object sender, RoutedEventArgs e)
        //{
        //    DataIntervalPanal.IsEnabled = true;
        //    btnDataReadIntervalSave.IsEnabled = true;
        //    btnDataReadIntervalEdit.IsEnabled = false;
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

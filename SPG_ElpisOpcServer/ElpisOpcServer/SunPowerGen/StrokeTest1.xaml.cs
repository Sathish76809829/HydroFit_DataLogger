using Elpis.Windows.OPC.Server;
using LiveCharts;
using LiveCharts.Wpf;
using Modbus.Device;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace ElpisOpcServer.SunPowerGen
{
    /// <summary>
    /// Interaction logic for StokeTest1.xaml
    /// </summary>
    public partial class StrokeTest1 : UserControl
    {
        public SeriesCollection FlowSeriesCollection { get; set; }
        public SeriesCollection PressureSeriesCollection { get; set; }
        public List<string> PressureLabels { get; set; }
        public List<string> FlowLabels { get; set; }
        public int NoofCyclesCompleted { get; set; }
        public Func<double, string> YFormatter { get; set; }

        LineSeries flowLineSeries { get; set; }
        LineSeries pressureLineSeries { get; set; }

        private DispatcherTimer timer { get; set; }
        private ModbusIpMaster ModbusTcpMaster { get; set; }
        private ModbusSerialMaster ModbusSerialPortMaster { get; set; }
        private ushort PressureTagAddress { get; set; }
        private ushort FlowTagAddress { get; set; }
        private StrokeTestInformation strokeTestInfo { get; set; }
        private byte slaveId { get; set; }

        public StrokeTest1()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Tick += T_Tick;
            timer.Interval = new TimeSpan(0, 0, 2);
            strokeTestInfo = new StrokeTestInformation();
            this.gridCeritificateInfo.DataContext = strokeTestInfo;
            NoofCyclesCompleted = 0;
            txtNoofCyclesCompleted.Text = NoofCyclesCompleted.ToString();
        }

        private void ConnectDevice()
        {
            try
            {
                if (SunPowerGenMainPage.DeviceObject != null)
                {
                    DeviceType deviceType = SunPowerGenMainPage.DeviceObject.DeviceType;
                    bool isCreated = false;
                    if (deviceType == DeviceType.ModbusEthernet)
                    {
                        //SunPowerGenMainPage.DeviceTcpClient = Helper.CreateTcpClient();
                        ModbusTcpMaster = Helper.CreateModbusMaster<ModbusIpMaster>(SunPowerGenMainPage.DeviceObject.DeviceType);
                        isCreated = true;

                    }
                    else if (deviceType == DeviceType.ModbusSerial)
                    {
                        Helper.CreateSerialPort();                        
                        ModbusSerialPortMaster = Helper.CreateModbusMaster<ModbusSerialMaster>(SunPowerGenMainPage.DeviceObject.DeviceType);                        
                        slaveId = ((ModbusSerialDevice)SunPowerGenMainPage.DeviceObject).SlaveId;
                        SunPowerGenMainPage.DeviceSerialPort.ReadTimeout = 500;
                        string data= ModbusSerialPortMaster.ReadHoldingRegisters(slaveId, FlowTagAddress, 1)[0].ToString();                        
                        isCreated = true;                       
                    }
                    //Update Device Status
                    if (isCreated)
                    {
                        tbxDeviceStatus.Text = "Connected";
                        tbxDeviceStatus.Foreground = Brushes.DarkGreen;
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

        private void T_Tick(object sender, EventArgs e)
        {
            try
            {
                if (ModbusTcpMaster != null || ModbusSerialPortMaster!=null)
                {
                    if (NoofCyclesCompleted < int.Parse(txtNoofCycles.Text))
                    {
                        if(SunPowerGenMainPage.DeviceObject.DeviceType==DeviceType.ModbusEthernet)
                        {
                            txtFlow.Text = ModbusTcpMaster.ReadHoldingRegisters(FlowTagAddress, 1)[0].ToString();
                            txtPressure.Text = ModbusTcpMaster.ReadHoldingRegisters(PressureTagAddress, 1)[0].ToString();
                        }
                        else if(SunPowerGenMainPage.DeviceObject.DeviceType==DeviceType.ModbusSerial)
                        {
                            txtFlow.Text = ModbusSerialPortMaster.ReadHoldingRegisters(slaveId, FlowTagAddress, 1)[0].ToString();
                            txtPressure.Text = ModbusSerialPortMaster.ReadHoldingRegisters(slaveId,PressureTagAddress, 1)[0].ToString();
                        }
                       
                        if (txtLineAPressure.Text == txtPressure.Text || txtLineBPressure.Text == txtPressure.Text)
                        {
                            NoofCyclesCompleted++;
                            txtNoofCyclesCompleted.Text = NoofCyclesCompleted.ToString();
                        }
                        flowLineSeries.Values.Add(double.Parse(txtFlow.Text.ToString()));
                        pressureLineSeries.Values.Add(double.Parse(txtPressure.Text.ToString()));
                    }
                }
                else
                {
                    StopTest();
                }

            }
            catch (Exception)
            {
                StopTest();
                //ConnectDevice();
            }
        }


        private void btnStartStop_Click(object sender, RoutedEventArgs e)
        {           
            if (btnStartStop.Content.ToString() == "Start Test")
            {
                ObservableCollection<Tag> tagsCollection = Helper.GetTagsCollection(TestType.StrokeTest,string.Empty,string.Empty);
                foreach (var item in tagsCollection)
                {
                    if (item.TagName.ToLower().Contains("pressure"))
                    {
                        PressureTagAddress = ushort.Parse(item.Address);
                    }
                    else if (item.TagName.ToLower().Contains("flow"))
                    {
                        FlowTagAddress = ushort.Parse(item.Address);
                    }
                }

                //MessageBox.Show(string.Format("Flow Address:{0},Pressure Address:{1}", FlowTagAddress, PressureTagAddress));
                //Call for Connecting to Device/PLC
                ConnectDevice();
                txtNoofCyclesCompleted.Text = "0";
                DisableInputs(true);
                spDeviceStatus.Visibility = Visibility.Visible;
                if (tbxDeviceStatus.Text== "Connected")
                {
                    flowLineSeries = new LineSeries
                    {
                        Title = "Flow",
                        Values = new ChartValues<double>(),
                        Stroke = Brushes.DarkOrange
                    };
                    pressureLineSeries = new LineSeries
                    {
                        Title = "Pressure",
                        Values = new ChartValues<double>(),
                        Stroke = Brushes.Blue
                    };



                    PressureSeriesCollection = new SeriesCollection
                {
                   pressureLineSeries
                };
                    FlowSeriesCollection = new SeriesCollection
                {
                    flowLineSeries
                };
                    chartFlow.Series = FlowSeriesCollection;
                    chartPressure.Series = PressureSeriesCollection;


                    PressureLabels = new List<string>();
                    FlowLabels = new List<string>();
                    YFormatter = value => value.ToString();
                    DataContext = this;
                    btnStartStop.Content = "Stop Test";
                    timer.Start();
                    tbxDateTine.Text = DateTime.Now.ToString();
                    lblDateTime.Visibility = Visibility.Visible;

                    btnGenerateReport.Visibility = Visibility.Hidden;
                }
                else
                {
                    btnStartStop.Content = "Start Test";
                }

            }
            else
            {
                StopTest();
            }
        }

        private void DisableInputs(bool isDisable)
        {
           if(isDisable)
            {
                txtNoofCycles.IsReadOnly = true;
                txtLineAPressure.IsReadOnly = true;
                txtLineBPressure.IsReadOnly = true;
            }
            if (isDisable)
            {
                txtNoofCycles.IsReadOnly = true;
                txtLineAPressure.IsReadOnly = true;
                txtLineBPressure.IsReadOnly = true;
            }
        }

        

        private void StopTest()
        {
            btnStartStop.Content = "Start Test";
            timer.Stop();
            btnGenerateReport.Visibility = Visibility.Visible;
            spDeviceStatus.Visibility = Visibility.Hidden;

        }





        private void Window_Closing(object sender, CancelEventArgs e)
        {

        }

        private void btnGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            //ReportGeneration reportGenerator = new ReportGeneration();
            //reportGenerator.GenerateCSVFile(TestType.StrokeTest, strokeTestInfo.JobNumber, strokeTestInfo.TestDateTime, flowLineSeries.Values, pressureLineSeries.Values, flowLineSeries.Title, pressureLineSeries.Title);
            //LineSeries series1 = new LineSeries() { Values = flowLineSeries.Values, Title = flowLineSeries.Title, Stroke = flowLineSeries.Stroke, Foreground = Brushes.Black };
            //LineSeries series2 = new LineSeries() { Values = pressureLineSeries.Values, Title = pressureLineSeries.Title, Stroke = pressureLineSeries.Stroke, Foreground = Brushes.Black };
            //List<LineSeries> seriesCollection = new List<LineSeries>() { series1, series2 };
            //List<List<string>> labelCollection = new List<List<string>>() { FlowLabels, PressureLabels };
           // reportGenerator.GeneratePDFReport(TestType.StrokeTest, strokeTestInfo, seriesCollection,labelCollection, NoofCyclesCompleted.ToString());

        }

        private void txtJobNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtJobNumber.Text != "")
            {
                txtReportNumber.Text = string.Format("SPG_{0}_R001", txtJobNumber.Text);
            }
        }

    }
}

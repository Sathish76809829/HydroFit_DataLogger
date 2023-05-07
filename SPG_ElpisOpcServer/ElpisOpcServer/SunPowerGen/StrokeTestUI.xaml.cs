using System.Windows;
using System.ComponentModel;
//using System.Windows.Controls.DataVisualization.Charting;
using System;
using System.Windows.Threading;
using System.Net.Sockets;
using Modbus.Device;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.ObjectModel;
using Elpis.Windows.OPC.Server;
using System.IO.Ports;

namespace ElpisOpcServer.SunPowerGen
{
    /// <summary>
    /// Interaction logic for StrokeTest.xaml
    /// </summary>
    public partial class StrokeTestUI : UserControl
    {
        public SeriesCollection FlowSeriesCollection { get; set; }
        public SeriesCollection PressureSeriesCollection { get; set; }
        public List<string> PressureLabels { get; set; }
        public List<string> FlowLabels { get; set; }
        public int NoofCycles { get; set; }
        public Func<double, string> YFormatter { get; set; }

        LineSeries flowLineSeries { get; set; }
        LineSeries pressureLineSeries { get; set; }

        private DispatcherTimer timer { get; set; }
        private ModbusIpMaster ModbusTcpMaster { get; set; }
        private ModbusSerialMaster ModbusSerialPortMaster { get; set; }
        private ushort PressureTagAddress { get; set; }
        private ushort FlowTagAddress { get; set; }
        private StrokeTestInformation strokeTestInfo { get; set; }

        public StrokeTestUI()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Tick += T_Tick;
            timer.Interval = new TimeSpan(0, 0, 2);
            strokeTestInfo = new StrokeTestInformation();
            this.gridCeritificateInfo.DataContext = strokeTestInfo;

        }

        private void ConnectDevice()
        {
            try
            {
                DeviceBase device = SunPowerGenMainPage.DeviceObject;
                if (device.DeviceType == DeviceType.ModbusEthernet)
                {
                   // SunPowerGenMainPage.DeviceTcpClient = Helper.CreateTcpClient();
                    if (SunPowerGenMainPage.DeviceTcpClient != null)
                    {
                        ModbusTcpMaster = ModbusIpMaster.CreateIp(SunPowerGenMainPage.DeviceTcpClient);
                        tbxDeviceStatus.Text = "Connected";
                        tbxDeviceStatus.Foreground = Brushes.DarkGreen;
                    }
                    else
                    {
                        tbxDeviceStatus.Text = "Not Connected";
                        tbxDeviceStatus.Foreground = Brushes.Red;
                    }
                }
                else if (device.DeviceType == DeviceType.ModbusSerial)
                {
                    SunPowerGenMainPage.DeviceSerialPort = Helper.CreateSerialPort();
                    ModbusSerialPortMaster = ModbusSerialMaster.CreateRtu(SunPowerGenMainPage.DeviceSerialPort);
                    tbxDeviceStatus.Text = "Connected";
                    tbxDeviceStatus.Foreground = Brushes.DarkGreen;
                }



            }
            catch (Exception)
            {
                tbxDeviceStatus.Text = "Not Connected";
                tbxDeviceStatus.Foreground = Brushes.Red;
            }
        }

        private void T_Tick(object sender, EventArgs e)
        {
            try
            {
                if (ModbusTcpMaster != null)
                {
                    if (NoofCycles <= int.Parse(txtNoofCycles.Text))
                    {
                        txtFlow.Text = ModbusTcpMaster.ReadHoldingRegisters(FlowTagAddress, 1)[0].ToString();
                        txtPressure.Text = ModbusTcpMaster.ReadHoldingRegisters(FlowTagAddress, 1)[0].ToString();
                        if (txtLineAPressure.Text == txtPressure.Text || txtLineBPressure.Text == txtPressure.Text)
                            NoofCycles++;
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
                ConnectDevice();                
            }
        }


        private void btnStartStop_Click(object sender, RoutedEventArgs e)
        {
            if (btnStartStop.Content.ToString() == "Start Record")
            {                
                ObservableCollection<Tag> tagsCollection = Helper.GetTagsCollection(TestType.StrokeTest,string.Empty,string.Empty);
                foreach (var item in tagsCollection)
                {
                    if (item.TagName.ToLower() == "pressure")
                    {
                        PressureTagAddress = ushort.Parse(item.Address);
                    }
                    else if (item.TagName.ToLower() == "flow")
                    {
                        FlowTagAddress = ushort.Parse(item.Address);
                    }

                }

                //Call for Connecting to Device/PLC
                ConnectDevice();
                spDeviceStatus.Visibility = Visibility.Visible;
                if (SunPowerGenMainPage.DeviceTcpClient != null && ModbusTcpMaster != null)
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
                        Stroke = Brushes.LightBlue             
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
                    btnStartStop.Content = "Start Record";
                }

            }
            else
            {
                //btnStartStop.Visibility = Visibility.Hidden;
                StopTest();
            }
        }

        private void StopTest()
        {
            //for (int i = 0; i < chartFlow.Series.Count; i++)
            //{
            //    chartFlow.Series.RemoveAt(i);
            //}
            //for (int i = 0; i < chartPressure.Series.Count; i++)
            //{
            //    chartPressure.Series.RemoveAt(0);
            //}

            btnStartStop.Content = "Start Record";
            timer.Stop();
            btnGenerateReport.Visibility = Visibility.Visible;
            spDeviceStatus.Visibility = Visibility.Hidden;
            // btnStartStop.Visibility = Visibility.Hidden;
            //var currentChart = new LiveCharts.Wpf.CartesianChart
            //{
            //    DisableAnimations = true,
            //    Width = 600,
            //    Height = 200,
            //    AxisX = new AxesCollection() { new Axis { Title = "Time" } },
            //    AxisY = new AxesCollection() { new Axis { Title = "Pressure/Flow" } },
            //    LegendLocation = LegendLocation.Right,
            //    Series = new SeriesCollection
            //    {
            //        flowLineSeries,pressureLineSeries
            //    }
            //};

            //var viewbox = new Viewbox();
            //viewbox.Child = currentChart;
            //viewbox.Measure(currentChart.RenderSize);
            //viewbox.Arrange(new Rect(new Point(0, 0), currentChart.RenderSize));
            //currentChart.Update(true, true); //force chart redraw
            //viewbox.UpdateLayout();
            ////png file was created at the root directory.
            //Helper.SaveToPng(currentChart, "chart.png");
        }





        private void Window_Closing(object sender, CancelEventArgs e)
        {

        }

        private void btnGenerateReport_Click(object sender, RoutedEventArgs e)
        {            
            //ReportGeneration reportGenerator = new ReportGeneration();
            //reportGenerator.GenerateCSVFile(TestType.StrokeTest, strokeTestInfo.JobNumber, strokeTestInfo.TestDateTime, flowLineSeries.Values, pressureLineSeries.Values,flowLineSeries.Title,pressureLineSeries.Title);
            //LineSeries series1 = new LineSeries() { Values = flowLineSeries.Values, Title = flowLineSeries.Title, Stroke = flowLineSeries.Stroke };
            //LineSeries series2 = new LineSeries() { Values = pressureLineSeries.Values, Title = pressureLineSeries.Title, Stroke = pressureLineSeries.Stroke };
            //List<LineSeries> seriesCollection = new List<LineSeries>() { series1, series2 };
            //List<List<string>> labelCollection = new List<List<string>>() { FlowLabels, PressureLabels };
          //reportGenerator.GeneratePDFReport(TestType.StrokeTest, strokeTestInfo, seriesCollection,labelCollection);

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

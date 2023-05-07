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
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;

namespace ElpisOpcServer.SunPowerGen
{
    /// <summary>
    /// Interaction logic for StrokeTest.xaml
    /// </summary>
    public partial class StrokeTest : Window
    {

        public SeriesCollection FlowSeriesCollection { get; set; }
        public SeriesCollection PressureSeriesCollection { get; set; }
        public List<string> PressureLabels { get; set; }
        public List<string> FlowLabels { get; set; }
        public int NoofCycles { get; set; }
        public Func<double, string> YFormatter { get; set; }

        LineSeries flowLineSeries = new LineSeries
        {
            Title = "Flow",
            Values = new ChartValues<double>(),
            Stroke = Brushes.DarkOrange
        };
        LineSeries pressureLineSeriesA = new LineSeries
        {
            Title = "Pressure LineA",
            Values = new ChartValues<double>()
        };

        LineSeries pressureLineSeriesB = new LineSeries
        {
            Title = "Pressure LineB",
            Values = new ChartValues<double>()
        };
        private DispatcherTimer timer { get; set; }
        private TcpClient client { get; set; }
        private ModbusIpMaster master { get; set; }

        public StrokeTest()
        {
            InitializeComponent();

            PressureSeriesCollection = new SeriesCollection
            {
                pressureLineSeriesA ,
                pressureLineSeriesB
            };
            FlowSeriesCollection = new SeriesCollection
            {
                flowLineSeries
            };

            DataContext = this;
            PressureLabels = new List<string>();
            FlowLabels = new List<string>();
            YFormatter = value => value.ToString();
            timer = new DispatcherTimer();
            timer.Tick += T_Tick;
            timer.Interval = new TimeSpan(0, 0, 5);
            NoofCycles = 0;

            //Call for Connecting to Device/PLC
            ConnectDevice();

        }

        private void ConnectDevice()
        {
            try
            {
                client = Helper.CreateTcpClient();
                //client = new TcpClient();
                //System.Net.IPAddress ipAddr = System.Net.IPAddress.Parse("192.168.24.1");
                //client.Connect(ipAddr, 550);
                master = ModbusIpMaster.CreateIp(client);
            }
            catch (Exception)
            {
                MessageBox.Show("Device is not connected", "Elpis Report Generation", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void T_Tick(object sender, EventArgs e)
        {
            try
            {
                if (master != null)
                {
                    ushort[] readValues = master.ReadHoldingRegisters(5, 5);
                    PressureLabels.Add(DateTime.Now.TimeOfDay.ToString());
                    FlowLabels.Add(DateTime.Now.TimeOfDay.ToString());
                    PressureLabels.Add(DateTime.Now.TimeOfDay.ToString());
                    if (readValues.Length == 5)
                    {
                        txtFlow.Text = readValues[0].ToString();
                        txtPressure.Text = readValues[1].ToString();
                        txtNoofCycles.Text = (++NoofCycles).ToString(); ;
                        txtMaximumAllowablePressure.Text = readValues[3].ToString();
                    }

                    flowLineSeries.Values.Add(double.Parse(readValues[0].ToString()));
                    pressureLineSeriesA.Values.Add(double.Parse(readValues[1].ToString()));
                    pressureLineSeriesB.Values.Add(double.Parse(readValues[2].ToString()));
                }
                else
                {
                    ConnectDevice();
                }

            }
            catch (Exception ex)
            {
                ConnectDevice();
                MessageBox.Show("Device is not connected", "Elpis Report Generation", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (btnStartStop.Content.ToString() == "Start Test")
            {
                btnStartStop.Content = "Stop Test";
                timer.Start();
                tbxDateTine.Text = DateTime.Now.ToString();
                lblDateTime.Visibility = Visibility.Visible;
                btnGenerateReport.Visibility = Visibility.Hidden;
            }
            else
            {
                btnStartStop.Content = "Start Test";
                timer.Stop();
                btnGenerateReport.Visibility = Visibility.Visible;
                btnStartStop.Visibility = Visibility.Hidden;
                var currentChart = new LiveCharts.Wpf.CartesianChart
                {
                    DisableAnimations = true,
                    Width = 600,
                    Height = 200,
                    AxisX = new AxesCollection() { new Axis { Title = "Time" } },
                    AxisY = new AxesCollection() { new Axis { Title = "Pressure/Flow" } },
                    LegendLocation = LegendLocation.Right,
                    Series = new SeriesCollection
                {
                    flowLineSeries,pressureLineSeriesA, pressureLineSeriesB
                }
                };

                var viewbox = new Viewbox();
                viewbox.Child = currentChart;
                viewbox.Measure(currentChart.RenderSize);
                viewbox.Arrange(new Rect(new Point(0, 0), currentChart.RenderSize));
                currentChart.Update(true, true); //force chart redraw
                viewbox.UpdateLayout();
                SaveToPng(currentChart, "chart.png");

                //png file was created at the root directory.
            }
        }



        private void SaveToPng(FrameworkElement visual, string fileName)
        {
            var encoder = new PngBitmapEncoder();
            EncodeVisual(visual, fileName, encoder);
        }

        private static void EncodeVisual(FrameworkElement visual, string fileName, BitmapEncoder encoder)
        {
            var bitmap = new RenderTargetBitmap((int)visual.ActualWidth, (int)visual.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            var frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);
            using (var stream = File.Create(fileName)) encoder.Save(stream);
        }


        private void Window_Closing(object sender, CancelEventArgs e)
        {

        }

        private void btnGenerateReport_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

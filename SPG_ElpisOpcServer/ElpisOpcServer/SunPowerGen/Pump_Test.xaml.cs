using Elpis.Windows.OPC.Server;
using ElpisOpcServer.SocketService.SocketServer;
using ElpisOpcServer.SunPowerGen;
using LiveCharts;
using LiveCharts.Wpf;
using Modbus;
using OPCEngine.View_Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ElpisOpcServer.SunPowerGen
{
    /// <summary>
    /// Interaction logic for Pump_Test.xaml
    /// </summary>
    public partial class Pump_Test : UserControl
    {
        BitmapImage starticon = new BitmapImage();
        BitmapImage stopicon = new BitmapImage();
        private DispatcherTimer dispatcherTimer;
        DispatcherTimer timer;
        private bool isInitialPressureSet;
        private int startAddressPos;
        private int retryCount;
        private string connectorName;
        private string deviceName;
        private List<Tag> selectedTagNames = new List<Tag>();

        public static  List<Tuple<string, Tag>> seletedItems;
        private List<string> allTags = new List<string>();
        private int dataCounts;
       // public TcpSocketDevice tcpSocketDevice { get; set; }
        public  List<string> Chart1xAxisParaValue { get; set; }
        public List<string> Chart1xAxisTimeValue { get; set; }
        public List<string> Chart2xAxisParaValue { get; set; }
        public List<string> Chart2xAxisTimeValue { get; set; }
        public Func<double, string> YFormatter { get; set; }
        private byte slaveId { get; set; }
        public SeriesCollection chart1SeriesCollections { get; private set; }
        public SeriesCollection chart2SeriesCollections { get; private set; }

        public static AxesCollection Chart1YAxisCollection = new AxesCollection();
        public static AxesCollection Chart1XAxisCollection = new AxesCollection();

        public static AxesCollection Chart2YAxisCollection = new AxesCollection();
        public static AxesCollection Chart2XAxisCollection = new AxesCollection();
        public Axis YAxis { get; private set; }
        public Axis XAxisPara { get; private set; }
        public Axis XAxisTime { get; private set; }
        Brush[] graphStrokes = new Brush[] { Brushes.DarkOrange, Brushes.DarkGreen, Brushes.DarkBlue, Brushes.Brown, Brushes.Red, Brushes.DarkCyan, Brushes.DarkMagenta, Brushes.DarkOliveGreen, Brushes.DarkOrange, Brushes.DarkSalmon };
        //private string formulaPara1;
        //private string formulaPara2;
        Stopwatch stopwatch;
        private Dictionary<string, List<string>> tempTable = new Dictionary<string, List<string>>();
        public Pump_Test()
        {

            InitializeComponent();
            stopwatch = new Stopwatch();
            DataContext = this;
            if (HomePage.PumpTestInformation == null)
                HomePage.PumpTestInformation = new PumpTestInformation();
            HomePage.PumpTestInformation.TestName = TestType.PumpTest;
            HomePage.PumpTestInformation.TableData.Add("Time", new Dictionary<string, string>());
            HomePage.PumpTestInformation.TableParameterList.Add("Time");
            //HomePage.PumpTestInformation.TableData = new Dictionary<string, List<string>>();
            connectorName = HomePage.SelectedConnector;
            deviceName = HomePage.SelectedDevice;
            TagsCollection = Helper.GetTagsCollection(TestType.PumpTest, connectorName, deviceName);
            HomePage.PumpTestInformation.TagInformation = TagsCollection;
            if (TagsCollection != null)
            {
                foreach (var item in TagsCollection)
                {
                    allTags.Add(item.TagName + " (" + item.Units + ")");
                }
                // lstAllTags.ItemsSource = allTags;
            }

            this.gridCeritificateInfo.DataContext = HomePage.PumpTestInformation;

            //chartPressure.AxisY[0].LabelFormatter = value => value.ToString("N2");
            PressureReadTime = double.Parse(ConfigurationManager.AppSettings["PressureReadInterval"].ToString());
            starticon.BeginInit();
            starticon.UriSource = new Uri("/ElpisOpcServer;component/Images/starticon.png", UriKind.Relative);
            starticon.EndInit();
            stopicon.BeginInit();
            stopicon.UriSource = new Uri("/ElpisOpcServer;component/Images/stopicon.png", UriKind.Relative);
            stopicon.EndInit();
           
            dispatcherTimer = new DispatcherTimer();
            //dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            HomePage.DeviceObject = new DeviceBase();
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(HomePage.DeviceObject.SamplingRate);
            dispatcherTimer.Tick += DispatcherTimer_Tick;

            //Hydrofit  samplingrate timer
            #region  Hydrofit test duration
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMinutes(3);
            timer.Tick += timer_Tick;
            //timer.Start();
            #endregion
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            lblTimer.Content = DateTime.Now.ToLongTimeString();
            StopTest();
        }
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            ReadDeviceData();
        }

        public double PressureReadTime { get; }
        public ObservableCollection<Tag> TagsCollection { get; private set; }
        public LineSeries PumpSeries { get; private set; }
        public AxesCollection AxisYCollection { get; }
        public List<string> Chart1YaxisParaList = new List<string>();
        public List<string> Chart2YaxisParaList = new List<string>();
         
        private void btnStartStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                if (lblStartStop.Content.ToString() == "Start Record")
                {
                    bool isValid = ValidateInputs();

                    //bool isXAisSelect = (rbtn_blockA.IsChecked == true||rbtn_blockB.IsChecked==true && chart1cmbXAxis.SelectedItem == null) ? false : true;
                    //bool isYAisSelect = rbtn_blockA.IsChecked == true||rbtn_blockB.IsChecked==true ? (Chart1YaxisParaList.Count > 0 ? true : false) : true;
                    // bool isformualParaSelect = (chkFormula.IsChecked == true && cmbFirstPara.SelectedItem == null && cmbSecondPara.SelectedItem == null) ? false : true;
                    // bool isformualParaSelect = true;

                    if (isValid)
                    {
                        if (chart1cmbXAxis.SelectedItem != null && chart2cmbXAxis.SelectedItem != null)
                        {
                            if (Chart1YaxisParaList.Count > 0 && Chart2YaxisParaList.Count > 0)
                            {
                                //if (isformualParaSelect)
                                //{
                                if (selectedTagNames.Count > 0)
                                {
                                    //if (HomePage.PumpTestInformation.TableParameterList.Count >= 2)
                                    //{
                                    connectorName = HomePage.SelectedConnector;
                                    deviceName = HomePage.SelectedDevice;

                                    if (chart1SeriesCollections == null && chart2SeriesCollections == null && isValid)
                                    {

                                        if (!string.IsNullOrEmpty(connectorName) && !string.IsNullOrEmpty(deviceName))
                                        {
                                            this.IsHitTestVisible = false;
                                            this.Cursor = Cursors.Wait;
                                            lblStartStop.Content = "Stop Record";
                                            imgStartStop.Source = stopicon;
                                            btnStartStop.ToolTip = "Stop Record";
                                            tbxDeviceStatus.Text = "";

                                            seletedItems = new List<Tuple<string, Tag>>();
                                            Chart1xAxisParaValue = new List<string>();
                                            Chart1xAxisTimeValue = new List<string>();
                                            Chart2xAxisParaValue = new List<string>();
                                            Chart2xAxisTimeValue = new List<string>();
                                            List<Tuple<string, bool>> tagStatus = new List<Tuple<string, bool>>();
                                            if (TagsCollection != null && TagsCollection.Count > 0)
                                            {
                                                foreach (var tag in TagsCollection)
                                                {
                                                    foreach (var seletedItem in selectedTagNames)
                                                    {
                                                        if (seletedItem.TagName.ToLower().Contains(tag.TagName.ToLower()))
                                                        {
                                                            seletedItems.Add(new Tuple<string, Tag>(seletedItem.TagName, tag));
                                                        }
                                                    }
                                                    
                                                }

                                                if (seletedItems != null)
                                                {
                                                    ConnectDevice();
                                                    
                                                    this.IsHitTestVisible = true;
                                                    this.IsHitTestVisible = true;

                                                    if (tbxDeviceStatus.Text == "Connected")
                                                    {
                                                        ElpisOPCServerMainWindow.homePage.PumpTestDisableInputs(true);
                                                        //ElpisOPCServerMainWindow.homePage.btnReset.IsEnabled = false;
                                                        //ElpisOPCServerMainWindow.pump_Test.btnReset.IsEnabled = false;

                                                        btnReset.IsEnabled = false;
                                                        btnGenerateReport.IsEnabled = false;
                                                        //ElpisOPCServerMainWindow.pump_Test.btnReset.IsEnabled = false;
                                                        //ElpisOPCServerMainWindow.pump_Test.btnGenerateReport.IsEnabled = false;

                                                        ElpisOPCServerMainWindow.homePage.ReportTab.IsEnabled = true;
                                                        ElpisOPCServerMainWindow.homePage.txtFilePath.IsEnabled = false;
                                                        //ElpisOPCServerMainWindow.homePage.PumpExpanderCertificate.IsExpanded = true;

                                                        chart1SeriesCollections = new SeriesCollection();
                                                        chart2SeriesCollections = new SeriesCollection();
                                                        chart1.AxisY.Clear();
                                                        chart2.AxisY.Clear();
                                                        
                                                        var chart1Xaxis = seletedItems.Find(item => item.Item1 == chart1cmbXAxis.SelectedItem.ToString());
                                                        var chart2Xaxis = seletedItems.Find(item => item.Item1 == chart2cmbXAxis.SelectedItem.ToString());
                                                        //plot chart 1 xAxis
                                                        try
                                                        {
                                                            if (chart1Xaxis != null)
                                                            {
                                                                chart1.AxisX.Clear();
                                                                Chart1XAxisCollection = new AxesCollection();
                                                                var XAxisPara = new Axis { Title = string.Format(chart1Xaxis.Item1 + "( Time )") };
                                                                XAxisPara.LabelFormatter = value => (value).ToString();
                                                                XAxisPara.Foreground = Brushes.Black;
                                                                Chart1XAxisCollection.Add(XAxisPara);
                                                                chart1.AxisX = Chart1XAxisCollection;
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {

                                                            throw;
                                                        }
                                                        

                                                        //plot chart2 xaxis
                                                         if (chart2Xaxis != null)
                                                         {
                                                            chart2.AxisX.Clear();
                                                            Chart2XAxisCollection = new AxesCollection();
                                                            var XAxisPara = new Axis { Title = string.Format(chart2Xaxis.Item1 + "( Time )") };
                                                            XAxisPara.LabelFormatter = value => (value).ToString();
                                                            XAxisPara.Foreground = Brushes.Black;
                                                            Chart2XAxisCollection.Add(XAxisPara);
                                                            chart2.AxisX = Chart2XAxisCollection;
                                                         }

                                                        //plot yaxis of chart1
                                                        if (Chart1YaxisParaList != null)
                                                        {
                                                            foreach (var item in Chart1YaxisParaList)
                                                            {
                                                                var yAxis = seletedItems.Find(q => q.Item1 == item);
                                                                if (yAxis != null)
                                                                {

                                                                    var PumpSeries = new LineSeries
                                                                    {
                                                                        Title = yAxis.Item1,
                                                                        Values = new ChartValues<double>(),
                                                                        PointGeometrySize = 5,
                                                                        //ScalesYAt = chart1SeriesCollections.Count,
                                                                        ScalesXAt = 0,
                                                                        Fill = Brushes.Transparent
                                                                    };
                                                                    //YAxis = new Axis { Title = tupleItem.Item1, MinValue = tupleItem.Item2.MinValue, MaxValue = tupleItem.Item2.MaxValue, Separator = new LiveCharts.Wpf.Separator() { Step = tupleItem.Item2.MaxValue / 10 } };
                                                                    var YAxis = new Axis { Title = yAxis.Item1, MinValue = yAxis.Item2.MinValue, MaxValue = yAxis.Item2.MaxValue, Separator = new LiveCharts.Wpf.Separator() { Step = yAxis.Item2.Divisions } };
                                                                    //YAxis = new Axis { Title = tupleItem.Item1};
                                                                    YAxis.LabelFormatter = value => (value).ToString("F2");
                                                                    YAxis.Foreground = Brushes.Black;
                                                                    Chart1YAxisCollection.Add(YAxis);
                                                                    chart1.AxisY = Chart1YAxisCollection;

                                                                    chart1SeriesCollections.Add(PumpSeries);
                                                                }
                                                            }


                                                        }


                                                        ////plot yaxis of chart2
                                                        if (Chart2YaxisParaList != null)
                                                        {
                                                            Chart2YAxisCollection = new AxesCollection();
                                                            foreach (var item in Chart2YaxisParaList)
                                                            {
                                                                var yAxis = seletedItems.Find(q => q.Item1 == item);
                                                                if (yAxis != null)
                                                                {
                                                                    Chart2YAxisCollection.Clear();
                                                                    var PumpSeries = new LineSeries
                                                                    {
                                                                        Title = yAxis.Item1,
                                                                        Values = new ChartValues<double>(),
                                                                        PointGeometrySize = 5,
                                                                        //ScalesYAt = chart2SeriesCollections.Count,
                                                                        ScalesXAt = 0,
                                                                        Fill = Brushes.Transparent
                                                                    };
                                                                    //YAxis = new Axis { Title = tupleItem.Item1, MinValue = tupleItem.Item2.MinValue, MaxValue = tupleItem.Item2.MaxValue, Separator = new LiveCharts.Wpf.Separator() { Step = tupleItem.Item2.MaxValue / 10 } };
                                                                    var YAxis = new Axis { Title = yAxis.Item1, MinValue = yAxis.Item2.MinValue, MaxValue = yAxis.Item2.MaxValue, Separator = new LiveCharts.Wpf.Separator() { Step = yAxis.Item2.Divisions } };
                                                                    //YAxis = new Axis { Title = tupleItem.Item1};
                                                                    YAxis.LabelFormatter = value => (value).ToString("F2");
                                                                    YAxis.Foreground = Brushes.Black;
                                                                    Chart2YAxisCollection.Add(YAxis);
                                                                    chart2.AxisY = Chart2YAxisCollection;

                                                                    chart2SeriesCollections.Add(PumpSeries);
                                                                }
                                                            }

                                                        }

                                                        #region old code
                                                        //foreach (var tupleItem in seletedItems)
                                                        //{
                                                        //    #region chart1 graph plot
                                                        //    //if (chart1chkXaxis.IsChecked == true && tupleItem.Item1 == chart1cmbXAxis.SelectedItem.ToString())
                                                        //    /*rbtn_blockA.IsChecked == true||rbtn_blockB.IsChecked==true &&*/
                                                        //    if (tupleItem.Item1 == chart1cmbXAxis.SelectedItem.ToString())
                                                        //    {
                                                        //        chart1.AxisX.Clear();
                                                        //        XAxisPara = new Axis { Title = string.Format(tupleItem.Item1 + "( Time )") };
                                                        //        XAxisPara.LabelFormatter = value => (value).ToString();
                                                        //        XAxisPara.Foreground = Brushes.Black;
                                                        //        XAxisCollection.Add(XAxisPara);
                                                        //        chart1.AxisX = XAxisCollection;

                                                        //        //XAxisTime = new Axis { Title = "Time" };
                                                        //        //XAxisPara.LabelFormatter = value =>value.ToString("HH: mm:ss tt");
                                                        //        //XAxisCollection.Add(XAxisTime);

                                                        //        //pumpTestChart.AxisX = XAxisCollection;


                                                        //    }
                                                        //    // TODO HYDROFIT
                                                        //    //if(tupleItem.Item1 != chart1cmbXAxis.SelectedItem.ToString())
                                                        //    ////if (chart1chkXaxis.IsChecked == false)
                                                        //    //{
                                                        //    //    chart1.AxisX.Clear();
                                                        //    //    XAxisTime = new Axis { Title = "Time" };
                                                        //    //    XAxisTime.Foreground = Brushes.Black;
                                                        //    //    chart1.AxisX.Add(XAxisTime);

                                                        //    //}
                                                        //    //if(tupleItem.Item1 != chart1cmbXAxis.SelectedItem.ToString())
                                                        //    //if (chart1chkXaxis.IsChecked == true)
                                                        //    if (chart1cmbXAxis.SelectedItem.ToString() != null)
                                                        //    {
                                                        //        foreach (var item in Chart1YaxisParaList)
                                                        //        {
                                                        //            if (item == tupleItem.Item1)
                                                        //            {
                                                        //                PumpSeries = new LineSeries
                                                        //                {
                                                        //                    Title = tupleItem.Item1,
                                                        //                    Values = new ChartValues<double>(),
                                                        //                    PointGeometrySize = 5,
                                                        //                    ScalesYAt = chart1SeriesCollections.Count,
                                                        //                    ScalesXAt = 0,
                                                        //                    Fill = Brushes.Transparent
                                                        //                };
                                                        //                //YAxis = new Axis { Title = tupleItem.Item1, MinValue = tupleItem.Item2.MinValue, MaxValue = tupleItem.Item2.MaxValue, Separator = new LiveCharts.Wpf.Separator() { Step = tupleItem.Item2.MaxValue / 10 } };
                                                        //                YAxis = new Axis { Title = tupleItem.Item1, MinValue = tupleItem.Item2.MinValue, MaxValue = tupleItem.Item2.MaxValue, Separator = new LiveCharts.Wpf.Separator() { Step = tupleItem.Item2.Divisions } };
                                                        //                //YAxis = new Axis { Title = tupleItem.Item1};
                                                        //                YAxis.LabelFormatter = value => (value).ToString("F2");
                                                        //                YAxis.Foreground = Brushes.Black;
                                                        //                YAxisCollection.Add(YAxis);
                                                        //                chart1.AxisY = YAxisCollection;

                                                        //                chart1SeriesCollections.Add(PumpSeries);
                                                        //            }
                                                        //        }

                                                        //    }
                                                        //    #endregion chart1 grph plot

                                                        //    #region chart2 grph plot

                                                        //    if (chart2cmbXAxis.SelectedItem.ToString() != null)
                                                        //    {
                                                        //        foreach (var item in Chart2YaxisParaList)
                                                        //        {
                                                        //            if (item == tupleItem.Item1)
                                                        //            {
                                                        //                PumpSeries = new LineSeries
                                                        //                {
                                                        //                    Title = tupleItem.Item1,
                                                        //                    Values = new ChartValues<double>(),
                                                        //                    PointGeometrySize = 5,
                                                        //                    ScalesYAt = chart2SeriesCollections.Count,
                                                        //                    ScalesXAt = 0,
                                                        //                    Fill = Brushes.Transparent


                                                        //                };
                                                        //                //YAxis = new Axis { Title = tupleItem.Item1, MinValue = tupleItem.Item2.MinValue, MaxValue = tupleItem.Item2.MaxValue, Separator = new LiveCharts.Wpf.Separator() { Step = tupleItem.Item2.MaxValue / 10 } };
                                                        //                YAxis = new Axis { Title = tupleItem.Item1, MinValue = tupleItem.Item2.MinValue, MaxValue = tupleItem.Item2.MaxValue, Separator = new LiveCharts.Wpf.Separator() { Step = tupleItem.Item2.Divisions } };
                                                        //                //YAxis = new Axis { Title = tupleItem.Item1};
                                                        //                YAxis.LabelFormatter = value => (value).ToString("F2");
                                                        //                YAxis.Foreground = Brushes.Black;
                                                        //                YAxisCollection.Add(YAxis);
                                                        //                chart2.AxisY = YAxisCollection;

                                                        //                chart2SeriesCollections.Add(PumpSeries);
                                                        //            }
                                                        //        }

                                                        //    }
                                                        //    #endregion chart2 grph plot
                                                        //    // TODO HYDROFIT
                                                        //    // else if (chart1chkXaxis.IsChecked == false)
                                                        //    //{
                                                        //    //    PumpSeries = new LineSeries
                                                        //    //    {
                                                        //    //        Title = tupleItem.Item1,
                                                        //    //        Values = new ChartValues<double>(),
                                                        //    //        PointGeometrySize = 5,
                                                        //    //        ScalesYAt = SeriesCollections.Count,
                                                        //    //        ScalesXAt = 0,
                                                        //    //        Fill = Brushes.Transparent

                                                        //    //    };
                                                        //    //    //YAxis = new Axis { Title = tupleItem.Item1, MinValue = tupleItem.Item2.MinValue, MaxValue = tupleItem.Item2.MaxValue, Separator = new LiveCharts.Wpf.Separator() { Step = tupleItem.Item2.MaxValue / 10 } };
                                                        //    //    YAxis = new Axis { Title = tupleItem.Item1, MinValue = tupleItem.Item2.MinValue, MaxValue = tupleItem.Item2.MaxValue, Separator = new LiveCharts.Wpf.Separator() { Step = tupleItem.Item2.Divisions } };
                                                        //    //    // YAxis = new Axis { Title = tupleItem.Item1 };
                                                        //    //    YAxis.LabelFormatter = value => (value).ToString("F2");
                                                        //    //    YAxis.Foreground = Brushes.Black;
                                                        //    //    YAxisCollection.Add(YAxis);
                                                        //    //    chart1.AxisY = YAxisCollection;
                                                        //    //    SeriesCollections.Add(PumpSeries);
                                                        //    //}




                                                        //}

                                                        //if (isformualParaSelect)
                                                        //{
                                                        //PumpSeries = new LineSeries
                                                        //{
                                                        //    Title = "Computed Value",
                                                        //    Values = new ChartValues<double>(),
                                                        //    PointGeometrySize = 5,
                                                        //    ScalesYAt = SeriesCollections.Count,
                                                        //    ScalesXAt = 0,

                                                        //};
                                                        //YAxis = new Axis { Title = "Computed Value" };
                                                        //YAxis.LabelFormatter = value => (value + 100.00).ToString("N2");
                                                        //YAxisCollection.Add(YAxis);
                                                        //pumpTestChart.AxisY = YAxisCollection;
                                                        //SeriesCollections.Add(PumpSeries);
                                                        //}
                                                        #endregion
                                                        chart1.Series = chart1SeriesCollections;
                                                        //TO DO FOR HYDROFIT
                                                        chart2.Series = chart2SeriesCollections;
                                                        chart1.LegendLocation = LiveCharts.LegendLocation.Top;
                                                        chart2.LegendLocation = LiveCharts.LegendLocation.Top;
                                                        SunPowerGenMainPage.isTestRunning = true;
                                                        TriggerPLC(true);
                                                        stopwatch.Start();
                                                        dispatcherTimer.Start();
                                                        HomePage.PumpTestInformation.TestDateTime = DateTime.Now.ToString();
                                                        dataCounts = 0;
                                                        blbStateOFF.Visibility = Visibility.Hidden;
                                                        blbStateON.Visibility = Visibility.Visible;
                                                        //ElpisOPCServerMainWindow.homePage.PumpExpanderCertificate.IsExpanded = false;
                                                        configGrid.IsEnabled = false;
                                                        // SelectionGrid.IsEnabled = false;
                                                        //  ReadDeviceData();

                                                       // string str = Helper.formatConfigData(seletedItems);
                                                       // Helper.StartStopCommand(str, Helper.client);
                                                        #region Tcpsocketdevice-implementation
                                                        if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.TcpSocketDevice)
                                                        {
                                                            
                                                            if (TcpServer.ServerResponse.Contains("getsignallist"))
                                                            {
                                                                string str = Helper.formatConfigData(seletedItems);
                                                                Helper.SendConfigPacketToServer(str, TcpServer.client);
                                                            }
                                                            if(TcpServer.ServerResponse.Contains("200400000001"))
                                                            {
                                                                StartStopcommand(true/* ref resp*/);
                                                            }



                                                            dispatcherTimer.Start();
                                                            timer.Start();

                                                           
                                                            ReadDeviceData();
                                                        }
                                                        
                                                        #endregion Tcpsocketdevice-implementation

                                                    }
                                                    else
                                                    {
                                                        lblStartStop.Content = "Start Record";
                                                        imgStartStop.Source = starticon;
                                                        btnStartStop.ToolTip = "Start Record";
                                                        this.IsHitTestVisible = true;
                                                        blbStateOFF.Visibility = Visibility.Visible;
                                                        blbStateON.Visibility = Visibility.Hidden;
                                                        //ElpisOPCServerMainWindow.homePage.PumpExpanderCertificate.IsExpanded = true;
                                                        configGrid.IsEnabled = true;
                                                    }
                                                }
                                                else
                                                {
                                                    StopTest();
                                                    MessageBox.Show("Configuration file having the invalid Tag Names", "SPG Reporting Tool", MessageBoxButton.OK, MessageBoxImage.Warning);

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
                                    //}
                                    //else
                                    //{
                                    //    MessageBox.Show("Please select Minimum 2 Parameters for table.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    //}
                                }
                                else
                                {
                                    MessageBoxResult messageOption = MessageBox.Show("Please select parameters.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Question);
                                }
                                //}
                                //else
                                //{
                                //    MessageBoxResult messageOption = MessageBox.Show("Please select formula parameters.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Question);
                                //}
                            }
                            else
                            {
                                MessageBox.Show("Please select minimum one parameter for Y Axis .", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Question);
                            }
                        }
                        else
                        {
                            MessageBoxResult messageOption = MessageBox.Show("Please select x Axis parameter.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Question);
                        }
                    }
                    // TO DO Hydrofit Code By Sathish
                    else
                    {
                        MessageBox.Show("Please Fill Configuration Details.");
                    }
                }
                else
                {
                    StopTest();

                }
                this.Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Arrow;
                ElpisServer.Addlogs("Report Tool", "Start Button-Pump Test", ex.Message, LogStatus.Error);
                StopTest();
            }
        }

        internal void ResetPumpTest()
        {
            ElpisOPCServerMainWindow.homePage.PumpTestDisableInputs(false);
            HomePage.PumpTestInformation = null;
            HomePage.PumpTestInformation = new PumpTestInformation();
            this.gridCeritificateInfo.DataContext = HomePage.PumpTestInformation;
            ElpisOPCServerMainWindow.homePage.PumpGridMain.DataContext = HomePage.PumpTestInformation;
            HomePage.PumpTestInformation.IsTestStarted = true;
            blbStateOFF.Visibility = Visibility.Visible;
            blbStateON.Visibility = Visibility.Hidden;
            //SelectionGrid.IsEnabled = true;
            //tablePara.ItemsSource = null;
            chart1cmbXAxis.Items.Clear();
            chart1cmbXAxis.ItemsSource = null;
            chart1YaxisPara.ItemsSource = null;
            Chart1YaxisParaList.Clear();
            chart1YaxisPara.Items.Clear();

            chart2cmbXAxis.Items.Clear();
            chart2cmbXAxis.ItemsSource = null;
            chart2YaxisPara.ItemsSource = null;
            Chart2YaxisParaList.Clear();
            chart2YaxisPara.Items.Clear();
            //chk_BlockA.IsChecked = false;
            //chk_BlockB.IsChecked = false;
            //sathish radio button line commented  for hydrofit
            //rbtn_blockA.IsChecked = false;
            //rbtn_blockB.IsChecked = false;
           configGrid.IsEnabled = true;
            if (stopwatch != null)
            {
                stopwatch.Reset();
            }


            // TODO HYDROFIT CHANGES

            //if (lstAllTags.Items.Count > 0 || allTags.Count() > 0)
            //{
            //    allTags.Clear();
            //    lstAllTags.ItemsSource = null;
            //}
            //if (lstSelectedTags.Items.Count > 0 || selectedTagNames.Count() > 0)
            //{
            //    selectedTagNames.Clear();
            //    lstSelectedTags.ItemsSource = null;
            //}
            //if (tempTable.Count > 0)
            //{
            //    tempTable.Clear();
            //}

            //if (HomePage.PumpTestInformation.TableData.Count > 0)
            //{
            //    HomePage.PumpTestInformation.TableData.Clear();
            //}
            //if (HomePage.PumpTestInformation.TableParameterList.Count > 0)
            //{

            //    HomePage.PumpTestInformation.TableParameterList.Clear();
            //}
            HomePage.PumpTestInformation.TableData.Add("Time", new Dictionary<string, string>());
            HomePage.PumpTestInformation.TableParameterList.Add("Time");
            connectorName = HomePage.SelectedConnector;
            deviceName = HomePage.SelectedDevice;
            TagsCollection = Helper.GetTagsCollection(TestType.PumpTest, connectorName, deviceName);
            HomePage.PumpTestInformation.TagInformation = TagsCollection;
         
            if (TagsCollection != null)
            {
                foreach (var item in TagsCollection)
                {
                    
                    allTags.Add(item.TagName + " (" + item.Units + ")");
                }
                //FOR HYDROFIT  CHANGES
                //lstAllTags.ItemsSource = allTags;

                this.gridCeritificateInfo.DataContext = HomePage.PumpTestInformation;
            }
            if (lblStartStop.Content.ToString() == "Stop Record")
                StopTest();
            if (chart1.Series != null && chart1.Series.Count > 0)
            {
                chart1.Series.Clear();
                chart1SeriesCollections.Clear();
                chart1SeriesCollections = null;
                Chart1xAxisParaValue.Clear();
                Chart1xAxisTimeValue.Clear();
                chart1.AxisY.Clear();
                chart1.AxisX.Clear();

            }
            if (chart2.Series != null && chart2.Series.Count > 0)
            {
                chart2.Series.Clear();
                chart2SeriesCollections.Clear();
                chart2SeriesCollections = null;
                Chart2xAxisParaValue.Clear();
                Chart2xAxisTimeValue.Clear();
                chart2.AxisY.Clear();
                chart2.AxisX.Clear();

            }
            for (int i = ChartView1.Children.Count - 1; i > 0; i--)
            {
                ChartView1.Children.Remove(ChartView1.Children[i]);

            }
            for (int i = ChartView2.Children.Count - 1; i > 0; i--)
            {
                ChartView2.Children.Remove(ChartView2.Children[i]);

            }
            this.DataContext = this;
        }

        private bool ValidateInputs()
        {
            bool isValid = false;
            try
            {
                isValid = true;
                if (string.IsNullOrWhiteSpace(HomePage.PumpTestInformation.PumpJobNumber) || string.IsNullOrEmpty(HomePage.PumpTestInformation.PumpJobNumber))
                    isValid = isValid && false;


                if (string.IsNullOrWhiteSpace(HomePage.PumpTestInformation.PumpReportNumber) || string.IsNullOrEmpty(HomePage.PumpTestInformation.PumpReportNumber))
                    isValid = isValid && false;
                //TO Do for Hydro fit by sathish
                //if (string.IsNullOrWhiteSpace(HomePage.PumpTestInformation.PumpSPGSerialNo) || string.IsNullOrEmpty(HomePage.PumpTestInformation.PumpSPGSerialNo))
                //    isValid = isValid && false;
                if (string.IsNullOrWhiteSpace(HomePage.PumpTestInformation.EqipCustomerName) || string.IsNullOrEmpty(HomePage.PumpTestInformation.EqipCustomerName))
                    isValid = isValid && false;
                if (string.IsNullOrWhiteSpace(HomePage.PumpTestInformation.EquipManufacturer) || string.IsNullOrEmpty(HomePage.PumpTestInformation.EquipManufacturer))
                    isValid = isValid && false;
                if (string.IsNullOrWhiteSpace(HomePage.PumpTestInformation.EquipModelNo) || string.IsNullOrEmpty(HomePage.PumpTestInformation.EquipModelNo))
                    isValid = isValid && false;
                if (string.IsNullOrWhiteSpace(HomePage.PumpTestInformation.EquipType) || string.IsNullOrEmpty(HomePage.PumpTestInformation.EquipType))
                    isValid = isValid && false;
                if (string.IsNullOrWhiteSpace(HomePage.PumpTestInformation.EquipControlType) || string.IsNullOrEmpty(HomePage.PumpTestInformation.EquipControlType))
                    isValid = isValid && false;
                if (string.IsNullOrWhiteSpace(HomePage.PumpTestInformation.EquipPumpType) || string.IsNullOrEmpty(HomePage.PumpTestInformation.EquipPumpType))
                    isValid = isValid && false;
                if (string.IsNullOrWhiteSpace(HomePage.PumpTestInformation.EquipSerialNo) || string.IsNullOrEmpty(HomePage.PumpTestInformation.EquipSerialNo))
                    isValid = isValid && false;
                if (string.IsNullOrWhiteSpace(HomePage.PumpTestInformation.TestManufacture) || string.IsNullOrEmpty(HomePage.PumpTestInformation.TestManufacture))
                    isValid = isValid && false;
                if (string.IsNullOrWhiteSpace(HomePage.PumpTestInformation.TestSerialNo) || string.IsNullOrEmpty(HomePage.PumpTestInformation.TestSerialNo))
                    isValid = isValid && false;
                if (string.IsNullOrWhiteSpace(HomePage.PumpTestInformation.TestType) || string.IsNullOrEmpty(HomePage.PumpTestInformation.TestType))
                    isValid = isValid && false;
                if (string.IsNullOrWhiteSpace(HomePage.PumpTestInformation.TestRange) || string.IsNullOrEmpty(HomePage.PumpTestInformation.TestRange))
                    isValid = isValid && false;
                if (string.IsNullOrWhiteSpace(HomePage.PumpTestInformation.TestedBy) || string.IsNullOrEmpty(HomePage.PumpTestInformation.TestedBy))
                    isValid = isValid && false;
                if (string.IsNullOrWhiteSpace(HomePage.PumpTestInformation.WitnessedBy) || string.IsNullOrEmpty(HomePage.PumpTestInformation.WitnessedBy))
                    isValid = isValid && false;
                if (string.IsNullOrWhiteSpace(HomePage.PumpTestInformation.ApprovedBy) || string.IsNullOrEmpty(HomePage.PumpTestInformation.ApprovedBy))
                    isValid = isValid && false;



            }
            catch (Exception ex)
            {
                isValid = false;
            }
            finally
            {
                string reportNumber = HomePage.PumpTestInformation.PumpReportNumber;
                this.gridCeritificateInfo.DataContext = null;
                ElpisOPCServerMainWindow.homePage.PumpGridMain.DataContext = null;
                HomePage.PumpTestInformation.PumpReportNumber = reportNumber;
                this.gridCeritificateInfo.DataContext = HomePage.PumpTestInformation;
                ElpisOPCServerMainWindow.homePage.PumpGridMain.DataContext = HomePage.PumpTestInformation;
            }
            if (isValid)
            {
                return true;
            }
                //ElpisOPCServerMainWindow.homePage.PumpExpanderCertificate.IsExpanded = false;
            else
                //ElpisOPCServerMainWindow.homePage.PumpExpanderCertificate.IsExpanded = true;
            return isValid;
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

        private void TriggerPLC(bool v)
        {
        }

        private void ReadDeviceData()
        {
            try
            {
                #region Old Device TCp Socket connection 
                if (SunPowerGenMainPage.ModbusTcpMaster != null || SunPowerGenMainPage.ModbusSerialPortMaster != null || SunPowerGenMainPage.ABEthernetClient != null || SunPowerGenMainPage.DeviceTcpSocketClient != null &&  retryCount <= 3)
                {
                    #region Modbus Ethernet
                    if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ModbusEthernet)
                    {
                        var val = "0";
                        dataCounts++;
                        foreach (var tupleItem in seletedItems)
                        {
                            if (tupleItem.Item2.Address.ToString().Length >= 5)
                            {
                                try
                                {
                                    if ((tupleItem.Item2.Address.ToString()[0]).ToString() == "3")
                                    {
                                        val = Helper.ReadDeviceInputRegisterValue(ushort.Parse(tupleItem.Item2.Address.ToString()), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusEthernet, startAddressPos, slaveId);
                                    }
                                    else if ((tupleItem.Item2.Address.ToString()[0]).ToString() == "4")
                                    {
                                        val = Helper.ReadDeviceHoldingRegisterValue(ushort.Parse(tupleItem.Item2.Address.ToString()), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusEthernet, startAddressPos, slaveId);
                                    }
                                }
                                catch (SlaveException)
                                {
                                    startAddressPos = 1;
                                    retryCount++;
                                }
                                catch (Exception ex)
                                {
                                    ElpisServer.Addlogs("All", "SPG Reporting Tool-Pump Test", ex.Message, LogStatus.Error);
                                    StopTest();
                                }
                            }
                            else
                            {

                                val = SunPowerGenMainPage.ModbusTcpMaster.ReadHoldingRegisters(ushort.Parse(tupleItem.Item2.Address), 1)[0].ToString();


                            }
                            UpdateData(val, tupleItem);
                        }

                    }
                    #endregion Modbus Ethernet

                    #region Serial Device
                    else if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ModbusSerial)
                    {
                        var val = "0";
                        dataCounts++;
                        foreach (var tupleItem in seletedItems)
                        {
                            if (tupleItem.Item2.Address.ToString().Length >= 5)
                            {
                                try
                                {
                                    if ((tupleItem.Item2.Address.ToString()[0]).ToString() == "1")
                                    {
                                        val = Helper.ReadDeviceCoilsRegisterValue(ushort.Parse(tupleItem.Item2.Address.ToString()), DeviceType.ModbusSerial, slaveId);
                                    }
                                    else if ((tupleItem.Item2.Address.ToString()[0]).ToString() == "2")
                                    {
                                        val = Helper.ReadDeviceDiscreteInputRegisterValue(ushort.Parse(tupleItem.Item2.Address.ToString()), DeviceType.ModbusSerial, slaveId);
                                    }
                                    else if ((tupleItem.Item2.Address.ToString()[0]).ToString() == "3")
                                    {
                                        val = Helper.ReadDeviceInputRegisterValue(ushort.Parse(tupleItem.Item2.Address.ToString()), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusSerial, 0, slaveId);
                                    }
                                    else if ((tupleItem.Item2.Address.ToString()[0]).ToString() == "4")
                                    {
                                        val = Helper.ReadDeviceHoldingRegisterValue(ushort.Parse(tupleItem.Item2.Address.ToString()), Elpis.Windows.OPC.Server.DataType.Short, DeviceType.ModbusSerial, 0, slaveId);
                                    }

                                }
                                catch (SlaveException)
                                {
                                    startAddressPos = 1;
                                    retryCount++;
                                }
                                catch (Exception ex)
                                {
                                    ElpisServer.Addlogs("All", "SPG Reporting Tool-Pump Test", ex.Message, LogStatus.Information);
                                    StopTest();
                                }
                            }
                            else
                            {

                                val = SunPowerGenMainPage.ModbusSerialPortMaster.ReadHoldingRegisters(slaveId, ushort.Parse(tupleItem.Item2.Address.ToString()), 1)[0].ToString();


                            }
                            UpdateData(val, tupleItem);
                        }


                    }
                    #endregion Serial Device

                    #region AB Micrologix Ethernet
                    else if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.ABMicroLogixEthernet)
                    {
                        if (Helper.MappedTagList != null)
                        {
                            var val = "0";
                            dataCounts++;
                            foreach (var item in Helper.MappedTagList)
                            {
                                foreach (var Selecteditem in seletedItems)
                                {
                                    if (item.Key.ToLower().Contains(Selecteditem.Item1.ToLower()))
                                    {
                                        val = Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                                        if (val == null)
                                        {
                                            MessageBox.Show(string.Format("Please, check Pump test device connection for One of the following reason:\n1.Device is disconnected.\n2.Processor Selection Mismatch. \n \n Check Tag: \" {0} \" for one of the following reason \n1.Invalid Tag Address.\n2.Mismatch the Tag DataType.", Selecteditem.Item1), "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                                            SunPowerGenMainPage.ABEthernetClient.Dispose();
                                            tbxDeviceStatus.Text = "Not Connected";
                                            tbxDeviceStatus.Foreground = Brushes.Red;
                                            StopTest();
                                            return;

                                        }
                                        UpdateData(val, Selecteditem);
                                    }

                                }

                                #region Old code commented by pravin
                                //if (!string.IsNullOrEmpty(HomePage.PumpTestInformation.Flow) && !string.IsNullOrEmpty(HomePage.PumpTestInformation.Pressure) && !string.IsNullOrEmpty(HomePage.PumpTestInformation.CylinderMovement))
                                //{
                                //    if (item.Key.ToLower().Contains("flow"))
                                //        HomePage.PumpTestInformation.Flow = Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                                //    if (HomePage.PumpTestInformation.Flow == null)
                                //    {
                                //        SunPowerGenMainPage.ABEthernetClient.Dispose();
                                //        tbxDeviceStatus.Text = "Not Connected";
                                //        tbxDeviceStatus.Foreground = Brushes.Red;
                                //        StopTest();
                                //        MessageBox.Show("Please, check slip stick test device connection. One of the following are reason:\n1.Device is disconnected.\n2.Invalid Tag Address.\n3.Mismatch the Tag DataType.\n4.Processor Selection Mismatch.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                                //        return;

                                //    }
                                //    else if (item.Key.ToLower().Contains("pressure"))
                                //    {
                                //        if (HomePage.PumpTestInformation.Pressure == null)
                                //        {
                                //            SunPowerGenMainPage.ABEthernetClient.Dispose();
                                //            tbxDeviceStatus.Text = "Not Connected";
                                //            tbxDeviceStatus.Foreground = Brushes.Red;
                                //            StopTest();
                                //            MessageBox.Show("Please, check slip stick test device connection. One of the following are reason:\n1.Device is disconnected.\n2.Invalid Tag Address.\n3.Mismatch the Tag DataType.\n4.Processor Selection Mismatch.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                                //            return;

                                //        }

                                //        HomePage.PumpTestInformation.Pressure = Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                                //        if (!isInitialPressureSet)
                                //        {
                                //            isInitialPressureSet = true;
                                //        }


                                //    }
                                //    else if (item.Key.ToLower().Contains("cylindermovement"))
                                //    {
                                //        HomePage.PumpTestInformation.CylinderMovement = Helper.ReadEthernetIPDevice(item.Value.Item1, item.Value.Item2);
                                //        if (HomePage.PumpTestInformation.CylinderMovement == null)
                                //        {
                                //            SunPowerGenMainPage.ABEthernetClient.Dispose();
                                //            tbxDeviceStatus.Text = "Not Connected";
                                //            tbxDeviceStatus.Foreground = Brushes.Red;
                                //            StopTest();
                                //            MessageBox.Show("Please, check slip stick test device connection. One of the following are reason:\n1.Device is disconnected.\n2.Invalid Tag Address.\n3.Mismatch the Tag DataType.\n4.Processor Selection Mismatch.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                                //            return;

                                //        }
                                //    }

                                //}


                                //if (string.IsNullOrEmpty(txtFlow.Text) || string.IsNullOrEmpty(txtCurrentPressure.Text) || string.IsNullOrEmpty(txtCylinderMovement.Text))
                                //{
                                //    if (PressureSeries.Values.Count > 0)
                                //    {
                                //        HomePage.PumpTestInformation.Pressure = PressureSeries.Values[PressureSeries.Values.Count - 1].ToString();

                                //    }
                                //    else
                                //    {
                                //        HomePage.PumpTestInformation.Pressure = "0";
                                //        HomePage.PumpTestInformation.PressureAfterFirstCylinderMovement = "0";
                                //        HomePage.PumpTestInformation.CylinderMovement = "0";
                                //        HomePage.PumpTestInformation.InitialCylinderMovement = "0";
                                //        HomePage.PumpTestInformation.CylinderFirstMovement = "0";
                                //        HomePage.PumpTestInformation.Flow = "0";
                                //    }
                                //    //SunPowerGenMainPage.ABEthernetClient.Dispose();
                                //    //tbxDeviceStatus.Text = "Not Connected";
                                //    //tbxDeviceStatus.Foreground = Brushes.Red;
                                //    //MessageBox.Show("Please, check slip stick test device conne
                                //    //StopTest();ction. One of the following are reason:\n1.Device is disconnected.\n2.Invalid Tag Address.\n3.Mismatch the Tag DataType.\n4.Processor Selection Mismatch.", "SPG Report Tool", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                                //    //return;
                                //}
                                #endregion Old code commented by pravin
                            }                                                     
                        }

                    }
                    #endregion AB Micrologix Ethernet                       
                    #region TcpSocket Device 
                    else if (SunPowerGenMainPage.DeviceObject.DeviceType == DeviceType.TcpSocketDevice)
                    {
                        string stream = Helper.TcpDeviceData(SunPowerGenMainPage.DeviceTcpSocketClient);

                        Tcp_DataReceived(stream);

                    }
                    #endregion TcpSocket Device
                    //PressureSeries.Values.Add(double.Parse(txtCurrentPressure.Text.ToString()));
                    //pressureXAxis.Labels = CylinderMovementLabels;
                    //DataContext = this;

                }
                #region TcpSocketServer Device
                else if (TcpServer.client.Connected)
                {
                    string stream = Helper.TcpDeviceData(TcpServer.client);

                    Tcp_DataReceived(stream);
                }
                #endregion TcpSocketServer Device
                else
                {
                    StopTest();
                    ElpisServer.Addlogs("All", "SPG Reporting Tool-Pump Test", string.Format("Retry Count:{0}", retryCount), LogStatus.Information);
                    MessageBox.Show("Problem in connecting device, please check it.", "SPG Reporting Tool", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                #endregion Old Device TCp Socket connection 
                

            }
            catch (Exception exe)
            {
                ElpisServer.Addlogs("All", "SPG Reporting Tool-Pump Test", string.Format("Error in Read value.{0}.", exe.Message), LogStatus.Error);
                StopTest();
            }
        }


        public void Tcp_DataReceived(string obj)
        {

            List<parsedDeviceData> data = Helper.ParsedDeviceDatas(obj);

            this.Dispatcher.Invoke(() =>
            {
                if (data != null)
                {
                    // var deviceInfo = DeviceFactory.GetDevice(SunPowerGenMainPage.DeviceObject) as KafkaDevice;
                    var deviceInfo = DeviceFactory.GetDevice(SunPowerGenMainPage.DeviceObject) as TcpSocketDevice;
                    var deviceData = data.Where(e => e.deviceId == deviceInfo.DeviceId).ToList();

                    if (deviceData.Count > 0)
                    {

                        foreach (var item in seletedItems)
                        {
                            var value = deviceData.Where(e => e.signalId == item.Item2.Address).ToList();
                            if (value.Count > 0)
                            {

                                DataParse(value[0].value, item);
                            }



                        }
                    }
                   
                }

            });

        }

        private void DataParse(string deviceData, Tuple<string, Tag> item)
        {
            string convertedData = string.Empty;
            switch (item.Item2.DataType)
            {
                case DataType.Boolean:
                    convertedData = Convert.ToString(Convert.ToInt64(deviceData) > 0 ? 1 : 0);
                    break;
                case DataType.Double:
                    convertedData = Convert.ToString(Convert.ToDouble(deviceData));
                    break;
                case DataType.Float:
                    convertedData = Convert.ToString(Convert.ToDecimal(deviceData));
                    break;
                case DataType.Integer:

                    //Convert.ToString(Math.Round( Convert.ToInt32((deviceData))));
                    convertedData = Convert.ToString(Convert.ToDouble(deviceData));
                    //convertedData = Convert.ToString(Convert.ToInt32(deviceData));
                    break;
                case DataType.Short:
                    convertedData = Convert.ToString(Convert.ToDouble(deviceData));
                    // convertedData =Convert.ToString( Double.Parse(deviceData));
                    //convertedData = Convert.ToString(Convert.ToInt16(deviceData));
                    break;
                case DataType.String:
                    convertedData = Convert.ToString(deviceData);
                    break;
                default:
                    break;
            }
            if (!string.IsNullOrEmpty(convertedData))
                UpdateData(convertedData, item);
        }

        internal void ConnectorTypeChanged()
        {
            connectorName = HomePage.SelectedConnector;
            deviceName = HomePage.SelectedDevice;
            TagsCollection = Helper.GetTagsCollection(TestType.PumpTest, connectorName, deviceName);

            if (TagsCollection != null)
            {
                bool checkpara = allTags.SequenceEqual(TagsCollection.Select(x => x.TagName).ToList<string>());
                if (TagsCollection.Count != allTags.Count || (!checkpara))
                {
                    HomePage.PumpTestInformation.TagInformation = TagsCollection;

                    //if (lstAllTags.Items.Count > 0)
                    //{
                    //    allTags.Clear();
                    //    lstAllTags.ItemsSource = null;
                    //}
                    //if (lstSelectedTags.Items.Count > 0)
                    //{
                    //    selectedTagNames.Clear();
                    //    lstSelectedTags.ItemsSource = null;
                    //}
                    if (tempTable.Count > 0)
                    {
                        tempTable.Clear();
                    }
                    if (HomePage.PumpTestInformation.TableData.Count > 0)
                    {
                        HomePage.PumpTestInformation.TableData.Clear();
                    }
                    if (HomePage.PumpTestInformation.TableParameterList.Count > 0)
                    {

                        HomePage.PumpTestInformation.TableParameterList.Clear();
                    }

                    HomePage.PumpTestInformation.TableData.Add("Time", new Dictionary<string, string>());
                    HomePage.PumpTestInformation.TableParameterList.Add("Time");
                    foreach (var item in TagsCollection)
                    {
                        if (!allTags.Contains(item.TagName + " (" + item.Units + ")"))
                            allTags.Add(item.TagName + " (" + item.Units + ")");
                    }
                    // lstAllTags.ItemsSource = allTags;

                    this.gridCeritificateInfo.DataContext = HomePage.PumpTestInformation;
                }
            }
            else
            {
                HomePage.PumpTestInformation.TagInformation = null;

                //if (lstAllTags.Items.Count > 0)
                //{
                //    allTags.Clear();
                //    lstAllTags.ItemsSource = null;
                //}
                //if (lstSelectedTags.Items.Count > 0)
                //{
                //    selectedTagNames.Clear();
                //    lstSelectedTags.ItemsSource = null;
                //}
                if (tempTable.Count > 0)
                {
                    tempTable.Clear();
                }
                if (HomePage.PumpTestInformation.TableData.Count > 0)
                {
                    HomePage.PumpTestInformation.TableData.Clear();
                }
                if (HomePage.PumpTestInformation.TableParameterList.Count > 0)
                {

                    HomePage.PumpTestInformation.TableParameterList.Clear();
                }
                HomePage.PumpTestInformation.TableData.Add("Time", new Dictionary<string, string>());
                HomePage.PumpTestInformation.TableParameterList.Add("Time");
                this.gridCeritificateInfo.DataContext = HomePage.PumpTestInformation;

            }
        }

        private void UpdateData(string val, Tuple<string, Tag> tupleItem)
        {


            TimeSpan ts = stopwatch.Elapsed;
            try
            {

                //xAxisTimeValue.Add(DateTime.Now.ToString("HH:mm:ss tt"));
                //var XAxisTime = pumpTestChart.AxisX.FirstOrDefault(e => e.Title == "Time");
                //XAxisTime.Labels = xAxisTimeValue;

                var selectedxAxis = tempTable.FirstOrDefault(e => e.Key == tupleItem.Item1);
                if (selectedxAxis.Key != null)
                    selectedxAxis.Value.Add(val);
            /// sathish done radion button  to checkbox changed for hydrofit

                if (chk_BlockA.IsChecked == true || chk_BlockB.IsChecked == true)
                {
                    if (tupleItem.Item1 == chart1cmbXAxis.SelectedItem.ToString())
                    {
                        //TimeSpan ts = stopwatch.Elapsed;
                        Chart1xAxisParaValue.Add(string.Format(val + " (" + string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds) + " sec)"));
                        var XAxis = chart1.AxisX.FirstOrDefault(e => e.Title == string.Format(chart1cmbXAxis.SelectedItem.ToString() + "( Time )"));
                        XAxis.Labels = Chart1xAxisParaValue;
                    }
                    // TO DO FOR Chart2Xaxis 
                    else if (tupleItem.Item1 == chart2cmbXAxis.SelectedItem.ToString())
                    {
                        //TimeSpan ts = stopwatch.Elapsed;
                        Chart2xAxisParaValue.Add(string.Format(val + " (" + string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds) + " sec)"));
                        var XAxis = chart2.AxisX.FirstOrDefault(e => e.Title == string.Format(chart2cmbXAxis.SelectedItem.ToString() + "( Time )"));
                        XAxis.Labels = Chart2xAxisParaValue;
                    }


                    //else if (chart1.Series.First(e => e.Title == tupleItem.Item1) != null)
                    //{
                    //    // here we need to check the the selected tupleitem is related to chart1 or chart2 
                    //    var ser = chart1.Series.First(e => e.Title == tupleItem.Item1);
                    //    //var ser1 = chart2.Series.First(e => e.Title == tupleItem.Item1);
                    //    if (ser != null)
                    //    {
                    //        ser.Values.Add(double.Parse(val));
                    //    }

                    //}
                    //else if (chart2.Series.First(e => e.Title == tupleItem.Item1) != null)
                    //{
                    //    var ser = chart2.Series.First(e => e.Title == tupleItem.Item1);
                    //    if (ser != null)
                    //    {
                    //        ser.Values.Add(double.Parse(val));
                    //    }
                    //}
                    #region chatgpt code
                    else if (chart1.Series.FirstOrDefault(e => e.Title == tupleItem.Item1) != null)
                    {
                        var ser = chart1.Series.First(e => e.Title == tupleItem.Item1);
                        if (ser != null)
                        {
                            ser.Values.Add(double.Parse(val));
                        }
                    }
                    else if (chart2.Series.FirstOrDefault(e => e.Title == tupleItem.Item1) != null)
                    {
                        var ser = chart2.Series.First(e => e.Title == tupleItem.Item1);
                        if (ser != null)
                        {
                            ser.Values.Add(double.Parse(val));
                        }
                    }


                    #endregion chatgpt code
                    #region chart code
                    //else if (tupleItem.Item1 != chart1cmbXAxis.SelectedItem.ToString() && tupleItem.Item1 != chart2cmbXAxis.SelectedItem.ToString())
                    //{
                    //    var ser = chart1.Series.First(e => e.Title == tupleItem.Item1);
                    //    if (ser != null)
                    //    {
                    //        ser.Values.Add(double.Parse(val));
                    //    }
                    //    var ser1 = chart2.Series.First(e => e.Title == tupleItem.Item1);
                    //    else
                    //    {

                    //        if (ser1 != null)
                    //        {
                    //            ser.Values.Add(double.Parse(val));
                    //        }
                    //    }
                    //}
                    #endregion chart code
                }
                else
                {
                    //TimeSpan ts = stopwatch.Elapsed;
                    //xAxisTimeValue.Add(DateTime.Now.ToString("HH:mm:ss tt"));
                    Chart1xAxisTimeValue.Add(string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds + " sec"));
                    var XAxis = chart1.AxisX.First(e => e.Title == "Time");
                    XAxis.Labels = Chart1xAxisTimeValue;
                    var ser = chart1.Series.First(e => e.Title == tupleItem.Item1);
                    ser.Values.Add(double.Parse(val));
                }

                //Formula perpose
                //if (chkFormula.IsChecked == true && cmbFirstPara.SelectedItem != null && cmbSecondPara.SelectedItem != null)
                //{
                //    if (tupleItem.Item1 == cmbFirstPara.SelectedItem.ToString())
                //    {
                //        formulaPara1 = val;
                //    }
                //    if (tupleItem.Item1 == cmbSecondPara.SelectedItem.ToString())
                //    {
                //        formulaPara2 = val; ;
                //    }
                //    if (formulaPara1 != null && formulaPara2 != null)
                //    {
                //        var para = pumpTestChart.Series.FirstOrDefault(e => e.Title == "Computed Value");
                //        var calVal = (Convert.ToDouble(formulaPara1) * Convert.ToDouble(formulaPara2)) / 180;
                //        para.Values.Add(calVal);
                //    }
                //}

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }








            //////TODO: check this logic
            //xAxis.Add(dataCounts.ToString());
            //pressureXAxis.Labels = xAxis;

            //if (HomePage.PumpTestInformation.TableData.Keys.Contains(tupleItem.Item1))
            //{

            //    var para = HomePage.PumpTestInformation.TableData.FirstOrDefault(e => e.Key == tupleItem.Item1);
            //    para.Value.Add(val);
            //}
            //else
            //{
            //   HomePage.PumpTestInformation.TableData.Add(tupleItem.Item1,new List<string> {val});

            //}

        }

        private void StopTest()
        {
            string deviceres = string.Empty;
          // ElpisOPCServerMainWindow.homePage.Generatereport();
            lblStartStop.Content = "Start Record";
            imgStartStop.Source = starticon;
            btnStartStop.ToolTip = "Start Record";
            dispatcherTimer.Stop();
            stopwatch.Stop();
            SunPowerGenMainPage.isTestRunning = false;
            this.IsHitTestVisible = true;
            //ElpisOPCServerMainWindow.homePage.btnReset.IsEnabled = true;
            /*ElpisOPCServerMainWindow.pump_Test.*/btnReset.IsEnabled = true;
            /*ElpisOPCServerMainWindow.pump_Test.*/btnGenerateReport.IsEnabled = true;
            //ElpisOPCServerMainWindow.homePage.PumpExpanderCertificate.IsExpanded = true;
            tbxDeviceStatus.Text = "DisConnected";
            tbxDeviceStatus.Foreground = Brushes.Red;

            blbStateOFF.Visibility = Visibility.Visible;
            blbStateON.Visibility = Visibility.Hidden;
            TriggerPLC(false);
            GenerateCSVDataFile();
           // StartStopcommand(false/*,ref deviceres*/);
        }

        private void GenerateCSVDataFile()
        {
            try
            {
                //PumpReportGeneration reportGeneration = new PumpReportGeneration();
                //ObservableCollection<LineSeries> lineSeriesList = null;
                //ObservableCollection<List<string>> labelCollection = null;
                //HomePage.PumpTestInformation.SeriesCounts = pumpTestChart.Series.Count;
                //HomePage.PumpTestInformation.SelectedXaxis = pumpTestChart.AxisX.FirstOrDefault().Title;
                //PumpTestInformation testData = HomePage.PumpTestInformation;
                //if (SeriesCollections != null)
                //{

                //    LineSeries series1;
                //    lineSeriesList = new ObservableCollection<LineSeries>();
                //    labelCollection = new ObservableCollection<List<string>>() { pumpTestChart.AxisX.FirstOrDefault().Labels.ToList() };
                //    foreach (var item in pumpTestChart.Series)
                //    {
                //        series1 = new LineSeries() { Values = item.Values, Title = item.Title, LabelPoint = item.LabelPoint, Foreground = Brushes.Black };
                //        lineSeriesList.Add(series1);
                //    }

                //    reportGeneration.GenerateCSVFile(TestType.PumpTest, testData, lineSeriesList, labelCollection);
                //}

                //new code
                PumpReportGeneration reportGeneration = new PumpReportGeneration();
                ObservableCollection<LineSeries> lineSeriesList = null;
                // ObservableCollection<List<string>> labelCollection = null;
                HomePage.PumpTestInformation.SeriesCounts = chart1.Series.Count;
                HomePage.PumpTestInformation.SelectedXaxis = chart1.AxisX.FirstOrDefault().Title;
                // PumpTestInformation testData = HomePage.PumpTestInformation;
                if (chart1SeriesCollections != null)
                {

                    LineSeries series1;
                    lineSeriesList = new ObservableCollection<LineSeries>();
                    HomePage.PumpTestInformation.LabelCollection = new ObservableCollection<List<string>>() { chart1.AxisX.FirstOrDefault().Labels.ToList() };
                    foreach (var item in chart1.Series)
                    {
                        series1 = new LineSeries() { Values = item.Values, Title = item.Title, LabelPoint = item.LabelPoint, Foreground = Brushes.Black };
                        lineSeriesList.Add(series1);
                    }

                    HomePage.PumpTestInformation.LineSeriesList = lineSeriesList;

                    // reportGeneration.GenerateCSVFile(TestType.PumpTest, testData, lineSeriesList, labelCollection);
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
        }

        private void lstAllTags_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void lstSelectedTags_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        public void generateButtonClicked()
        {
            ConvertXamltoImage(ChartView1, (int)ChartView1.ActualWidth, (int)ChartView1.ActualHeight);
            ConvertXamltoImage(ChartView2, (int)ChartView2.ActualWidth, (int)ChartView2.ActualHeight);
        }
        #region SubcribebtnClick
        //private void OnUnsubBtnClick(object sender, RoutedEventArgs e)
        //{

        //    if (lstSelectedTags.SelectedItems != null)
        //    {

        //        for (int i = lstSelectedTags.SelectedItems.Count; i > 0; i--)
        //        {
        //            //IPublishers publisher = publisher;//PublisherSettingsPropertyGrid.SelectedObject as IPublishers;
        //            selectedTagNames.Remove(lstSelectedTags.SelectedItems[i - 1].ToString());
        //            tempTable.Remove(lstAllTags.SelectedItems[i - 1].ToString());
        //        }
        //        lstSelectedTags.ItemsSource = null;
        //       // tablePara.ItemsSource = null;
        //        chart1cmbXAxis.ItemsSource = null;
        //        //cmbYAxis.ItemsSource = null;
        //       // cmbFirstPara.ItemsSource = null;
        //       // cmbSecondPara.ItemsSource = null;
        //        lstAllTags.ItemsSource = allTags.Except(selectedTagNames).ToList();
        //        lstSelectedTags.ItemsSource = selectedTagNames;
        //       // tablePara.ItemsSource = selectedTagNames;
        //        chart1cmbXAxis.ItemsSource = selectedTagNames;
        //        //cmbYAxis.ItemsSource = selectedTagNames;
        //      // cmbFirstPara.ItemsSource = selectedTagNames;
        //       // cmbSecondPara.ItemsSource = selectedTagNames;
        //    }

        //}
        #endregion SubcribebtnClick
        //#region UnsubscribeBtnclick

        //private void OnSubBtnClick(object sender, RoutedEventArgs e)
        //{
        //    if (lstAllTags.SelectedItems != null)
        //    {
        //        lstSelectedTags.ItemsSource = null;
        //       // tablePara.ItemsSource = null;
        //        chart1cmbXAxis.ItemsSource = null;
        //        //cmbYAxis.ItemsSource = null;
        //        //cmbFirstPara.ItemsSource = null;
        //        //cmbSecondPara.ItemsSource = null;
        //        //int count = lstAllTags.SelectedItems.Count;
        //        for (int i = lstAllTags.SelectedItems.Count; i > 0; i--)
        //        {
        //            //IPublishers publisher = publisher;//PublisherSettingsPropertyGrid.SelectedObject as IPublishers;
        //            selectedTagNames.Add(lstAllTags.SelectedItems[i - 1].ToString());
        //            tempTable.Add(lstAllTags.SelectedItems[i - 1].ToString(), new List<string>());
        //        }
        //        lstAllTags.ItemsSource = allTags.Except(selectedTagNames).ToList();
        //        lstSelectedTags.ItemsSource = selectedTagNames;
        //        //tablePara.ItemsSource = selectedTagNames;
        //        chart1cmbXAxis.ItemsSource = selectedTagNames;
        //        //cmbYAxis.ItemsSource = selectedTagNames;
        //        //cmbFirstPara.ItemsSource = selectedTagNames;
        //        //cmbSecondPara.ItemsSource = selectedTagNames;

        //    }

        //}
        //#endregion UnsubscribeBtnclick
        private void TablePara_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ChkPrameters_Checked(object sender, RoutedEventArgs e)
        {
            var da = e.Source as CheckBox;

            HomePage.PumpTestInformation.TableParameterList.Add(da.Content.ToString());
            HomePage.PumpTestInformation.TableData.Add(da.Content.ToString(), new Dictionary<string, string>());

            //  ParamterComboLabel();
        }

        private void ParamterComboLabel()
        {
            StringBuilder label = new StringBuilder();
            foreach (var item in HomePage.PumpTestInformation.TableParameterList)
            {
                label.Append(item);
                label.Append(',');
            }
            // tablePara.Text = label.ToString().TrimEnd(new char[] { ',' });
        }

        private void ChkPrameters_Unchecked(object sender, RoutedEventArgs e)
        {
            var da = e.Source as CheckBox;
            HomePage.PumpTestInformation.TableParameterList.Remove(da.Content.ToString());
            HomePage.PumpTestInformation.TableData.Remove(da.Content.ToString());
            // ParamterComboLabel();
        }


        private void PumpTestChart1_DataClick(object sender, ChartPoint chartPoint)
        {

            Point point = Mouse.GetPosition(ChartView1);
            var xVal = chart1.AxisX.FirstOrDefault().Labels[Convert.ToInt32(chartPoint.X)];
            var xAxisVal = string.Empty;
            var xAxisTime = string.Empty;
            var time = string.Empty;
            //sathish radion button to checkbox changed for hydrofit
            if (chk_BlockB.IsChecked == true && chart1cmbXAxis.SelectedItem != null)
            {
                xAxisVal = string.Format("{0} : {1}", chart1cmbXAxis.SelectedItem.ToString(), xVal.Split('(')[0]);
                time = xVal.Split('(')[1].Replace('(', ' ').Replace(')', ' ');
                xAxisTime = string.Format("{0} : {1}", "Time", time);
            }
            //sathish radion button  to checkbox changed for hydrofit
            else if (chk_BlockB.IsChecked == true && chart2cmbXAxis.SelectedItem != null)
            {
                xAxisVal = string.Format("{0} : {1}", chart2cmbXAxis.SelectedItem.ToString(), xVal.Split('(')[0]);
                time = xVal.Split('(')[1].Replace('(', ' ').Replace(')', ' ');
                xAxisTime = string.Format("{0} : {1}", "Time", time);
            }
            else if (chk_BlockA.IsChecked == false || chk_BlockB.IsChecked == false)
            {
                time = xVal;
                xAxisTime = string.Format("{0} : {1}", "Time", time);
            }
            //else if (rbtn_blockA.IsChecked == false || rbtn_blockB.IsChecked == false)
            //{
            //    time = xVal;
            //    xAxisTime = string.Format("{0} : {1}", "Time", time);
            //}

            int index = -1;

            var tableTimepara = HomePage.PumpTestInformation.TableData.FirstOrDefault(e => e.Key == "Time");
            if (tableTimepara.Key != null)
            {               // we use here Dictionary concept.
                index = tableTimepara.Value.Count;
                tableTimepara.Value["Time" + "_" + index] = time;

            }

            //Collecting selected points for table
            foreach (var key in tempTable.Keys)
            {
                var tablepara = HomePage.PumpTestInformation.TableData.FirstOrDefault(e => e.Key == key);
                if (tablepara.Key != null)
                {
                    index = tablepara.Value.Count;
                    tablepara.Value[key + "_" + index] = tempTable.FirstOrDefault(e => e.Key == tablepara.Key).Value[(int)chartPoint.X].ToString();

                }
            }


            CustomTooltipModel customTooltipModel = new CustomTooltipModel()
            {
                SeriesName = chartPoint.SeriesView.Title,
                SeriesValue = chartPoint.Y.ToString(),
                YAxisValue = string.Format("{0} : {1}", chartPoint.SeriesView.Title, chartPoint.Y.ToString()),
                XAxisValue = xAxisVal,
                xAxisTime = xAxisTime,
                CustomNote = "gfdgdfg",
                Index = index
            };

            var va = HomePage.PumpTestInformation.TableData;


            HomePage.PumpTestInformation.CustomTooltip = customTooltipModel;
            //canvasView.DataContext = HomePage.PumpTestInformation.CustomTooltip;
            //Canvas.SetTop(canvasView, (chartPoint.ChartLocation.Y - 150));
            //Canvas.SetLeft(canvasView, (chartPoint.ChartLocation.X - 300));
            //CustomToolTip.Visibility = Visibility.Visible;
            //canvasView.DataContext = HomePage.PumpTestInformation.CustomTooltip;
            //createView(chartPoint.ChartLocation.Y, chartPoint.ChartLocation.X);
            var mousePoint = Mouse.GetPosition(ChartView1);
            string actualVal = string.Format("{0} : {1}", chartPoint.SeriesView.Title, chartPoint.Y.ToString());
            pumpCustomTooltip pumpCustomTooltip = new pumpCustomTooltip(mousePoint.Y, mousePoint.X, customTooltipModel);
            //DraggabalePopup draggabalePopup = new DraggabalePopup();
            //draggabalePopup.IsOpen = true;
            //pumpCustomTooltip.CustomToolTip.Visibility = Visibility.Visible;
            //pumpCustomTooltip.canvasView.DataContext = HomePage.PumpTestInformation.CustomTooltip;
            ChartView1.Children.Add(pumpCustomTooltip);
        }
        private void PumpTestChart2_DataClick(object sender, ChartPoint chartPoint)
        {
            Point point = Mouse.GetPosition(ChartView2);
            var xVal = chart1.AxisX.FirstOrDefault().Labels[Convert.ToInt32(chartPoint.X)];
            var xAxisVal = string.Empty;
            var xAxisTime = string.Empty;
            var time = string.Empty;
            //sathish radion button to checkbox changed for hydrofit
            if (chk_BlockB.IsChecked == true && chart1cmbXAxis.SelectedItem != null)
            {
                xAxisVal = string.Format("{0} : {1}", chart1cmbXAxis.SelectedItem.ToString(), xVal.Split('(')[0]);
                time = xVal.Split('(')[1].Replace('(', ' ').Replace(')', ' ');
                xAxisTime = string.Format("{0} : {1}", "Time", time);
            }
            //sathish radion button  to checkbox changed for hydrofit
            else if (chk_BlockB.IsChecked == true && chart2cmbXAxis.SelectedItem != null)
            {
                xAxisVal = string.Format("{0} : {1}", chart2cmbXAxis.SelectedItem.ToString(), xVal.Split('(')[0]);
                time = xVal.Split('(')[1].Replace('(', ' ').Replace(')', ' ');
                xAxisTime = string.Format("{0} : {1}", "Time", time);
            }
            else if (chk_BlockA.IsChecked == false || chk_BlockB.IsChecked == false)
            {
                time = xVal;
                xAxisTime = string.Format("{0} : {1}", "Time", time);
            }
            //else if (rbtn_blockA.IsChecked == false || rbtn_blockB.IsChecked == false)
            //{
            //    time = xVal;
            //    xAxisTime = string.Format("{0} : {1}", "Time", time);
            //}

            int index = -1;

            var tableTimepara = HomePage.PumpTestInformation.TableData.FirstOrDefault(e => e.Key == "Time");
            if (tableTimepara.Key != null)
            {               // we use here Dictionary concept.
                index = tableTimepara.Value.Count;
                tableTimepara.Value["Time" + "_" + index] = time;

            }

            //Collecting selected points for table
            foreach (var key in tempTable.Keys)
            {
                var tablepara = HomePage.PumpTestInformation.TableData.FirstOrDefault(e => e.Key == key);
                if (tablepara.Key != null)
                {
                    index = tablepara.Value.Count;
                    tablepara.Value[key + "_" + index] = tempTable.FirstOrDefault(e => e.Key == tablepara.Key).Value[(int)chartPoint.X].ToString();

                }
            }


            CustomTooltipModel customTooltipModel = new CustomTooltipModel()
            {
                SeriesName = chartPoint.SeriesView.Title,
                SeriesValue = chartPoint.Y.ToString(),
                YAxisValue = string.Format("{0} : {1}", chartPoint.SeriesView.Title, chartPoint.Y.ToString()),
                XAxisValue = xAxisVal,
                xAxisTime = xAxisTime,
                CustomNote = "gfdgdfg",
                Index = index
            };

            var va = HomePage.PumpTestInformation.TableData;


            HomePage.PumpTestInformation.CustomTooltip = customTooltipModel;
            //canvasView.DataContext = HomePage.PumpTestInformation.CustomTooltip;
            //Canvas.SetTop(canvasView, (chartPoint.ChartLocation.Y - 150));
            //Canvas.SetLeft(canvasView, (chartPoint.ChartLocation.X - 300));
            //CustomToolTip.Visibility = Visibility.Visible;
            //canvasView.DataContext = HomePage.PumpTestInformation.CustomTooltip;
            //createView(chartPoint.ChartLocation.Y, chartPoint.ChartLocation.X);
            var mousePoint = Mouse.GetPosition(ChartView2);
            string actualVal = string.Format("{0} : {1}", chartPoint.SeriesView.Title, chartPoint.Y.ToString());
            pumpCustomTooltip pumpCustomTooltip = new pumpCustomTooltip(mousePoint.Y, mousePoint.X, customTooltipModel);
            //DraggabalePopup draggabalePopup = new DraggabalePopup();
            //draggabalePopup.IsOpen = true;
            //pumpCustomTooltip.CustomToolTip.Visibility = Visibility.Visible;
            //pumpCustomTooltip.canvasView.DataContext = HomePage.PumpTestInformation.CustomTooltip;
            ChartView2.Children.Add(pumpCustomTooltip);
        }
        private void createView(double y, double x)
        {
            Canvas canvas = new Canvas();
            StackPanel stackPanel = new StackPanel();
            stackPanel.Name = "canvasView";
            // stackPanel.Background = (Brush)ColorConverter.ConvertFromString("#FFF0E6DC");
            stackPanel.Width = 195;
            stackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;

            StackPanel childStack = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };

            Binding binding = new Binding()
            {
                Source = HomePage.PumpTestInformation.CustomTooltip.SeriesName + " : " + HomePage.PumpTestInformation.CustomTooltip.SeriesValue,

            };

            Label label = new Label();
            BindingOperations.SetBinding(label, Label.ContentProperty, binding);
            childStack.Children.Add(label);
            stackPanel.Children.Add(childStack);
            Canvas.SetTop(stackPanel, (y - 150));
            Canvas.SetTop(stackPanel, (x - 300));
            canvas.Children.Add(stackPanel);
            Canvas.SetZIndex(stackPanel, 1);
        }

        public void ConvertXamltoImage(UIElement visual, int Width, int Height)
        {
            //UIElement visual = XamlReader.Load(System.Xml.XmlReader.Create(new StringReader(XamlString))) as UIElement;

            RenderTargetBitmap bmpCopied = new RenderTargetBitmap(Width, Height, 92, 92, PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(visual);
                Rect rect = new Rect(new Point(), new Size(Width, Height));
                rect.Offset(10, 10);
                dc.DrawRectangle(vb, null, rect);

            }

            bmpCopied.Render(dv);

            PngBitmapEncoder png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(bmpCopied));
            using (var stream = File.Create(string.Format("{0}\\PumpChart.png", Directory.GetCurrentDirectory())))
            {
                png.Save(stream);
            }

        }

        private void ConfigExpander_Expanded(object sender, RoutedEventArgs e)
        {

        }

        private void ConfigExpander_Collapsed(object sender, RoutedEventArgs e)
        {

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ConnectorTypeChanged();
        }

        private void chart1ChkYaxisPrameters_Checked(object sender, RoutedEventArgs e)
        {
            //var da = e.Source as CheckBox;
            //Chart1YaxisParaList.Add(da.Content.ToString());
            var da = e.Source as CheckBox;

            Chart1YaxisParaList.Add(da.Content.ToString());
            chart2cmbXAxis.Items.Clear();
            foreach (var item in chart1YaxisPara.Items)
            {
                if (!Chart1YaxisParaList.Contains(item))
                {
                    chart2cmbXAxis.Items.Add(item);
                }
            }




        }

        private void chart1ChkYaxisPrameters_Unchecked(object sender, RoutedEventArgs e)
        {
            var da = e.Source as CheckBox;
            // Chart1YaxisParaList.Remove(da.Content.ToString());

            Chart1YaxisParaList.Remove(da.Content.ToString());
            chart2cmbXAxis.Items.Clear();
            foreach (var item in chart1YaxisPara.Items)
            {
                if (!Chart1YaxisParaList.Contains(item))
                {
                    chart2cmbXAxis.Items.Add(item);
                }
            }


        }
        private void chart2ChkYaxisPrameters_Checked(object sender, RoutedEventArgs e)
        {
            //var da = e.Source as CheckBox;
            //Chart1YaxisParaList.Add(da.Content.ToString());
            var da = e.Source as CheckBox;
            Chart2YaxisParaList.Add(da.Content.ToString());
        }

        private void chart2ChkYaxisPrameters_Unchecked(object sender, RoutedEventArgs e)
        {
            var da = e.Source as CheckBox;
            Chart2YaxisParaList.Remove(da.Content.ToString());
            //string values = da.Content.ToString();
            //foreach (var item in values)
            //{
            //    chart2cmbXAxis.Items.Add(item);
            //}

        }

        private void chart1cmbXAxis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (chart1cmbXAxis.SelectedItem != null)
                {
                    Chart1YaxisParaList.Clear();
                    if (chart1YaxisPara.Items.Count > 0)
                        chart1YaxisPara.Items.Clear();
                    //foreach (var tag in TagsCollection)
                    //{
                    foreach (var seletedItem in selectedTagNames)
                    {
                        if (seletedItem.TagName != chart1cmbXAxis.SelectedItem.ToString())
                        {

                            chart1YaxisPara.Items.Add(seletedItem.TagName);

                        }
                    }

                    //}
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void chart2CmbXAxis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (chart2cmbXAxis.SelectedItem != null)
                {
                    Chart2YaxisParaList.Clear();
                    if (chart2YaxisPara.Items.Count > 0)
                        chart2YaxisPara.Items.Clear();
                    //foreach (var tag in TagsCollection)
                    //{
                    foreach (string item in chart2cmbXAxis.Items)
                    {
                        if (item != chart2cmbXAxis.SelectedItem.ToString())
                        {

                            chart2YaxisPara.Items.Add(item);

                        }
                    }

                    //}
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void txtTimer_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void btnSaveOperation_Click(object sender, RoutedEventArgs e)
        {

        }


        private void rbtn_blockA_Checked(object sender, RoutedEventArgs e)
        {
            selectedTagNames.Clear();
            chart1cmbXAxis.Items.Clear();
            chart2cmbXAxis.Items.Clear();
            Chart1YaxisParaList.Clear();
            chart1YaxisPara.Items.Clear();
            Chart2YaxisParaList.Clear();
            chart2YaxisPara.Items.Clear();
            #region RadioButton Function Commented
            
            //if (rbtn_blockA.IsChecked == true)
            //{

            //    var tags = HomePage.PumpTestInformation.TagInformation;
            //    var block1Tags = tags.Where(a => a.BlockType == BlockTypes.Block1);
            //    foreach (var item in block1Tags)
            //    {
            //        selectedTagNames.Add(item);
            //        chart1cmbXAxis.Items.Add(item.TagName);
            //    }
            //}
            //else if (rbtn_blockB.IsChecked == true)
            //{
            //    var tags = HomePage.PumpTestInformation.TagInformation;
            //    var block2Tags = tags.Where(a => a.BlockType == BlockTypes.Block2);
            //    foreach (var item in block2Tags)
            //    {
            //        selectedTagNames.Add(item);
            //        chart1cmbXAxis.Items.Add(item.TagName);
            //    }
            //}
            #endregion RadioButton Function Commented
        }

        private void chart1YaxisPara_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void chart1_DataClick(object sender, ChartPoint chartPoint)
        {

        }

       
        public bool StartStopcommand(bool IsStart/*, ref string resp*/)
        {
            string startCmd;
            //string data =IsStart==true?"01":"00";
            var tcpdevice = SunPowerGenMainPage.DeviceObject as TcpSocketDevice; 
            string data = IsStart == true ? "start" : "stop";
            if(data=="start")
            {
                string d = $"{data};{tcpdevice.DeviceId}";
                int value = Encoding.UTF8.GetByteCount(d);
                string hexString = DecimalToHexTwosComplement(value);
                string header = $"#01{hexString}000000";
                //string header = $"#01{Encoding.UTF8.GetBytes(data.Length.ToString())}000000";
                startCmd = header + data+";" + tcpdevice.DeviceId + "$";
            }
            else
            {
                string d = $"{data};{tcpdevice.DeviceId}";
                int value = Encoding.UTF8.GetByteCount(d);
                string hexString = DecimalToHexTwosComplement(value);
                string header = $"#01{hexString}000000";
                //string header = $"#01{Encoding.UTF8.GetBytes(data.Length.ToString())}000000";
               startCmd = header + data +"$";
            }
            
            //return  Helper.SendingStartStopCmdtoserver(startCmd, HomePage.DeviceTcpClient);
            return Helper.SendingStartStopCmdtoserver (startCmd, TcpServer.client/*,ref resp*/);
        }

        #region Decimal to a hexadecimal string
        // Convert the decimal integer to a hexadecimal string
        public  static string DecimalToHexTwosComplement(int decimalValue)
        {
            // Determine the number of bits needed to represent the input value in 2's complement form
            int numBits = (int)Math.Ceiling(Math.Log(Math.Abs(decimalValue), 2)) + 1;

            // Convert the decimal value to its 2's complement binary representation
            int binaryValue = decimalValue < 0 ? (1 << numBits) + decimalValue : decimalValue;

            // Convert the binary value to a hexadecimal string
            string hexValue = binaryValue.ToString("X4");

            return hexValue;
        }
        #endregion Decimal to a hexadecimal string
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
           ElpisOPCServerMainWindow.homePage.DisableInputs(false);
            ElpisOPCServerMainWindow.homePage.BtnCommentBox.IsChecked = false;
            ElpisOPCServerMainWindow.homePage.txtCommentBox.Text = "";
            ElpisOPCServerMainWindow.homePage.txtCommnetUpdateMessage.Text = "";
            ElpisOPCServerMainWindow.homePage.BtnTestDetails.IsChecked = false;
            ElpisOPCServerMainWindow.homePage.txtTestDetailsBox.Text = "";
            ElpisOPCServerMainWindow.homePage.txtTestDetailsMessage.Text = "";
            //StrokeTestWindow.ResetStrokeTest();
            HomePage.HoldMidPositionTestWindow.ResetHoldMidPosition();
            //SlipStickTestWindow.ResetSlipStickTest();
            HomePage.PumpTestWindow.ResetPumpTest();

            if (ElpisOPCServerMainWindow.homePage.tabControlTest.SelectedIndex == 0)
            {
                HomePage.strokeTestInfo.IsTestStarted = false;
                ElpisOPCServerMainWindow.homePage.gridMain.DataContext = HomePage.strokeTestInfo;
                HomePage.strokeTestInfo.IsTestStarted = true;


            }
            else if (ElpisOPCServerMainWindow.homePage.tabControlTest.SelectedIndex == 1)
            {
                if (HomePage.HoldMidPositionTestWindow.rb_LineA.IsChecked == true)
                {
                    HomePage.holdMidPositionLineAInfo.IsTestStarted = false;
                    ElpisOPCServerMainWindow.homePage.gridMain.DataContext = HomePage.holdMidPositionLineAInfo;
                    HomePage.holdMidPositionLineAInfo.IsTestStarted = true;

                }
                else if (HomePage.HoldMidPositionTestWindow.rb_LineB.IsChecked == true)
                {
                    HomePage.holdMidPositionLineBinfo.IsTestStarted = false;
                   ElpisOPCServerMainWindow.homePage.gridMain.DataContext = HomePage.holdMidPositionLineBinfo;
                    HomePage.holdMidPositionLineBinfo.IsTestStarted = true;
                    ElpisOPCServerMainWindow.homePage.txtStatus.Text = "";
                }
            }
            else if (ElpisOPCServerMainWindow.homePage.tabControlTest.SelectedIndex == 2)
            {
                HomePage.slipStickTestInformation.IsTestStarted = false;
                ElpisOPCServerMainWindow.homePage.gridMain.DataContext = HomePage.slipStickTestInformation;
                HomePage.slipStickTestInformation.IsTestStarted = true;
            }

            else if (ElpisOPCServerMainWindow.homePage.tabControlTest.SelectedIndex == 3)
            {
                HomePage.PumpTestInformation.IsTestStarted = false;
                ElpisOPCServerMainWindow.homePage.gridMain.DataContext = HomePage.PumpTestInformation;
                HomePage.PumpTestInformation.IsTestStarted = true;
            }

            //ElpisOPCServerMainWindow.homePage.panelStatusMessage.Visibility = Visibility.Hidden;
            /*ElpisOPCServerMainWindow.pump_Test.*/panelStatusMessage.Visibility = Visibility.Hidden;
            ElpisOPCServerMainWindow.homePage.ReportTab.IsEnabled = true;
            ElpisOPCServerMainWindow.homePage.txtFilePath.IsEnabled = true;
        }

        private void btnGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            HomePage homePage = new HomePage();
            homePage.Generatereport();
        }

        private void chk_BlockA_Checked(object sender, RoutedEventArgs e)
        {
           
            
            selectedTagNames.Clear();
            chart1cmbXAxis.Items.Clear();
            chart2cmbXAxis.Items.Clear();
            Chart1YaxisParaList.Clear();
            chart1YaxisPara.Items.Clear();
            Chart2YaxisParaList.Clear();
            chart2YaxisPara.Items.Clear();

            if (chk_BlockA.IsChecked == true && chk_BlockB.IsChecked == false)
            {

                var tags = HomePage.PumpTestInformation.TagInformation;
                var block1Tags = tags.Where(a => a.BlockType == BlockTypes.Block1);
                foreach (var item in block1Tags)
                {
                    selectedTagNames.Add(item);
                    chart1cmbXAxis.Items.Add(item.TagName);

                }
            }
            else if (chk_BlockA.IsChecked == false && chk_BlockB.IsChecked == true)
            {
                var tags = HomePage.PumpTestInformation.TagInformation;
                var block2Tags = tags.Where(a => a.BlockType == BlockTypes.Block2);
                foreach (var item in block2Tags)
                {
                    selectedTagNames.Add(item);
                    chart1cmbXAxis.Items.Add(item.TagName);
                }
            }
            else if (chk_BlockA.IsChecked == true && chk_BlockB.IsChecked == true)
            {
                selectedTagNames.Clear();
                chart1cmbXAxis.Items.Clear();
                chart2cmbXAxis.Items.Clear();
                Chart1YaxisParaList.Clear();
                chart1YaxisPara.Items.Clear();
                Chart2YaxisParaList.Clear();
                chart2YaxisPara.Items.Clear();
                var tags = HomePage.PumpTestInformation.TagInformation;
                foreach (var item in tags)
                {
                    selectedTagNames.Add(item);
                    chart1cmbXAxis.Items.Add(item.TagName);
                }
            }

        }

        private void chk_BlockA_Unchecked(object sender, RoutedEventArgs e)
        {
            
            if (chk_BlockA.IsChecked == true && chk_BlockB.IsChecked == false)
            {

                var tags = HomePage.PumpTestInformation.TagInformation;
                var block2Tags = tags.Where(a => a.BlockType == BlockTypes.Block2);
               
                foreach (var item in block2Tags)
                {
                    selectedTagNames.Remove(item);
                    chart1cmbXAxis.Items.Remove(item.TagName);
                    chart1YaxisPara.Items.Clear();
                    chart2cmbXAxis.Items.Clear();
                    chart2YaxisPara.Items.Clear();
                }
            }
            else if (chk_BlockA.IsChecked == false && chk_BlockB.IsChecked == true)
            {
                var tags = HomePage.PumpTestInformation.TagInformation;
                var block1Tags = tags.Where(a => a.BlockType == BlockTypes.Block1);
                foreach (var item in block1Tags)
                {
                    selectedTagNames.Remove(item);
                    chart1cmbXAxis.Items.Remove(item.TagName);
                    chart1YaxisPara.Items.Clear();
                    chart2cmbXAxis.Items.Clear();
                    chart2YaxisPara.Items.Clear();
                }
            }
            else if (chk_BlockA.IsChecked == false && chk_BlockB.IsChecked == false)
            {
                selectedTagNames.Clear();
                chart1cmbXAxis.Items.Clear();
                chart2cmbXAxis.Items.Clear();
                Chart1YaxisParaList.Clear();
                chart1YaxisPara.Items.Clear();
                Chart2YaxisParaList.Clear();
                chart2YaxisPara.Items.Clear();
                //var tags = HomePage.PumpTestInformation.TagInformation;
                //foreach (var item in tags)
                //{
                //    selectedTagNames.Add(item);
                //    chart1cmbXAxis.Items.Add(item.TagName);
                //}
            }
        }
    }

}

//using LicenseManager;
using Modbus.Device;
using OPCEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Elpis.Windows.OPC.Server;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Windows.Threading;
using System.Threading.Tasks;
using ElpisOpcServer.SunPowerGen;

namespace ElpisOpcServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ElpisOPCServerMainWindow : Window
    {
        #region Public Members
        //public DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public TcpClient client;                 
        private System.Windows.Forms.NotifyIcon notifyIcon = null;

        //LicenseHandler licenseManeger { get; set; }
        ConfigurationSettingsUI configuration { get; set; }
        UACertificateSettingsUI uaCertificate { get; set; }
        UAConfigurationSettingsUI uaConfiguration { get; set; }
        SunPowerGenMainPage sunPowerGenMainPage { get; set; }
        public static HomePage homePage { get; set; }
        public static Pump_Test pump_Test { get; set; }

        AboutPageUI about { get; set; }
        License license { get; set; }
        IoTSettingsUI IoTSettings { get; set; }

        ApplicationTour applicationTour { get; set; }

        //public SLIKServer slikserverForMainWindow { get; set; }


        // public ConfigurationViewModel opcEngineConfigurationViewModel { get; set; }
        public UAConfigurationViewModel opcEngineUAConfigurationViewModel { get; set; }
        LicenseViewModel opcEngineLicenseViewModel { get; set; }
        public bool isLicenseActivated { get; }

        LoggerWindow loggerWindow;

        #endregion End Of Public Members

        #region Constructor

        public ElpisOPCServerMainWindow()
        {
            InitializeComponent();
            //licenseManeger = new LicenseHandler();
            configuration = new ConfigurationSettingsUI();

            //pump_Test = new Pump_Test();
            about = new AboutPageUI ();
            license = new License();
            
            //jey_check
#if SunPowerGen
            spButtonList.Children.Clear();
            spButtonList.Orientation = Orientation.Vertical;

            //Adding License Button
            Border licenseBtnBorder = new Border();
            licenseBtnBorder.Style = this.FindResource("MetroBorder") as Style;
            System.Windows.Controls.Button LicenceButton = new Button();
            StackPanel licenseContent = new StackPanel();
            licenseContent.Orientation = Orientation.Vertical;
            TextBlock licenseTb = new TextBlock();
            licenseTb.Text = "License";
            Image licenseImg = new Image();
            licenseImg.Source = new BitmapImage(new Uri(@"Images/closed_lock.png", UriKind.Relative));
            licenseImg.Height = 40;
            licenseImg.Width = 40;

            licenseContent.Children.Add(licenseImg);
            licenseContent.Children.Add(licenseTb);

            LicenceButton.Content = licenseContent;
            LicenceButton.Name = "btnLicense";
            LicenceButton.Height = 100;
            LicenceButton.Click += btnLicence_Click;
            licenseBtnBorder.Child = LicenceButton;
            spButtonList.Children.Add(licenseBtnBorder);
            LicenceButton.Style = this.FindResource("MetroButton") as Style;

            //Adding Configuration Button
            Border configBtnBorder = new Border();
            configBtnBorder.Style = this.FindResource("MetroBorder") as Style;
            System.Windows.Controls.Button configButton = new Button();
            StackPanel content = new StackPanel();
            content.Orientation = Orientation.Vertical;         
            TextBlock tb = new TextBlock();
            tb.Text = "Configuration";
            Image img = new Image();
            img.Source = new BitmapImage(new Uri(@"Images/settings.png", UriKind.Relative));
            img.Height = 40;
            img.Width = 40;
           
            content.Children.Add(img);
            content.Children.Add(tb);

            configButton.Content = content;
            configButton.Name = "btnConfiguration";
            configButton.Height = 100;
            configBtnBorder.Child = configButton;
            spButtonList.Children.Add(configBtnBorder);
            configButton.Style = this.FindResource("MetroButton") as Style;            
            configButton.Click += btnConfiguration_Click;

            //Adding Monitor Button
            Border monitorBtnBorder = new Border();
            monitorBtnBorder.Style = this.FindResource("MetroBorder") as Style;
            System.Windows.Controls.Button monitoringButton = new Button();
            StackPanel monContent = new StackPanel();
            monContent.Orientation = Orientation.Vertical;
            TextBlock monTb = new TextBlock();
            monTb.Text = "Monitoring";
            Image monImg = new Image();
            monImg.Source = new BitmapImage(new Uri(@"Images/Monitoring.png", UriKind.Relative));
            monImg.Height = 50;
            monImg.Width = 60;

            monContent.Children.Add(monImg);
            monContent.Children.Add(monTb);

            monitoringButton.Content = monContent;
            monitoringButton.Name = "btnMonitoring";
            monitoringButton.Click += btnMonitoring_Click;
            monitoringButton.Height = 100;
            monitorBtnBorder.Child = monitoringButton;
            spButtonList.Children.Add(monitorBtnBorder);
            monitoringButton.Style = this.FindResource("MetroButton") as Style;


           

            //Adding Log Button

            Border logBtnBorder = new Border();
            logBtnBorder.Style = this.FindResource("MetroBorder") as Style;
            System.Windows.Controls.Button logButton = new Button();
            StackPanel logContent = new StackPanel();
            logContent.Orientation = Orientation.Vertical;
            TextBlock logTb = new TextBlock();
            logTb.Text = "Logs";
            Image logImg = new Image();
            logImg.Source = new BitmapImage(new Uri(@"Images/LoggerWindow.png", UriKind.Relative));
            logImg.Height = 40;
            logImg.Width = 40;

            logContent.Children.Add(logImg);
            logContent.Children.Add(logTb);

            logButton.Content = logContent;
            logButton.Name = "btnLog";
            logButton.Height = 100;
            logBtnBorder.Child = logButton;
            spButtonList.Children.Add(logBtnBorder);
            logButton.Style = this.FindResource("MetroButton") as Style;
            logButton.Click += btnLoggerView_Click;

            //check for license
            isLicenseActivated = license.CheckActivation();
#else

             
            uaCertificate = new UACertificateSettingsUI();
            uaConfiguration = new UAConfigurationSettingsUI();           
            
            
            //license = new License();

            //opcEngineConfigurationViewModel = new ConfigurationViewModel();
            opcEngineUAConfigurationViewModel = new UAConfigurationViewModel();
            opcEngineLicenseViewModel = new LicenseViewModel();
            IoTSettings = new IoTSettingsUI();
           // if ((DateTime.Parse("01-May-18 12:00:00 AM")).CompareTo(DateTime.Now) < 0)
           // {
           //     btnStartStop.IsEnabled = false;
          //  }
#endif
        }

       
        private void btnMonitoring_Click(object sender, RoutedEventArgs e)
        {
            //if(sunPowerGenMainPage==null)
            //    sunPowerGenMainPage = new SunPowerGen.SunPowerGenMainPage();
            //contentPresenter.Content = sunPowerGenMainPage;

            if(isLicenseActivated)
            {
                if (homePage == null)
                    homePage = new HomePage();
                contentPresenter.Content = homePage;

                
            }
        }



        #endregion End Of Constructor

        #region Main Window Loaded Event
        /// <summary>
        /// Window_Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //for Configuration loaded          
            ConfigurationSettingsUI.elpisServer.OpenLastLoadedProject();
            //if (sunPowerGenMainPage == null)
            //    sunPowerGenMainPage = new SunPowerGen.SunPowerGenMainPage();
            //contentPresenter.Content = sunPowerGenMainPage;
          if(isLicenseActivated)
            {
                if (homePage == null)
                    homePage = new HomePage();
                contentPresenter.Content = homePage;
                ElpisMainWindow.Title = "Elpis OPC Server (Activated)";

            }
            else
            {
                
                if (license == null)
                    license = new License();
                contentPresenter.Content = license;
                ElpisMainWindow.Title = "Elpis OPC Server (license expired)";
            }
#if !SunPowerGen
            ConfigurationSettingsUI.elpisServer.RunTimeDisplay = tbstartStopText.Text;
#endif
            //opcEngineMainWindowViewModel.TitleTxt = txttitle.Text;
            //txttitle.Text = opcEngineMainWindowViewModel.TitleTxt;
            //txttitle.Text = opcEngineMainWindowViewModel.TimerExecutes();

            /// this.contentPresenter.Content = applicationTour;
        }

#endregion End Of Main Window Loaded Event
        private void btnConfiguration_Click(object sender, RoutedEventArgs e)
        {
            if(isLicenseActivated)
            {
                ShowInTaskbar = true;
                this.Visibility = Visibility.Visible;
                configuration.Visibility = Visibility.Visible;
                //ConfigurationSettingsUI.elpisServer.OpenLastLoadedProject();
                this.contentPresenter.Content = configuration;
            }
        }
        /// <summary>
        /// UA Configuration window click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUAConfig_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;
            //To check whether the file is present or not
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(@"SLIKDAUAConfig.xml");
            }
            catch (Exception ex)
            {
                string ErrMessage = ex.Message;

                MessageBox.Show("Could not find the SLIK-DA Configuration File.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                ConfigurationSettingsUI.elpisServer.LoadDataLogCollection("UA Configuration", @"Elpis OPC Server\UA Configuration",
                "Could not find the SLIK-DA Configuration File.", LogStatus.Error);
                return;
            }
            if (uaConfiguration != null)
            {
                uaConfiguration.Visibility = Visibility.Visible;
                this.contentPresenter.Content = uaConfiguration;
            }
            else
            {
                
            }
        }

        private void btnStartStop_Click(object sender, RoutedEventArgs e)
        {
           
            this.Visibility = Visibility.Visible;              
            this.contentPresenter.Content = configuration;            
            this.Visibility = Visibility.Visible;
            #if !SunPowerGen
            ConfigurationSettingsUI.elpisServer.RunTimeDisplay = tbstartStopText.Text;

           
            tbstartStopText.Text = ConfigurationSettingsUI.elpisServer.StartStop();           
            if (tbstartStopText.Text.Contains("Stop")) 
            {
                configuration.TimeSetter();
                
                //configuration.PropertiesPropertyGrid.IsReadOnly = true;
                //tbstartStopText.Text = " Start Server";
                BitmapImage bi3 = new BitmapImage();
                bi3.BeginInit();
                bi3.UriSource = new Uri(@"Images\stop_black.png", UriKind.Relative);//(@"Images\player_stop.png", UriKind.Relative);
                bi3.EndInit();
                startStopImgage.Source = bi3;
                var currentCollection = ConfigurationSettingsUI.elpisServer.LoadDataLogCollection("All", @"Elpis OPC Server/Runtime",
                   "Runtime Engine started by " + WindowsIdentity.GetCurrent().Name + " as Default User.", LogStatus.Information);

                //var currentCollection1 = ConfigurationSettingsUI.elpisServer.LoadDataLogCollection("Internet Of Things", @"Elpis OPC Server\IoT",
                //    "Internet Of Things session started. ", LogStatus.Information);

                //var currentCollection2 = ConfigurationSettingsUI.elpisServer.LoadDataLogCollection("Internet Of Things", @"Elpis OPC Server\IoT",
                //    "Azure IoT Cloud session is ready.", LogStatus.Information);

            }
            else
            {
                //configuration.PropertiesPropertyGrid.IsReadOnly = false;
                //tbstartStopText.Text = " Stop Server";
                BitmapImage bi3 = new BitmapImage();
                bi3.BeginInit();
                bi3.UriSource = new Uri(@"Images\start_black.png", UriKind.Relative);//@"Images\play_1.png", UriKind.Relative);
                bi3.EndInit();
                startStopImgage.Source = bi3;
                var currentCollection = ConfigurationSettingsUI.elpisServer.LoadDataLogCollection("All", @"Elpis OPC Server/Runtime",
                    "Runtime Engine stopped by " +
                    WindowsIdentity.GetCurrent().Name + " as Default User.", LogStatus.Information);
                //WaitWindow window = new WaitWindow();
                //window.Show();                
                //Thread.Sleep(1000);
                //window.Close();
            }
#endif
        }

        /// <summary>
        /// Logger button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLoggerView_Click(object sender, RoutedEventArgs e)
        {
           if(isLicenseActivated)
            {
                if (loggerWindow != null)
                    loggerWindow.Close();
                loggerWindow = new LoggerWindow();
                loggerWindow.Show();
            }
        }

        /// <summary>
        /// License button Click Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLicence_Click(object sender, RoutedEventArgs e)
        {
          if(!isLicenseActivated)
            {
                this.Visibility = Visibility.Visible;
                license.Visibility = Visibility.Visible;
                this.contentPresenter.Content = license;
            }
        }

        private void btnIOT_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;
            IoTSettings.Visibility = Visibility.Visible;
            this.contentPresenter.Content = IoTSettings;
        }

        private void btnUACertificate_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;
            if (uaCertificate != null)
            {               
                uaCertificate.Visibility = Visibility.Visible;
                this.contentPresenter.Content = uaCertificate;
            }
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;
            about.Visibility = Visibility.Visible;
            this.contentPresenter.Content = about;
        }

        /// <summary>
        /// event for closing the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ////opcEngineMainWindowViewModel.LastLoadedProject();

            //ConfigurationSettingsUI.elpisServer.Unregister();
            //ConfigurationSettingsUI.elpisServer.SaveLastLoadedProject();
            ////this.Close();
            //ConfigurationSettingsUI.elpisServer.ClosingServer();
            //this.Visibility = Visibility.Collapsed;
            //if (tbstartStopText.Text.Contains("Stop"))
            //{
            //    MessageBox.Show("Please stop the server to close the application", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Information);
            //}
            //else
            //{
            // }


            //Commented and Added on 17 Feb 2018-Hari
            ShowInTaskbar = false;
            e.Cancel = true;
            //ConfigurationSettingsUI.elpisServer.UnregisterServer();
            //ConfigurationSettingsUI.elpisServer.SaveLastLoadedProject();
            ////this.Close();            
            //this.Visibility = Visibility.Collapsed;
            //ConfigurationSettingsUI.elpisServer.setQualityofTags();
            //Application.Current.Shutdown();


        }
        /// <summary>
        /// click event for button exit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //if (tbstartStopText.Text.Contains("Stop")) 
            //{
            //    //MessageBox.Show("Please stop the server to close the application", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Information);
            //}
            //else
            //{
            //    ConfigurationSettingsUI.elpisServer.UnregisterServer();
            //    ConfigurationSettingsUI.elpisServer.SaveLastLoadedProject();
            //    //this.Close();                
            //    this.Visibility = Visibility.Collapsed;
            //}
        }
        


#region notifyIcon
        private void minimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            // Find the window that contains the control
            Window window = Window.GetWindow(this);

            // Minimize
            window.WindowState =WindowState.Minimized;            
            // Restore
            //window.WindowState = WindowState.Normal;
        }

        private void ElpisMainWindow_Initialized(object sender, EventArgs e)
        {
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.MouseDown += NotifyIcon_Click;
            //notifyIcon.;
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
            Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/Images/Elpis OnlyLogo.ico")).Stream;
            notifyIcon.Icon = new System.Drawing.Icon(iconStream);
            notifyIcon.Visible = true;

            //Use_Notify();

        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            if(this.WindowState==WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                //this.WindowState = WindowState.Normal;
                this.WindowState = WindowState.Maximized;
            }
            //this.Show();
            //this.WindowState = WindowState.Normal;
        }

        private void Use_Notify()
        {
            //notifyIcon.ContextMenuStrip = contextMenuStrip1;
            notifyIcon.BalloonTipText = "This is Elpis OPC Server";
            //notifyIcon.BalloonTipTitle = "Your Application Name";
            notifyIcon.ShowBalloonTip(1);
            //notifyIcon.Click += NotifyIcon_Click1;
            //notifyIcon.BalloonTipClosed += NotifyIcon_BalloonTipClosed;
        }

        //private void NotifyIcon_BalloonTipClosed(object sender, EventArgs e)
        //{
        //    //var thisIcon = (System.Windows.Forms.NotifyIcon)sender;
        //    //thisIcon.Visible = false;
        //    //thisIcon.Dispose();
        //}

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            ShowInTaskbar = true;
            ContextMenu cm = this.FindResource("cntextMenu") as ContextMenu;
            cm.PlacementTarget = sender as ContextMenu;
            cm.IsOpen = true;
        }
#endregion

#region Context menu for Notify
        private void cntextMenu_LostFocus(object sender, RoutedEventArgs e)
        {

        }
        private void ContextMenuMouseLeave(object sender, MouseEventArgs e)
        {
            ContextMenu cm = this.FindResource("cntextMenu") as ContextMenu;
            cm.IsOpen = false;
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // App.Current.Shutdown();
            try
            {
                TaskHelper.isTaskRunning = false;
                ConfigurationSettingsUI.elpisServer.UnregisterServer();
                ConfigurationSettingsUI.elpisServer.SaveLastLoadedProject();
                //this.Close();            
                this.Visibility = Visibility.Collapsed;
                ConfigurationSettingsUI.elpisServer.setQualityofTags();
            }
            catch(Exception ex)
            {
                ConfigurationSettingsUI.elpisServer.LoadDataLogCollection("Configuration", @"Elpis OPC Server\Configuration",
                "Error in Closing application. "+ex.Message, LogStatus.Error);
            }
            Application.Current.Shutdown();
        }
#endregion

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            //ConfigurationSettingsUI.elpisServer.Unregister();
            ConfigurationSettingsUI.elpisServer.SaveLastLoadedProject();
            this.Visibility = Visibility.Hidden;

        }

     
    }
}

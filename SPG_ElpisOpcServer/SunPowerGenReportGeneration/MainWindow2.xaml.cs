using System;
using System.IO;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ElpisOpcServer.SunPowerGen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow2 : Window
    {
        #region Public Members
        //public DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public TcpClient client;                 
        private System.Windows.Forms.NotifyIcon notifyIcon = null;

        ConfigurationSettingsUI configuration { get; set; }
       
       
       // AboutPageUI about { get; set; }
       
        

       

       
     
       

        #endregion End Of Public Members

        #region Constructor

        public MainWindow2()
        {
            InitializeComponent();
           

            spButtonList.Children.Clear();
            spButtonList.Orientation = Orientation.Vertical;
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
            licenseBtnBorder.Child = LicenceButton;
            spButtonList.Children.Add(licenseBtnBorder);
            LicenceButton.Style = this.FindResource("MetroButton") as Style;

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

        }

        private void btnMonitoring_Click(object sender, RoutedEventArgs e)
        {
            SunPowerGen.SunPowerGenMainPage mainPage = new SunPowerGen.SunPowerGenMainPage();
            contentPresenter.Content = mainPage;
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
            ShowInTaskbar = true;
            this.Visibility = Visibility.Visible;
            configuration.Visibility = Visibility.Visible;
            //ConfigurationSettingsUI.elpisServer.OpenLastLoadedProject();
            this.contentPresenter.Content = configuration;
        }
        
       

        

        /// <summary>
        /// event for closing the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {          


            
            ShowInTaskbar = false;
            e.Cancel = true;
            

        }
        
        
        private void btnLoggerView_Click(object sender, RoutedEventArgs e) 
        {
            
        }

        private void btnLicence_Click(object sender, RoutedEventArgs e)
        {
            
        }

        

        

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
           
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
                this.WindowState = WindowState.Normal;
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
            
            ConfigurationSettingsUI.elpisServer.UnregisterServer();
            ConfigurationSettingsUI.elpisServer.SaveLastLoadedProject();                      
            this.Visibility = Visibility.Collapsed;
            ConfigurationSettingsUI.elpisServer.setQualityofTags();
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

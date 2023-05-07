using Elpis.Windows.OPC.Server;
using OPCEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace ElpisOpcServer
{
    /// <summary>
    /// Interaction logic for LoggerWindow.xaml
    /// </summary>
    public partial class LoggerWindow : Window
    {
        public LoggerWindow()
        {
            InitializeComponent();

            LoggerUserControl.ListViewLogger.ItemsSource = ElpisServer.LoggerCollection; //ConfigurationSettingsUI.elpisServer.LoggerCollection;
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            // Find the window that contains the control
            Window window = Window.GetWindow(this);

            // Minimize
            window.WindowState = WindowState.Minimized;
        }

        private void exitbtn_Click(object sender, RoutedEventArgs e)
        {
            Window parent = Window.GetWindow(this);
            parent.Close();
        }

        private void ElpisLoggerWindow_Closed(object sender, EventArgs e)
        {
            LoggerUserControl.ListViewLogger.ItemsSource = null;
            LoggerUserControl.ListViewLogger.Items.Clear();
        }
    }
}

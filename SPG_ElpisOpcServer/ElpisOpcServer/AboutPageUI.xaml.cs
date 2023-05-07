#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
#endregion Namespaces

namespace ElpisOpcServer
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class AboutPageUI : UserControl
    {
        public AboutPageUI()
        {
            InitializeComponent();
            tbxYear.Text = DateTime.Now.Year.ToString();             
        }       

        private void slidebtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void minbtn_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            // Minimize
            window.WindowState = WindowState.Minimized;
        }

        private void closebtn_Click(object sender, RoutedEventArgs e)
        {
            Window parent = Window.GetWindow(this);
            parent.Close();
            //this.Visibility = Visibility.Hidden;
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}

using System;
using System.Collections.Generic;
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

namespace ElpisOpcServer.SunPowerGen
{
    /// <summary>
    /// Interaction logic for GenerateReportMessageBox.xaml
    /// </summary>
    public partial class GenerateReportMessageBox : Window
    {
        internal string SelectedOpeeration { get; set; }
        public GenerateReportMessageBox()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }

        private void GridOfWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch(Exception ex)
            {

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if(button!=null)
            {
                SelectedOpeeration = button.Content.ToString();
                this.Close();
            }

        }

       

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            SelectedOpeeration = null;
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            SelectedOpeeration = null;
            this.Close();
        }
    }
}

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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ElpisOpcServer.SunPowerGen
{
    /// <summary>
    /// Interaction logic for SunPowerGenMainPage.xaml
    /// </summary>
    public partial class SunPowerGenMainPage : UserControl
    {
        public SunPowerGenMainPage()
        {
            InitializeComponent();
        }
        private void btnStrokeTest_Click(object sender, RoutedEventArgs e)
        {
            StrokeTest strokeTestWindow = new StrokeTest();
            strokeTestWindow.Show();
        }
    }
}

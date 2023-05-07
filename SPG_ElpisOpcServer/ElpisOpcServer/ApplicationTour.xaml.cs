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

namespace ElpisOpcServer
{
    /// <summary>
    /// Interaction logic for ApplicationTour.xaml
    /// </summary>
    public partial class ApplicationTour : Page
    {
        public ApplicationTour()
        {
            InitializeComponent();
        }


        public void PlayVideo(object sender, RoutedEventArgs e)
        {            
            VideoPreview.Visibility = Visibility.Collapsed;
            redangVideo.Visibility = Visibility.Visible;
            redangVideo.Play();
        }


        public void PauseVideo(object sender, RoutedEventArgs e)
        {
            VideoPreview.Visibility = Visibility.Collapsed;
            redangVideo.Visibility = Visibility.Visible;
            redangVideo.Pause();
        }


        public void StopVideo(object sender, RoutedEventArgs e)
        {
            VideoPreview.Visibility = Visibility.Collapsed;
            redangVideo.Visibility = Visibility.Visible;
            redangVideo.Stop();
        }
    }
}

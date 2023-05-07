using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ElpisOpcServer.SunPowerGen
{
    /// <summary>
    /// Interaction logic for pumpCustomTooltip.xaml
    /// </summary>
    public partial class pumpCustomTooltip : UserControl
    {
        double startPointX = 0;
        double startPointY = 0;
        private bool isDragging;
        private Point clickPosition;
        private Action<string> CancelAction;
        private int index;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Ypoint"></param>
        /// <param name="Xpoint"></param>
        /// <param name="customTooltipModel"></param>
        public pumpCustomTooltip(double Ypoint, double Xpoint, CustomTooltipModel customTooltipModel)
        {
            

            InitializeComponent();

            startPointX = Xpoint;
            startPointY = Ypoint;
            XaxisTime.Content = customTooltipModel.xAxisTime;
            YaxisValue.Content = customTooltipModel.YAxisValue;
            index = customTooltipModel.Index;
            if (string.IsNullOrEmpty(customTooltipModel.XAxisValue))
            {
                XaxisValue.Visibility = Visibility.Collapsed;
            }
            else
            {
                XaxisValue.Visibility = Visibility.Visible;
                XaxisValue.Content = customTooltipModel.XAxisValue;
            }

            LineControl.X2 = Xpoint;
            LineControl.Y2 = Ypoint;
            // Loaded += OnLoaded;
            ContentPanel.RenderTransform = new TranslateTransform()
            {
                X = Xpoint - 100,
                Y = Ypoint - 50
            };
            LineControl.SetBinding(Line.X1Property, new Binding
            {
                Source = ContentPanel.RenderTransform,
                Path = new PropertyPath("X")
            });
            LineControl.RenderTransformOrigin = new Point(50, 50);
            LineControl.SetBinding(Line.Y1Property, new Binding
            {
                Source = ContentPanel.RenderTransform,
                Path = new PropertyPath("Y")
            });
            //Canvas.SetTop(canvasView, Ypoint);
            //Canvas.SetLeft(canvasView, Xpoint+90);

        }

        /// <summary>
        /// mouse down event to get mouse activity
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = false;
        }

        private void OnBorderLoaded(object sender, RoutedEventArgs e)
        {
            Border border = (Border)sender;
            Canvas.SetLeft(border, 0);
            Canvas.SetTop(border, 0);
        }

        private void OnBorderLeftUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isDragging = false;
            ((UIElement)sender).ReleaseMouseCapture();
        }

        private void OnBorderLeftDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isDragging = true;
            var item = ((UIElement)sender);

            clickPosition = item.RenderTransformOrigin;
            item.CaptureMouse();
        }

        private void OnBorderMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var draggableControl = (FrameworkElement)sender;
            if (isDragging && draggableControl != null)
            {
                Point currentPosition = e.GetPosition(cnv);

                var transform = draggableControl.RenderTransform as TranslateTransform;
                if (transform == null)
                {
                    transform = new TranslateTransform();
                    draggableControl.RenderTransform = transform;
                }

                double x = currentPosition.X - clickPosition.X;
                if (x > 0 && x < (ActualWidth - draggableControl.ActualWidth))
                    transform.X = x;
                double y = currentPosition.Y - clickPosition.Y;
                if (y > 0 && y < (ActualHeight - draggableControl.ActualHeight - 30))
                    transform.Y = y;
            }
        }

        private void btnClose(object sender, RoutedEventArgs e)
        {
            ((Panel)this.Parent).Children.Remove(this);
            if (index != -1)
            {
                foreach (var key in HomePage.PumpTestInformation.TableData.Keys)
                {
                    HomePage.PumpTestInformation.TableData[key].Remove(key + "_" + index);
                }
            }

        }
    }
}


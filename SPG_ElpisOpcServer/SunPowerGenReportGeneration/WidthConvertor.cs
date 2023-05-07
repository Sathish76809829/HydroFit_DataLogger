using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace ElpisOpcServer.SunPowerGen
{
    public class WidthConvertor : MarkupExtension, IValueConverter
    {
        public double Factor { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double val = System.Convert.ToDouble(value);
            return val -Factor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}

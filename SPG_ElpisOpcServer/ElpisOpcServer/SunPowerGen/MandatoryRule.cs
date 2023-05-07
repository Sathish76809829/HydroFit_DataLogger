using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ElpisOpcServer.SunPowerGen
{
    public class MandatoryRule : ValidationRule
    {
        public string PropertyName
        {
            get;
            set;
        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (String.IsNullOrEmpty((string)value))
            {
                if (PropertyName!=null && PropertyName.Length == 0)
                    PropertyName = "Field";
                return new ValidationResult(false, PropertyName + " is mandatory.");
            }
            return ValidationResult.ValidResult;
        }
    }
}

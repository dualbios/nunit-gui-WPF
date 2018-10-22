using System;
using System.Windows;
using System.Windows.Data;

namespace NUnit3GUIWPF.Converters
{
    public class EnumToBoolConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object trueValue, System.Globalization.CultureInfo culture)
        {
            if (value != null && value.GetType().IsEnum)
                return (Equals(value, trueValue));
            else
                return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object trueValue, System.Globalization.CultureInfo culture)
        {
            if (value is bool && (bool)value)
                return trueValue;
            else
                return DependencyProperty.UnsetValue;
        }

        #endregion IValueConverter Members
    }
}
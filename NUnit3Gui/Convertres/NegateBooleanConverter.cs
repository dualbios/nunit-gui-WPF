using System;
using System.Globalization;
using System.Windows.Data;

namespace NUnit3Gui.Convertres
{
    public class NegateBooleanConverter : IValueConverter
    {
        public NegateBooleanConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool val = System.Convert.ToBoolean(value, CultureInfo.InvariantCulture);

            return !val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool val = System.Convert.ToBoolean(value, CultureInfo.InvariantCulture);

            return !val;
        }
    }
}
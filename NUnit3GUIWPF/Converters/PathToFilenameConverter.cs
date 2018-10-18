using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace NUnit3GUIWPF.Converters
{
    public class PathToFilenameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? null : Path.GetFileName(value.ToString().Trim());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
using System;
using System.Globalization;
using System.Windows.Data;
using NUnit3GUIWPF.Models;

namespace NUnit3GUIWPF.Converters
{
    public class ProjectStateToEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ProjectState ps = (ProjectState)value;
            return (ps == ProjectState.Finished || ps == ProjectState.Loaded || ps == ProjectState.NotLoaded);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
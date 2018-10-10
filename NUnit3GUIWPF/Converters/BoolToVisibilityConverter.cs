using System;
using System.Windows;
using System.Windows.Data;

namespace NUnit3GUIWPF.Converters
{
    internal class BoolToVisibilityConverter : IValueConverter
    {
        public BoolToVisibilityConverter(bool isReversed, Visibility falseState)
        {
            IsReversed = isReversed;
            FalseState = falseState;
        }

        public BoolToVisibilityConverter()
        {
            IsReversed = false;
        }

        public Visibility FalseState { get; set; } = Visibility.Collapsed;

        public bool IsReversed { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is bool)) return DependencyProperty.UnsetValue;
            bool val = (bool)value;
            if (IsReversed)
                return !val ? Visibility.Visible : FalseState;
            else
                return val ? Visibility.Visible : FalseState;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is Visibility)) return DependencyProperty.UnsetValue;
            Visibility v = (Visibility)value;
            return v == Visibility.Visible;
        }
    }
}
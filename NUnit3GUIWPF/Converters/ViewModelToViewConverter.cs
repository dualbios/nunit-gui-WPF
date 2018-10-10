using System;
using System.ComponentModel;
using System.Windows;

namespace NUnit3GUIWPF.Converters
{
    public class ViewModelToViewConverter<TViewModel, TView> : TypeConverter where TView : FrameworkElement, new()
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(TViewModel))
                return true;

            return false;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context,
            Type destinationType)
        {
            return destinationType == typeof(UIElement);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
                    System.Globalization.CultureInfo culture, object value)
        {
            TView v = new TView();
            v.DataContext = value;

            return v;
        }

        public override object ConvertTo(ITypeDescriptorContext context,
            System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            TView v = new TView();
            v.DataContext = value;

            return v;
        }
    }
}
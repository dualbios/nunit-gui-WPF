using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;

namespace NUnit3Gui.Convertres
{
    public class ViewModelToViewConverter<TViewModel, TView> : TypeConverter where TView : FrameworkElement, new()
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(TViewModel))
                return true;

            return false;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(UIElement);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return new TView { DataContext = value };
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return new TView { DataContext = value };
        }
    }
}
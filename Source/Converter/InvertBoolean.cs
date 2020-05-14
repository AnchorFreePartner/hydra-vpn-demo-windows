using System;
using System.Globalization;
using System.Windows.Data;

namespace Hydra.Sdk.Wpf.Converter
{
    /// <summary>
    /// <see cref="bool"/> to <see cref="bool"/> converter. true => false, false => true.
    /// </summary>
    [ValueConversion(typeof (bool), typeof(bool))]
    internal class InvertBoolean : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var original = (bool) value;
            return !original;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var original = (bool) value;
            return !original;
        }
    }
}
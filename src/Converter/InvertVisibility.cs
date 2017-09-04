namespace Hydra.Sdk.Wpf.Converter
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// <see cref="bool"/> to <see cref="Visibility"/> converter. true => Visibility.Hidden, false => Visibility.Visible.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InvertVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var original = (bool)value;
            return original ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var original = (Visibility)value;
            return original == Visibility.Hidden;
        }
    }
}
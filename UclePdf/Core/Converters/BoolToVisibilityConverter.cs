using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace UclePdf.Core.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool flag = value is true;
        bool invert = parameter?.ToString()?.Equals("Invert", StringComparison.OrdinalIgnoreCase) == true;
        var result = flag ? Visibility.Visible : Visibility.Collapsed;
        return invert ? (result == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible) : result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility vis)
        {
            bool invert = parameter?.ToString()?.Equals("Invert", StringComparison.OrdinalIgnoreCase) == true;
            bool flag = vis == Visibility.Visible;
            return invert ? !flag : flag;
        }
        return false;
    }
}

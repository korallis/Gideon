using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Gideon.WPF.Presentation.Converters;

/// <summary>
/// Converts null values to Visibility.Collapsed and non-null values to Visibility.Visible
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
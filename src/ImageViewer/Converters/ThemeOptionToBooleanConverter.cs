using System.Globalization;
using System.Windows.Data;
using ImageViewer.Models;

namespace ImageViewer.Converters;

public class ThemeOptionToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ThemeOption themeOption && parameter is string parameterString)
        {
            return Enum.TryParse<ThemeOption>(parameterString, out var targetTheme) && themeOption == targetTheme;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked && parameter is string parameterString)
        {
            return Enum.TryParse<ThemeOption>(parameterString, out var themeOption) ? themeOption : ThemeOption.Dark;
        }
        return Binding.DoNothing;
    }
}
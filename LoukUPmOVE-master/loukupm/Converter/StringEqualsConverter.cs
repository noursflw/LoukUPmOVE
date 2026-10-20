using Microsoft.Maui.Controls;
using System.Globalization;

namespace loukupm.Converter
{
    /// <summary>
    /// Converter to compare a string value with a parameter
    /// Usage: IsVisible="{Binding Type, Converter={local:StringEqualsConverter}, ConverterParameter=unordered_list}"
    /// Returns: true if value == parameter, false otherwise
    /// </summary>
    public class StringEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string stringValue = value.ToString() ?? string.Empty;
            string compareValue = parameter.ToString() ?? string.Empty;

            return stringValue.Equals(compareValue, StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

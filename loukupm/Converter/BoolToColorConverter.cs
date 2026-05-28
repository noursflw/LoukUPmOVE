using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Globalization;

namespace loukupm.Converter
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected)
            {

                if (isSelected)
                    return Color.FromArgb("#000000"); // أسود عند الاختيار
                else
                    return Colors.WhiteSmoke; // شفاف عند عدم الاختيار
            }
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    
    public class BoolToBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected)
            {
                if (isSelected)
                    return Color.FromArgb("#A8883C"); // أصفر عند الاختيار
                else
                    return Colors.Transparent; // شفاف عند عدم الاختيار
            }
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected)
            {
                if (isSelected)
                    return Color.FromArgb("#000000"); // أسود عند الاختيار
                else
                    return Color.FromArgb("#CCCCCC"); // فضي فاتح عند عدم الاختيار
            }
            return Color.FromArgb("#CCCCCC"); // فضي فاتح كقيمة افتراضية
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

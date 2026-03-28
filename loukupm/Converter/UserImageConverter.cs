using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace loukupm.Converter
{
    public class UserImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "imagesafe.png";

            string imageUrl = value as string;

            if (string.IsNullOrWhiteSpace(imageUrl))
                return "imagesafe.png";

            string processedUrl = imageUrl;

            if (processedUrl.Contains("'"))
                processedUrl = processedUrl.Replace("'", "%27");

            if (processedUrl.Contains("\""))
                processedUrl = processedUrl.Replace("\"", "%22");

            if (processedUrl.Contains(" "))
                processedUrl = processedUrl.Replace(" ", "%20");

            return processedUrl;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}

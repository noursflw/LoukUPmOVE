using Microsoft.Maui.Controls;
using System.Globalization;

namespace loukupm.Converter
{
    public class DateToArabicFormatConverter : IValueConverter
    {
        private static readonly Dictionary<string, string> ArabicMonths = new()
        {
            { "January", "يناير" },
            { "February", "فبراير" },
            { "March", "مارس" },
            { "April", "ابريل" },
            { "May", "مايو" },
            { "June", "يونيو" },
            { "July", "يوليو" },
            { "August", "اغسطس" },
            { "September", "سبتمبر" },
            { "October", "اكتوبر" },
            { "November", "نوفمبر" },
            { "December", "ديسمبر" }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value as string))
                return "N/A";

            string dateString = value as string;

            // Try to parse the date string
            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                // Get device language
                string deviceLanguage = CultureInfo.CurrentUICulture.Name;
                bool isArabic = deviceLanguage.StartsWith("ar", StringComparison.OrdinalIgnoreCase);

                string monthName;
                if (isArabic)
                {
                    // If device is Arabic, show Arabic month
                    string monthNameEnglish = date.ToString("MMMM", CultureInfo.InvariantCulture);
                    monthName = ArabicMonths.TryGetValue(monthNameEnglish, out var arabic) ? arabic : monthNameEnglish;
                }
                else
                {
                    // If device is English or any other language, show English month
                    monthName = date.ToString("MMMM", CultureInfo.InvariantCulture);
                }

                // Format: Month\nDay\nYear (with newlines for vertical layout)
                return $"{monthName}\n{date.Day}\n{date.Year}";
            }

            return dateString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using Microsoft.Maui.Controls;
using System.Globalization;
using loukupm.Model;
using loukupm.Langue;

namespace loukupm.Converter
{
    /// <summary>
    /// Converts MultiLanguageText objects to localized strings based on the current app culture.
    /// Supports dynamic language switching without requiring page restart.
    /// </summary>
    public class MultiLanguageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not MultiLanguageText multiText)
                return string.Empty;

            // Use the current app culture from LocalizationResourcesManager
            var currentCulture = LocalizationResourcesManager.Instanse.CurrentCulture;
            string languageCode = currentCulture?.TwoLetterISOLanguageName ?? "en";

            // Delegate to MultiLanguageText's GetText method for consistent fallback logic
            return multiText.GetText(languageCode);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("MultiLanguageConverter does not support reverse conversion.");
        }
    }
}

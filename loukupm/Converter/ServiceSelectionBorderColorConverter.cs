using System.Globalization;
using loukupm.Model;
using Microsoft.Maui.Controls;

namespace loukupm.Converter
{
    public class ServiceSelectionBorderColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Servies service)
            {
                // #D4AF37 ذهبي إذا مختار، #808080 فضي إذا لا
                return service.IsSelected ? Color.Parse("#D4AF37") : Color.Parse("#808080");
            }
            return Color.Parse("#808080"); // فضي افتراضي
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

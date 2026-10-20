using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace loukupm.Converter
{
    public class SelectedTabColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int selectedIndex && parameter is string tabType)
            {
                return tabType switch
                {
                    "upcoming" when selectedIndex == 0 => new Color(255, 215, 0),
                    "previous" when selectedIndex == 1 => new Color(255, 215, 0),
                    "canceled" when selectedIndex == 2 => new Color(255, 215, 0),
                    _ => new Color(153, 153, 153)
                };
            }

            return new Color(153, 153, 153);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

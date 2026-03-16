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
                // ≈–« ﬂ«‰ «·Êﬁ  „Œ «— ? »Ê—œ— –Â»Ì ›ﬁÿ
                if (isSelected)
                    return Color.FromArgb("#FFD700"); // ? –Â»Ì ··»Ê—œ—
                else
                    return Color.FromArgb("#444444"); // —„«œÌ
            }
            return Color.FromArgb("#444444");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter · ÕœÌœ ·Ê‰ «·Œ·›Ì… (BackgroundColor) - Ì»ﬁÏ —„«œÌ œ«∆„«
    /// </summary>
    public class BoolToBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // «·Œ·›Ì… œ«∆„« —„«œÌ »€÷ «·‰Ÿ— ⁄‰ «·«Œ Ì«—
            return Color.FromArgb("#444444");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

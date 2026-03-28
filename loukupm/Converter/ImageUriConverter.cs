using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace loukupm.Converter
{
    /// <summary>
    /// تحويل URLs الصور مع معالجة URLs غير الصحيحة والحروف الخاصة
    /// </summary>
    public class ImageUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                Console.WriteLine("🖼️ Image URL is null, using fallback");
                return "profile_placeholder.png";
            }

            string urlString = value as string;

            // إذا كانت القيمة فارغة أو whitespace فقط
            if (string.IsNullOrWhiteSpace(urlString))
            {
                Console.WriteLine("🖼️ Image URL is empty, using fallback");
                return "profile_placeholder.png";
            }

            // إذا كانت تشير إلى resource محلي (مثل placeholder.png)
            if (!urlString.StartsWith("http://") && !urlString.StartsWith("https://"))
            {
                Console.WriteLine($"🖼️ Local resource: {urlString}");
                return urlString;
            }

            try
            {
                // معالجة الـ URL Encoding للحروف الخاصة
                // تحويل المسافات والحروف الخاصة مثل '
                string encodedUrl = EncodeUrlForSpecialCharacters(urlString);

                Console.WriteLine($"✅ Image URL converted: {encodedUrl}");
                return encodedUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error converting image URL: {ex.Message}");
                return "profile_placeholder.png";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        /// <summary>
        /// معالجة URL Encoding للحروف الخاصة
        /// مثل: Men's_Haircut_1.png -> Men%27s_Haircut_1.png
        /// </summary>
        private string EncodeUrlForSpecialCharacters(string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            // الحروف التي تحتاج encoding
            var specialChars = new Dictionary<string, string>
            {
                { "'", "%27" },      // Single quote
                { "\"", "%22" },     // Double quote
                { " ", "%20" },      // Space
                { "#", "%23" },      // Hash
                { "%", "%25" },      // Percent
                { "&", "%26" },      // Ampersand
                { "?", "%3F" },      // Question mark
            };

            string encoded = url;
            foreach (var kvp in specialChars)
            {
                // فقط encode الأحرف في المسار (بعد آخر /)
                if (url.Contains('/'))
                {
                    var parts = url.Split('/');
                    var lastPart = parts[parts.Length - 1];
                    var encodedLastPart = lastPart.Replace(kvp.Key, kvp.Value);

                    if (lastPart != encodedLastPart)
                    {
                        encoded = encoded.Replace(lastPart, encodedLastPart);
                    }
                }
            }

            return encoded;
        }
    }
}

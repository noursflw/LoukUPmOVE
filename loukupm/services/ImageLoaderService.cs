using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace loukupm.Services
{
    /// <summary>
    /// خدمة متخصصة لتحميل وإدارة الصور من الـ URLs
    /// </summary>
    public class ImageLoaderService
    {
        private readonly HttpClient _httpClient;
        private static readonly ImageLoaderService _instance = new();

        public static ImageLoaderService Instance => _instance;

        public ImageLoaderService()
        {
            // إنشاء HttpClient محسّن
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                {
                    Console.WriteLine($"🔒 Image SSL validation - Errors: {errors}");
                    // في الإنتاج، تحقق من الشهادات بشكل صحيح
                    return true;
                }
            };

            _httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(15) };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MAUI-App/1.0");
        }

        /// <summary>
        /// التحقق من صحة وتحميل صورة من URL
        /// </summary>
        public async Task<bool> ValidateImageUrlAsync(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                Console.WriteLine("🖼️ Image URL is empty");
                return false;
            }

            if (!imageUrl.StartsWith("http://") && !imageUrl.StartsWith("https://"))
            {
                Console.WriteLine($"✅ Local resource: {imageUrl}");
                return true;
            }

            try
            {
                // استخدام GetAsync مع HEAD request
                var request = new HttpRequestMessage(HttpMethod.Head, imageUrl);
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"✅ Image URL valid: {imageUrl}");
                    return true;
                }

                Console.WriteLine($"❌ Image URL returned {response.StatusCode}: {imageUrl}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error validating image URL: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// معالجة URL الصورة وإرجاع URL صحيح أو fallback
        /// </summary>
        public string ProcessImageUrl(string imageUrl, string fallbackImage = "profile_placeholder.png")
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                Console.WriteLine($"🖼️ Empty image URL, using fallback: {fallbackImage}");
                return fallbackImage;
            }

            // إذا كانت صورة محلية
            if (!imageUrl.StartsWith("http://") && !imageUrl.StartsWith("https://"))
            {
                Console.WriteLine($"✅ Local image: {imageUrl}");
                return imageUrl;
            }

            try
            {
                // معالجة الـ URL Encoding
                var processedUrl = EncodeUrl(imageUrl);
                Console.WriteLine($"✅ Processed URL: {processedUrl}");
                return processedUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error processing URL: {ex.Message}");
                return fallbackImage;
            }
        }

        /// <summary>
        /// ترميز الـ URL بشكل صحيح
        /// </summary>
        private string EncodeUrl(string url)
        {
            // تحويل الحروف الخاصة مثل ' إلى %27
            var encoded = Uri.EscapeUriString(url);
            Console.WriteLine($"🔗 URL Encoded: {url} -> {encoded}");
            return encoded;
        }
    }
}

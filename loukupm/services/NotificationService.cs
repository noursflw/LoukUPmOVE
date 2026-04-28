using loukupm.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace loukupm.services
{
    
    public class NotificationService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://test.center-yazan.com/api/notifications";

        public NotificationService()
        {
            var handler = new HttpClientHandler();

#if DEBUG
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MAUI-App/1.0");
        }

       
        private async Task SetAuthorizationHeaderAsync()
        {
            string? token = await SecureStorage.GetAsync("auth_token");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        
        public async Task<List<NotificationItem>> GetAllNotificationsAsync()
        {
            try
            {
                await SetAuthorizationHeaderAsync();

             
                string url = $"{BaseUrl}?per_page=15&cursor=&status=all";

                Console.WriteLine($"📬 Fetching notifications from: {url}");

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ API error: {response.StatusCode}");
                    Console.WriteLine($"❌ Error response: {errorContent.Substring(0, Math.Min(200, errorContent.Length))}...");
                    return new List<NotificationItem>();
                }

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"📬 Response preview: {json.Substring(0, Math.Min(300, json.Length))}...");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

             
                var apiResponse = JsonSerializer.Deserialize<NotificationApiResponseWithItems>(json, options);

                if (apiResponse?.Data == null || apiResponse.Data.Count == 0)
                {
                    Console.WriteLine("⚠️ No notification data in response");
                    return new List<NotificationItem>();
                }

                Console.WriteLine($"✅ Successfully loaded {apiResponse.Data.Count} notifications");

                return apiResponse.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception loading notifications: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                return new List<NotificationItem>();
            }
        }

        
        private class NotificationApiResponseWithItems
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("message")]
            public string Message { get; set; }

            [JsonPropertyName("data")]
            public List<NotificationItem> Data { get; set; } = new();

            [JsonPropertyName("pagination")]
            public NotificationPagination Pagination { get; set; }

            [JsonPropertyName("unread_count")]
            public int UnreadCount { get; set; }
        }

        private class NotificationPagination
        {
            [JsonPropertyName("per_page")]
            public int PerPage { get; set; }

            [JsonPropertyName("next_cursor")]
            public string NextCursor { get; set; }

            [JsonPropertyName("prev_cursor")]
            public string PrevCursor { get; set; }

            [JsonPropertyName("has_more_pages")]
            public bool HasMorePages { get; set; }
        }
    }
}

using loukupm.Model;
using loukupm.Model.ApiResponses;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;

namespace loukupm.services
{
    public class NotificationService
    {
        private const string NotificationsBaseUrl = "https://lookupfriseur.com/api/notifications";
        private const string UnreadNotificationsUrl = "https://lookupfriseur.com/api/notifications/unread";
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;
        private static readonly SemaphoreSlim _authLock = new(1, 1);

        public NotificationService()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };

#if DEBUG
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
#endif

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "MAUI-App/1.0");
            }

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async Task<HttpResponseMessage> SendWithRefreshAsync(Func<Task<HttpResponseMessage>> send)
        {
            var response = await send().ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.Unauthorized)
                return response;

            var authService = loukupm.Services.AppServices.Get<loukupm.Services.AuthenticationService>();
            if (authService == null || !await authService.RefreshTokenAsync().ConfigureAwait(false))
                return response;

            response.Dispose();
            await SetAuthorizationHeaderAsync().ConfigureAwait(false);
            return await send().ConfigureAwait(false);
        }

        private async Task SetAuthorizationHeaderAsync()
        {
            // Serialize access to SecureStorage to avoid race on token reads
            await _authLock.WaitAsync();
            try
            {
                try
                {
                    string? token = await SecureStorage.GetAsync("auth_token");
                    _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
                        ? null
                        : new AuthenticationHeaderValue("Bearer", token);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ [NotificationService] Failed to read auth token: {ex.Message}");
                    _httpClient.DefaultRequestHeaders.Authorization = null;
                }
            }
            finally
            {
                _authLock.Release();
            }
        }

        public async Task<List<NotificationItem>> GetNotificationsAsync(string? cursor = null, int perPage = 15, string status = "all")
        {
            Console.WriteLine("\uD83D\uDD35 LOAD START");

            try
            {
                await SetAuthorizationHeaderAsync();

                var safeCursor = string.IsNullOrWhiteSpace(cursor) ? string.Empty : cursor;
                var url = $"{UnreadNotificationsUrl}?per_page={perPage}&cursor={Uri.EscapeDataString(safeCursor)}&status={Uri.EscapeDataString(status)}";

                Console.WriteLine($"\uD83D\uDCEC [NotificationService] GET {url}");

                using var response = await SendWithRefreshAsync(() => _httpClient.GetAsync(url));
                var payload = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    LogHttpFailure("GetNotificationsAsync", response.StatusCode, payload);
                    Console.WriteLine("\uD83D\uDD34 LOAD FAILED");
                    return new List<NotificationItem>();
                }

                var apiResponse = JsonSerializer.Deserialize<NotificationApiResponse>(payload, JsonOptions);
                if (apiResponse == null)
                {
                    Console.WriteLine("❌ [NotificationService] Failed to deserialize notifications response.");
                    Console.WriteLine("\uD83D\uDD34 LOAD FAILED");
                    return new List<NotificationItem>();
                }

                Console.WriteLine($"✅ [NotificationService] Loaded {apiResponse.Data?.Count ?? 0} notifications. Unread={apiResponse.UnreadCount}");
                Console.WriteLine("\uD83D\uDFE2 LOAD SUCCESS");
                return apiResponse.Data ?? new List<NotificationItem>();
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"⏱️ [NotificationService] Timeout while loading notifications: {ex.Message}");
                Console.WriteLine("\uD83D\uDD34 LOAD FAILED");
                return new List<NotificationItem>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\uD83C\uDF10 [NotificationService] Network error while loading notifications: {ex.Message}");
                Console.WriteLine("\uD83D\uDD34 LOAD FAILED");
                return new List<NotificationItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [NotificationService] Unexpected error while loading notifications: {ex.Message}");
                Console.WriteLine("\uD83D\uDD34 LOAD FAILED");
                return new List<NotificationItem>();
            }
        }

        public async Task<int> GetUnreadCountAsync()
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                var url = $"{NotificationsBaseUrl}/unread-count";
                Console.WriteLine($"\uD83D\uDCEC [NotificationService] GET {url}");

                using var response = await SendWithRefreshAsync(() => _httpClient.GetAsync(url));
                var payload = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    LogHttpFailure("GetUnreadCountAsync", response.StatusCode, payload);
                    return 0;
                }

                var apiResponse = JsonSerializer.Deserialize<NotificationUnreadCountResponse>(payload, JsonOptions);
                var count = apiResponse?.Data?.UnreadCount ?? 0;
                Console.WriteLine($"✅ [NotificationService] Unread count loaded: {count}");
                return count;
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"⏱️ [NotificationService] Timeout while loading unread count: {ex.Message}");
                return 0;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\uD83C\uDF10 [NotificationService] Network error while loading unread count: {ex.Message}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [NotificationService] Unexpected error while loading unread count: {ex.Message}");
                return 0;
            }
        }

        public async Task<bool> MarkAsReadAsync(string notificationId)
        {
            if (string.IsNullOrWhiteSpace(notificationId))
            {
                Console.WriteLine("❌ [NotificationService] MarkAsReadAsync called with empty notificationId.");
                return false;
            }

            try
            {
                await SetAuthorizationHeaderAsync();

                var url = $"{NotificationsBaseUrl}/{Uri.EscapeDataString(notificationId)}/read";
                var path = $"/api/notifications/{notificationId}/read";
                Console.WriteLine($"📡 POST URL: {path}");

                using var response = await SendWithRefreshAsync(() => _httpClient.PostAsync(url, content: null));
                var payload = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"📡 RESPONSE CODE: {(int)response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    LogHttpFailure("MarkAsReadAsync", response.StatusCode, payload);
                    return false;
                }

                Console.WriteLine($"✅ [NotificationService] Notification marked as read: {notificationId}");
                return true;
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"⏱️ [NotificationService] Timeout while marking notification as read: {ex.Message}");
                return false;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"🌐 [NotificationService] Network error while marking notification as read: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [NotificationService] Unexpected error while marking notification as read: {ex.Message}");
                return false;
            }
        }

        private static void LogHttpFailure(string operation, System.Net.HttpStatusCode statusCode, string payload)
        {
            var preview = string.IsNullOrWhiteSpace(payload) ? "<empty>" : payload[..Math.Min(500, payload.Length)];
            Console.WriteLine($"❌ [NotificationService] {operation} failed with HTTP {(int)statusCode} ({statusCode})");
            Console.WriteLine($"❌ [NotificationService] Response: {preview}");
        }
    }
}

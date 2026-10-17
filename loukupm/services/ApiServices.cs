
using loukupm.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace loukupm.Services
{
    public class ApiServices
    {
        private readonly HttpClient _httpClient;

        public ApiServices()
        {
            var factory = AppServices.Get<IHttpClientFactory>();
            if (factory != null)
            {
                _httpClient = factory.CreateClient("ApiClient");
                return;
            }

            var inner = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            HttpMessageHandler pipeline = inner;
            var tokenManager = AppServices.Get<TokenManager>();
            var authService = AppServices.Get<AuthenticationService>();
            if (tokenManager != null && authService != null)
                pipeline = new AuthenticatedHttpClientHandler(tokenManager, authService) { InnerHandler = inner };

            _httpClient = new HttpClient(pipeline)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "LoukUPmOVE-MAUI/1.0");
        }

        private async Task SetAuthorizationHeaderAsync()
        {
            var tokenManager = AppServices.Get<TokenManager>();
            string? token = tokenManager != null
                ? await tokenManager.GetAccessTokenAsync()
                : await SecureStorage.GetAsync(TokenManager.AccessTokenKey);

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

        /// <summary>
        /// Creates a JsonSerializerOptions instance configured for CMS API responses.
        /// Includes custom converters for flexible int? and string deserialization.
        /// </summary>
        private JsonSerializerOptions CreateCmsJsonSerializerOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };

            // Register custom converters for CMS data
            options.Converters.Add(new FlexibleNullableIntConverter());
            options.Converters.Add(new FlexibleStringConverter());

            return options;
        }

        /// <summary>
        /// Verify OTP for email or phone verification.
        /// 
        /// IMPORTANT: Use the 'otp' parameter, NOT 'code'. The API expects field name "otp".
        /// 
        /// EMAIL vs PHONE:
        /// - For email registration: Pass email + otp + "email" as registration_method
        /// - For phone registration: Pass phone + otp + "phone" as registration_method
        /// - The backend validates based on registration_method
        /// 
        /// Returns (success, accessToken, refreshToken, user, statusCode, errorMessage)
        /// </summary>
        public async Task<(bool Success, string AccessToken, string RefreshToken, Auth.UserData User, int StatusCode, string ErrorMessage)> VerifyOtpAsync(
            string email = null,
            string phone = null,
            string otp = null,
            string registrationMethod = "email")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(otp) || otp.Length != 6)
                {
                    return (false, null, null, null, 400, "Invalid OTP format");
                }

                var request = new Auth.OtpVerificationRequest
                {
                    Email = email,
                    Phone = phone,
                    Otp = otp,
                    RegistrationMethod = registrationMethod
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://lookupfriseur.com/api/auth/verify-otp", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"🔐 OTP Verification Response ({response.StatusCode}): {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var otpResponse = JsonSerializer.Deserialize<Auth.OtpVerificationResponse>(responseContent,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (!string.IsNullOrEmpty(otpResponse?.AccessToken))
                        {
                            var tokenManager = AppServices.Get<TokenManager>();
                            if (tokenManager != null)
                            {
                                await tokenManager.SaveTokensAsync(
                                    otpResponse.AccessToken,
                                    otpResponse.RefreshToken,
                                    otpResponse.AccessExpiresAt,
                                    otpResponse.RefreshExpiresAt);
                            }
                        }

                        return (true, otpResponse?.AccessToken, otpResponse?.RefreshToken, otpResponse?.User,
                            (int)response.StatusCode, null);
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"❌ Failed to deserialize OTP response: {ex.Message}");
                        return (false, null, null, null, 500, "Failed to parse server response");
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity) // 422
                {
                    return (false, null, null, null, 422, "Invalid verification code or missing required fields");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden) // 403
                {
                    return (false, null, null, null, 403, "Account not verified or OTP expired");
                }
                else
                {
                    return (false, null, null, null, (int)response.StatusCode, "Verification failed");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ Network error during OTP verification: {ex.Message}");
                return (false, null, null, null, 0, "Network connection error");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Unexpected error during OTP verification: {ex.Message}");
                return (false, null, null, null, 0, "Unexpected error occurred");
            }
        }

        /// <summary>
        /// Resend OTP to email or phone.
        /// Returns (success, statusCode, message, resendAfter)
        /// </summary>
        public async Task<(bool Success, int StatusCode, string Message, int? ResendAfter)> ResendOtpAsync(
            string email = null,
            string phone = null,
            string registrationMethod = "email")
        {
            try
            {
                var request = new Auth.ResendOtpRequest
                {
                    Email = email,
                    Phone = phone,
                    RegistrationMethod = registrationMethod
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://lookupfriseur.com/api/auth/resend-verification-otp", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"📤 Resend OTP Response ({response.StatusCode}): {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var resendResponse = JsonSerializer.Deserialize<Auth.ResendOtpResponse>(responseContent,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        return (true, (int)response.StatusCode, resendResponse?.Message ?? "OTP resent successfully",
                            resendResponse?.ResendAfter);
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"❌ Failed to deserialize resend response: {ex.Message}");
                        return (true, (int)response.StatusCode, "OTP resent successfully", null);
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // 429
                {
                    return (false, 429, "Too many resend attempts. Please try later.", null);
                }
                else
                {
                    return (false, (int)response.StatusCode, "Failed to resend OTP", null);
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ Network error during resend OTP: {ex.Message}");
                return (false, 0, "Network connection error", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Unexpected error during resend OTP: {ex.Message}");
                return (false, 0, "Unexpected error occurred", null);
            }
        }






        public async Task<List<WorkTeam>> GetWorkTeamsAsync()
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                var response = await _httpClient.GetAsync("https://lookupfriseur.com/api/providers?search=&branch_id=1&service_id=&sort_by=first_name&sort_direction=asc&per_page=15");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ API error: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"🧾 Raw JSON: {json}");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // تحويل JSON إلى الكلاس
                var wrapper = JsonSerializer.Deserialize<WorkTeamWrapper>(json, options);

                var list = wrapper?.Data ?? new List<WorkTeam>();

                Console.WriteLine($"✅ Loaded {list.Count} work team members");
                return list;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception while loading work teams: {ex.Message}");
                return null;
            }
        }


        public async Task<(List<Notification> Notifications, int UnreadCount, bool HasMore)> GetNotificationsAsync(string cursor = null, int perPage = 15)
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                // Build URL with pagination support
                string url = $"https://lookupfriseur.com/api/notifications?per_page={perPage}&cursor={cursor ?? ""}&status=all";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Notifications API error: {response.StatusCode}");
                    return (new List<Notification>(), 0, false);
                }

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"📬 Notifications JSON response: {json.Substring(0, Math.Min(200, json.Length))}...");

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResponse = JsonSerializer.Deserialize<Model.ApiResponses.NotificationApiResponse>(json, options);

                if (apiResponse?.Data == null)
                {
                    Console.WriteLine("⚠️ No notification data in response");
                    return (new List<Notification>(), 0, false);
                }

                var notifications = apiResponse.Data.Select(notification => new Notification
                {
                    Id = 0,
                    Title = notification.Title,
                    Message = notification.Message,
                    CreatedAt = notification.CreatedAt,
                    IsRead = notification.IsRead
                }).ToList();

                Console.WriteLine($"✅ Loaded {notifications.Count} notifications, Unread: {apiResponse.UnreadCount}, HasMore: {apiResponse.Pagination?.HasMorePages}");

                return (notifications, apiResponse.UnreadCount, apiResponse.Pagination?.HasMorePages ?? false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception loading notifications: {ex.Message}");
                return (new List<Notification>(), 0, false);
            }
        }


        [Obsolete("Use GetNotificationsAsync instead for pagination support")]
        public async Task<List<Notification>> GetNotificationsLegacyAsync()
        {
            var (notifications, _, _) = await GetNotificationsAsync();
            return notifications;
        }

        public async Task<List<Appointment>> GetUserAppointmentsAsync(User user, string status = "ALL")
        {
            await SetAuthorizationHeaderAsync();

            if (user == null || user.Id == 0)
                throw new Exception("المستخدم غير صالح أو لا يحتوي على رقم معرف (Id).");

            // رابط الطلب مع user_id و status فقط
            string url = $"https://lookupfriseur.com/api/appointments?user_id={user.Id}&status={status}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"📋 Appointments JSON: {json}");

                    var result = JsonSerializer.Deserialize<AppointmentResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var appointments = result?.Data?.Data ?? new List<Appointment>();
                    Console.WriteLine($"✅ Loaded {appointments.Count} appointments");

                    foreach (var apt in appointments)
                    {
                        Console.WriteLine($"  - ID: {apt.Id}, Date: {apt.FormattedDate}, Provider: {apt.Provider?.FullName}");
                    }

                    return appointments;
                }
                else
                {
                    Console.WriteLine($"❌ API error: {response.StatusCode}");
                    return new List<Appointment>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception loading appointments: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                return new List<Appointment>();
            }
        }
        public async Task<User?> GetUserAsync()
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                var response = await _httpClient.GetAsync("https://lookupfriseur.com/api/profile");

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<UserResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception while loading user: {ex.Message}");
                return null;
            }
        }

        public Task<bool> RegisterDeviceAsync(string subscriptionId) =>
            PostDeviceAsync("https://lookupfriseur.com/api/register-device", subscriptionId);

        public Task<bool> DeregisterDeviceAsync(string subscriptionId) =>
            PostDeviceAsync("https://lookupfriseur.com/api/deregister-device", subscriptionId);

        private async Task<bool> PostDeviceAsync(string url, string subscriptionId)
        {
            if (string.IsNullOrWhiteSpace(subscriptionId))
                return false;

            var platform = DeviceInfo.Platform == DevicePlatform.iOS ? "ios" : "android";
            var payload = new Dictionary<string, object>
            {
                ["player_id"] = subscriptionId,
                ["device_type"] = platform
            };

            var (success, statusCode, body) = await SendDevicePayloadAsync(url, payload);
            if (success)
                return true;

            if (statusCode == 422 &&
                TryBuildDeviceRetryPayload(body, subscriptionId, platform, out var retryPayload))
            {
                foreach (var field in payload)
                {
                    if (!retryPayload.ContainsKey(field.Key))
                        retryPayload[field.Key] = field.Value;
                }

                Console.WriteLine($"↻ Retrying {url} with server validation fields");
                (success, _, body) = await SendDevicePayloadAsync(url, retryPayload);
                if (success)
                    return true;
            }

            Console.WriteLine($"❌ Device request failed ({statusCode}) {url}: {body}");
            return false;
        }

        private async Task<(bool Success, int StatusCode, string Body)> SendDevicePayloadAsync(
            string url,
            Dictionary<string, object> payload)
        {
            await SetAuthorizationHeaderAsync();

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync(url, content);
            var body = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"📱 Device API {(int)response.StatusCode} {url}: {body}");
            return (response.IsSuccessStatusCode, (int)response.StatusCode, body);
        }

        private static bool TryBuildDeviceRetryPayload(
            string responseBody,
            string subscriptionId,
            string platform,
            out Dictionary<string, object> payload)
        {
            payload = new Dictionary<string, object>();

            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                if (!doc.RootElement.TryGetProperty("errors", out var errors) ||
                    errors.ValueKind != JsonValueKind.Object)
                {
                    return false;
                }

                foreach (var property in errors.EnumerateObject())
                {
                    var message = property.Value.ToString().ToLowerInvariant();
                    var key = property.Name;
                    var isPlatformField =
                        key.Contains("type", StringComparison.OrdinalIgnoreCase) ||
                        key.Contains("platform", StringComparison.OrdinalIgnoreCase);

                    if (!isPlatformField)
                    {
                        payload[key] = subscriptionId;
                        continue;
                    }

                    if (message.Contains("integer") || message.Contains("numeric"))
                        payload[key] = platform == "ios" ? 0 : 1;
                    else
                        payload[key] = platform;
                }

                return payload.Count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Could not read device validation errors: {ex.Message}");
                return false;
            }
        }





        public async Task<List<Servies>> GetServiesAsync()
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                // Step 1: Read saved application language preference
                string savedCulture = Preferences.Get("AppLanguage", "de-DE");
                Console.WriteLine($"📍 [T&C API] Saved culture value: '{savedCulture}'");

                // Step 2: Convert culture code to ISO language code (e.g., "de-DE" → "de", "ar-AR" → "ar")
                string languageCode = savedCulture.Split('-')[0].ToLower();
                Console.WriteLine($"📍 [T&C API] Generated language code: '{languageCode}'");
                string url = $"https://lookupfriseur.com/api/services?category_id=&featured=&search=&sort_by=sort_order&sort_direction=asc&per_page=15&lang={languageCode}";
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ API error: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"🧾 Raw JSON: {json}");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // ✅ Deserialize حسب الموديل الجديد
                var wrapper = JsonSerializer.Deserialize<ServiesWrapper>(json, options);

                // ✅ الـ Data صارت قائمة جاهزة
                var list = wrapper?.Data ?? new List<Servies>();

                Console.WriteLine($"✅ Loaded {list.Count} services");
                return list;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception while loading services: {ex.Message}");
                return null;
            }
        }


        public async Task<List<Servies>> GetProviderServicesAsync(int providerId)
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                // محاولة أولاً: جلب خدمات البروفايدر من endpoint منفصل
                string url = $"https://lookupfriseur.com/api/providers/{providerId}/services";

                var response = await _httpClient.GetAsync(url);

                // إذا نجحت الطلبة: رجع البيانات
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"📋 Provider {providerId} Services: {json}");

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var wrapper = JsonSerializer.Deserialize<ServiesWrapper>(json, options);
                    var list = wrapper?.Data ?? new List<Servies>();

                    Console.WriteLine($"✅ Loaded {list.Count} services for provider {providerId}");
                    return list;
                }
                else
                {
                    // إذا فشلت: استخدم جميع الخدمات كبديل
                    Console.WriteLine($"⚠️ Provider services endpoint not available ({response.StatusCode})");
                    Console.WriteLine($"⚠️ Returning all services as fallback for provider {providerId}");

                    return await GetServiesAsync() ?? new List<Servies>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception loading provider services: {ex.Message}");
                // في حالة الخطأ: رجع جميع الخدمات كبديل
                return await GetServiesAsync() ?? new List<Servies>();
            }
        }


        public Task<bool> RefreshTokenAsync()
        {
            var authService = AppServices.Get<AuthenticationService>();
            return authService != null
                ? authService.RefreshTokenAsync()
                : Task.FromResult(false);
        }

        // Base URL is configurable via Preferences. Do NOT hardcode in UI.
        private string GetBaseUrl()
        {
            try
            {
                // Store short form like https://lookupfriseur.com in Preferences under key ApiBaseUrl
                var baseUrl = Preferences.Get("ApiBaseUrl", "https://lookupfriseur.com");
                if (string.IsNullOrWhiteSpace(baseUrl))
                    baseUrl = "https://lookupfriseur.com";
                return baseUrl.TrimEnd('/');
            }
            catch
            {
                return "https://lookupfriseur.com";
            }
        }

        private string GetLanguageCode()
        {
            try
            {
                string savedCulture = Preferences.Get("AppLanguage", "de-DE");
                var languageCode = savedCulture.Split('-')[0].ToLower();
                return languageCode;
            }
            catch
            {
                return "de";
            }
        }

        private static string ExtractValidationMessage(JsonDocument parsed, string responseBody)
        {
            if (parsed == null)
                return string.IsNullOrWhiteSpace(responseBody) ? "Validation error" : responseBody;

            try
            {
                var root = parsed.RootElement;
                var parts = new List<string>();

                if (root.TryGetProperty("message", out var messageEl) && messageEl.ValueKind == JsonValueKind.String)
                {
                    var msg = messageEl.GetString();
                    if (!string.IsNullOrWhiteSpace(msg))
                        parts.Add(msg);
                }

                if (root.TryGetProperty("errors", out var errorsEl) && errorsEl.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in errorsEl.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in prop.Value.EnumerateArray())
                            {
                                var text = item.ValueKind == JsonValueKind.String ? item.GetString() : item.ToString();
                                if (!string.IsNullOrWhiteSpace(text))
                                    parts.Add($"{prop.Name}: {text}");
                            }
                        }
                        else
                        {
                            var text = prop.Value.ValueKind == JsonValueKind.String ? prop.Value.GetString() : prop.Value.ToString();
                            if (!string.IsNullOrWhiteSpace(text))
                                parts.Add($"{prop.Name}: {text}");
                        }
                    }
                }

                if (parts.Count > 0)
                    return string.Join(" | ", parts);
            }
            catch
            {
            }

            return string.IsNullOrWhiteSpace(responseBody) ? "Validation error" : responseBody;
        }

        /// <summary>
        /// Internal helper method to send a booking request without retry logic.
        /// </summary>
        private async Task<(bool Success, int StatusCode, string Message, JsonDocument ResponseJson)> SendBookingRequestAsync(object payload)
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                // Ensure authorization header present when calling authenticated endpoints
                var url = $"{GetBaseUrl()}/api/bookings";

                var json = JsonSerializer.Serialize(payload);
                System.Diagnostics.Debug.WriteLine("===== BOOKING REQUEST =====");
                System.Diagnostics.Debug.WriteLine($"URL: {url}");
                System.Diagnostics.Debug.WriteLine($"Payload Type: {payload?.GetType().Name ?? "null"}");
                System.Diagnostics.Debug.WriteLine($"Final JSON: {json}");
                System.Diagnostics.Debug.WriteLine("===========================");
                Console.WriteLine("===== BOOKING REQUEST =====");
                Console.WriteLine($"URL: {url}");
                Console.WriteLine($"Payload Type: {payload?.GetType().Name ?? "null"}");
                Console.WriteLine($"Final JSON: {json}");
                Console.WriteLine("===========================");

                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Required headers
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Remove("Accept-Language");
                _httpClient.DefaultRequestHeaders.Add("Accept-Language", GetLanguageCode());

                var response = await _httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine("===== BOOKING RESPONSE =====");
                System.Diagnostics.Debug.WriteLine($"HTTP Status: {(int)response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Response Headers: {string.Join(", ", response.Headers.Select(h => h.Key))}");
                System.Diagnostics.Debug.WriteLine($"Response Body: {responseBody}");
                System.Diagnostics.Debug.WriteLine("============================");
                Console.WriteLine("===== BOOKING RESPONSE =====");
                Console.WriteLine($"HTTP Status: {(int)response.StatusCode}");
                Console.WriteLine($"Response Headers: {string.Join(", ", response.Headers.Select(h => h.Key))}");
                Console.WriteLine($"Response Body: {responseBody}");
                Console.WriteLine("============================");

                JsonDocument parsed = null;
                try { parsed = JsonDocument.Parse(responseBody); } catch { }

                if (response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    return (true, (int)response.StatusCode, "Created", parsed);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
                {
                    var message = ExtractValidationMessage(parsed, responseBody);
                    System.Diagnostics.Debug.WriteLine($"[422 ERROR] Extracted Message: {message}");
                    Console.WriteLine($"[422 ERROR] Extracted Message: {message}");
                    return (false, (int)response.StatusCode, message, parsed);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return (false, (int)response.StatusCode, "Unauthorized", parsed);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    var message = "Forbidden";
                    try
                    {
                        if (parsed != null && parsed.RootElement.TryGetProperty("message", out var m))
                            message = m.GetString();
                    }
                    catch { }
                    return (false, (int)response.StatusCode, message, parsed);
                }

                if (response.StatusCode == (System.Net.HttpStatusCode)429)
                {
                    // Respect Retry-After when available
                    var retryAfter = response.Headers.RetryAfter?.Delta?.TotalSeconds ?? 0;
                    var msg = "Rate limited" + (retryAfter > 0 ? $". Retry after {retryAfter} seconds." : "");
                    return (false, (int)response.StatusCode, msg, parsed);
                }
                // Other errors (fallback)
                return (false, (int)response.StatusCode, responseBody, parsed);
            }
            catch (HttpRequestException ex)
            {
                return (false, 0, ex.Message, null);
            }
        }

        /// <summary>
        /// Create a booking. Returns tuple: Success, StatusCode, Message, ResponseJson
        /// Handles 401 Unauthorized by attempting to refresh the token and retry once.
        /// </summary>
        public Task<(bool Success, int StatusCode, string Message, JsonDocument ResponseJson)> CreateBookingAsync(object payload)
        {
            return SendBookingRequestAsync(payload);
        }


        /// <summary>
        /// Internal helper method to send a cancel booking request without retry logic.
        /// </summary>
        private async Task<(bool Success, int StatusCode, string Message)> SendCancelBookingRequestAsync(int bookingId, string cancellationReason)
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                var url = $"{GetBaseUrl()}/api/bookings/{bookingId}/cancel";

                var payload = new { cancellation_reason = cancellationReason };
                var json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Remove("Accept-Language");
                _httpClient.DefaultRequestHeaders.Add("Accept-Language", GetLanguageCode());

                var response = await _httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    return (true, (int)response.StatusCode, "Cancelled");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return (false, (int)response.StatusCode, "Unauthorized");

                return (false, (int)response.StatusCode, responseBody);
            }
            catch (Exception ex)
            {
                return (false, 0, ex.Message);
            }
        }

        /// <summary>
        /// Cancel a booking with automatic token refresh on 401.
        /// </summary>
        public Task<(bool Success, int StatusCode, string Message)> CancelBookingAsync(int bookingId, string cancellationReason)
        {
            return SendCancelBookingRequestAsync(bookingId, cancellationReason);
        }

        public async Task<AvailabilityResponse> GetProviderAvailabilityAsync(int serviceId, int providerId, DateTime date, int? branchId = null)
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                var dateStr = date.ToString("yyyy-MM-dd");
                var url = $"{GetBaseUrl()}/api/availability/provider?service_id={serviceId}&provider_id={providerId}&date={dateStr}";
                if (branchId.HasValue) url += $"&branch_id={branchId.Value}";

                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Remove("Accept-Language");
                _httpClient.DefaultRequestHeaders.Add("Accept-Language", GetLanguageCode());

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<AvailabilityResponse>(json, options);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetProviderAvailabilityAsync] Error: {ex.Message}");
                return null;
            }
        }


        public async Task<List<string>> SearchServiessAsync(string searchTerm)
        {
            await SetAuthorizationHeaderAsync();

            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return new List<string>();

                var encoded = Uri.EscapeDataString(searchTerm.Trim());
                var url = $"https://center-yazan.com/api/sandbox/products?search={encoded}";

                var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[SEARCH ERROR] {response.StatusCode}: {json}");
                    return new List<string>();
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // لو رجع Array مباشرة
                try
                {
                    var products = JsonSerializer.Deserialize<List<Servies>>(json, options);
                    if (products != null)
                        return products.Select(p => p.NameServies).Where(n => !string.IsNullOrEmpty(n)).ToList();
                }
                catch (JsonException)
                {
                    // لو رجع { data: [...] } أو { servies: [...] }
                    var wrapper = JsonSerializer.Deserialize<ServiesResponse>(json, options);
                    var products = wrapper?.Data ?? wrapper?.Servies ?? new List<Servies>();
                    return products.Select(p => p.NameServies).Where(n => !string.IsNullOrEmpty(n)).ToList();
                }

                return new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SEARCH EXCEPTION] {ex.Message} -- {ex.StackTrace}");
                return new List<string>();
            }
        }


        public async Task<CreatePaymentIntentResponse?> CreatePaymentIntentAsync(decimal amount, string currency, string email)
        {
            await SetAuthorizationHeaderAsync();

            var payload = new
            {
                amount = (int)(amount * 100), // Stripe expects amount in cents
                currency = currency,
                email = email
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Endpoint Backend لإنشاء PaymentIntent
            var endpoint = "https://your-backend.example.com/api/stripe/create-payment-intent";

            var response = await _httpClient.PostAsync(endpoint, content).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[STRIPE PAYMENT INTENT ERROR] {response.StatusCode}: {body}");
                return null;
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<CreatePaymentIntentResponse>(body, options);
        }

        public async Task<CreateCheckoutSessionResponse?> CreateCheckoutSessionAsync(int amount, string currency, string email)
        {
            await SetAuthorizationHeaderAsync();

            var payload = new CreateCheckoutSessionRequest
            {
                Amount = amount,
                Currency = currency,
                Description = "Test Product",
                SuccessUrl = "myapp://payment-success",
                CancelUrl = "myapp://payment-cancel"
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // استبدل هذا بالـ Backend endpoint الخاص بك
            var endpoint = "https://donate.stripe.com/test_9B63cn516f04amv1fBgnK00";

            var response = await _httpClient.PostAsync(endpoint, content).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[STRIPE CHECKOUT ERROR] {response.StatusCode}: {body}");
                return null;
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<CreateCheckoutSessionResponse>(body, options);
        }

        public class CreateCheckoutSessionResponse
        {
            public string Url { get; set; } = string.Empty;
        }

        /// <summary>
        /// Internal helper method to send update profile request without retry logic.
        /// Returns (response, statusCode) tuple
        /// </summary>
        private async Task<(ProfileUpdateApiResponse Response, int StatusCode)> SendUpdateProfileRequestAsync(string firstName, string avatarImagePath, string phonenumber)
        {
            var response = new ProfileUpdateApiResponse();
            int statusCode = 0;

            try
            {
                await SetAuthorizationHeaderAsync();

                var tokenManager = AppServices.Get<TokenManager>();
                string? token = tokenManager != null
                    ? await tokenManager.GetAccessTokenAsync()
                    : await SecureStorage.GetAsync(TokenManager.AccessTokenKey);
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("❌ Authorization: No token found");
                    response.Success = false;
                    response.Message = "خطأ في المصادقة - لم يتم العثور على token";
                    return (response, 0);
                }
                Console.WriteLine($"✅ Authorization: Bearer token present (length: {token.Length})");

                using (var form = new MultipartFormDataContent())
                {
                    if (!string.IsNullOrWhiteSpace(firstName))
                    {
                        form.Add(new StringContent(firstName.Trim()), "first_name");
                        Console.WriteLine($"📋 Field added: first_name = '{firstName.Trim()}'");
                    }
                    if (!string.IsNullOrWhiteSpace(phonenumber))
                    {
                        form.Add(new StringContent(phonenumber.Trim()), "phone");
                        Console.WriteLine($"📋 Field added: phone = '{phonenumber.Trim()}'");
                    }
                    if (!string.IsNullOrWhiteSpace(avatarImagePath) && File.Exists(avatarImagePath))
                    {
                        try
                        {
                            byte[] fileBytes = await File.ReadAllBytesAsync(avatarImagePath);
                            var fileName = Path.GetFileName(avatarImagePath);
                            var fileContent = new ByteArrayContent(fileBytes);
                            string mimeType = GetMimeType(avatarImagePath);
                            fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
                            form.Add(fileContent, "image", fileName);
                            Console.WriteLine($"📋 Field added: image = file '{fileName}' (size: {fileBytes.Length} bytes, content-type: {mimeType})");
                        }
                        catch (Exception fileEx)
                        {
                            Console.WriteLine($"❌ Error reading avatar file: {fileEx.Message}");
                            response.Success = false;
                            response.Message = $"خطأ في قراءة الصورة: {fileEx.Message}";
                            return (response, 0);
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(avatarImagePath))
                    {
                        Console.WriteLine($"⚠️ Avatar file not found at path: {avatarImagePath}");
                    }

                    Console.WriteLine("📤 Sending update request to API:");
                    Console.WriteLine($"   URL: https://lookupfriseur.com/api/profile");
                    Console.WriteLine($"   Method: POST");
                    Console.WriteLine($"   Content-Type: multipart/form-data");
                    Console.WriteLine($"   Authorization: Bearer [token]");

                    var httpResponse = await _httpClient.PostAsync("https://lookupfriseur.com/api/profile", form);
                    statusCode = (int)httpResponse.StatusCode;
                    Console.WriteLine($"📊 Response Status: {httpResponse.StatusCode}");
                    string responseBody = await httpResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"📄 Response Body: {responseBody}");

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        try
                        {
                            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                            var parsedResponse = JsonSerializer.Deserialize<ProfileUpdateApiResponse>(responseBody, options);

                            if (parsedResponse != null)
                            {
                                response = parsedResponse;
                                response.Success = parsedResponse.Success ?? true;
                                Console.WriteLine($"✅ Response parsed successfully");
                                return (response, statusCode);
                            }
                            else
                            {
                                Console.WriteLine($"⚠️ Response parsed as null");
                                response.Success = true;
                                response.Message = "تم التحديث (استجابة فارغة)";
                                return (response, statusCode);
                            }
                        }
                        catch (JsonException jsonEx)
                        {
                            Console.WriteLine($"⚠️ JSON parsing failed: {jsonEx.Message}");
                            response.Success = true;
                            response.Message = "تم التحديث (لم نتمكن من قراءة الاستجابة)";
                            return (response, statusCode);
                        }
                    }

                    if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Console.WriteLine("❌ Unauthorized (401) - Token invalid or expired");
                        response.Success = false;
                        response.Message = "Unauthorized";
                        return (response, statusCode);
                    }

                    if (httpResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        Console.WriteLine($"❌ Bad Request (400): {responseBody}");
                        response.Success = false;
                        response.Message = "البيانات المدخلة غير صحيحة";
                        return (response, statusCode);
                    }

                    if (httpResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
                    {
                        Console.WriteLine($"❌ Unprocessable Entity (422): {responseBody}");
                        response.Success = false;
                        response.Message = "فشل التحقق من البيانات";
                        return (response, statusCode);
                    }

                    Console.WriteLine($"❌ API Error: {httpResponse.StatusCode} - {responseBody}");
                    response.Success = false;
                    response.Message = $"خطأ: {(int)httpResponse.StatusCode}";
                    return (response, statusCode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception: {ex.Message}");
                Console.WriteLine($"❌ Stack Trace: {ex.StackTrace}");
                response.Success = false;
                response.Message = $"خطأ: {ex.Message}";
                return (response, 0);
            }
        }

        /// <summary>
        /// Update user profile with automatic token refresh on 401.
        /// </summary>
        public async Task<ProfileUpdateApiResponse> UpdateUserProfileAsync(string firstName, string avatarImagePath, string phonenumber)
        {
            var (result, _) = await SendUpdateProfileRequestAsync(firstName, avatarImagePath, phonenumber);
            return result;
        }

        /// <summary>
        /// DELETE /api/profile with current_password and confirmation.
        /// </summary>
        private async Task<string?> SendDeleteProfileRequestAsync(string currentPassword)
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                var tokenManager = AppServices.Get<TokenManager>();
                var token = tokenManager != null
                    ? await tokenManager.GetAccessTokenAsync()
                    : await SecureStorage.GetAsync(TokenManager.AccessTokenKey);

                if (string.IsNullOrWhiteSpace(token))
                {
                    return "NoToken";
                }

                var payload = new Auth.DeleteProfileRequest
                {
                    CurrentPassword = currentPassword,
                    Confirmation = true
                };

                var json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{GetBaseUrl()}/api/profile";
                using var request = new HttpRequestMessage(HttpMethod.Delete, url)
                {
                    Content = content
                };

                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                Console.WriteLine($"🗑️ Delete profile DELETE {url}");

                var httpResponse = await _httpClient.SendAsync(request);
                var responseBody = await httpResponse.Content.ReadAsStringAsync();

                Console.WriteLine($"📊 Delete profile response: {httpResponse.StatusCode}");
                Console.WriteLine($"📄 Delete profile body: {responseBody}");

                if (httpResponse.IsSuccessStatusCode)
                {
                    return null;
                }

                if (httpResponse.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    return "WrongPassword";
                }

                if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return "Unauthorized";
                }

                return "Error";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "Error";
            }
        }

        /// <summary>
        /// Delete current user profile with automatic token refresh on 401.
        /// </summary>
        public Task<string?> DeleteCurrentUserProfileAsync(string currentPassword)
        {
            return SendDeleteProfileRequestAsync(currentPassword);
        }

        /// <summary>
        /// Change the authenticated user's password via POST api/profile/change-password.
        /// Body: current_password, password, password_confirmation.
        /// </summary>
        public async Task<(bool Success, int StatusCode, string ErrorMessage)> ChangePasswordAsync(
            string currentPassword,
            string password,
            string passwordConfirmation)
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                var request = new Auth.ChangePasswordRequest
                {
                    CurrentPassword = currentPassword,
                    Password = password,
                    PasswordConfirmation = passwordConfirmation
                };

                var json = JsonSerializer.Serialize(request);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{GetBaseUrl()}/api/profile/change-password";
                Console.WriteLine($"🔐 Change password POST {url}");

                var response = await _httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"🔐 Change password response ({response.StatusCode}): {responseBody}");

                if (response.IsSuccessStatusCode)
                {
                    return (true, (int)response.StatusCode, null);
                }

                return (false, (int)response.StatusCode, ParseErrorMessage(responseBody));
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ Network error during change password: {ex.Message}");
                return (false, 0, "Network connection error");
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"❌ Timeout during change password: {ex.Message}");
                return (false, 0, "Request timeout");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Unexpected error during change password: {ex.Message}");
                return (false, 0, ex.Message);
            }
        }

        /// <summary>
        /// Detect MIME type based on file extension
        /// </summary>
        private string GetMimeType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

        /// <summary>
        /// Fetch dynamic settings from the API.
        /// CRITICAL: Do NOT hardcode settings toggles - always fetch from API.
        /// Includes i18n support via Accept-Language header.
        /// </summary>
        public async Task<List<SettingItem>> GetSettingsAsync()
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                string languageCode = GetLanguageCode();
                if (languageCode != "ar" && languageCode != "de" && languageCode != "en")
                    languageCode = "de";

                _httpClient.DefaultRequestHeaders.Remove("Accept-Language");
                _httpClient.DefaultRequestHeaders.Add("Accept-Language", languageCode);

                var url = $"{GetBaseUrl()}/api/settings?lang={Uri.EscapeDataString(languageCode)}";
                Console.WriteLine($"📥 Fetching settings (language: {languageCode}) from {url}");

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Failed to get settings: {response.StatusCode}");
                    return new List<SettingItem>();
                }

                var json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<SettingsResponse>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                Console.WriteLine($"📌 Settings loaded: {result?.Data?.Count ?? 0} items");
                return result?.Data ?? new List<SettingItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception while getting settings: {ex.Message}");
                return new List<SettingItem>();
            }
        }
        public async Task<bool> UpdateSettingAsync(string key, bool value)
        {
            await SetAuthorizationHeaderAsync();

            var payload = new
            {
                value = value
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PatchAsync(
                $"https://lookupfriseur.com/api/settings/{key}",
                content
            );

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateUserLocaleAsync(string languageCode)
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                if (string.IsNullOrWhiteSpace(languageCode))
                    return false;

                using var form = new MultipartFormDataContent();
                form.Add(new StringContent(languageCode), "locale");

                var response = await _httpClient.PostAsync($"{GetBaseUrl()}/api/profile", form);
                Console.WriteLine($"[UpdateUserLocaleAsync] {response.StatusCode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateUserLocaleAsync] {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cancel a booking/appointment by ID
        /// </summary>
        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            await SetAuthorizationHeaderAsync();

            try
            {
                string url = $"https://lookupfriseur.com/api/bookings/{bookingId}/cancel";

                // POST request without body (some APIs require this for cancel operations)
                var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"✅ Booking {bookingId} cancelled successfully");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Cancel booking failed: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception while cancelling booking {bookingId}: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Get AboutUs page data from API
        /// </summary>
        public async Task<AboutUsResponse> GetAboutUsAsync()
        {
            try
            {
                string languageCode = GetLanguageCode();
                string apiUrl = $"https://lookupfriseur.com/api/about-us?locale={Uri.EscapeDataString(languageCode)}";
                Console.WriteLine($"📍 [AboutUs] Final request URL: '{apiUrl}'");

                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ AboutUs API error: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"✅ AboutUs data retrieved successfully");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<AboutUsResponse>(json, options);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception while loading AboutUs data: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get Home Slider data from API
        /// </summary>
        public async Task<HomeSliderResponse> GetHomeSlidersAsync()
        {
            try
            {
                // Step 1: Read saved application language preference
                string savedCulture = Preferences.Get("AppLanguage", "de-DE");
                Console.WriteLine($"📍 [HomeSliders] Saved culture value: '{savedCulture}'");

                // Step 2: Convert culture code to ISO language code (e.g., "de-DE" → "de", "ar-AR" → "ar")
                string languageCode = savedCulture.Split('-')[0].ToLower();
                Console.WriteLine($"📍 [HomeSliders] Generated language code: '{languageCode}'");

                // Step 3: Build URL with language query parameter
                string apiUrl = $"https://lookupfriseur.com/api/sliders/home?locale={languageCode}";
                Console.WriteLine($"📍 [HomeSliders] Final request URL: '{apiUrl}'");

                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Home Sliders API error: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"✅ Home Sliders data retrieved successfully");

                // Optional: handle double-encoded JSON (same pattern as CMS pages)
                if (json.StartsWith("\"") && json.EndsWith("\""))
                {
                    Console.WriteLine($"⚠️ Detected double-encoded JSON response, attempting to decode...");
                    try
                    {
                        json = System.Text.RegularExpressions.Regex.Unescape(json.Substring(1, json.Length - 2));
                        Console.WriteLine($"✅ Successfully decoded double-encoded JSON");
                    }
                    catch (Exception decodeEx)
                    {
                        Console.WriteLine($"⚠️ Failed to decode JSON: {decodeEx.Message}");
                    }
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<HomeSliderResponse>(json, options);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception while loading Home Sliders data: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get Terms and Conditions content from CMS API with dynamic language parameter
        /// </summary>
        public async Task<TermsConditionsResponse> GetTermsAndConditionsAsync()
        {
            try
            {
                // Step 1: Read saved application language preference
                string savedCulture = Preferences.Get("AppLanguage", "de-DE");
                Console.WriteLine($"📍 [T&C API] Saved culture value: '{savedCulture}'");

                // Step 2: Convert culture code to ISO language code (e.g., "de-DE" → "de", "ar-AR" → "ar")
                string languageCode = savedCulture.Split('-')[0].ToLower();
                Console.WriteLine($"📍 [T&C API] Generated language code: '{languageCode}'");

                // Step 3: Build URL with language query parameter
                string apiUrl = $"https://lookupfriseur.com/api/pages/terms-conditions?lang={languageCode}";
                Console.WriteLine($"📍 [T&C API] Final request URL: '{apiUrl}'");

                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Terms & Conditions API error: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"✅ Terms & Conditions data retrieved successfully");

                // Handle potential double-encoded JSON
                // Example: "{\"success\":true,\"data\":{...}}" instead of {"success":true,"data":{...}}
                if (json.StartsWith("\"") && json.EndsWith("\""))
                {
                    Console.WriteLine($"⚠️ Detected double-encoded JSON response, attempting to decode...");
                    try
                    {
                        // Remove surrounding quotes and unescape
                        json = System.Text.RegularExpressions.Regex.Unescape(json.Substring(1, json.Length - 2));
                        Console.WriteLine($"✅ Successfully decoded double-encoded JSON");
                    }
                    catch (Exception decodeEx)
                    {
                        Console.WriteLine($"⚠️ Failed to decode double-encoded JSON: {decodeEx.Message}");
                        // Continue with original json if decode fails
                    }
                }

                var options = CreateCmsJsonSerializerOptions();

                var result = JsonSerializer.Deserialize<TermsConditionsResponse>(json, options);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception while loading Terms & Conditions data: {ex.Message}");
                return null;
            }
        }

        public class CreateCheckoutSessionRequest
        {
            public int Amount { get; set; }
            public string Currency { get; set; } = "eur";
            public string SuccessUrl { get; set; } = string.Empty;
            public string CancelUrl { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }

        public class WorkTeamWrapper
        {
            public List<WorkTeam> Data { get; set; }
        }

        public class ServiesResponse
        {
            public List<Servies> Data { get; set; }
            public List<Servies> Servies { get; set; }
        }

        public class CategoriesResponse
        {
            public List<string> Categories { get; set; }
            public string Message { get; set; } = string.Empty;
            public bool Success { get; set; }
        }

        public class CreatePaymentIntentResponse
        {
            public string ClientSecret { get; set; } = string.Empty;
        }

        public class ProfileUpdateApiResponse
        {
            public bool? Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public ProfileData Data { get; set; }
        }

        /// <summary>
        /// Get Privacy Policy content from CMS API with dynamic language parameter
        /// </summary>
        public async Task<PrivacyPolicyResponse> GetPrivacyPolicyAsync()
        {
            try
            {
                // Step 1: Read saved application language preference
                string savedCulture = Preferences.Get("AppLanguage", "de-DE");
                Console.WriteLine($"📍 [PrivacyPolicy] Saved culture value: '{savedCulture}'");

                // Step 2: Convert culture code to ISO language code (e.g., "de-DE" → "de", "ar-AR" → "ar")
                string languageCode = savedCulture.Split('-')[0].ToLower();
                Console.WriteLine($"📍 [PrivacyPolicy] Generated language code: '{languageCode}'");

                // Step 3: Build URL with language query parameter
                string apiUrl = $"https://lookupfriseur.com/api/pages/privacy-policy?lang={languageCode}";
                Console.WriteLine($"📍 [PrivacyPolicy] Final request URL: '{apiUrl}'");

                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Privacy Policy API error: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"✅ Privacy Policy data retrieved successfully");

                // Handle potential double-encoded JSON
                // Example: "{\"success\":true,\"data\":{...}}" instead of {"success":true,"data":{...}}
                if (json.StartsWith("\"") && json.EndsWith("\""))
                {
                    Console.WriteLine($"⚠️ Detected double-encoded JSON response, attempting to decode...");
                    try
                    {
                        // Remove surrounding quotes and unescape
                        json = System.Text.RegularExpressions.Regex.Unescape(json.Substring(1, json.Length - 2));
                        Console.WriteLine($"✅ Successfully decoded double-encoded JSON");
                    }
                    catch (Exception decodeEx)
                    {
                        Console.WriteLine($"⚠️ Failed to decode double-encoded JSON: {decodeEx.Message}");
                        // Continue with original json if decode fails
                    }
                }

                var options = CreateCmsJsonSerializerOptions();

                var result = JsonSerializer.Deserialize<PrivacyPolicyResponse>(json, options);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception while loading Privacy Policy data: {ex.Message}");
                return null;
            }
        }


        public async Task<(bool Success, int StatusCode, string ErrorMessage, int? RetryAfter)> SendPhoneOtpAsync(
            string phone)
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                if (string.IsNullOrWhiteSpace(phone))
                {
                    return (false, 400, "Phone number is required", null);
                }

                var request = new
                {
                    phone = phone
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    "https://lookupfriseur.com/api/profile/phone/send-otp",
                    content);

                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"📤 Send OTP Response ({response.StatusCode}): {responseContent}");

                // Success case (200-299)
                if (response.IsSuccessStatusCode)
                {
                    return (true, (int)response.StatusCode, null, null);
                }

                // Extract retry-after from header if present
                int? retryAfter = null;
                if (response.Headers.TryGetValues("Retry-After", out var retryValues))
                {
                    if (int.TryParse(retryValues.FirstOrDefault(), out var retrySeconds))
                    {
                        retryAfter = retrySeconds;
                    }
                }

                // HTTP 429 - Too Many Requests (Rate Limit)
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    return (false, 429, "Too Many Attempts", retryAfter ?? 60);
                }

                // HTTP 400/422 - Bad Request or Validation Error
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest ||
                    response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
                {
                    string errorMessage = ParseErrorMessage(responseContent);
                    return (false, (int)response.StatusCode, errorMessage, retryAfter);
                }

                // HTTP 403 - Forbidden
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    string errorMessage = ParseErrorMessage(responseContent);
                    return (false, 403, errorMessage ?? "Phone verification failed", retryAfter);
                }

                // Other HTTP errors
                string genericError = ParseErrorMessage(responseContent);
                return (false, (int)response.StatusCode, genericError ?? "Failed to send OTP", retryAfter);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ Network error during send OTP: {ex.Message}");
                return (false, 0, "Network connection error", null);
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"❌ Timeout during send OTP: {ex.Message}");
                return (false, 0, "Request timeout", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Unexpected error during send OTP: {ex.Message}");
                return (false, 0, $"Unexpected error: {ex.Message}", null);
            }
        }
        /// <summary>
        /// Verify phone OTP with comprehensive error handling.
        /// Returns (success, statusCode, errorMessage, retryAfter)
        /// - success: true if OTP verification succeeded
        /// - statusCode: HTTP status code from the API (200, 400, 429, etc.)
        /// - errorMessage: Detailed error message from API or generic message
        /// - retryAfter: Seconds to wait before retry (from header or response body)
        /// </summary>
        public async Task<(bool Success, int StatusCode, string ErrorMessage, int? RetryAfter)> VerifyPhoneOtpAsync(
            string phone,
            string otp)
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(otp))
                {
                    return (false, 400, "Phone and OTP are required", null);
                }

                var request = new
                {
                    phone = phone,
                    otp = otp
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    "https://lookupfriseur.com/api/profile/phone/verify-otp",
                    content);

                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"🔐 Phone OTP Verification Response ({response.StatusCode}): {responseContent}");

                // Success case (200-299)
                if (response.IsSuccessStatusCode)
                {
                    return (true, (int)response.StatusCode, null, null);
                }

                // Extract retry-after from header if present
                int? retryAfter = null;
                if (response.Headers.TryGetValues("Retry-After", out var retryValues))
                {
                    if (int.TryParse(retryValues.FirstOrDefault(), out var retrySeconds))
                    {
                        retryAfter = retrySeconds;
                    }
                }

                // HTTP 429 - Too Many Requests (Rate Limit)
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    return (false, 429, "Too Many Attempts", retryAfter ?? 60);
                }

                // HTTP 400/422 - Bad Request or Validation Error
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest ||
                    response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
                {
                    // Try to parse structured error response
                    string errorMessage = ParseErrorMessage(responseContent);
                    return (false, (int)response.StatusCode, errorMessage, retryAfter);
                }

                // HTTP 403 - Forbidden (Throttled, Account issues)
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    string errorMessage = ParseErrorMessage(responseContent);
                    return (false, 403, errorMessage ?? "Account verification failed or OTP expired", retryAfter);
                }

                // Other HTTP errors
                string genericError = ParseErrorMessage(responseContent);
                return (false, (int)response.StatusCode, genericError ?? "Verification failed", retryAfter);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ Network error during phone OTP verification: {ex.Message}");
                return (false, 0, "Network connection error", null);
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"❌ Timeout during phone OTP verification: {ex.Message}");
                return (false, 0, "Request timeout", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Unexpected error during phone OTP verification: {ex.Message}");
                return (false, 0, $"Unexpected error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Attempts to extract error messages from various API response formats.
        /// Handles common formats: { message }, { error }, { errors[] }, etc.
        /// </summary>
        private string ParseErrorMessage(string responseContent)
        {
            if (string.IsNullOrWhiteSpace(responseContent))
                return null;

            try
            {
                using (JsonDocument doc = JsonDocument.Parse(responseContent))
                {
                    var root = doc.RootElement;

                    // Try to get message field
                    if (root.TryGetProperty("message", out var messageProp))
                    {
                        return messageProp.GetString();
                    }

                    // Try to get error field
                    if (root.TryGetProperty("error", out var errorProp))
                    {
                        if (errorProp.ValueKind == JsonValueKind.String)
                        {
                            return errorProp.GetString();
                        }

                        if (errorProp.TryGetProperty("message", out var errorMessageProp))
                        {
                            return errorMessageProp.GetString();
                        }
                    }

                    // Try to get errors array (first item's message)
                    if (root.TryGetProperty("errors", out var errorsProp))
                    {
                        if (errorsProp.ValueKind == JsonValueKind.Array && errorsProp.GetArrayLength() > 0)
                        {
                            var firstError = errorsProp[0];
                            if (firstError.TryGetProperty("message", out var firstErrorMsg))
                            {
                                return firstErrorMsg.GetString();
                            }
                            if (firstError.ValueKind == JsonValueKind.String)
                            {
                                return firstError.GetString();
                            }
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"⚠️ Failed to parse error message: {ex.Message}");
            }

            return null;
        }
        public async Task<ImpressumResponse> GetImpressumAsync()
        {
            try
            {
                // Step 1: Read saved application language preference
                string savedCulture = Preferences.Get("AppLanguage", "de-DE");
                Console.WriteLine($"📍 [Impressum] Saved culture value: '{savedCulture}'");

                // Step 2: Convert culture code to ISO language code (e.g., "de-DE" → "de", "ar-AR" → "ar")
                string languageCode = savedCulture.Split('-')[0].ToLower();
                Console.WriteLine($"📍 [Impressum] Generated language code: '{languageCode}'");

                // Step 3: Build URL with language query parameter
                string apiUrl = $"https://lookupfriseur.com/api/pages/impressum?lang={languageCode}";
                Console.WriteLine($"📍 [Impressum] Final request URL: '{apiUrl}'");

                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Impressum API error: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"✅ Impressum data retrieved successfully");

                // Handle potential double-encoded JSON
                // Example: "{\"success\":true,\"data\":{...}}" instead of {"success":true,"data":{...}}
                if (json.StartsWith("\"") && json.EndsWith("\""))
                {
                    Console.WriteLine($"⚠️ Detected double-encoded JSON response, attempting to decode...");
                    try
                    {
                        // Remove surrounding quotes and unescape
                        json = System.Text.RegularExpressions.Regex.Unescape(json.Substring(1, json.Length - 2));
                        Console.WriteLine($"✅ Successfully decoded double-encoded JSON");
                    }
                    catch (Exception decodeEx)
                    {
                        Console.WriteLine($"⚠️ Failed to decode double-encoded JSON: {decodeEx.Message}");
                        // Continue with original json if decode fails
                    }
                }

                var options = CreateCmsJsonSerializerOptions();

                var result = JsonSerializer.Deserialize<ImpressumResponse>(json, options);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception while loading Impressum data: {ex.Message}");
                return null;
            }
        }

        // ============================================================
        // REMINDER API METHODS
        // ============================================================

        /// <summary>
        /// Get reminder configuration options for the current language.
        /// Sends Accept-Language header with current app language.
        /// </summary>
        /// <summary>
        /// Fetch reminder options and channel configuration from the API.
        /// Includes has_active_channel status and available notification channels.
        /// CRITICAL: Do NOT hardcode reminder options - always fetch from API.
        /// </summary>
        public async Task<ReminderOptionsResponse> GetReminderOptionsAsync()
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                // Get current language and ensure i18n compliance
                string savedCulture = Preferences.Get("AppLanguage", "en-US");
                string languageCode = savedCulture.Split('-')[0].ToLower();

                // Ensure valid language code (ar, en, de)
                if (languageCode != "ar" && languageCode != "de" && languageCode != "en")
                    languageCode = "en";

                string url = $"{GetBaseUrl()}/api/appointments/reminders/options?lang={Uri.EscapeDataString(languageCode)}";

                _httpClient.DefaultRequestHeaders.Remove("Accept-Language");
                _httpClient.DefaultRequestHeaders.Add("Accept-Language", languageCode);

                Console.WriteLine($"📥 Fetching reminder options (language: {languageCode}) from {url}");

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Failed to get reminder options: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ReminderOptionsResponse>(json);

                // Log channel availability for debugging
                if (result?.Data != null)
                {
                    Console.WriteLine($"📌 Reminder options loaded: {result.Data.Options.Count} options");
                    Console.WriteLine($"📌 Has active channel: {result.Data.HasActiveChannel}");
                    var channels = result.Data.GetChannels();
                    if (channels != null && channels.Count > 0)
                    {
                        foreach (var ch in channels)
                        {
                            Console.WriteLine($"   - {ch.Type}: enabled={ch.Enabled}, deliverable={ch.Deliverable}");
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception while getting reminder options: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create or update a reminder for an appointment.
        /// CRITICAL: POST is an idempotent upsert operation - replaces existing reminder if present.
        /// 
        /// ARCHITECTURAL RULES:
        /// - Always send ONLY appointment_id and offset_hours (NEVER send absolute datetimes)
        /// - Do NOT call DELETE before POST - use POST directly to replace
        /// - Do NOT send timezone-aware datetimes to prevent server/client mismatch
        /// </summary>
        public async Task<(bool Success, int StatusCode, string Message, ReminderResponse ResponseData)> CreateReminderAsync(int appointmentId, int offsetHours)
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                var url = $"{GetBaseUrl()}/api/appointments/reminders";

                var request = new CreateReminderRequest
                {
                    AppointmentId = appointmentId,
                    OffsetHours = offsetHours
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"📤 Creating/updating reminder - URL: {url}");
                Console.WriteLine($"📤 Request payload: {json}");

                var response = await _httpClient.PostAsync(url, content);

                var responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"📥 Response: {response.StatusCode} - {responseJson}");

                ReminderResponse responseData = null;
                if (!string.IsNullOrEmpty(responseJson))
                {
                    try
                    {
                        responseData = JsonSerializer.Deserialize<ReminderResponse>(responseJson);
                    }
                    catch { }
                }

                var message = responseData?.FirstErrorMessage();
                if (string.IsNullOrWhiteSpace(message))
                    message = response.IsSuccessStatusCode ? "Success" : "Failed";

                return (response.IsSuccessStatusCode, (int)response.StatusCode, message, responseData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception while creating reminder: {ex.Message}");
                return (false, 500, ex.Message, null);
            }
        }

        /// <summary>
        /// Get the reminder for an appointment.
        /// Returns null data if no reminder exists (HTTP 200 with data: null is valid).
        /// </summary>
        public async Task<(bool Success, int StatusCode, ReminderResponse ResponseData)> GetReminderAsync(int appointmentId)
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                var url = $"{GetBaseUrl()}/api/appointments/{appointmentId}/reminders";

                var response = await _httpClient.GetAsync(url);

                var responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"📥 Get reminder response: {response.StatusCode} - {responseJson}");

                ReminderResponse responseData = null;
                if (!string.IsNullOrEmpty(responseJson))
                {
                    try
                    {
                        responseData = JsonSerializer.Deserialize<ReminderResponse>(responseJson);
                    }
                    catch { }
                }

                // 200 with data:null is valid (no reminder set)
                // 404 means no reminder exists
                return (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound, (int)response.StatusCode, responseData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception while getting reminder: {ex.Message}");
                return (false, 500, null);
            }
        }

        /// <summary>
        /// Delete/disable a reminder for an appointment.
        /// Both 200 and 404 (not found) are treated as success.
        /// </summary>
        public async Task<(bool Success, int StatusCode, string Message)> DeleteReminderAsync(int appointmentId)
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                var url = $"{GetBaseUrl()}/api/appointments/{appointmentId}/reminders";

                Console.WriteLine($"🗑️ Deleting reminder for appointment {appointmentId}");

                var response = await _httpClient.DeleteAsync(url);

                Console.WriteLine($"📥 Delete reminder response: {response.StatusCode}");

                // Both 200 and 404 are success cases
                bool success = response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.NotFound;

                return (success, (int)response.StatusCode, success ? "Reminder deleted" : "Failed to delete reminder");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception while deleting reminder: {ex.Message}");
                return (false, 500, ex.Message);
            }
        }

        /// <summary>
        /// Analyze reminder options and generate user warnings for unavailable channels.
        /// 
        /// CHANNEL WARNING LOGIC:
        /// - If has_active_channel = false: Show banner "No active notification channel"
        /// - If push: enabled && !deliverable: Prompt "Enable push notifications" / "Register device"
        /// - If SMS: enabled && !deliverable: Warn "Phone number missing in profile"
        /// - If Email: enabled && !deliverable: Warn "Email missing in profile"
        /// </summary>
        public ChannelWarnings GetChannelWarnings(ReminderOptionsResponse options)
        {
            var warnings = new ChannelWarnings();

            if (options?.Data == null)
            {
                Console.WriteLine("⚠️  No reminder options data available");
                return warnings;
            }

            var channels = options.Data.GetChannels();
            bool hasActive = options.Data.ResolveHasActiveChannel();
            warnings.HasNoActiveChannel = !hasActive;
            warnings.PushNotDeliverable = channels.Any(c => string.Equals(c.Type, "push", StringComparison.OrdinalIgnoreCase) && c.Enabled && !c.Deliverable);
            warnings.SmsNotDeliverable = channels.Any(c => string.Equals(c.Type, "sms", StringComparison.OrdinalIgnoreCase) && c.Enabled && !c.Deliverable);
            warnings.EmailNotDeliverable = channels.Any(c => string.Equals(c.Type, "email", StringComparison.OrdinalIgnoreCase) && c.Enabled && !c.Deliverable);

            foreach (var warning in ReminderChannelHelper.BuildWarnings(channels, hasActive))
                warnings.Warnings.Add(warning);

            return warnings;
        }

        /// <summary>
        /// Represents warnings related to notification channel availability.
        /// </summary>
        public class ChannelWarnings
        {
            public bool HasNoActiveChannel { get; set; }
            public bool PushNotDeliverable { get; set; }
            public bool SmsNotDeliverable { get; set; }
            public bool EmailNotDeliverable { get; set; }
            public List<string> Warnings { get; set; } = new();

            public bool HasAnyWarning => HasNoActiveChannel || PushNotDeliverable || SmsNotDeliverable || EmailNotDeliverable;
        }

    }
}

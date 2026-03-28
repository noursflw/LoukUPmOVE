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

namespace loukupm.services
{
    public class ApiServices
    {
        private readonly HttpClient _httpClient;

        public ApiServices()
        {
            // ✅ إنشاء HttpClientHandler محسّن مع معالجة SSL
            var handler = new HttpClientHandler();

            #if DEBUG
            // في بيئة التطوير: قبول جميع الشهادات (غير آمن - للاختبار فقط)
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                Console.WriteLine($"🔒 Certificate validation: {errors}");
                return true; // قبول الشهادة حتى لو كانت غير موثوقة
            };
            #else
            // في الإنتاج: استخدام التحقق الطبيعي
            handler.ServerCertificateCustomValidationCallback = null;
            #endif

            // تعطيل الضغط لتجنب مشاكل التوافق
            handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30) // ✅ إضافة timeout
            };

            // ✅ إضافة User-Agent
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


     public async Task<List<PolicyandPrivacyS>> GetPolicyandPrivaciesAsync()
        {
            var response = await _httpClient.GetAsync("https://api.example.com/policies");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<PolicyandPrivacyS>>(json);
            }
            return new List<PolicyandPrivacyS>();
        }



        public async Task<List<WorkTeam>> GetWorkTeamsAsync()
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                var response = await _httpClient.GetAsync("https://test.center-yazan.com/api/providers?search=&branch_id=1&service_id=&sort_by=first_name&sort_direction=asc&per_page=15");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ API error: {response.StatusCode}");
                    return new List<WorkTeam>();
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
                return new List<WorkTeam>();
            }
        }


        public async Task<(List<Notification> Notifications, int UnreadCount, bool HasMore)> GetNotificationsAsync(string cursor = null, int perPage = 15)
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                // Build URL with pagination support
                string url = $"https://test.center-yazan.com/api/notifications?per_page={perPage}&cursor={cursor ?? ""}&status=all";

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

                Console.WriteLine($"✅ Loaded {apiResponse.Data.Count} notifications, Unread: {apiResponse.UnreadCount}, HasMore: {apiResponse.Pagination?.HasMorePages}");

                return (apiResponse.Data, apiResponse.UnreadCount, apiResponse.Pagination?.HasMorePages ?? false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception loading notifications: {ex.Message}");
                return (new List<Notification>(), 0, false);
            }
        }

        /// <summary>
        /// Legacy method for backward compatibility - returns only notification list
        /// </summary>
        [Obsolete("Use GetNotificationsAsync instead for pagination support")]
        public async Task<List<Notification>> GetNotificationsLegacyAsync()
        {
            var (notifications, _, _) = await GetNotificationsAsync();
            return notifications;
        }
        
        public async Task<List<Appointment>> GetUserAppointmentsAsync(User user, string status = "PENDING")
        {
            await SetAuthorizationHeaderAsync();

            if (user == null || user.Id == 0)
                throw new Exception("المستخدم غير صالح أو لا يحتوي على رقم معرف (Id).");

            // رابط الطلب مع user_id و status فقط
            string url = $"https://test.center-yazan.com/api/appointments?user_id={user.Id}&status={status}";

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
            await SetAuthorizationHeaderAsync();

            var response = await _httpClient.GetAsync("https://test.center-yazan.com/api/profile");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<UserResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Data;
        }





        public async Task<List<Servies>> GetServiesAsync()
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                var response = await _httpClient.GetAsync("https://test.center-yazan.com/api/services?category_id=&featured=&search=&sort_by=sort_order&sort_direction=asc&per_page=15");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ API error: {response.StatusCode}");
                    return new List<Servies>();
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
                return new List<Servies>();
            }
        }

        /// <summary>
        /// جلب خدمات البروفايدر المحددة فقط
        /// ملاحظة: إذا لم يكن هناك endpoint منفصل، سنستخدم جميع الخدمات
        /// </summary>
        public async Task<List<Servies>> GetProviderServicesAsync(int providerId)
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                // محاولة أولاً: جلب خدمات البروفايدر من endpoint منفصل
                string url = $"https://test.center-yazan.com/api/providers/{providerId}/services";
                
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
                    
                    return await GetServiesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception loading provider services: {ex.Message}");
                // في حالة الخطأ: رجع جميع الخدمات كبديل
                return await GetServiesAsync();
            }
        }


        public async Task<bool> RefreshTokenAsync()
{
    try
    {
        string refreshToken = await SecureStorage.GetAsync("refresh_token");
        if (string.IsNullOrEmpty(refreshToken))
            return false;

        var payload = new
        {
            refresh_token = refreshToken
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://test.center-yazan.com/api/auth/refresh", content);

        if (!response.IsSuccessStatusCode)
            return false;

        var responseJson = await response.Content.ReadAsStringAsync();

        var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (!string.IsNullOrEmpty(authResponse?.AccessToken))
        {
            await SecureStorage.SetAsync("auth_token", authResponse.AccessToken);

            // إذا رجع refresh_token جديد، حدّثه كمان
            if (!string.IsNullOrEmpty(authResponse.RefreshToken))
                await SecureStorage.SetAsync("refresh_token", authResponse.RefreshToken);

            return true;
        }

        return false;
    }
    catch
    {
        return false;
    }
}

        public async Task<List<string>> GetCategoriesAsync()
        {
            try
            {

                await SetAuthorizationHeaderAsync();

                var url = "https://center-yazan.com/api/sandbox/categories";


                var response = await _httpClient.GetAsync(url);


                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();


                    var result = JsonSerializer.Deserialize<CategoriesResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var categories = result?.Categories ?? new List<string>();



                    return categories;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();

                    return new List<string>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CATEGORIES EXCEPTION] {ex.Message}");
                Console.WriteLine($"[CATEGORIES EXCEPTION StackTrace]: {ex.StackTrace}");
                return new List<string>();
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
        public async Task<AuthResponse> LoginAsync(string email, string password)
        {
            try
            {
                var loginData = new { Email = email, Password = password };
                var json = JsonSerializer.Serialize(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://api.example.com/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseJson);
                    if (authResponse?.Token != null)
                    {
                        Preferences.Set("auth_token", authResponse.Token);
                    }
                    return authResponse;
                }

                return new AuthResponse { Success = false, Message = "Login failed" };
            }
            catch (Exception ex)
            {
                return new AuthResponse { Success = false, Message = ex.Message };
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
        /// Update user profile: first_name and avatar via MultipartFormDataContent
        /// Returns response object with success status, message, and profile data
        /// </summary>
        public async Task<ProfileUpdateApiResponse> UpdateUserProfileAsync(string firstName, string avatarImagePath)
        {
            var response = new ProfileUpdateApiResponse();

            try
            {
                // ✅ Set authorization header
                await SetAuthorizationHeaderAsync();

                // Validate token
                string? token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("❌ Authorization: No token found");
                    response.Success = false;
                    response.Message = "خطأ في المصادقة - لم يتم العثور على token";
                    return response;
                }
                Console.WriteLine($"✅ Authorization: Bearer token present (length: {token.Length})");

                // ✅ Build MultipartFormDataContent
                using (var form = new MultipartFormDataContent())
                {
                    // Add first_name field (only if provided)
                    if (!string.IsNullOrWhiteSpace(firstName))
                    {
                        form.Add(new StringContent(firstName.Trim()), "first_name");
                        Console.WriteLine($"📋 Field added: first_name = '{firstName.Trim()}'");
                    }

                    // Add avatar file (only if image path is valid)
                    if (!string.IsNullOrWhiteSpace(avatarImagePath) && File.Exists(avatarImagePath))
                    {
                        try
                        {
                            // ✅ Read file as byte array (solves stream disposal issue)
                            byte[] fileBytes = await File.ReadAllBytesAsync(avatarImagePath);
                            var fileName = Path.GetFileName(avatarImagePath);

                            // Create ByteArrayContent from bytes (won't dispose while sending)
                            var fileContent = new ByteArrayContent(fileBytes);

                            // Detect MIME type based on file extension
                            string mimeType = GetMimeType(avatarImagePath);
                            fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

                            form.Add(fileContent, "Avatar", fileName);
                            Console.WriteLine($"📋 Field added: Avatar = file '{fileName}' (size: {fileBytes.Length} bytes, content-type: {mimeType})");
                        }
                        catch (Exception fileEx)
                        {
                            Console.WriteLine($"❌ Error reading avatar file: {fileEx.Message}");
                            response.Success = false;
                            response.Message = $"خطأ في قراءة الصورة: {fileEx.Message}";
                            return response;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(avatarImagePath))
                    {
                        Console.WriteLine($"⚠️ Avatar file not found at path: {avatarImagePath}");
                    }

                    // ✅ Log request details
                    Console.WriteLine("📤 Sending update request to API:");
                    Console.WriteLine($"   URL: https://test.center-yazan.com/api/profile");
                    Console.WriteLine($"   Method: POST");
                    Console.WriteLine($"   Content-Type: multipart/form-data");
                    Console.WriteLine($"   Authorization: Bearer [token]");
                    Console.WriteLine($"   Fields being sent: {(string.IsNullOrWhiteSpace(firstName) ? "" : "first_name ")}{(string.IsNullOrWhiteSpace(avatarImagePath) ? "" : "Avatar")}");

                    // ✅ Send request
                    var httpResponse = await _httpClient.PostAsync(
                        "https://test.center-yazan.com/api/profile",
                        form
                    );

                    // ✅ Log response
                    Console.WriteLine($"📊 Response Status: {httpResponse.StatusCode}");
                    string responseBody = await httpResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"📄 Response Body: {responseBody}");

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        // ✅ Try to deserialize JSON response
                        try
                        {
                            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                            var parsedResponse = JsonSerializer.Deserialize<ProfileUpdateApiResponse>(responseBody, options);

                            if (parsedResponse != null)
                            {
                                response = parsedResponse;
                                response.Success = parsedResponse.Success ?? true; // Assume success if not explicitly set
                                Console.WriteLine($"✅ Response parsed successfully");
                                return response;
                            }
                            else
                            {
                                // Parsed but result is null
                                Console.WriteLine($"⚠️ Response parsed as null");
                                response.Success = true;
                                response.Message = "تم التحديث (استجابة فارغة)";
                                return response;
                            }
                        }
                        catch (JsonException jsonEx)
                        {
                            // JSON parsing failed but HTTP was successful
                            Console.WriteLine($"⚠️ JSON parsing failed: {jsonEx.Message}");
                            response.Success = true;
                            response.Message = "تم التحديث (لم نتمكن من قراءة الاستجابة)";
                            return response;
                        }
                    }
                    else if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        // 401: Unauthorized
                        Console.WriteLine("❌ Unauthorized (401) - Token invalid or expired");
                        response.Success = false;
                        response.Message = "جلسة المستخدم انتهت، يرجى تسجيل الدخول مجدداً";
                        return response;
                    }
                    else if (httpResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        // 400: Bad Request
                        Console.WriteLine($"❌ Bad Request (400): {responseBody}");
                        response.Success = false;
                        response.Message = "البيانات المدخلة غير صحيحة";
                        return response;
                    }
                    else if (httpResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
                    {
                        // 422: Unprocessable Entity
                        Console.WriteLine($"❌ Unprocessable Entity (422): {responseBody}");
                        response.Success = false;
                        response.Message = "فشل التحقق من البيانات";
                        return response;
                    }
                    else
                    {
                        // Other HTTP errors
                        Console.WriteLine($"❌ API Error: {httpResponse.StatusCode} - {responseBody}");
                        response.Success = false;
                        response.Message = $"خطأ: {(int)httpResponse.StatusCode}";
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception: {ex.Message}");
                Console.WriteLine($"❌ Stack Trace: {ex.StackTrace}");
                response.Success = false;
                response.Message = $"خطأ: {ex.Message}";
                return response;
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
            /// Cancel a booking/appointment by ID
            /// </summary>
            public async Task<bool> CancelBookingAsync(int bookingId)
            {
                await SetAuthorizationHeaderAsync();

                try
                {
                    string url = $"https://test.center-yazan.com/api/bookings/{bookingId}/cancel";

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

        }



    public class CreateCheckoutSessionRequest
    {
        public int Amount { get; set; }
        public string Currency { get; set; } = "eur";
        public string SuccessUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
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

    /// <summary>
    /// Response model for profile update API
    /// </summary>
    public class ProfileUpdateApiResponse
    {
        public bool? Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object Data { get; set; }
    }
}



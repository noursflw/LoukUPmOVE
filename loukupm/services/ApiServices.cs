
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
            // ✅ HttpClientHandler configuration with security best practices
            var handler = new HttpClientHandler();

#if DEBUG
            // ⚠️ DEBUG ONLY: Relaxed SSL validation for testing with self-signed certificates
            // This should NEVER be used in production
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (errors != System.Net.Security.SslPolicyErrors.None)
                {
                    Console.WriteLine($"⚠️ [DEBUG] Certificate validation bypassed: {errors}");
                    Console.WriteLine($"   Subject: {cert?.Subject}");
                    // Only accept specific test certificates, not any certificate
                    if (cert?.Subject?.Contains("test.center-yazan.com") == true ||
                        cert?.Subject?.Contains("test-23def.web.app") == true)
                    {
                        return true;
                    }
                    return false; // Still reject unknown certificates
                }
                return true;
            };
#else
            // ✅ PRODUCTION: Use strict SSL validation
            handler.ServerCertificateCustomValidationCallback = null;
            Console.WriteLine("✅ [ApiServices] Production SSL validation enabled");
#endif

            // ✅ Properly configure decompression
            handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            // ✅ Add User-Agent for API compliance
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "LoukUPmOVE-MAUI/1.0");

            Console.WriteLine("✅ [ApiServices] Initialized with 30s timeout");
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

                var response = await _httpClient.PostAsync("https://test.center-yazan.com/api/auth/verify-otp", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"🔐 OTP Verification Response ({response.StatusCode}): {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var otpResponse = JsonSerializer.Deserialize<Auth.OtpVerificationResponse>(responseContent,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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

                var response = await _httpClient.PostAsync("https://test.center-yazan.com/api/auth/resend-verification-otp", content);
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
        public async Task<ProfileUpdateApiResponse> UpdateUserProfileAsync(string firstName, string avatarImagePath, string phonenumber)
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
                    // Add phone number field (only if provided)
                    if (!string.IsNullOrWhiteSpace(phonenumber))
                    {
                        form.Add(new StringContent(phonenumber.Trim()), "phone");
                        Console.WriteLine($"📋 Field added: phone = '{phonenumber.Trim()}'");
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

                            form.Add(fileContent, "image", fileName);
                            Console.WriteLine($"📋 Field added: image = file '{fileName}' (size: {fileBytes.Length} bytes, content-type: {mimeType})");
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
                    Console.WriteLine($"   Fields being sent: {(string.IsNullOrWhiteSpace(firstName) ? "" : "first_name ")}{(string.IsNullOrWhiteSpace(avatarImagePath) ? "" : "image")}");

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

        public async Task<List<SettingItem>> GetSettingsAsync()
        {
            await SetAuthorizationHeaderAsync();

            var response = await _httpClient.GetAsync(
                "https://test.center-yazan.com/api/settings"
            );

            if (!response.IsSuccessStatusCode)
                return new List<SettingItem>();

            var json = await response.Content.ReadAsStringAsync();
            

            var result = JsonSerializer.Deserialize<SettingsResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return result?.Data ?? new List<SettingItem>();
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
                $"https://test.center-yazan.com/api/settings/{key}",
                content
            );

            return response.IsSuccessStatusCode;
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

        /// <summary>
        /// Get AboutUs page data from API
        /// </summary>
        public async Task<AboutUsResponse> GetAboutUsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://test.center-yazan.com/api/about-us");

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
                string apiUrl = $"https://test.center-yazan.com/api/sliders/home?locale={languageCode}";
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
                string apiUrl = $"https://test.center-yazan.com/api/pages/terms-conditions?lang={languageCode}";
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
                string apiUrl = $"https://test.center-yazan.com/api/pages/privacy-policy?lang={languageCode}";
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
                    "https://test.center-yazan.com/api/profile/phone/send-otp",
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
                    "https://test.center-yazan.com/api/profile/phone/verify-otp",
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
                string apiUrl = $"https://test.center-yazan.com/api/pages/impressum?lang={languageCode}";
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
    }
}

using loukupm.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
            _httpClient = new HttpClient();


        }

        private async Task SetAuthorizationHeaderAsync()
        {
            // استدعاء SecureStorage بشكل غير متزامن
            string? token = await SecureStorage.GetAsync("auth_token");
           
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            }
            else
            {
                // إزالة أي Authorization قديمة إذا لم يكن هناك توكن
                _httpClient.DefaultRequestHeaders.Authorization = null;
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


        public async Task<List<Notifiction>> GetNotifictionsAsync()
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.GetAsync("https://api.example.com/notifications");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Notifiction>>(json);
            }
            return new List<Notifiction>();
        }
        //public async Task<List<Appointment>> GetAppointmentsAsync()
        //{
        //    await SetAuthorizationHeaderAsync();

        //    var response = await _httpClient.GetAsync("https://test.center-yazan.com/api/appointments");

        //    if (response.IsSuccessStatusCode)
        //    {
        //        var json = await response.Content.ReadAsStringAsync();

        //        var result = JsonSerializer.Deserialize<AppointmentResponse>(json, new JsonSerializerOptions
        //        {
        //            PropertyNameCaseInsensitive = true
        //        });


        //        return result?.Data?.Data ?? new List<Appointment>();
        //    }

        //    return new List<Appointment>();
        //}
        public async Task<List<Appointment>> GetUserAppointmentsAsync(User user, string status = "ALL")
        {
            await SetAuthorizationHeaderAsync();

            if (user == null || user.Id == 0)
                throw new Exception("المستخدم غير صالح أو لا يحتوي على رقم معرف (Id).");

            // رابط الطلب مع user_id و status فقط
            string url = $"https://test.center-yazan.com/api/appointments?user_id={user.Id}&status={status}";

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<AppointmentResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Data?.Data ?? new List<Appointment>();
            }

            return new List<Appointment>();
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



        public async Task<bool> SubmitBookingAsync(Booking booking)
        {
            await SetAuthorizationHeaderAsync();
            var json = JsonSerializer.Serialize(booking);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.example.com/bookings", content);
            return response.IsSuccessStatusCode;
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

        public async Task<AuthResponse> RegisterAsync(User user)
        {
            try
            {
                var json = JsonSerializer.Serialize(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://api.example.com/auth/register", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<AuthResponse>(responseJson);
                }

                return new AuthResponse { Success = false, Message = "Registration failed" };
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
        public async Task<AvailabilityResponseWrapper?> GetProviderAvailabilityAsync(int providerId, string date = "")
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                // يمكنك تمرير التاريخ إذا أردت فلترة حسب يوم محدد
                string url = $"https://test.center-yazan.com/api/provider/{providerId}/availability";
                if (!string.IsNullOrEmpty(date))
                {
                    url += $"?date={date}";
                }

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ API error: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var wrapper = JsonSerializer.Deserialize<AvailabilityResponseWrapper>(json, options);

                return wrapper;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception while fetching availability: {ex.Message}");
                return null;
            }
        }
        public async Task<bool> UpdateUserAsync(string name, string email, string avatarBase64 = null)
        {
            await SetAuthorizationHeaderAsync();

            try
            {
                var userData = new Dictionary<string, string>();

                if (!string.IsNullOrWhiteSpace(name))
                    userData.Add("name", name);

                if (!string.IsNullOrWhiteSpace(email))
                    userData.Add("email", email);

                if (!string.IsNullOrWhiteSpace(avatarBase64))
                    userData.Add("avatar", avatarBase64);

                var content = new FormUrlEncodedContent(userData);

                var response = await _httpClient.PostAsync(
                    "https://test.center-yazan.com/api/profile",
                    content
                );

                return response.IsSuccessStatusCode;
            }
            catch
            {
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
}



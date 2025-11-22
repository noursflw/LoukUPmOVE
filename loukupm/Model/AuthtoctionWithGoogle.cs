using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Authentication;   // WebAuthenticator
using Microsoft.Maui.Storage;          // SecureStorage

namespace loukupm.Model
{
    // نموذج الاستجابة المتوقع من الـ backend بعد تسجيل الدخول
    public class UserTokenResponse
    {
        public string Token { get; set; }       // JWT from your backend
        public string RefreshToken { get; set; } // optional
        public BasicUserDto User { get; set; }
    }

    public class BasicUserDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string PictureUrl { get; set; }
    }

    // نموذج الإرسال إلى backend
    public class GoogleCodeRequest
    {
        public string Code { get; set; }
    }

    public class AuthtoctionWithGoogle
    {
        // TODO: غيّر هالقيم بما يتناسب مع مشروعك
        private const string ClientId = "YOUR_GOOGLE_CLIENT_ID";
        private const string RedirectUri = "myapp://callback"; // نفس اللي سجلته في Google Console
        private const string BackendUrl = "https://your-api.com"; // بدون نهاية slash

        // مفتاح لحفظ التوكن في SecureStorage
        private const string SecureKey_Jwt = "app_jwt_token";
        private const string SecureKey_Refresh = "app_refresh_token";

        private readonly HttpClient _httpClient;

        public AuthtoctionWithGoogle(HttpClient httpClient = null)
        {
            // يمكنك تمرير HttpClient من DI أو سيترك هنا واحد جديد
            _httpClient = httpClient ?? new HttpClient();
        }

        /// <summary>
        /// يبدأ عملية تسجيل الدخول عبر Google: يفتح WebAuthenticator، يستقبل code ثم يرسله للـ backend.
        /// يرجع UserTokenResponse (يحوي JWT وبيانات المستخدم) أو null لو فشل.
        /// </summary>
        public async Task<UserTokenResponse> LoginAsync()
        {
            try
            {
                // أبني رابط المصادقة
                var authUrl = new Uri(
                    $"https://accounts.google.com/o/oauth2/v2/auth" +
                    $"?client_id={Uri.EscapeDataString(ClientId)}" +
                    $"&redirect_uri={Uri.EscapeDataString(RedirectUri)}" +
                    $"&response_type=code" +
                    $"&scope={Uri.EscapeDataString("openid email profile")}" +
                    $"&access_type=offline" + // للحصول على refresh token (Google يطلب التحقق)
                    $"&prompt=consent"       // لطلب refresh token في كل مرة (اختياري)
                );

                // استدعاء WebAuthenticator لفتح نافذة Google
                var webAuthResult = await WebAuthenticator.AuthenticateAsync(authUrl, new Uri(RedirectUri));

                // Google يرجع في Properties قيمة "code" عند response_type=code
                if (webAuthResult?.Properties != null && webAuthResult.Properties.TryGetValue("code", out var code))
                {
                    // أرسل الكود للـ backend ليتبادل الكود بـ tokens ويُنشئ / يحدث حساب المستخدم
                    var response = await SendCodeToBackendAsync(code);

                    if (response != null && !string.IsNullOrEmpty(response.Token))
                    {
                        // خزّن JWT وآخر (إذا يوجد)
                        await SecureStorage.SetAsync(SecureKey_Jwt, response.Token);
                        if (!string.IsNullOrEmpty(response.RefreshToken))
                            await SecureStorage.SetAsync(SecureKey_Refresh, response.RefreshToken);

                        return response;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                // تعامل مع الاستثناءات حسب مشروعك (logging مثلاً)
                System.Diagnostics.Debug.WriteLine($"Google login failed: {ex}");
                return null;
            }
        }

        /// <summary>
        /// يرسل الـ authorization code إلى endpoint في الـ backend المسؤول عن تبادل الـ code مع Google
        /// endpoint يجب أن يستدعي Google token endpoint ويتحقق من الـ id_token ثم يُرجع JWT للتطبيق.
        /// </summary>
        private async Task<UserTokenResponse> SendCodeToBackendAsync(string code)
        {
            var url = $"{BackendUrl}/auth/google"; // توقع POST endpoint في backend

            var request = new GoogleCodeRequest { Code = code };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await _httpClient.PostAsync(url, content);

            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Backend /auth/google failed: {resp.StatusCode} - {err}");
                return null;
            }

            var respJson = await resp.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var tokenResponse = JsonSerializer.Deserialize<UserTokenResponse>(respJson, options);

            return tokenResponse;
        }

        /// <summary>
        /// يحصل على JWT المحفوظ محليًا إن وُجد.
        /// </summary>
        public static async Task<string> GetSavedJwtAsync()
        {
            try
            {
                return await SecureStorage.GetAsync(SecureKey_Jwt);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// يسجّل الخروج ويزيل التوكنات من التخزين الآمن.
        /// </summary>
        public static async Task LogoutAsync()
        {
            try
            {
                SecureStorage.Remove(SecureKey_Jwt);
                SecureStorage.Remove(SecureKey_Refresh);
            }
            catch
            {
                // ignore
            }
        }

        /// <summary>
        /// مثال: طريقة لإضافة JWT لطلب HttpClient (يمكن استخدامها في DI أو interceptor)
        /// </summary>
        public async Task AttachJwtToHttpClientAsync()
        {
            var token = await GetSavedJwtAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }
    }
}

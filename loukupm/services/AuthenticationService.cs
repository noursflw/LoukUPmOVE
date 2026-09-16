using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using loukupm.Model;
using static loukupm.Model.Auth;

namespace loukupm.Services
{
    public class AuthenticationService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly HttpClient _httpClient;
        private readonly TokenManager _tokenManager;

        private readonly object _refreshSync = new();
        private Task<bool> _refreshTask;

        public AuthenticationService(
            HttpClient httpClient,
            TokenManager tokenManager)
        {
            _httpClient = httpClient
                ?? throw new ArgumentNullException(nameof(httpClient));

            _tokenManager = tokenManager
                ?? throw new ArgumentNullException(nameof(tokenManager));
        }

        // =========================================================
        // LOGIN
        // =========================================================

        public async Task<(
            bool Success,
            string AccessToken,
            string RefreshToken,
            UserData User,
            int StatusCode,
            string ErrorMessage)> LoginAsync(
                string email,
                string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(password))
                {
                    return (
                        false,
                        null,
                        null,
                        null,
                        400,
                        "Email and password are required");
                }

                var request = new LoginRequest
                {
                    Email = email,
                    Password = password,
                    RegistrationMethod = "email"
                };

                using var response = await _httpClient
                    .PostAsync(
                        "https://lookupfriseur.com/api/auth/login",
                        ToJson(request))
                    .ConfigureAwait(false);

                var responseContent = await response.Content
                    .ReadAsStringAsync()
                    .ConfigureAwait(false);

                int statusCode = (int)response.StatusCode;

                Console.WriteLine(
                    $"[AuthService] Login response status: {statusCode}");

                Console.WriteLine(
                    $"[AuthService] Login response body: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var loginResponse =
                            JsonSerializer.Deserialize<LoginResponse>(
                                responseContent,
                                JsonOptions);

                        if (loginResponse != null &&
                            !string.IsNullOrEmpty(loginResponse.Token))
                        {
                            await _tokenManager.SaveTokensAsync(
                                loginResponse.Token,
                                loginResponse.Refresh_Token,
                                loginResponse.AccessExpiresAt,
                                loginResponse.RefreshExpiresAt)
                                .ConfigureAwait(false);

                            return (
                                true,
                                loginResponse.Token,
                                loginResponse.Refresh_Token,
                                loginResponse.User,
                                statusCode,
                                null);
                        }

                        return (
                            false,
                            null,
                            null,
                            null,
                            500,
                            "Invalid login response from server");
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine(
                            $"❌ [AuthService] Login JSON error: {ex}");

                        return (
                            false,
                            null,
                            null,
                            null,
                            500,
                            "Failed to parse server response");
                    }
                }

                if (response.StatusCode ==
                    HttpStatusCode.Unauthorized)
                {
                    return (
                        false,
                        null,
                        null,
                        null,
                        401,
                        "Invalid email or password");
                }

                var errorMsg =
                    ParseErrorMessage(responseContent)
                    ?? "Login failed";

                return (
                    false,
                    null,
                    null,
                    null,
                    statusCode,
                    errorMsg);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"❌ [AuthService] Login network error: {ex}");

                Console.WriteLine(
                    $"Message: {ex.Message}");

                Console.WriteLine(
                    $"InnerException: {ex.InnerException?.Message}");

                return (
                    false,
                    null,
                    null,
                    null,
                    0,
                    "Network connection error");
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine(
                    $"❌ [AuthService] Login timeout/cancelled: {ex}");

                return (
                    false,
                    null,
                    null,
                    null,
                    0,
                    "The request timed out. Please try again.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"❌ [AuthService] Unexpected login error: {ex}");

                Console.WriteLine(
                    $"Type: {ex.GetType().FullName}");

                Console.WriteLine(
                    $"Message: {ex.Message}");

                Console.WriteLine(
                    $"InnerException: {ex.InnerException?.Message}");

                return (
                    false,
                    null,
                    null,
                    null,
                    0,
                    "Unexpected error occurred");
            }
        }

        // =========================================================
        // REGISTER
        // =========================================================

        public async Task<(
            bool Success,
            string AccessToken,
            string RefreshToken,
            UserData User,
            int StatusCode,
            string ErrorMessage)> RegisterAsync(
                string firstName,
                string lastName,
                string email,
                string phone,
                string password,
                string passwordConfirmation)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(firstName) ||
                    string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(password))
                {
                    return (
                        false,
                        null,
                        null,
                        null,
                        400,
                        "Required fields are missing");
                }

                var request = new RegisterRequest
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Phone = phone,
                    Password = password,
                    PasswordConfirmation = passwordConfirmation,
                    RegistrationMethod = "email"
                };

                Console.WriteLine(
                    "[AuthService] Sending register request...");

                using var response = await _httpClient
                    .PostAsync(
                        "https://lookupfriseur.com/api/auth/register",
                        ToJson(request))
                    .ConfigureAwait(false);

                var responseContent = await response.Content
                    .ReadAsStringAsync()
                    .ConfigureAwait(false);

                int statusCode = (int)response.StatusCode;

                Console.WriteLine(
                    $"[AuthService] Register response status: {statusCode}");

                Console.WriteLine(
                    $"[AuthService] Register response body: {responseContent}");

                // =====================================================
                // 429 TOO MANY REQUESTS
                // =====================================================

                if (response.StatusCode ==
                    HttpStatusCode.TooManyRequests)
                {
                    Console.WriteLine(
                        "⚠️ [AuthService] Register rate limited (429)");

                    string retryAfterHeader = null;

                    if (response.Headers.TryGetValues(
                        "Retry-After",
                        out var retryAfterValues))
                    {
                        retryAfterHeader =
                            string.Join(", ", retryAfterValues);

                        Console.WriteLine(
                            $"⏱️ [AuthService] Retry-After: {retryAfterHeader}");
                    }

                    string errorMessage =
                        ParseErrorMessage(responseContent);

                    if (string.IsNullOrWhiteSpace(errorMessage))
                    {
                        errorMessage =
                            "Too many registration attempts. Please wait and try again.";
                    }

                    Console.WriteLine(
                        $"⚠️ [AuthService] Register 429 message: {errorMessage}");

                    return (
                        false,
                        null,
                        null,
                        null,
                        429,
                        errorMessage);
                }

                // =====================================================
                // SUCCESS
                // =====================================================

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var registerResponse =
                            JsonSerializer.Deserialize<RegisterResponse>(
                                responseContent,
                                JsonOptions);

                        if (registerResponse != null &&
                            !string.IsNullOrEmpty(
                                registerResponse.AccessToken))
                        {
                            await _tokenManager.SaveTokensAsync(
                                registerResponse.AccessToken,
                                registerResponse.RefreshToken,
                                registerResponse.AccessExpiresAt,
                                registerResponse.RefreshExpiresAt)
                                .ConfigureAwait(false);

                            Console.WriteLine(
                                "✅ [AuthService] Registration successful.");

                            return (
                                true,
                                registerResponse.AccessToken,
                                registerResponse.RefreshToken,
                                registerResponse.User,
                                statusCode,
                                null);
                        }

                        // =================================================
                        // OTP REQUIRED
                        // =================================================

                        if (registerResponse != null &&
                            (!string.IsNullOrEmpty(registerResponse.Otp) ||
                             !string.IsNullOrEmpty(
                                 registerResponse.MaskedDestination)))
                        {
                            Console.WriteLine(
                                "📩 [AuthService] Registration requires OTP.");

                            return (
                                true,
                                null,
                                null,
                                registerResponse.User,
                                statusCode,
                                "OTP verification required");
                        }

                        Console.WriteLine(
                            "❌ [AuthService] Invalid registration response.");

                        return (
                            false,
                            null,
                            null,
                            null,
                            500,
                            "Invalid registration response from server");
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine(
                            $"❌ [AuthService] Register JSON error: {ex}");

                        Console.WriteLine(
                            $"Response body: {responseContent}");

                        return (
                            false,
                            null,
                            null,
                            null,
                            500,
                            "Failed to parse server response");
                    }
                }

                // =====================================================
                // 422 VALIDATION ERROR
                // =====================================================

                if (response.StatusCode ==
                    HttpStatusCode.UnprocessableEntity)
                {
                    var errorMsg =
                        ParseErrorMessage(responseContent)
                        ?? "Validation error";

                    Console.WriteLine(
                        $"⚠️ [AuthService] Register validation error: {errorMsg}");

                    return (
                        false,
                        null,
                        null,
                        null,
                        422,
                        errorMsg);
                }

                // =====================================================
                // OTHER SERVER ERRORS
                // =====================================================

                var msg =
                    ParseErrorMessage(responseContent)
                    ?? "Registration failed";

                Console.WriteLine(
                    $"❌ [AuthService] Register failed: {msg}");

                return (
                    false,
                    null,
                    null,
                    null,
                    statusCode,
                    msg);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"❌ [AuthService] Register network error: {ex}");

                Console.WriteLine(
                    $"Message: {ex.Message}");

                Console.WriteLine(
                    $"InnerException: {ex.InnerException?.Message}");

                return (
                    false,
                    null,
                    null,
                    null,
                    0,
                    "Network connection error");
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine(
                    $"❌ [AuthService] Register request timeout/cancelled: {ex}");

                return (
                    false,
                    null,
                    null,
                    null,
                    0,
                    "The request timed out. Please try again.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"❌ [AuthService] Unexpected register error: {ex}");

                Console.WriteLine(
                    $"Type: {ex.GetType().FullName}");

                Console.WriteLine(
                    $"Message: {ex.Message}");

                Console.WriteLine(
                    $"InnerException: {ex.InnerException?.Message}");

                return (
                    false,
                    null,
                    null,
                    null,
                    0,
                    "Unexpected error occurred");
            }
        }

        // =========================================================
        // GOOGLE LOGIN
        // =========================================================

        public async Task<(
            bool Success,
            string AccessToken,
            string RefreshToken,
            UserData User,
            int StatusCode,
            string ErrorMessage)> LoginWithGoogleAsync(
                string idToken,
                string userId,
                string email,
                string displayName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(idToken))
                {
                    return (
                        false,
                        null,
                        null,
                        null,
                        400,
                        "Google token is required");
                }

                var payload = new
                {
                    Token = idToken,
                    UserId = userId,
                    Email = email,
                    DisplayName = displayName
                };

                using var response = await _httpClient
                    .PostAsync(
                        "https://lookupfriseur.com/api/auth/google",
                        ToJson(payload))
                    .ConfigureAwait(false);

                var responseContent = await response.Content
                    .ReadAsStringAsync()
                    .ConfigureAwait(false);

                int statusCode = (int)response.StatusCode;

                Console.WriteLine(
                    $"[AuthService] Google login response status: {statusCode}");

                Console.WriteLine(
                    $"[AuthService] Google login response body: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg =
                        ParseErrorMessage(responseContent)
                        ?? "Google authentication failed";

                    return (
                        false,
                        null,
                        null,
                        null,
                        statusCode,
                        errorMsg);
                }

                var loginResponse =
                    JsonSerializer.Deserialize<LoginResponse>(
                        responseContent,
                        JsonOptions);

                var accessToken = loginResponse?.Token;
                var refreshToken = loginResponse?.Refresh_Token;
                var user = loginResponse?.User;
                var accessExpires = loginResponse?.AccessExpiresAt;
                var refreshExpires = loginResponse?.RefreshExpiresAt;

                // =====================================================
                // FALLBACK TO REGISTER RESPONSE
                // =====================================================

                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    var registerResponse =
                        JsonSerializer.Deserialize<RegisterResponse>(
                            responseContent,
                            JsonOptions);

                    accessToken =
                        registerResponse?.AccessToken;

                    refreshToken =
                        registerResponse?.RefreshToken;

                    user =
                        registerResponse?.User;

                    accessExpires =
                        registerResponse?.AccessExpiresAt;

                    refreshExpires =
                        registerResponse?.RefreshExpiresAt;
                }

                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    return (
                        false,
                        null,
                        null,
                        null,
                        500,
                        "Backend did not return an access token.");
                }

                await _tokenManager.SaveTokensAsync(
                    accessToken,
                    refreshToken,
                    accessExpires,
                    refreshExpires)
                    .ConfigureAwait(false);

                return (
                    true,
                    accessToken,
                    refreshToken,
                    user,
                    statusCode,
                    null);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"❌ [AuthService] Google network error: {ex}");

                return (
                    false,
                    null,
                    null,
                    null,
                    0,
                    "Network connection error");
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine(
                    $"❌ [AuthService] Google timeout/cancelled: {ex}");

                return (
                    false,
                    null,
                    null,
                    null,
                    0,
                    "The request timed out. Please try again.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"❌ [AuthService] Unexpected Google error: {ex}");

                Console.WriteLine(
                    $"Type: {ex.GetType().FullName}");

                Console.WriteLine(
                    $"Message: {ex.Message}");

                Console.WriteLine(
                    $"InnerException: {ex.InnerException?.Message}");

                return (
                    false,
                    null,
                    null,
                    null,
                    0,
                    "Unexpected error occurred");
            }
        }

        // =========================================================
        // REFRESH TOKEN
        // =========================================================

        public Task<bool> RefreshTokenAsync()
        {
            lock (_refreshSync)
            {
                if (_refreshTask == null ||
                    _refreshTask.IsCompleted)
                {
                    _refreshTask =
                        PerformTokenRefreshAsync();
                }

                return _refreshTask;
            }
        }

        private async Task<bool> PerformTokenRefreshAsync()
        {
            try
            {
                var refreshToken =
                    await _tokenManager
                        .GetRefreshTokenAsync()
                        .ConfigureAwait(false);

                if (string.IsNullOrEmpty(refreshToken))
                    return false;

                if (await _tokenManager
                    .IsRefreshTokenExpiredAsync()
                    .ConfigureAwait(false))
                {
                    await _tokenManager
                        .ClearTokensAsync()
                        .ConfigureAwait(false);

                    return false;
                }

                var payload = new
                {
                    refresh_token = refreshToken
                };

                using var response = await _httpClient
                    .PostAsync(
                        "https://lookupfriseur.com/api/auth/refresh",
                        ToJson(payload))
                    .ConfigureAwait(false);

                var responseContent = await response.Content
                    .ReadAsStringAsync()
                    .ConfigureAwait(false);

                int statusCode = (int)response.StatusCode;

                Console.WriteLine(
                    $"[AuthService] Refresh token response status: {statusCode}");

                Console.WriteLine(
                    $"[AuthService] Refresh token response body: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var authResponse =
                            JsonSerializer.Deserialize<AuthResponse>(
                                responseContent,
                                JsonOptions);

                        if (authResponse != null &&
                            !string.IsNullOrEmpty(
                                authResponse.AccessToken))
                        {
                            await _tokenManager.SaveTokensAsync(
                                authResponse.AccessToken,
                                string.IsNullOrWhiteSpace(
                                    authResponse.RefreshToken)
                                    ? refreshToken
                                    : authResponse.RefreshToken,
                                authResponse.AccessExpiresAt,
                                authResponse.RefreshExpiresAt)
                                .ConfigureAwait(false);

                            Console.WriteLine(
                                "✅ [AuthService] Token refresh successful.");

                            return true;
                        }

                        return false;
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine(
                            $"❌ [AuthService] Refresh JSON error: {ex}");

                        return false;
                    }
                }

                if (response.StatusCode ==
                        HttpStatusCode.Unauthorized ||
                    response.StatusCode ==
                        HttpStatusCode.Forbidden)
                {
                    Console.WriteLine(
                        "⚠️ [AuthService] Refresh token invalid or expired.");

                    await _tokenManager
                        .ClearTokensAsync()
                        .ConfigureAwait(false);

                    return false;
                }

                return false;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"❌ [AuthService] Refresh network error: {ex}");

                return false;
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine(
                    $"❌ [AuthService] Refresh timeout/cancelled: {ex}");

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"❌ [AuthService] Unexpected refresh error: {ex}");

                Console.WriteLine(
                    $"Type: {ex.GetType().FullName}");

                Console.WriteLine(
                    $"Message: {ex.Message}");

                Console.WriteLine(
                    $"InnerException: {ex.InnerException?.Message}");

                return false;
            }
        }

        // =========================================================
        // LOGOUT
        // =========================================================

        public async Task LogoutAsync()
        {
            await _tokenManager
                .ClearTokensAsync()
                .ConfigureAwait(false);
        }

        // =========================================================
        // AUTHENTICATION CHECK
        // =========================================================

        public Task<bool> IsAuthenticatedAsync()
        {
            return _tokenManager.HasValidTokensAsync();
        }

        // =========================================================
        // GET ACCESS TOKEN
        // =========================================================

        public Task<string> GetAccessTokenAsync()
        {
            return _tokenManager.GetAccessTokenAsync();
        }

        // =========================================================
        // JSON
        // =========================================================

        private static StringContent ToJson(object value)
        {
            var json =
                JsonSerializer.Serialize(
                    value,
                    JsonOptions);

            return new StringContent(
                json,
                Encoding.UTF8,
                "application/json");
        }

        // =========================================================
        // ERROR PARSER
        // =========================================================

        private static string ParseErrorMessage(
            string responseContent)
        {
            if (string.IsNullOrWhiteSpace(responseContent))
                return null;

            try
            {
                using var doc =
                    JsonDocument.Parse(responseContent);

                var root =
                    doc.RootElement;

                // -------------------------------------------------
                // { "message": "..." }
                // -------------------------------------------------

                if (root.TryGetProperty(
                    "message",
                    out var messageProp))
                {
                    if (messageProp.ValueKind ==
                        JsonValueKind.String)
                    {
                        return messageProp.GetString();
                    }
                }

                // -------------------------------------------------
                // { "error": "..." }
                // -------------------------------------------------

                if (root.TryGetProperty(
                    "error",
                    out var errorProp))
                {
                    if (errorProp.ValueKind ==
                        JsonValueKind.String)
                    {
                        return errorProp.GetString();
                    }

                    // ---------------------------------------------
                    // { "error": { "message": "..." } }
                    // ---------------------------------------------

                    if (errorProp.ValueKind ==
                        JsonValueKind.Object &&
                        errorProp.TryGetProperty(
                            "message",
                            out var errorMessageProp))
                    {
                        if (errorMessageProp.ValueKind ==
                            JsonValueKind.String)
                        {
                            return errorMessageProp.GetString();
                        }
                    }
                }

                // -------------------------------------------------
                // { "errors": [...] }
                // -------------------------------------------------

                if (root.TryGetProperty(
                    "errors",
                    out var errorsProp))
                {
                    // Array
                    if (errorsProp.ValueKind ==
                            JsonValueKind.Array &&
                        errorsProp.GetArrayLength() > 0)
                    {
                        var firstError =
                            errorsProp[0];

                        if (firstError.ValueKind ==
                            JsonValueKind.String)
                        {
                            return firstError.GetString();
                        }

                        if (firstError.ValueKind ==
                                JsonValueKind.Object &&
                            firstError.TryGetProperty(
                                "message",
                                out var firstErrorMsg))
                        {
                            if (firstErrorMsg.ValueKind ==
                                JsonValueKind.String)
                            {
                                return firstErrorMsg.GetString();
                            }
                        }
                    }

                    // Object
                    if (errorsProp.ValueKind ==
                        JsonValueKind.Object)
                    {
                        foreach (var property in
                                 errorsProp.EnumerateObject())
                        {
                            var value =
                                property.Value;

                            if (value.ValueKind ==
                                JsonValueKind.Array &&
                                value.GetArrayLength() > 0)
                            {
                                var first =
                                    value[0];

                                if (first.ValueKind ==
                                    JsonValueKind.String)
                                {
                                    return first.GetString();
                                }
                            }

                            if (value.ValueKind ==
                                JsonValueKind.String)
                            {
                                return value.GetString();
                            }
                        }
                    }
                }

                // -------------------------------------------------
                // Plain string JSON
                // -------------------------------------------------

                if (root.ValueKind ==
                    JsonValueKind.String)
                {
                    return root.GetString();
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine(
                    $"⚠️ [AuthService] Error response is not valid JSON: {ex.Message}");
            }

            return null;
        }
    }
}
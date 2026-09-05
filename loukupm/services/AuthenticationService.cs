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

        public AuthenticationService(HttpClient httpClient, TokenManager tokenManager)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        }

        public async Task<(bool Success, string AccessToken, string RefreshToken, UserData User, int StatusCode, string ErrorMessage)> LoginAsync(
            string email,
            string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                    return (false, null, null, null, 400, "Email and password are required");

                var request = new LoginRequest
                {
                    Email = email,
                    Password = password,
                    RegistrationMethod = "email"
                };

                var response = await _httpClient.PostAsync("https://lookupfriseur.com/api/auth/login", ToJson(request)).ConfigureAwait(false);
                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                Console.WriteLine($"[AuthService] Login response status: {(int)response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, JsonOptions);

                        if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                        {
                            await _tokenManager.SaveTokensAsync(
                                loginResponse.Token,
                                loginResponse.Refresh_Token,
                                loginResponse.AccessExpiresAt,
                                loginResponse.RefreshExpiresAt).ConfigureAwait(false);

                            return (true, loginResponse.Token, loginResponse.Refresh_Token, loginResponse.User,
                                (int)response.StatusCode, null);
                        }

                        return (false, null, null, null, 500, "Invalid login response from server");
                    }
                    catch (JsonException)
                    {
                        return (false, null, null, null, 500, "Failed to parse server response");
                    }
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return (false, null, null, null, 401, "Invalid email or password");

                var errorMsg = ParseErrorMessage(responseContent) ?? "Login failed";
                return (false, null, null, null, (int)response.StatusCode, errorMsg);
            }
            catch (HttpRequestException)
            {
                return (false, null, null, null, 0, "Network connection error");
            }
            catch (Exception)
            {
                return (false, null, null, null, 0, "Unexpected error occurred");
            }
        }

        public async Task<(bool Success, string AccessToken, string RefreshToken, UserData User, int StatusCode, string ErrorMessage)> RegisterAsync(
            string firstName,
            string lastName,
            string email,
            string phone,
            string password,
            string passwordConfirmation)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(password))
                {
                    return (false, null, null, null, 400, "Required fields are missing");
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

                var response = await _httpClient.PostAsync("https://lookupfriseur.com/api/auth/register", ToJson(request)).ConfigureAwait(false);
                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                Console.WriteLine($"[AuthService] Register response status: {(int)response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var registerResponse = JsonSerializer.Deserialize<RegisterResponse>(responseContent, JsonOptions);

                        if (registerResponse != null && !string.IsNullOrEmpty(registerResponse.AccessToken))
                        {
                            await _tokenManager.SaveTokensAsync(
                                registerResponse.AccessToken,
                                registerResponse.RefreshToken,
                                registerResponse.AccessExpiresAt,
                                registerResponse.RefreshExpiresAt).ConfigureAwait(false);

                            return (true, registerResponse.AccessToken, registerResponse.RefreshToken,
                                registerResponse.User, (int)response.StatusCode, null);
                        }

                        if (!string.IsNullOrEmpty(registerResponse?.Otp) || !string.IsNullOrEmpty(registerResponse?.MaskedDestination))
                        {
                            return (true, null, null, registerResponse.User, (int)response.StatusCode,
                                "OTP verification required");
                        }

                        return (false, null, null, null, 500, "Invalid registration response from server");
                    }
                    catch (JsonException)
                    {
                        return (false, null, null, null, 500, "Failed to parse server response");
                    }
                }

                if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
                {
                    var errorMsg = ParseErrorMessage(responseContent) ?? "Validation error";
                    return (false, null, null, null, 422, errorMsg);
                }

                var msg = ParseErrorMessage(responseContent) ?? "Registration failed";
                return (false, null, null, null, (int)response.StatusCode, msg);
            }
            catch (HttpRequestException)
            {
                return (false, null, null, null, 0, "Network connection error");
            }
            catch (Exception)
            {
                return (false, null, null, null, 0, "Unexpected error occurred");
            }
        }

        public async Task<(bool Success, string AccessToken, string RefreshToken, UserData User, int StatusCode, string ErrorMessage)> LoginWithGoogleAsync(
            string idToken,
            string userId,
            string email,
            string displayName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(idToken))
                    return (false, null, null, null, 400, "Google token is required");

                var payload = new
                {
                    Token = idToken,
                    UserId = userId,
                    Email = email,
                    DisplayName = displayName
                };

                var response = await _httpClient.PostAsync("https://lookupfriseur.com/api/auth/google", ToJson(payload)).ConfigureAwait(false);
                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                Console.WriteLine($"[AuthService] Google login response status: {(int)response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = ParseErrorMessage(responseContent) ?? "Google authentication failed";
                    return (false, null, null, null, (int)response.StatusCode, errorMsg);
                }

                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, JsonOptions);
                var accessToken = loginResponse?.Token;
                var refreshToken = loginResponse?.Refresh_Token;
                var user = loginResponse?.User;
                var accessExpires = loginResponse?.AccessExpiresAt;
                var refreshExpires = loginResponse?.RefreshExpiresAt;

                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    var registerResponse = JsonSerializer.Deserialize<RegisterResponse>(responseContent, JsonOptions);
                    accessToken = registerResponse?.AccessToken;
                    refreshToken = registerResponse?.RefreshToken;
                    user = registerResponse?.User;
                    accessExpires = registerResponse?.AccessExpiresAt;
                    refreshExpires = registerResponse?.RefreshExpiresAt;
                }

                if (string.IsNullOrWhiteSpace(accessToken))
                    return (false, null, null, null, 500, "Backend did not return an access token.");

                await _tokenManager.SaveTokensAsync(accessToken, refreshToken, accessExpires, refreshExpires).ConfigureAwait(false);
                return (true, accessToken, refreshToken, user, (int)response.StatusCode, null);
            }
            catch (HttpRequestException)
            {
                return (false, null, null, null, 0, "Network connection error");
            }
            catch (Exception)
            {
                return (false, null, null, null, 0, "Unexpected error occurred");
            }
        }

        public Task<bool> RefreshTokenAsync()
        {
            lock (_refreshSync)
            {
                if (_refreshTask == null || _refreshTask.IsCompleted)
                    _refreshTask = PerformTokenRefreshAsync();

                return _refreshTask;
            }
        }

        private async Task<bool> PerformTokenRefreshAsync()
        {
            try
            {
                var refreshToken = await _tokenManager.GetRefreshTokenAsync().ConfigureAwait(false);

                if (string.IsNullOrEmpty(refreshToken))
                    return false;

                if (await _tokenManager.IsRefreshTokenExpiredAsync().ConfigureAwait(false))
                {
                    await _tokenManager.ClearTokensAsync().ConfigureAwait(false);
                    return false;
                }

                var payload = new { refresh_token = refreshToken };
                var response = await _httpClient.PostAsync("https://lookupfriseur.com/api/auth/refresh", ToJson(payload)).ConfigureAwait(false);
                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                Console.WriteLine($"[AuthService] Refresh token response status: {(int)response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent, JsonOptions);

                        if (authResponse != null && !string.IsNullOrEmpty(authResponse.AccessToken))
                        {
                            await _tokenManager.SaveTokensAsync(
                                authResponse.AccessToken,
                                string.IsNullOrWhiteSpace(authResponse.RefreshToken) ? refreshToken : authResponse.RefreshToken,
                                authResponse.AccessExpiresAt,
                                authResponse.RefreshExpiresAt).ConfigureAwait(false);

                            return true;
                        }

                        return false;
                    }
                    catch (JsonException)
                    {
                        return false;
                    }
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    await _tokenManager.ClearTokensAsync().ConfigureAwait(false);
                    return false;
                }

                return false;
            }
            catch (HttpRequestException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            await _tokenManager.ClearTokensAsync().ConfigureAwait(false);
        }

        public Task<bool> IsAuthenticatedAsync()
        {
            return _tokenManager.HasValidTokensAsync();
        }

        public Task<string> GetAccessTokenAsync()
        {
            return _tokenManager.GetAccessTokenAsync();
        }

        private static StringContent ToJson(object value)
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private static string ParseErrorMessage(string responseContent)
        {
            if (string.IsNullOrWhiteSpace(responseContent))
                return null;

            try
            {
                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;

                if (root.TryGetProperty("message", out var messageProp))
                    return messageProp.GetString();

                if (root.TryGetProperty("error", out var errorProp))
                {
                    if (errorProp.ValueKind == JsonValueKind.String)
                        return errorProp.GetString();

                    if (errorProp.TryGetProperty("message", out var errorMessageProp))
                        return errorMessageProp.GetString();
                }

                if (root.TryGetProperty("errors", out var errorsProp) &&
                    errorsProp.ValueKind == JsonValueKind.Array &&
                    errorsProp.GetArrayLength() > 0)
                {
                    var firstError = errorsProp[0];
                    if (firstError.TryGetProperty("message", out var firstErrorMsg))
                        return firstErrorMsg.GetString();
                    if (firstError.ValueKind == JsonValueKind.String)
                        return firstError.GetString();
                }
            }
            catch (JsonException)
            {
            }

            return null;
        }
    }
}

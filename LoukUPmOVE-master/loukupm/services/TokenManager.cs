using System.Globalization;
using System.Text;
using System.Text.Json;

namespace loukupm.Services
{
    /// <summary>
    /// Single source of truth for access/refresh tokens and expiry metadata.
    /// </summary>
    public class TokenManager
    {
        public const string AccessTokenKey = "auth_token";
        public const string RefreshTokenKey = "refresh_token";

        private const string AccessExpiresAtKey = "access_expires_at";
        private const string RefreshExpiresAtKey = "refresh_expires_at";
        private const int ProactiveRefreshBufferSeconds = 60;

        private readonly SemaphoreSlim _storageLock = new(1, 1);

        public async Task SaveTokensAsync(
            string accessToken,
            string refreshToken,
            string accessExpiresAt = null,
            string refreshExpiresAt = null)
        {
            await _storageLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (!string.IsNullOrWhiteSpace(accessToken))
                    await SecureStorage.SetAsync(AccessTokenKey, accessToken).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(refreshToken))
                    await SecureStorage.SetAsync(RefreshTokenKey, refreshToken).ConfigureAwait(false);

                var accessExpiry = ParseToUtc(accessExpiresAt)
                    ?? TryGetJwtExpiryUtc(accessToken);

                if (accessExpiry.HasValue)
                {
                    await SecureStorage.SetAsync(
                        AccessExpiresAtKey,
                        accessExpiry.Value.ToString("o", CultureInfo.InvariantCulture)).ConfigureAwait(false);
                }

                var refreshExpiry = ParseToUtc(refreshExpiresAt);
                if (refreshExpiry.HasValue)
                {
                    await SecureStorage.SetAsync(
                        RefreshExpiresAtKey,
                        refreshExpiry.Value.ToString("o", CultureInfo.InvariantCulture)).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TokenManager] Error saving tokens: {ex.Message}");
                throw;
            }
            finally
            {
                _storageLock.Release();
            }
        }

        public async Task<string> GetAccessTokenAsync()
        {
            try
            {
                return await SecureStorage.GetAsync(AccessTokenKey).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TokenManager] Error retrieving access token: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetRefreshTokenAsync()
        {
            try
            {
                return await SecureStorage.GetAsync(RefreshTokenKey).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TokenManager] Error retrieving refresh token: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> IsAccessTokenExpiredOrExpiringSoonAsync()
        {
            try
            {
                var expiry = await GetAccessTokenExpiryAsync().ConfigureAwait(false);
                if (!expiry.HasValue)
                {
                    var token = await GetAccessTokenAsync().ConfigureAwait(false);
                    expiry = TryGetJwtExpiryUtc(token);
                }

                if (!expiry.HasValue)
                    return false;

                var remaining = expiry.Value - DateTime.UtcNow;
                return remaining.TotalSeconds <= ProactiveRefreshBufferSeconds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TokenManager] Error checking access token expiration: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IsRefreshTokenExpiredAsync()
        {
            try
            {
                var expiry = await GetRefreshTokenExpiryAsync().ConfigureAwait(false);
                if (!expiry.HasValue)
                    return false;

                return DateTime.UtcNow >= expiry.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TokenManager] Error checking refresh token expiration: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> HasValidTokensAsync()
        {
            try
            {
                var accessToken = await GetAccessTokenAsync().ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(accessToken))
                    return false;

                if (!await IsAccessTokenExpiredOrExpiringSoonAsync().ConfigureAwait(false))
                    return true;

                var refreshToken = await GetRefreshTokenAsync().ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(refreshToken))
                    return false;

                return !await IsRefreshTokenExpiredAsync().ConfigureAwait(false);
            }
            catch
            {
                return false;
            }
        }

        public async Task ClearTokensAsync()
        {
            await _storageLock.WaitAsync().ConfigureAwait(false);
            try
            {
                SecureStorage.Remove(AccessTokenKey);
                SecureStorage.Remove(RefreshTokenKey);
                SecureStorage.Remove(AccessExpiresAtKey);
                SecureStorage.Remove(RefreshExpiresAtKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TokenManager] Error clearing tokens: {ex.Message}");
                throw;
            }
            finally
            {
                _storageLock.Release();
            }
        }

        public async Task<DateTime?> GetAccessTokenExpiryAsync()
        {
            return await ReadExpiryAsync(AccessExpiresAtKey).ConfigureAwait(false);
        }

        public async Task<DateTime?> GetRefreshTokenExpiryAsync()
        {
            return await ReadExpiryAsync(RefreshExpiresAtKey).ConfigureAwait(false);
        }

        private static async Task<DateTime?> ReadExpiryAsync(string key)
        {
            try
            {
                var raw = await SecureStorage.GetAsync(key).ConfigureAwait(false);
                return ParseToUtc(raw);
            }
            catch
            {
                return null;
            }
        }

        internal static DateTime? ParseToUtc(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto))
                return dto.UtcDateTime;

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                return DateTime.SpecifyKind(dt, DateTimeKind.Utc);

            return null;
        }

        internal static DateTime? TryGetJwtExpiryUtc(string jwt)
        {
            if (string.IsNullOrWhiteSpace(jwt))
                return null;

            try
            {
                var parts = jwt.Split('.');
                if (parts.Length < 2)
                    return null;

                var payload = parts[1].Replace('-', '+').Replace('_', '/');
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("exp", out var expProp) && expProp.TryGetInt64(out var seconds))
                    return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
            }
            catch
            {
                // Not a JWT or payload is not JSON — ignore.
            }

            return null;
        }
    }
}

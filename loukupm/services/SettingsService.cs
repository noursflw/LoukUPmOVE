using loukupm.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace loukupm.Services
{
    /// <summary>
    /// Implementation of settings service that communicates with the backend API.
    /// Handles authentication, error responses, and token refresh on 401.
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiServices _apiServices;
        private const string BaseApiUrl = "https://test.center-yazan.com/api/settings";

        public SettingsService(HttpClient httpClient, ApiServices apiServices)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiServices = apiServices ?? throw new ArgumentNullException(nameof(apiServices));
        }

        /// <summary>
        /// Fetches all settings from GET /api/settings endpoint.
        /// </summary>
        public async Task<List<SettingItem>> GetSettingsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await SetAuthorizationHeaderAsync().ConfigureAwait(false);

                var request = new HttpRequestMessage(HttpMethod.Get, BaseApiUrl);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

                // Handle 401: Unauthorized - attempt token refresh
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    if (await _apiServices.RefreshTokenAsync().ConfigureAwait(false))
                    {
                        // Retry with new token
                        await SetAuthorizationHeaderAsync().ConfigureAwait(false);
                        request = new HttpRequestMessage(HttpMethod.Get, BaseApiUrl);
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        response.Dispose();

                        using var retryResponse = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                        return await ProcessGetSettingsResponse(retryResponse, cancellationToken).ConfigureAwait(false);
                    }

                    throw new SettingsServiceException(
                        "Authentication failed. Please log in again.",
                        (int)HttpStatusCode.Unauthorized);
                }

                return await ProcessGetSettingsResponse(response, cancellationToken).ConfigureAwait(false);
            }
            catch (SettingsServiceException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                throw new SettingsServiceException(
                    "Network error. Please check your internet connection.",
                    null,
                    ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new SettingsServiceException(
                    "Request timeout. Please try again.",
                    null,
                    ex);
            }
            catch (Exception ex)
            {
                throw new SettingsServiceException(
                    "Failed to load settings. Please try again later.",
                    null,
                    ex);
            }
        }

        /// <summary>
        /// Updates a setting via PATCH /api/settings/{key}.
        /// Returns true on success, false on failure.
        /// Throws SettingsServiceException for network/auth errors.
        /// </summary>
        public async Task<bool> UpdateSettingAsync(string key, bool value, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Setting key cannot be empty.", nameof(key));

            try
            {
                await SetAuthorizationHeaderAsync().ConfigureAwait(false);

                var requestBody = new PatchSettingRequest { Value = value };
                var url = $"{BaseApiUrl}/{Uri.EscapeDataString(key)}";

                var request = new HttpRequestMessage(HttpMethod.Patch, url)
                {
                    Content = JsonContent.Create(requestBody)
                };
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

                // Handle 401: Unauthorized - attempt token refresh
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    if (await _apiServices.RefreshTokenAsync().ConfigureAwait(false))
                    {
                        // Retry with new token
                        await SetAuthorizationHeaderAsync().ConfigureAwait(false);
                        request = new HttpRequestMessage(HttpMethod.Patch, url)
                        {
                            Content = JsonContent.Create(requestBody)
                        };
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        response.Dispose();

                        using var retryResponse = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                        return await ProcessUpdateSettingResponse(retryResponse, key, value, cancellationToken).ConfigureAwait(false);
                    }

                    throw new SettingsServiceException(
                        "Authentication failed. Please log in again.",
                        (int)HttpStatusCode.Unauthorized);
                }

                return await ProcessUpdateSettingResponse(response, key, value, cancellationToken).ConfigureAwait(false);
            }
            catch (SettingsServiceException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                throw new SettingsServiceException(
                    "Network error. Please check your internet connection.",
                    null,
                    ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new SettingsServiceException(
                    "Request timeout. Please try again.",
                    null,
                    ex);
            }
            catch (Exception ex)
            {
                throw new SettingsServiceException(
                    "Failed to update setting. Please try again later.",
                    null,
                    ex);
            }
        }

        /// <summary>
        /// Processes the response from GET /api/settings endpoint.
        /// </summary>
        private async Task<List<SettingItem>> ProcessGetSettingsResponse(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                {
                    var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var settingsResponse = await response.Content.ReadAsAsync<SettingsResponse>(cancellationToken).ConfigureAwait(false);

                    if (settingsResponse?.Success == true && settingsResponse.Data != null)
                    {
                        return settingsResponse.Data;
                    }

                    throw new SettingsServiceException(
                        "Invalid settings response format.",
                        (int)HttpStatusCode.OK);
                }

                case HttpStatusCode.NotFound:
                    throw new SettingsServiceException(
                        "Settings endpoint not found.",
                        (int)HttpStatusCode.NotFound);

                case HttpStatusCode.UnprocessableEntity:
                    throw new SettingsServiceException(
                        "Invalid request parameters.",
                        (int)HttpStatusCode.UnprocessableEntity);

                case HttpStatusCode.InternalServerError:
                    throw new SettingsServiceException(
                        "Server error. Please try again later.",
                        (int)HttpStatusCode.InternalServerError);

                default:
                    throw new SettingsServiceException(
                        $"Unexpected response status: {response.StatusCode}",
                        (int)response.StatusCode);
            }
        }

        /// <summary>
        /// Processes the response from PATCH /api/settings/{key} endpoint.
        /// Returns true on success, false if the server rejects the value.
        /// </summary>
        private async Task<bool> ProcessUpdateSettingResponse(
            HttpResponseMessage response,
            string key,
            bool value,
            CancellationToken cancellationToken)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                case HttpStatusCode.NoContent:
                {
                    // Success - setting was updated
                    return true;
                }

                case HttpStatusCode.BadRequest:
                {
                    // Validation failed - return false for UI handling
                    var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    System.Diagnostics.Debug.WriteLine($"Setting update validation failed for {key}: {content}");
                    return false;
                }

                case HttpStatusCode.NotFound:
                    throw new SettingsServiceException(
                        $"Setting '{key}' not found.",
                        (int)HttpStatusCode.NotFound);

                case HttpStatusCode.UnprocessableEntity:
                    throw new SettingsServiceException(
                        "Invalid setting value.",
                        (int)HttpStatusCode.UnprocessableEntity);

                case HttpStatusCode.InternalServerError:
                    throw new SettingsServiceException(
                        "Server error updating setting. Please try again later.",
                        (int)HttpStatusCode.InternalServerError);

                default:
                    throw new SettingsServiceException(
                        $"Unexpected response status: {response.StatusCode}",
                        (int)response.StatusCode);
            }
        }

        /// <summary>
        /// Sets the Authorization header with the stored token.
        /// </summary>
        private async Task SetAuthorizationHeaderAsync()
        {
            string? token = await SecureStorage.GetAsync("auth_token").ConfigureAwait(false);

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
    }

    /// <summary>
    /// Extension method for reading HttpContent as JSON.
    /// </summary>
    internal static class HttpContentExtensions
    {
        public static async Task<T?> ReadAsAsync<T>(this HttpContent content, CancellationToken cancellationToken = default)
        {
            var stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return await JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken).ConfigureAwait(false);
        }
    }
}

using System.Net;
using System.Net.Http;
<<<<<<< HEAD
using System.Net.Http.Headers;
=======
>>>>>>> dabe2bf5d4b48b99ca5d457647b100383d8c8797

namespace loukupm.Services
{
    public class AuthenticatedHttpClientHandler : DelegatingHandler
    {
        private readonly TokenManager _tokenManager;
        private readonly AuthenticationService _authService;

        public AuthenticatedHttpClientHandler(TokenManager tokenManager, AuthenticationService authService)
        {
            _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
<<<<<<< HEAD
            EnsureJsonAccept(request);

=======
>>>>>>> dabe2bf5d4b48b99ca5d457647b100383d8c8797
            if (IsPublicEndpoint(request.RequestUri))
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            await ProactiveTokenRefreshAsync().ConfigureAwait(false);

            var bufferedContent = await BufferContentAsync(request, cancellationToken).ConfigureAwait(false);
            await SetAuthorizationHeaderAsync(request).ConfigureAwait(false);

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

<<<<<<< HEAD
            if (!IsAuthenticationFailure(response))
=======
            if (response.StatusCode != HttpStatusCode.Unauthorized)
>>>>>>> dabe2bf5d4b48b99ca5d457647b100383d8c8797
                return response;

            var refreshed = await _authService.RefreshTokenAsync().ConfigureAwait(false);
            if (!refreshed)
                return response;

            response.Dispose();

            var retry = CloneRequest(request, bufferedContent);
<<<<<<< HEAD
            EnsureJsonAccept(retry);
=======
>>>>>>> dabe2bf5d4b48b99ca5d457647b100383d8c8797
            await SetAuthorizationHeaderAsync(retry).ConfigureAwait(false);
            return await base.SendAsync(retry, cancellationToken).ConfigureAwait(false);
        }

<<<<<<< HEAD
        private static void EnsureJsonAccept(HttpRequestMessage request)
        {
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private static bool IsAuthenticationFailure(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return true;

            if (response.StatusCode is not (
                HttpStatusCode.Moved or
                HttpStatusCode.Redirect or
                HttpStatusCode.RedirectMethod or
                HttpStatusCode.TemporaryRedirect or
                HttpStatusCode.PermanentRedirect))
                return false;

            var location = response.Headers.Location?.ToString() ?? string.Empty;
            return location.Contains("login", StringComparison.OrdinalIgnoreCase);
        }

=======
>>>>>>> dabe2bf5d4b48b99ca5d457647b100383d8c8797
        private async Task ProactiveTokenRefreshAsync()
        {
            try
            {
                if (await _tokenManager.IsAccessTokenExpiredOrExpiringSoonAsync().ConfigureAwait(false))
                    await _authService.RefreshTokenAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthHandler] Proactive refresh failed: {ex.Message}");
            }
        }

        private async Task SetAuthorizationHeaderAsync(HttpRequestMessage request)
        {
            try
            {
                var accessToken = await _tokenManager.GetAccessTokenAsync().ConfigureAwait(false);

                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                }
                else
                {
                    request.Headers.Authorization = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthHandler] Error setting authorization header: {ex.Message}");
            }
        }

        private static async Task<byte[]> BufferContentAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Content == null)
                return null;

            var bytes = await request.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            var content = new ByteArrayContent(bytes);
            foreach (var header in request.Content.Headers)
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);

            request.Content = content;
            return bytes;
        }

        private static HttpRequestMessage CloneRequest(HttpRequestMessage request, byte[] bufferedContent)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Version = request.Version
            };

            if (bufferedContent != null)
            {
                clone.Content = new ByteArrayContent(bufferedContent);
                if (request.Content != null)
                {
                    foreach (var header in request.Content.Headers)
                        clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            foreach (var header in request.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            return clone;
        }

        private static bool IsPublicEndpoint(Uri uri)
        {
            if (uri == null)
                return false;

            var path = uri.AbsolutePath.ToLowerInvariant();
            return path.Contains("/api/auth/login") ||
                   path.Contains("/api/auth/register") ||
                   path.Contains("/api/auth/google") ||
                   path.Contains("/api/auth/refresh") ||
                   path.Contains("/api/auth/verify-otp") ||
                   path.Contains("/api/auth/resend-otp") ||
                   path.Contains("/api/pages/");
        }
    }
}

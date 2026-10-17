using System.Reflection;
using Firebase.Auth;
using loukupm.View;

namespace loukupm.Services;

public sealed class GoogleSignInResult
{
    public bool Success { get; init; }

    public bool Cancelled { get; init; }

    public string? Error { get; init; }

    public string? UserId { get; init; }

    public string? Email { get; init; }

    public string? DisplayName { get; init; }

    public static GoogleSignInResult Canceled() =>
        new() { Cancelled = true };

    public static GoogleSignInResult Failed(string message) =>
        new() { Error = message };
}

public static class GoogleAuthSession
{
    private static readonly SemaphoreSlim Gate = new(1, 1);

    public static async Task<GoogleSignInResult> SignInAndConfirmAsync()
    {
        if (!await Gate.WaitAsync(0).ConfigureAwait(false))
            return GoogleSignInResult.Failed("Google sign-in is already in progress.");

        var client = MauiProgram.firebaseclient;

        try
        {
            var config = MauiProgram.firebaseconfig;
            if (client == null ||
                config == null ||
                string.IsNullOrWhiteSpace(config.AuthDomain) ||
                config.Providers == null ||
                !config.Providers.Any(provider => provider.ProviderType == FirebaseProviderType.Google))
            {
                return GoogleSignInResult.Failed("Google sign-in is not configured.");
            }

            UserCredential? credential;
            string? redirectResult = null;
            try
            {
                credential = await client.SignInWithRedirectAsync(
                    FirebaseProviderType.Google,
                    async uri =>
                    {
                        redirectResult = await GoogleAuthPage.CaptureAsync(uri, config.RedirectUri)
                            .ConfigureAwait(false);
                        return redirectResult ?? string.Empty;
                    })
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return GoogleSignInResult.Canceled();
            }

            if (credential?.User == null || string.IsNullOrWhiteSpace(credential.User.Uid))
            {
                SafeSignOut(client);
                return GoogleSignInResult.Canceled();
            }

            var idToken = FirstNonEmpty(
                ExtractParameter(redirectResult, "id_token"),
                ReadGoogleIdToken(credential.AuthCredential),
                await credential.User.GetIdTokenAsync().ConfigureAwait(false));
            if (string.IsNullOrWhiteSpace(idToken))
            {
                SafeSignOut(client);
                return GoogleSignInResult.Failed("Google did not return a valid account.");
            }

            var authService = AppServices.Get<AuthenticationService>();
            var tokenManager = AppServices.Get<TokenManager>();
            if (authService == null || tokenManager == null)
            {
                SafeSignOut(client);
                return GoogleSignInResult.Failed("Authentication service is not available.");
            }

            var email = credential.User.Info?.Email;
            var displayName = credential.User.Info?.DisplayName;

            var (success, accessToken, _, _, _, error) =
                await authService.LoginWithGoogleAsync(
                    idToken,
                    credential.User.Uid,
                    email,
                    displayName).ConfigureAwait(false);

            var savedToken = success
                ? await tokenManager.GetAccessTokenAsync().ConfigureAwait(false)
                : null;

            if (!success ||
                string.IsNullOrWhiteSpace(accessToken) ||
                string.IsNullOrWhiteSpace(savedToken) ||
                !string.Equals(savedToken, accessToken, StringComparison.Ordinal))
            {
                SafeSignOut(client);
                return GoogleSignInResult.Failed(
                    string.IsNullOrWhiteSpace(error)
                        ? "The server did not confirm this Google account."
                        : error);
            }

            return new GoogleSignInResult
            {
                Success = true,
                UserId = credential.User.Uid,
                Email = email,
                DisplayName = displayName
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Google sign-in failed: {ex}");
            SafeSignOut(client);
            return GoogleSignInResult.Failed(
                string.IsNullOrWhiteSpace(ex.Message)
                    ? "Google sign-in failed."
                    : ex.Message);
        }
        finally
        {
            Gate.Release();
        }
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
    }

    private static string? ReadGoogleIdToken(AuthCredential? credential)
    {
        if (credential == null)
            return null;

        var type = credential.GetType();
        var tokenType = type.GetProperty("TokenType")?.GetValue(credential)?.ToString();
        if (!string.Equals(tokenType, "IdToken", StringComparison.Ordinal))
            return null;

        var token = type.GetProperty("Token")?.GetValue(credential) as string;
        return string.IsNullOrWhiteSpace(token) ? null : token;
    }

    private static string? ExtractParameter(string? url, string name)
    {
        if (string.IsNullOrWhiteSpace(url) ||
            !Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return null;
        }

        foreach (var part in new[] { uri.Fragment, uri.Query })
        {
            var data = part.TrimStart('#', '?');
            if (string.IsNullOrEmpty(data))
                continue;

            foreach (var pair in data.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var separator = pair.IndexOf('=');
                if (separator <= 0)
                    continue;

                var key = Uri.UnescapeDataString(pair[..separator]);
                if (!string.Equals(key, name, StringComparison.OrdinalIgnoreCase))
                    continue;

                var value = Uri.UnescapeDataString(pair[(separator + 1)..]);
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
        }

        return null;
    }

    private static void SafeSignOut(FirebaseAuthClient? client)
    {
        try
        {
            client?.SignOut();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Firebase sign-out after failed Google login: {ex.Message}");
        }
    }
}

# أكواد Firebase

المسار: `c:\Users\Nour\Desktop\t\LoukUPmOVE-master\FIREBASE_CODE.md`

الكود منسوخ من الملفات المصدرية. صفحات الدخول والتسجيل كبيرة، وهنا فقط دالة زر Google من كل صفحة.

---

## loukupm/appsettings.json

```json
{
  "Firebase": {
    "ApiKey": "AIzaSyCB5tz3iuvqgKzL6QldndSrK94ePgMIAOg",
    "AuthDomain": "app-test-2c25c.firebaseapp.com"
  },
  "OneSignal": {
    "AppId": "68c49ad8-113c-4160-91cc-5eb9d2c908d5"
  }
}
```

---

## loukupm/loukupm.csproj

```xml
<PackageReference Include="FirebaseAuthentication.net" Version="4.1.0" />
```

---

## loukupm/MauiProgram.cs

```csharp
using CommunityToolkit.Maui;
using FFImageLoading.Maui;
using Firebase.Auth;
using Firebase.Auth.Providers;
using loukupm.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using OneSignalSDK.DotNet;
using The49.Maui.BottomSheet;
using UraniumUI;
using UraniumUI.Blurs;
using loukupm.View;

namespace loukupm;

public static class MauiProgram
{
    public static FirebaseAuthClient firebaseclient;
    public static FirebaseAuthConfig firebaseconfig;
    public static IServiceProvider Services { get; private set; }

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("georgia.ttf", "georgia");
                fonts.AddFont("georgia-bold.ttf", "georgia-bold");
                fonts.AddFont("Oswald-VariableFont_wght.ttf", "Oswald");
            })
            .UseUraniumUI()
            .UseUraniumUIMaterial()
            .UseBottomSheet()
            .UseMauiCommunityToolkit()
            .UseUraniumUIBlurs()
            .UseFFImageLoading();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Load Firebase config
        var configPath = GetConfigurationFilePath();
        var config = new ConfigurationBuilder()
            .AddJsonFile(configPath, optional: false, reloadOnChange: true)
            .Build();

        var firebaseSettings = config.GetSection("Firebase");

        firebaseconfig = new FirebaseAuthConfig
        {
            ApiKey = firebaseSettings["ApiKey"],
            AuthDomain = firebaseSettings["AuthDomain"],
            Providers = new FirebaseAuthProvider[]
            {
                new GoogleProvider().AddScopes("email"),
                new EmailProvider()
            }
        };

        firebaseclient = new FirebaseAuthClient(firebaseconfig);

        builder.Services.AddSingleton<loukupm.Services.TokenManager>();

        builder.Services.AddHttpClient("AuthClient", client =>
        {
            client.BaseAddress = new Uri("https://lookupfriseur.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "LoukUPmOVE-MAUI/1.0");
        });

        builder.Services.AddSingleton(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return new loukupm.Services.AuthenticationService(
                factory.CreateClient("AuthClient"),
                sp.GetRequiredService<loukupm.Services.TokenManager>());
        });

        // ... بقية تسجيل الخدمات في الملف الأصلي
        return builder.Build();
    }

    private static string GetConfigurationFilePath()
    {
#if __ANDROID__
        var documentsPath = FileSystem.AppDataDirectory;
        var configPath = Path.Combine(documentsPath, "appsettings.json");

        using var stream = FileSystem.OpenAppPackageFileAsync("appsettings.json").Result;
        using var fileStream = File.Create(configPath);
        stream.CopyTo(fileStream);

        return configPath;
#else
        return "appsettings.json";
#endif
    }
}
```

---

## loukupm/services/GoogleAuthSession.cs

```csharp
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
```

---

## loukupm/View/GoogleAuthPage.xaml

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="loukupm.View.GoogleAuthPage"
             BackgroundColor="#111111"
             NavigationPage.HasNavigationBar="False"
             Shell.NavBarIsVisible="False">
    <Grid RowDefinitions="Auto,*">
        <Grid Grid.Row="0"
              Padding="8,10"
              BackgroundColor="#111111">
            <Button x:Name="CloseButton"
                    Text="✕"
                    FontSize="22"
                    TextColor="#A8883C"
                    BackgroundColor="Transparent"
                    HorizontalOptions="Start"
                    VerticalOptions="Center"
                    WidthRequest="52"
                    HeightRequest="44"
                    Clicked="OnCloseClicked" />
            <Label Text="Google"
                   TextColor="White"
                   FontSize="18"
                   HorizontalOptions="Center"
                   VerticalOptions="Center" />
        </Grid>

        <WebView x:Name="AuthWebView"
                 Grid.Row="1"
                 BackgroundColor="White" />

        <ActivityIndicator x:Name="LoadingIndicator"
                           Grid.Row="1"
                           IsRunning="True"
                           IsVisible="True"
                           Color="#A8883C"
                           WidthRequest="48"
                           HeightRequest="48"
                           HorizontalOptions="Center"
                           VerticalOptions="Center" />
    </Grid>
</ContentPage>
```

---

## loukupm/View/GoogleAuthPage.xaml.cs

```csharp
using System.Threading;

namespace loukupm.View;

public partial class GoogleAuthPage : ContentPage
{
#if IOS || MACCATALYST
    private const string BrowserUserAgent =
        "Mozilla/5.0 (iPhone; CPU iPhone OS 18_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/18.0 Mobile/15E148 Safari/604.1";
#elif WINDOWS
    private const string BrowserUserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36";
#else
    private const string BrowserUserAgent =
        "Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Mobile Safari/537.36";
#endif

    private readonly string _authUri;
    private readonly string _handlerPrefix;
    private readonly TaskCompletionSource<string?> _done = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private CancellationTokenSource? _timeoutCts;
    private int _completed;
    private bool _started;
    private bool _closing;

    public GoogleAuthPage(string authUri, string handlerPrefix)
    {
        InitializeComponent();
        _authUri = authUri;
        _handlerPrefix = handlerPrefix;
        AuthWebView.UserAgent = BrowserUserAgent;
        AuthWebView.Navigating += OnNavigating;
        AuthWebView.Navigated += OnNavigated;
    }

    public static Task<string> CaptureAsync(string authUri, string handlerPrefix)
    {
        var page = new GoogleAuthPage(authUri, handlerPrefix);

        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var host = Application.Current?.Windows.FirstOrDefault()?.Page
                       ?? Application.Current?.MainPage
                       ?? throw new InvalidOperationException("No page is available for Google sign-in.");

            await host.Navigation.PushModalAsync(page);
            return await page._done.Task ?? string.Empty;
        });
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        ConfigureBrowser();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ConfigureBrowser();

        if (_started)
            return;

        _started = true;
        _timeoutCts = new CancellationTokenSource();
        _ = WatchTimeoutAsync(_timeoutCts.Token);
        AuthWebView.Source = _authUri;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _closing = true;

        if (Interlocked.CompareExchange(ref _completed, 0, 0) == 0)
            _ = CompleteAsync(null, cancelled: true, error: null);
    }

    private void OnCloseClicked(object? sender, EventArgs e)
    {
        _ = CompleteAsync(null, cancelled: true, error: null);
    }

    private void OnNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (!TryTakeRedirect(e.Url, out var denied))
            return;

        e.Cancel = true;
        var url = e.Url;
        Dispatcher.Dispatch(async () =>
        {
            if (denied)
            {
                await CompleteAsync(
                    null,
                    cancelled: false,
                    error: new InvalidOperationException("Google sign-in was denied."));
            }
            else
            {
                await CompleteAsync(url, cancelled: false, error: null);
            }
        });
    }

    private void OnNavigated(object? sender, WebNavigatedEventArgs e)
    {
        if (LoadingIndicator.IsVisible)
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }

        if (!TryTakeRedirect(e.Url, out var denied))
            return;

        if (denied)
            _ = CompleteAsync(null, cancelled: false, error: new InvalidOperationException("Google sign-in was denied."));
        else
            _ = CompleteAsync(e.Url, cancelled: false, error: null);
    }

    private bool TryTakeRedirect(string? url, out bool denied)
    {
        denied = false;

        if (string.IsNullOrWhiteSpace(url) ||
            !url.StartsWith(_handlerPrefix, StringComparison.OrdinalIgnoreCase) ||
            !Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var data = (uri.Query + uri.Fragment).ToLowerInvariant();
        if (data.Contains("error=", StringComparison.Ordinal))
        {
            denied = true;
            return true;
        }

        return data.Contains("code=", StringComparison.Ordinal)
               || data.Contains("id_token=", StringComparison.Ordinal)
               || data.Contains("access_token=", StringComparison.Ordinal)
               || data.Contains("oauth_token=", StringComparison.Ordinal);
    }

    private async Task WatchTimeoutAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(TimeSpan.FromMinutes(3), token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        await CompleteAsync(
            null,
            cancelled: false,
            error: new TimeoutException("Google sign-in timed out. Please try again."));
    }

    private async Task CompleteAsync(string? url, bool cancelled, Exception? error)
    {
        if (Interlocked.Exchange(ref _completed, 1) != 0)
            return;

        try
        {
            _timeoutCts?.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }

        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (!_closing && Navigation.ModalStack.Contains(this))
                {
                    _closing = true;
                    await Navigation.PopModalAsync();
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Google auth page close failed: {ex.Message}");
        }

        if (cancelled)
            _done.TrySetResult(null);
        else if (error != null)
            _done.TrySetException(error);
        else
            _done.TrySetResult(url);
    }

    private void ConfigureBrowser()
    {
        AuthWebView.UserAgent = BrowserUserAgent;

#if ANDROID
        if (AuthWebView.Handler?.PlatformView is Android.Webkit.WebView androidWebView)
        {
            androidWebView.Settings.JavaScriptEnabled = true;
            androidWebView.Settings.DomStorageEnabled = true;
            androidWebView.Settings.UserAgentString = BrowserUserAgent;
        }
#elif IOS || MACCATALYST
        if (AuthWebView.Handler?.PlatformView is WebKit.WKWebView wkWebView)
            wkWebView.CustomUserAgent = BrowserUserAgent;
#endif
    }
}
```

---

## loukupm/services/AuthenticationService.cs

```csharp
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

        var payload = new Dictionary<string, string>();
        AddGoogleField(payload, "token", "Token", idToken);
        AddGoogleField(payload, "id_token", "IdToken", idToken);
        AddGoogleField(payload, "user_id", "UserId", userId);
        AddGoogleField(payload, "email", "Email", email);
        AddGoogleField(payload, "display_name", "DisplayName", displayName);

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

private static void AddGoogleField(
    Dictionary<string, string> payload,
    string snakeName,
    string pascalName,
    string value)
{
    if (string.IsNullOrWhiteSpace(value))
        return;

    payload[snakeName] = value;
    payload[pascalName] = value;
}
```

---

## loukupm/View/LoginPage.xaml.cs

```csharp
private async void GoogleButton_Clicked(
    object sender,
    EventArgs e)
{
    if (_googleInProgress)
        return;

    _googleInProgress = true;
    StopGoogleButtonAnimation();

    var signedIn = false;

    try
    {
        GoogleButton.IsVisible = false;
        GoogleLoadingIndicator.IsVisible = true;
        GoogleLoadingIndicator.IsRunning = true;

        var result = await GoogleAuthSession.SignInAndConfirmAsync();

        if (result.Cancelled)
            return;

        if (!result.Success)
        {
            await DisplayAlert(
                "Sign-in failed",
                string.IsNullOrWhiteSpace(result.Error)
                    ? "Google sign-in was not confirmed."
                    : result.Error,
                "OK");
            return;
        }

        try
        {
            await AppViewModel.Instance.LoadUserDataAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Profile load after Google login failed: {ex.Message}");
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(result.Email))
                OneSignalService.AddTag("email", result.Email);

            OneSignalService.AddTag("login_type", "google");

            if (!string.IsNullOrWhiteSpace(result.DisplayName))
                OneSignalService.AddTag("display_name", result.DisplayName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OneSignal Google login tag failed: {ex.Message}");
        }

        signedIn = true;
        IsLogged = true;

        NavigationService.ResetLogoutFlag();
        NavigationService.SetAppContext();

        CancelLoginButtonAnimations();
        await ShellNavigationManager.NavigateToHomeAndClear();
        QueueResetLoginStateAfterLeave();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Exception during Google sign-in: " + ex);
        IsLogged = false;
        signedIn = false;

        await DisplayAlert(
            "Sign-in failed",
            string.IsNullOrWhiteSpace(ex.Message)
                ? "Google sign-in failed."
                : ex.Message,
            "OK");
    }
    finally
    {
        _googleInProgress = false;

        if (!signedIn && this.Handler?.MauiContext != null)
        {
            GoogleLoadingIndicator.IsRunning = false;
            GoogleLoadingIndicator.IsVisible = false;
            GoogleButton.IsVisible = true;
            fglogin.IsVisible = true;

            isGoogleButtonAnimating = true;
            StartGoogleButtonAnimation();
        }
    }
}
```

---

## loukupm/View/SinginPage.xaml.cs

```csharp
private async void GoogleButton_Clicked(
    object sender,
    EventArgs e)
{
    if (_googleInProgress || !DD.IsChecked)
        return;

    _googleInProgress = true;
    StopGoogleButtonAnimation();

    var signedIn = false;

    try
    {
        GoogleButton.IsVisible = false;
        GoogleLoadingIndicator.IsVisible = true;
        GoogleLoadingIndicator.IsRunning = true;

        var result = await GoogleAuthSession.SignInAndConfirmAsync();

        if (result.Cancelled)
            return;

        if (!result.Success)
        {
            await DisplayAlert(
                "Sign-in failed",
                string.IsNullOrWhiteSpace(result.Error)
                    ? "Google sign-in was not confirmed."
                    : result.Error,
                "OK");
            return;
        }

        try
        {
            await AppViewModel.Instance.LoadUserDataAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Profile load after Google signup failed: {ex.Message}");
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(result.Email))
                OneSignalService.AddTag("email", result.Email);

            OneSignalService.AddTag("signup_type", "google");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OneSignal Google signup tag failed: {ex.Message}");
        }

        signedIn = true;
        IsLogged = true;

        NavigationService.ResetLogoutFlag();
        NavigationService.SetAppContext();

        await ShellNavigationManager.NavigateToHomeAndClear();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Exception during Google sign-in: " + ex);
        IsLogged = false;
        signedIn = false;

        await DisplayAlert(
            "Sign-in failed",
            string.IsNullOrWhiteSpace(ex.Message)
                ? "Google sign-in failed."
                : ex.Message,
            "OK");
    }
    finally
    {
        _googleInProgress = false;

        if (!signedIn && Handler?.MauiContext != null)
        {
            GoogleLoadingIndicator.IsRunning = false;
            GoogleLoadingIndicator.IsVisible = false;
            GoogleButton.IsVisible = true;
            fglogin.IsVisible = true;

            isGoogleButtonAnimating = true;
            StartGoogleButtonAnimation();
        }
    }
}
```

---

## loukupm/Model/AuthtoctionWithGoogle.cs

هذا المسار القديم لا يستخدم `FirebaseAuthClient`. القيم داخله ما زالت placeholders.

```csharp
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Storage;

namespace loukupm.Model
{
    public class UserTokenResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public BasicUserDto User { get; set; }
    }

    public class BasicUserDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string PictureUrl { get; set; }
    }

    public class GoogleCodeRequest
    {
        public string Code { get; set; }
    }

    public class AuthtoctionWithGoogle
    {
        private const string ClientId = "YOUR_GOOGLE_CLIENT_ID";
        private const string RedirectUri = "myapp://callback";
        private const string BackendUrl = "https://your-api.com";

        private const string SecureKey_Jwt = "app_jwt_token";
        private const string SecureKey_Refresh = "app_refresh_token";

        private readonly HttpClient _httpClient;

        public AuthtoctionWithGoogle(HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<UserTokenResponse> LoginAsync()
        {
            try
            {
                var authUrl = new Uri(
                    $"https://accounts.google.com/o/oauth2/v2/auth" +
                    $"?client_id={Uri.EscapeDataString(ClientId)}" +
                    $"&redirect_uri={Uri.EscapeDataString(RedirectUri)}" +
                    $"&response_type=code" +
                    $"&scope={Uri.EscapeDataString("openid email profile")}" +
                    $"&access_type=offline" +
                    $"&prompt=consent"
                );

                var webAuthResult = await WebAuthenticator.AuthenticateAsync(authUrl, new Uri(RedirectUri));

                if (webAuthResult?.Properties != null && webAuthResult.Properties.TryGetValue("code", out var code))
                {
                    var response = await SendCodeToBackendAsync(code);

                    if (response != null && !string.IsNullOrEmpty(response.Token))
                    {
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
                System.Diagnostics.Debug.WriteLine($"Google login failed: {ex}");
                return null;
            }
        }

        private async Task<UserTokenResponse> SendCodeToBackendAsync(string code)
        {
            var url = $"{BackendUrl}/auth/google";

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

        public static async Task LogoutAsync()
        {
            try
            {
                SecureStorage.Remove(SecureKey_Jwt);
                SecureStorage.Remove(SecureKey_Refresh);
            }
            catch
            {
            }
        }

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
```

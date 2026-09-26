using loukupm.View;
using Microsoft.Maui.Storage;
using OneSignalSDK.DotNet;
using OneSignalSDK.DotNet.Core.Debug;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using loukupm.Services;

namespace loukupm;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        try
        {
            OneSignal.Debug.LogLevel = LogLevel.VERBOSE;

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var oneSignalAppId = config.GetSection("OneSignal")["AppId"];

            OneSignal.Initialize(oneSignalAppId);
            OneSignal.Notifications.RequestPermissionAsync(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ OneSignal init failed: {ex.Message}");
        }

        var savedLang = Preferences.Get("AppLanguage", string.Empty);
        CultureInfo initialCulture;

        if (!string.IsNullOrEmpty(savedLang))
        {
            // Use saved language
            initialCulture = new CultureInfo(savedLang);
            Langue.LocalizationResourcesManager.Instanse.SetCulture(initialCulture);

            Console.WriteLine($"🌍 Loaded saved language: {initialCulture.DisplayName}");
        }
        else
        {
            // First run: Use system language
            initialCulture = CultureInfo.CurrentCulture;

            // Support Arabic from system, otherwise default to German
            string langCode = initialCulture.TwoLetterISOLanguageName.ToLower();
            if (langCode != "ar")
            {
                initialCulture = new CultureInfo("de-DE");
            }

            Langue.LocalizationResourcesManager.Instanse.SetCulture(initialCulture);
            Preferences.Set("AppLanguage", initialCulture.Name);

            Console.WriteLine($"🌍 First run: System language detected, set to {initialCulture.DisplayName}");
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(CreateRootPage());
    }

    internal static Page CreateRootPage()
    {
        var direction = GetSavedFlowDirection();
        string? token = null;
        try
        {
            token = SecureStorage.GetAsync(TokenManager.AccessTokenKey).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Auth token read failed: {ex.Message}");
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            NavigationService.SetLoginContext();
            return new NavigationPage(new LoginPage { FlowDirection = direction })
            {
                FlowDirection = direction
            };
        }

        NavigationService.SetAppContext();
        return new AppShell { FlowDirection = direction };
    }

    internal static Microsoft.Maui.FlowDirection GetSavedFlowDirection()
    {
        var lang = Preferences.Get("AppLanguage", "de-DE");
        return lang.StartsWith("ar", StringComparison.OrdinalIgnoreCase)
            ? Microsoft.Maui.FlowDirection.RightToLeft
            : Microsoft.Maui.FlowDirection.LeftToRight;
    }

    public static void SetRootPage(Page page)
    {
        var app = Current;
        if (app == null)
            return;

        if (app.Windows.Count > 0)
            app.Windows[0].Page = page;
        else
            app.MainPage = page;
    }

    /// <summary>
    /// Performs authentication checks on app startup.
    /// Attempts to refresh token if it's expired.
    /// </summary>
    private async Task PerformAuthenticationCheckAsync()
    {
        try
        {
            // Get services from the app's service provider
            var services = this.Handler?.MauiContext?.Services;
            if (services == null)
            {
                Console.WriteLine("⚠️ Services not available for auth check");
                return;
            }

            var tokenManager = services.GetService(typeof(TokenManager)) as TokenManager;
            var authService = services.GetService(typeof(AuthenticationService)) as AuthenticationService;

            if (tokenManager == null || authService == null)
            {
                Console.WriteLine("⚠️ TokenManager or AuthenticationService not available");
                return;
            }

            // Check if user has valid tokens
            bool hasValidTokens = await tokenManager.HasValidTokensAsync();

            if (!hasValidTokens)
            {
                // Keep the current session if tokens exist but expiry metadata is not ready yet.
                // Only send the user to login when there is truly no access token.
                var accessToken = await tokenManager.GetAccessTokenAsync();
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    await tokenManager.ClearTokensAsync();
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        SetRootPage(CreateRootPage());
                    });
                    return;
                }
            }

            // Check if access token needs proactive refresh
            bool accessTokenExpired = await tokenManager.IsAccessTokenExpiredOrExpiringSoonAsync();

            if (accessTokenExpired)
            {
                Console.WriteLine("⏰ [App] Access token expiring, attempting proactive refresh...");
                bool refreshed = await authService.RefreshTokenAsync();

                if (refreshed)
                {
                    Console.WriteLine("✅ [App] Token refreshed successfully on startup");
                }
                else
                {
                    // Network blips on cold start must not log the user out.
                    bool refreshExpired = await tokenManager.IsRefreshTokenExpiredAsync();
                    var refreshToken = await tokenManager.GetRefreshTokenAsync();
                    if (refreshExpired || string.IsNullOrWhiteSpace(refreshToken))
                    {
                        await tokenManager.ClearTokensAsync();
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            SetRootPage(CreateRootPage());
                        });
                    }
                    else
                    {
                        Console.WriteLine("⚠️ [App] Token refresh failed temporarily — keeping session");
                    }
                }
            }
            else
            {
                Console.WriteLine("✅ [App] Authentication valid on startup");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [App] Authentication check failed: {ex.Message}");
        }
    }

    protected override void OnStart()
    {
        _ = PerformAuthenticationCheckAsync();
    }
    protected override void OnResume()
    {
        _ = ResumeHomeIfNeededAsync();
    }

    private static async Task ResumeHomeIfNeededAsync()
    {
        try
        {
            if (Current?.Windows?.FirstOrDefault()?.Page is not AppShell
                && Current?.MainPage is not AppShell)
            {
                return;
            }

            var vm = loukupm.ViewModel.AppViewModel.Instance;
            if (vm.HasNoHomeData && vm.RefreshHomeCommand?.CanExecute(null) == true)
                await vm.RefreshHomeCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ [App] Resume home refresh skipped: {ex.Message}");
        }
    }
    public static void ResetAuthenticationCheck()
    {
        Console.WriteLine("🔄 ResetAuthenticationCheck called (no-op in new flow)");
    }

    public void LoadLoginPage()
    {
        _ = NavigationService.NavigateToLoginAndClear();
    }
}
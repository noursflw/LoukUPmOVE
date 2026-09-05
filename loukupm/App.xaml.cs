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
        var direction = Microsoft.Maui.FlowDirection.LeftToRight;
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

        direction = initialCulture.TwoLetterISOLanguageName == "ar"
            ? Microsoft.Maui.FlowDirection.RightToLeft
            : Microsoft.Maui.FlowDirection.LeftToRight;

        string? token = null;
        try
        {
            token = SecureStorage.GetAsync(TokenManager.AccessTokenKey).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Auth token read failed: {ex.Message}");
        }

        MainPage = string.IsNullOrWhiteSpace(token)
            ? new NavigationPage(new LoginPage { FlowDirection = direction })
            : new AppShell { FlowDirection = direction };
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
                await tokenManager.ClearTokensAsync();
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    var direction = MainPage?.FlowDirection ?? Microsoft.Maui.FlowDirection.LeftToRight;
                    MainPage = new NavigationPage(new LoginPage { FlowDirection = direction });
                });
                return;
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
                    await tokenManager.ClearTokensAsync();
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        var direction = MainPage?.FlowDirection ?? Microsoft.Maui.FlowDirection.LeftToRight;
                        MainPage = new NavigationPage(new LoginPage { FlowDirection = direction });
                    });
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
    protected override void OnResume() { }
    public static void ResetAuthenticationCheck()
    {
        Console.WriteLine("🔄 ResetAuthenticationCheck called (no-op in new flow)");
    }
}
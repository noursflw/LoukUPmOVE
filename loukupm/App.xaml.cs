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
            token = SecureStorage.GetAsync("auth_token").GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Auth token read failed: {ex.Message}");
        }

        MainPage = string.IsNullOrWhiteSpace(token)
            ? new NavigationPage(new LoginPage { FlowDirection = direction })
            : new AppShell { FlowDirection = direction };
    }

    protected override void OnStart() { }
    protected override void OnResume() { }
    public static void ResetAuthenticationCheck()
    {
        Console.WriteLine("🔄 ResetAuthenticationCheck called (no-op in new flow)");
    }
}
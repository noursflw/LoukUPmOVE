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

        if (!string.IsNullOrEmpty(savedLang))
        {
            var culture = new CultureInfo(savedLang);
            Langue.LocalizationResourcesManager.Instanse.SetCulture(culture);

            direction = culture.TwoLetterISOLanguageName == "ar"
                ? Microsoft.Maui.FlowDirection.RightToLeft
                : Microsoft.Maui.FlowDirection.LeftToRight;
        }

        // ⭐ أهم نقطة: التطبيق يبدأ من LoadingPage فقط
        MainPage = new NavigationPage(new LoadingPage())
        {
            FlowDirection = direction
        };

        // تشغيل فحص الدخول بعد ما UI يجهز بالكامل
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Task.Delay(600);
            await HandleStartNavigation();
        });
    }

    private async Task HandleStartNavigation()
    {
        try
        {
            await Task.Delay(800); // يعطي وقت لتجهيز الـ UI

            var token = await SecureStorage.GetAsync("auth_token");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    MainPage = new NavigationPage(new LoginPage());
                }
                else
                {
                    MainPage = new NavigationPage(new AppShell());
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Auth error: {ex.Message}");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                MainPage = new NavigationPage(new LoginPage());
            });
        }
    }

    protected override void OnStart() { }
    protected override void OnResume() { }
    public static void ResetAuthenticationCheck()
    {
        Console.WriteLine("🔄 ResetAuthenticationCheck called (no-op in new flow)");
    }
}
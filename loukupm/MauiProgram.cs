using CommunityToolkit.Maui;
using FFImageLoading.Maui;
using Firebase.Auth;
using Firebase.Auth.Providers;
using loukupm.Services;
using Microsoft.Extensions.Configuration;
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

        // 🖤 Global Theme (Android + iOS)
        builder.ConfigureLifecycleEvents(events =>
        {
#if ANDROID
            events.AddAndroid(android => android.OnResume(activity =>
            {
                var black = Android.Graphics.Color.Black;

                activity.Window.SetNavigationBarColor(black);
                activity.Window.SetStatusBarColor(black);
            }));
#endif

#if IOS
            events.AddiOS(ios => ios.FinishedLaunching((app, options) =>
            {
                // iOS does not fully allow status bar color control
                // so we keep system appearance clean via Info.plist

                return true;
            }));
#endif
        });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // 📦 Load Firebase config
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

        // Register services and viewmodels (no singleton AppViewModel)
        builder.Services.AddSingleton<loukupm.services.NotificationStateService>();
        builder.Services.AddSingleton<loukupm.services.NotificationService>();
        builder.Services.AddTransient<loukupm.ViewModel.NotificationViewModel>();
        builder.Services.AddTransient<loukupm.ViewModel.AppViewModel>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<loukupm.View.NotifictionPage>();
        builder.Services.AddTransient<loukupm.View.NotificationBadgeView>();

        return builder.Build();

    }

    private static string GetConfigurationFilePath()
    {
#if __ANDROID__
        var documentsPath = FileSystem.AppDataDirectory;
        var configPath = Path.Combine(documentsPath, "appsettings.json");

        if (!File.Exists(configPath))
        {
            using var stream = FileSystem.OpenAppPackageFileAsync("appsettings.json").Result;
            using var fileStream = File.Create(configPath);
            stream.CopyTo(fileStream);
        }

        return configPath;
#else
        return "appsettings.json";
#endif
    }
}
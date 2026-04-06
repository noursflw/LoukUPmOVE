using CommunityToolkit.Maui;
using FFImageLoading.Maui;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Microsoft.Extensions.Configuration;
using  Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Logging;
using The49.Maui.BottomSheet;
using UraniumUI.Blurs;
using UraniumUI;
using OneSignalSDK.DotNet;
using loukupm.Services;

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

#if DEBUG
        builder.Logging.AddDebug();
#endif

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

        return builder.Build();
    }
  
    private static string GetConfigurationFilePath()
    {
#if __ANDROID__
        var documentsPath = FileSystem.AppDataDirectory;
        var configPath = Path.Combine(documentsPath, "appsettings.json");

        // If the file doesn't exist in AppDataDirectory, try to read from assets
        if (!File.Exists(configPath))
        {
            // For Android, the file should be in the assets directory
            // We'll copy it from assets to the app data directory
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


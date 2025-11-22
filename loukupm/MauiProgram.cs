using CommunityToolkit.Maui;
using FFImageLoading.Maui;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Microsoft.Extensions.Logging;
using The49.Maui.BottomSheet;
using UraniumUI;

namespace loukupm
{
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
                .UseFFImageLoading();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // 🔹 إعداد Firebase
            firebaseconfig = new FirebaseAuthConfig
            {
                ApiKey = "AIzaSyCB5tz3iuvqgKzL6QldndSrK94ePgMIAOg", // استخدم API Key الخاص بك
                AuthDomain = "app-test-2c25c.firebaseapp.com", // AuthDomain الصحيح
                Providers = new FirebaseAuthProvider[]
                {
                    new GoogleProvider().AddScopes("email"),
                    new EmailProvider()
                }
            };

            firebaseclient = new FirebaseAuthClient(firebaseconfig);

            return builder.Build();
        }
    }
}

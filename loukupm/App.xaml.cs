using loukupm.View;
using Microsoft.Maui.Storage;
using OneSignalSDK.DotNet;
using OneSignalSDK.DotNet.Core.Debug;
using System.Globalization;
using loukupm.Services;

namespace loukupm
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            try 
            {
                _ = OneSignalService.Init();
                Console.WriteLine("✅ OneSignal initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ OneSignal init failed: {ex.Message}");
                // يمكنك تسجيل الخطأ في Sentry أو logging service
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

           
            MainPage = new AppShell
            {
                FlowDirection = direction
            };

          
            MainPage.Loaded += async (s, e) => await CheckAuthentication();
        }

        
        private async Task CheckAuthentication()
        {
            try
            {
                await Task.Delay(600); // تأخير بسيط لضمان تحميل الواجهة

                string token = string.Empty;
                try
                {
                    token = await SecureStorage.GetAsync("auth_token");
                }
                catch (Exception storageEx)
                {
                    Console.WriteLine($"[SecureStorage ERROR]: {storageEx.Message}");
                }

                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("🔐 No token found → LoginPage");
                    await Shell.Current.GoToAsync("LoginPage");
                }
                else
                {
                    Console.WriteLine("✅ Token found → HomePage");
                    await Shell.Current.GoToAsync("//HomePage");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[App CheckAuthentication ERROR]: {ex.Message}");
                await Shell.Current.GoToAsync("LoginPage");
            }
        }
    }
}

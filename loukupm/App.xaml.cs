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
        // Flag to track if authentication has been checked
        private static bool _authenticationChecked = false;
        // Flag to track if app is freshly started
        private static bool _appJustStarted = true;

        public App()
        {
            InitializeComponent();
           
            try

            {
                OneSignal.Debug.LogLevel = LogLevel.VERBOSE;
                OneSignal.Initialize("68c49ad8-113c-4160-91cc-5eb9d2c908d5");
                OneSignal.Notifications.RequestPermissionAsync(true);

                //_ = OneSignalService.Init();

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

           
            MainPage = new AppShell
            {
                FlowDirection = direction
            };

          
            MainPage.Loaded += async (s, e) => await CheckAuthentication();
        }

        /// <summary>
        /// Reset the authentication check flag when user logs out
        /// </summary>
        public static void ResetAuthenticationCheck()
        {
            _authenticationChecked = false;
            _appJustStarted = true;
            Console.WriteLine("🔄 Authentication check reset");
        }

        
        
        private async Task CheckAuthentication()
        {
            try
            {
                // Only run authentication check on app startup, not after manual navigation
                if (!_appJustStarted)
                {
                    Console.WriteLine("⏭️ Skipping auth check - app already initialized");
                    return;
                }

                _appJustStarted = false;

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
                    // Use relative routing for LoginPage (global route)
                    await Shell.Current.GoToAsync("LoginPage");
                }
                else
                {
                    Console.WriteLine("✅ Token found → HomePage");
                    // Mark authentication as checked
                    _authenticationChecked = true;
                    // Use absolute routing for HomePage (inside TabBar)
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

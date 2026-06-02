using loukupm.View;
using Microsoft.Maui.Storage;
using OneSignalSDK.DotNet;
using OneSignalSDK.DotNet.Core.Debug;
using System.Globalization;
using Microsoft.Extensions.Configuration; 
using loukupm.Services;

namespace loukupm
{
    public partial class App : Application
    {
       
        private static bool _authenticationChecked = false;
       
        private static bool _appJustStarted = true;

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

            MainPage = new AppShell()
            {
                FlowDirection = direction
            };

            MainPage.Loaded += async (s, e) =>
            {
                try
                {
                    await CheckAuthentication();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ [App] Error in CheckAuthentication: {ex.Message}");
                    Console.WriteLine($"   Stack: {ex.StackTrace}");
                    // Fallback to login page on any error
                    try
                    {
                        await NavigationService.NavigateToPage(NavigationService.ROUTE_LOGIN);
                    }
                    catch (Exception navEx)
                    {
                        Console.WriteLine($"❌ [App] Emergency fallback navigation failed: {navEx.Message}");
                    }
                }
            };
        }

        // ─────────────────────────────────────────────────────────
        // APP LIFECYCLE - Handle notifications on app resume
        // ─────────────────────────────────────────────────────────
        protected override void OnStart()
        {
            base.OnStart();
            Console.WriteLine("📱 [App] OnStart - app started or resumed from background");
        }

        protected override void OnResume()
        {
            base.OnResume();
            Console.WriteLine("📱 [App] OnResume - app resumed from background");

            // Check if app was opened via notification tap
            // Platform-specific handlers in MainActivity.cs and AppDelegate.cs
            // will call OneSignalService.HandleNotificationTapped() if needed
        }


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
                    Console.WriteLine("🔐 No token found → NavigateToLoginPage (absolute routing)");
                    await NavigationService.NavigateToLoginPage();
                }
                else
                {
                    Console.WriteLine("✅ Token found → NavigateToMainApp (absolute routing)");
                    _authenticationChecked = true;
                    await NavigationService.NavigateToMainApp();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[App CheckAuthentication ERROR]: {ex.Message}");
                await NavigationService.NavigateToLoginPage();
            }
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Console.WriteLine("💥 UNHANDLED CRASH:");
                Console.WriteLine(e.ExceptionObject.ToString());
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                Console.WriteLine("💥 TASK CRASH:");
                Console.WriteLine(e.Exception.ToString());
                e.SetObserved();
            };
        }
    }
}
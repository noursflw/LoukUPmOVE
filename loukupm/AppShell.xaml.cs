using loukupm.View;
using loukupm.Services;
using OneSignalSDK.DotNet;

namespace loukupm
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            RegisterAllRoutes();
            ValidateNavigation();
            SetupNotificationTapHandler();
        }

        // ─────────────────────────────────────────────────────────
        // ONESIGNAL NOTIFICATION TAP HANDLER
        // Handles notification taps in foreground and background states
        // ─────────────────────────────────────────────────────────
        private void SetupNotificationTapHandler()
        {
            try
            {
                // Set up handler for when notifications are tapped while app is running
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    // Delay briefly to allow Shell to fully initialize
                    await Task.Delay(500);
                    Console.WriteLine("✅ [AppShell] OneSignal notification tap handler ready for foreground/background");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ [AppShell] Error setting up notification handler: {ex.Message}");
            }
        }

       
        protected override bool OnBackButtonPressed()
        {
            try
            {
                var currentPage = NavigationService.GetCurrentPageName();
                Console.WriteLine($"[AppShell] OnBackButtonPressed triggered from page: {currentPage}");

                // Delegate to centralized HandleBackButton logic
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        bool handled = await NavigationService.HandleBackButton(currentPage);
                        Console.WriteLine($"[AppShell] Back button handling result: {handled}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[AppShell] ERROR in HandleBackButton: {ex.Message}");
                        Console.WriteLine($"[AppShell] Exception: {ex.GetType().Name}");
                        Console.WriteLine($"[AppShell] Stack trace: {ex.StackTrace}");
                    }
                });

                return true; // Always handled by centralized logic
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppShell] CRITICAL ERROR in OnBackButtonPressed: {ex.Message}");
                Console.WriteLine($"[AppShell] Stack trace: {ex.StackTrace}");
                return true; // Prevent crash by returning true
            }
        }

        // ─────────────────────────────────────────────────────────
        // ROUTE REGISTRATION
        // ─────────────────────────────────────────────────────────

        private void RegisterAllRoutes()
        {
            // Auth pages
            Routing.RegisterRoute(NavigationService.ROUTE_MAIN_PAGE, typeof(MainPage));
            Routing.RegisterRoute(NavigationService.ROUTE_LOGIN, typeof(LoginPage));
            Routing.RegisterRoute(NavigationService.ROUTE_SIGNIN, typeof(SinginPage));

            // Booking subpages
            Routing.RegisterRoute(NavigationService.ROUTE_TERM_BOOKING, typeof(TerminbuchenPage));
            Routing.RegisterRoute(NavigationService.ROUTE_PAYMENT, typeof(Paymentgetway));

            // Info / legal subpages
            Routing.RegisterRoute(NavigationService.ROUTE_POLICY_PRIVACY, typeof(PolicyandPrivacyPage));
            Routing.RegisterRoute(NavigationService.ROUTE_REST_PASSWORD, typeof(RestPassword));
            Routing.RegisterRoute(NavigationService.ROUTE_TERMS_CONDITIONS, typeof(TermsAndConditions));

            // Profile subpages
            Routing.RegisterRoute(NavigationService.ROUTE_EDIT_USER, typeof(EditeUserPage));
            Routing.RegisterRoute(NavigationService.ROUTE_EDIT_PASSWORD, typeof(EditePasswordPage));
            Routing.RegisterRoute(NavigationService.ROUTE_EDIT_PASSWORD_VERIFICATION, typeof(EditPasswordVerification));
            Routing.RegisterRoute(NavigationService.ROUTE_CHACKOUT, typeof(ChackoutPage));
            Routing.RegisterRoute(NavigationService.ROUTE_ABOUT_US, typeof(AboutUS));
            Routing.RegisterRoute(NavigationService.ROUTE_NOTIFICATION, typeof(NotifictionPage));
            Routing.RegisterRoute(NavigationService.ROUTE_SETTING, typeof(SettingPage));

            // TabBar pages (also registered here for Release-mode safety)
            Routing.RegisterRoute(NavigationService.ROUTE_HOME, typeof(HomePage));
            Routing.RegisterRoute(NavigationService.ROUTE_SERVICES, typeof(ServicesPage));
            Routing.RegisterRoute(NavigationService.ROUTE_BOOKING, typeof(BookingPage));
            Routing.RegisterRoute(NavigationService.ROUTE_PROFILE, typeof(ProfilePage));

            Console.WriteLine("[AppShell] All routes registered");
        }

        private void ValidateNavigation()
        {
            try
            {
                NavigationService.ValidateRoutes();
                Console.WriteLine("[AppShell] Navigation validation passed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppShell] Navigation validation error: {ex.Message}");
            }
        }
    }
}

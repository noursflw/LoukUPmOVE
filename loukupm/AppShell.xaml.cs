using loukupm.Services;
using loukupm.View;
using loukupm.ViewModel;
using OneSignalSDK.DotNet;

namespace loukupm
{
    public partial class AppShell : Shell
    {
        // ✅ Flag to prevent concurrent navigation from back button presses
        private bool _isNavigating = false;

        public AppShell()
        {
            InitializeComponent();
            RegisterAllRoutes();
            ValidateNavigation();
            SetupNotificationTapHandler();
            this.BindingContext = AppViewModel.Instance;
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

                // ✅ Use a flag to prevent concurrent back navigations
                if (_isNavigating)
                {
                    Console.WriteLine($"[AppShell] Navigation already in progress, ignoring back button");
                    return true;
                }

                _isNavigating = true;

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
                    finally
                    {
                        _isNavigating = false;
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
            try
            {
                // ✅ Wrap route registration in try-catch per group
                var routesToRegister = new[]
                {
                    // Auth pages
                    (NavigationService.ROUTE_MAIN_PAGE, typeof(MainPage)),
                    (NavigationService.ROUTE_LOGIN, typeof(LoginPage)),
                    (NavigationService.ROUTE_SIGNIN, typeof(SinginPage)),
                    (NavigationService.ROUTE_OTP, typeof(OTPSINGIN)),

                    // Booking subpages
                    (NavigationService.ROUTE_TERM_BOOKING, typeof(TerminbuchenPage)),
                    (NavigationService.ROUTE_PAYMENT, typeof(Paymentgetway)),

                    // Info / legal subpages
                    (NavigationService.ROUTE_POLICY_PRIVACY, typeof(PolicyandPrivacyPage)),
                    (NavigationService.ROUTE_REST_PASSWORD, typeof(RestPassword)),
                    (NavigationService.ROUTE_TERMS_CONDITIONS, typeof(TermsAndConditions)),

                    // Profile subpages
                    (NavigationService.ROUTE_EDIT_USER, typeof(EditeUserPage)),
                    (NavigationService.ROUTE_EDIT_PASSWORD, typeof(EditePasswordPage)),
                    (NavigationService.ROUTE_EDIT_PASSWORD_VERIFICATION, typeof(EditPasswordVerification)),
                    (NavigationService.ROUTE_CHACKOUT, typeof(ChackoutPage)),
                    (NavigationService.ROUTE_ABOUT_US, typeof(AboutUS)),
                    (NavigationService.ROUTE_NOTIFICATION, typeof(NotifictionPage)),
                    (NavigationService.ROUTE_SETTING, typeof(SettingPage)),

                    // TabBar pages (also registered here for Release-mode safety)
                    (NavigationService.ROUTE_HOME, typeof(HomePage)),
                    (NavigationService.ROUTE_SERVICES, typeof(ServicesPage)),
                    (NavigationService.ROUTE_BOOKING, typeof(BookingPage)),
                    (NavigationService.ROUTE_PROFILE, typeof(ProfilePage)),
                };

                int successCount = 0;
                foreach (var (route, pageType) in routesToRegister)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(route))
                        {
                            Console.WriteLine($"⚠️ [AppShell] Route key is null or empty for type {pageType.Name}");
                            continue;
                        }

                        Routing.RegisterRoute(route, pageType);
                        successCount++;
                        Console.WriteLine($"✅ Registered route: {route} → {pageType.Name}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Failed to register route {route} ({pageType.Name}): {ex.Message}");
                    }
                }

                Console.WriteLine($"[AppShell] Route registration complete: {successCount}/{routesToRegister.Length} successful");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppShell] CRITICAL ERROR in RegisterAllRoutes: {ex.Message}");
                throw; // Critical failure - don't allow app to continue
            }
        }

        private void ValidateNavigation()
        {
            try
            {
                NavigationService.ValidateRoutes();
                Console.WriteLine("[AppShell] Navigation validation passed ✅");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppShell] Navigation validation error: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
            }
        }
    }
}

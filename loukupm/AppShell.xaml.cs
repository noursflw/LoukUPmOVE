using loukupm.View;
using loukupm.Services;

namespace loukupm
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            RegisterAllRoutes();
            ValidateNavigation();
        }

        /// <summary>
        /// Register all routes used in the application
        /// CRITICAL: Every navigable page must be registered here for Release mode to work!
        /// </summary>
        private void RegisterAllRoutes()
        {
            // ============================================
            // AUTH PAGES (Outside TabBar, Hidden)
            // ============================================
            Routing.RegisterRoute(NavigationService.ROUTE_MAIN_PAGE, typeof(MainPage));
            Routing.RegisterRoute(NavigationService.ROUTE_LOGIN, typeof(LoginPage));
            Routing.RegisterRoute(NavigationService.ROUTE_SIGNIN, typeof(SinginPage));

            // ============================================
            // HIDDEN MODAL PAGES (Outside TabBar)
            // ============================================
            Routing.RegisterRoute(NavigationService.ROUTE_TERM_BOOKING, typeof(TerminbuchenPage));
            Routing.RegisterRoute(NavigationService.ROUTE_PAYMENT, typeof(Paymentgetway));

            // ============================================
            // POLICY/TERMS PAGES (Outside TabBar, Hidden)
            // ============================================
            Routing.RegisterRoute(NavigationService.ROUTE_POLICY_PRIVACY, typeof(PolicyandPrivacyPage));
            Routing.RegisterRoute(NavigationService.ROUTE_REST_PASSWORD, typeof(RestPassword));
            Routing.RegisterRoute(NavigationService.ROUTE_TERMS_CONDITIONS, typeof(TermsAndConditions));

            // ============================================
            // PROFILE SECTION PAGES (Outside TabBar, Hidden)
            // ============================================
            Routing.RegisterRoute(NavigationService.ROUTE_EDIT_USER, typeof(EditeUserPage));
            Routing.RegisterRoute(NavigationService.ROUTE_EDIT_PASSWORD, typeof(EditePasswordPage));
            Routing.RegisterRoute(NavigationService.ROUTE_ABOUT_US, typeof(AboutUS));
            Routing.RegisterRoute(NavigationService.ROUTE_NOTIFICATION, typeof(NotifictionPage));
            Routing.RegisterRoute(NavigationService.ROUTE_SETTING, typeof(SettingPage));

            // ============================================
            // TABBAR PAGES (Inside TabBar)
            // Note: These are defined in AppShell.xaml with Route="PageName"
            // but we register them here for consistency and Release mode safety
            // ============================================
            Routing.RegisterRoute(NavigationService.ROUTE_HOME, typeof(HomePage));
            Routing.RegisterRoute(NavigationService.ROUTE_SERVICES, typeof(ServicesPage));
            Routing.RegisterRoute(NavigationService.ROUTE_BOOKING, typeof(BookingPage));
            Routing.RegisterRoute(NavigationService.ROUTE_PROFILE, typeof(ProfilePage));

            Console.WriteLine("✅ [AppShell] All routes registered successfully");
        }

        /// <summary>
        /// Validate navigation setup on app startup
        /// </summary>
        private void ValidateNavigation()
        {
            try
            {
                var isValid = NavigationService.ValidateRoutes();
                
                if (isValid)
                {
                    Console.WriteLine("✅ [AppShell] Navigation validation PASSED");
                }
                else
                {
                    Console.WriteLine("⚠️ [AppShell] Navigation validation skipped");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AppShell] Navigation validation error: {ex.Message}");
            }
        }
    }
}
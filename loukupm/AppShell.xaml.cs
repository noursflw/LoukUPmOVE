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

        // ─────────────────────────────────────────────────────────
        // BACK BUTTON — global handler for the entire Shell
        //
        // Navigation rules enforced here:
        //
        //   TabBar page (any tab except Home) → Back → //HomePage
        //   TabBar page (Home)                → Back → exit app (return false)
        //   Subpage                           → Back → pop one level (..)
        //
        // Subpages that inherit NavigationAwarePage also override
        // OnBackButtonPressed, but AppShell fires first in MAUI Shell,
        // so this handler takes precedence for system/hardware back.
        // ─────────────────────────────────────────────────────────
        protected override bool OnBackButtonPressed()
        {
            var currentPage = NavigationService.GetCurrentPageName();

            if (NavigationService.IsTabBarPage(currentPage))
            {
                // On HomePage — let the OS handle it (exit or minimize the app)
                if (currentPage == NavigationService.ROUTE_HOME)
                    return false;

                // On any other tab — go to Home without adding to the stack
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await NavigationService.NavigateToTabBarPage(NavigationService.ROUTE_HOME);
                });

                return true; // Handled
            }

            // Subpage — pop one level off the stack
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await NavigationService.HandleBackButton(currentPage);
            });

            return true; // Handled
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

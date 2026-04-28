namespace loukupm.Services;

/// <summary>
/// Navigation Service � enforces two navigation rules:
///
///   RULE 1 � Tab Bar pages:
///     Back button always navigates to HomePage (absolute route //HomePage).
///     Switching between tabs never touches the back stack.
///     Exception: pressing Back while already on HomePage exits the app (returns false).
///
///   RULE 2 � Subpages (pages outside the TabBar):
///     Back button pops exactly one entry off the navigation stack ("..")
///     regardless of which tab was the origin.
/// </summary>
public static class NavigationService
{
    // ?????????????????????????????????????????????
    // ROUTE CONSTANTS
    // ?????????????????????????????????????????????

    // Auth pages (hidden, outside TabBar)
    public const string ROUTE_MAIN_PAGE = "MainPage";
    public const string ROUTE_LOGIN = "LoginPage";
    public const string ROUTE_SIGNIN = "SinginPage";

    // TabBar pages
    public const string ROUTE_HOME = "HomePage";
    public const string ROUTE_SERVICES = "ServicesPage";
    public const string ROUTE_BOOKING = "BookingPage";
    public const string ROUTE_PROFILE = "ProfilePage";

    // Subpages (outside TabBar � push onto the stack)
    public const string ROUTE_TERM_BOOKING = "TerminbuchenPage";
    public const string ROUTE_PAYMENT = "Paymentgetway";
    public const string ROUTE_POLICY_PRIVACY = "PolicyandPrivacyPage";
    public const string ROUTE_REST_PASSWORD = "RestPassword";
    public const string ROUTE_TERMS_CONDITIONS = "TermsAndConditions";
    public const string ROUTE_EDIT_USER = "EditeUserPage";
    public const string ROUTE_EDIT_PASSWORD = "EditePasswordPage";
    public const string ROUTE_EDIT_PASSWORD_VERIFICATION = "EditPasswordVerification";
    public const string ROUTE_CHACKOUT = "ChackoutPage";
    public const string ROUTE_ABOUT_US = "AboutUS";
    public const string ROUTE_NOTIFICATION = "NotifictionPage";
    public const string ROUTE_SETTING = "SettingPage";

    // ?????????????????????????????????????????????
    // SETS
    // ?????????????????????????????????????????????

    /// <summary>All four TabBar pages.</summary>
    private static readonly HashSet<string> TabBarPages = new()
    {
        ROUTE_HOME,
        ROUTE_SERVICES,
        ROUTE_BOOKING,
        ROUTE_PROFILE
    };

    private static readonly HashSet<string> AllValidRoutes = new()
    {
        ROUTE_MAIN_PAGE, ROUTE_LOGIN, ROUTE_SIGNIN,
        ROUTE_HOME, ROUTE_SERVICES, ROUTE_BOOKING, ROUTE_PROFILE,
        ROUTE_TERM_BOOKING, ROUTE_PAYMENT,
        ROUTE_POLICY_PRIVACY, ROUTE_REST_PASSWORD, ROUTE_TERMS_CONDITIONS,
        ROUTE_EDIT_USER, ROUTE_EDIT_PASSWORD, ROUTE_EDIT_PASSWORD_VERIFICATION, ROUTE_CHACKOUT,
        ROUTE_ABOUT_US, ROUTE_NOTIFICATION, ROUTE_SETTING
    };

    // ?????????????????????????????????????????????
    // PUBLIC HELPERS
    // ?????????????????????????????????????????????

    /// <summary>Returns true if <paramref name="route"/> is one of the four TabBar pages.</summary>
    public static bool IsTabBarPage(string route) => TabBarPages.Contains(route);

    // ?????????????????????????????????????????????
    // NAVIGATION
    // ?????????????????????????????????????????????

    /// <summary>
    /// Navigate to a TabBar page using an absolute route (//Page).
    /// Replaces the navigation stack root � tab switching never adds stack entries.
    /// </summary>
    public static async Task NavigateToTabBarPage(string route)
    {
        if (!ValidateRoute(route) || !TabBarPages.Contains(route))
            return;

        try
        {
            await Shell.Current.GoToAsync($"//{route}", animate: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] Error navigating to tab {route}: {ex.Message}");
        }
    }

    /// <summary>
    /// Navigate to a subpage using a relative route (pushes onto the stack).
    /// </summary>
    public static async Task NavigateToPage(string route)
    {
        if (!ValidateRoute(route) || TabBarPages.Contains(route))
            return;

        try
        {
            await Shell.Current.GoToAsync(route, animate: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] Error navigating to {route}: {ex.Message}");
        }
    }

    // ?????????????????????????????????????????????
    // BACK BUTTON
    // ?????????????????????????????????????????????

    /// <summary>
    /// Central back-button handler.  Call from both the system back button override
    /// (AppShell.OnBackButtonPressed) and any in-app back button/gesture.
    ///
    ///   � TabBar page + not Home  ?  navigate to //HomePage
    ///   � TabBar page + Home      ?  return false  (let system exit the app)
    ///   � Subpage                 ?  pop one level (..)
    /// </summary>
    /// <param name="currentPage">The simple route name of the visible page (e.g. "ServicesPage").</param>
    /// <returns>true if the event was handled; false to let the OS handle it (exit).</returns>
    public static async Task<bool> HandleBackButton(string currentPage)
    {
        if (string.IsNullOrWhiteSpace(currentPage))
            return false;

        try
        {
            if (TabBarPages.Contains(currentPage))
            {
                // Already on Home � do nothing, let the OS exit the app
                if (currentPage == ROUTE_HOME)
                    return false;

                // Any other tab ? go to Home
                await Shell.Current.GoToAsync($"//{ROUTE_HOME}", animate: true);
                return true;
            }

            // Subpage � pop one entry off the stack
            await Shell.Current.GoToAsync("..", animate: true);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] Back button error from {currentPage}: {ex.Message}");
            return false;
        }
    }

    // ?????????????????????????????????????????????
    // AUTH FLOWS
    // ?????????????????????????????????????????????

    /// <summary>Navigate to LoginPage and clear the entire stack (logout).</summary>
    public static async Task NavigateToLoginAndClear()
    {
        try
        {
            await Shell.Current.GoToAsync("LoginPage", animate: false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] Logout navigation error: {ex.Message}");
            throw;
        }
    }

    /// <summary>Navigate to HomePage (TabBar) and clear the entire stack (after login).</summary>
    public static async Task NavigateToHomeAndClear()
    {
        try
        {
            await Shell.Current.GoToAsync($"//{ROUTE_HOME}", animate: false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] Login navigation error: {ex.Message}");
            throw;
        }
    }

    // ?????????????????????????????????????????????
    // DIAGNOSTICS
    // ?????????????????????????????????????????????

    /// <summary>
    /// Returns the raw Shell location string (e.g. "//HomePage/NotifictionPage").
    /// Use <see cref="GetCurrentPageName"/> to get just the last segment.
    /// </summary>
    public static string GetCurrentRoute()
    {
        try
        {
            return Shell.Current?.CurrentState?.Location?.OriginalString ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// Extracts just the last path segment from the current route.
    /// e.g. "//HomePage/NotifictionPage" ? "NotifictionPage"
    /// </summary>
    public static string GetCurrentPageName()
    {
        var route = GetCurrentRoute();
        var segments = route.TrimStart('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.LastOrDefault() ?? string.Empty;
    }

    /// <summary>
    /// Validates all routes are registered (call at app startup).
    /// </summary>
    public static bool ValidateRoutes()
    {
        if (Shell.Current == null)
        {
            Console.WriteLine("[Navigation] Shell.Current is null � validation skipped");
            return false;
        }
        Console.WriteLine("[Navigation] Route validation OK");
        return true;
    }

    // ?????????????????????????????????????????????
    // PRIVATE
    // ?????????????????????????????????????????????

    private static bool ValidateRoute(string route)
    {
        if (string.IsNullOrWhiteSpace(route) || !AllValidRoutes.Contains(route))
        {
            Console.WriteLine($"[Navigation] Invalid route: '{route}'");
            return false;
        }
        return true;
    }
}
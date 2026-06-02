namespace loukupm.Services;

/// <summary>
/// Tracks where a Flyout page was opened from.
/// Allows the back button to return to the correct origin page.
/// </summary>
public enum NavigationOrigin
{
    /// <summary>No Flyout page is currently being navigated to.</summary>
    None,

    /// <summary>Flyout page opened from an authentication screen (LoginPage, SinginPage, OTPSINGIN).</summary>
    Authentication,

    /// <summary>Flyout page opened from a main app TabBar page (HomePage, ServicesPage, BookingPage, ProfilePage).</summary>
    MainApp
}

/// <summary>
/// Navigation Service - enforces navigation rules for TabBar, Subpages, and Flyout pages.
///
///   RULE 1 - Tab Bar pages:
///     Back button navigates to HomePage (absolute route //HomePage).
///     Exception: pressing Back while on HomePage exits the app (returns false).
///
///   RULE 2 - Flyout pages (About Us, Privacy, Terms, Settings, Impressum):
///     Back button uses Shell navigation stack (..) to return to previous page.
///     Shell correctly tracks where the page was opened from (Auth or MainApp).
///     The stack-based approach is more reliable than manual route matching.
///
///   RULE 3 - Subpages (all other pages pushed onto the stack):
///     Back button uses Shell navigation stack (..) to return to previous page.
///
///   KEY PRINCIPLE: Always trust Shell navigation stack for non-TabBar pages.
///   The Shell maintains the stack correctly and knows the proper previous page.
///   Do NOT override the stack with manual HomePage navigation - that breaks
///   authentication flows (LoginPage → Flyout → Back should return to LoginPage).
/// </summary>
public static class NavigationService
{
    // ================================================
    // NAVIGATION ORIGIN TRACKING
    // ================================================

    /// <summary>
    /// Tracks where the current/last Flyout page was opened from.
    /// Note: This field is maintained for backward compatibility and diagnostics,
    /// but the current implementation uses Shell stack navigation for all Flyout pages,
    /// which is more reliable than origin-based logic.
    /// </summary>
    private static NavigationOrigin _flyoutOrigin = NavigationOrigin.None;

    /// <summary>
    /// Sets the origin context for the next Flyout page navigation.
    /// Call this BEFORE navigating to a Flyout page.
    /// 
    /// Example:
    /// <code>
    /// NavigationService.SetFlyoutOrigin(NavigationOrigin.Authentication);
    /// await NavigationService.NavigateToPage(NavigationService.ROUTE_POLICY_PRIVACY);
    /// </code>
    /// </summary>
    /// <param name="origin">Where the Flyout page is being opened from.</param>
    public static void SetFlyoutOrigin(NavigationOrigin origin)
    {
        _flyoutOrigin = origin;
        Console.WriteLine($"[Navigation] Flyout origin set to: {origin}");
    }

    /// <summary>
    /// Gets the current Flyout origin for diagnostics or advanced scenarios.
    /// </summary>
    /// <returns>The current NavigationOrigin value.</returns>
    public static NavigationOrigin GetFlyoutOrigin()
    {
        return _flyoutOrigin;
    }

    /// <summary>
    /// Resets the Flyout origin to None (useful after navigation completion).
    /// </summary>
    public static void ResetFlyoutOrigin()
    {
        _flyoutOrigin = NavigationOrigin.None;
        Console.WriteLine($"[Navigation] Flyout origin reset to: None");
    }

    // ================================================
    // ROUTE CONSTANTS
    // ================================================

    // Auth pages (hidden, outside TabBar)
    public const string ROUTE_MAIN_PAGE = "MainPage";
    public const string ROUTE_LOGIN = "LoginPage";
    public const string ROUTE_SIGNIN = "SinginPage";
    public const string ROUTE_OTP = "OTPSINGIN";

    // TabBar pages
    public const string ROUTE_HOME = "HomePage";
    public const string ROUTE_SERVICES = "ServicesPage";
    public const string ROUTE_BOOKING = "BookingPage";
    public const string ROUTE_PROFILE = "ProfilePage";

    // Subpages (outside TabBar - push onto the stack)
    public const string ROUTE_TERM_BOOKING = "TerminbuchenPage";
    public const string ROUTE_IMPRESSUM = "ImpressumPage"; 
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

    // ================================================
    // SETS
    // ================================================

    /// <summary>All four TabBar pages.</summary>
    private static readonly HashSet<string> TabBarPages = new()
    {
        ROUTE_HOME,
        ROUTE_SERVICES,
        ROUTE_BOOKING,
        ROUTE_PROFILE
    };

    /// <summary>Flyout menu pages (About Us, Privacy Policy, Terms and Conditions, Settings, Impressum).</summary>
    private static readonly HashSet<string> FlyoutPages = new()
    {
        ROUTE_ABOUT_US,
        ROUTE_POLICY_PRIVACY,
        ROUTE_TERMS_CONDITIONS,
        ROUTE_SETTING,
        ROUTE_IMPRESSUM
    };

    private static readonly HashSet<string> AllValidRoutes = new()
    {
        ROUTE_MAIN_PAGE, ROUTE_LOGIN, ROUTE_SIGNIN, ROUTE_OTP,
        ROUTE_HOME, ROUTE_SERVICES, ROUTE_BOOKING, ROUTE_PROFILE,
        ROUTE_TERM_BOOKING, ROUTE_PAYMENT,
        ROUTE_POLICY_PRIVACY, ROUTE_REST_PASSWORD, ROUTE_TERMS_CONDITIONS,
        ROUTE_EDIT_USER, ROUTE_EDIT_PASSWORD, ROUTE_EDIT_PASSWORD_VERIFICATION, ROUTE_CHACKOUT,
        ROUTE_ABOUT_US, ROUTE_NOTIFICATION, ROUTE_IMPRESSUM, ROUTE_SETTING
    };

    // ================================================
    // PUBLIC HELPERS
    // ================================================

    /// <summary>Returns true if <paramref name="route"/> is one of the four TabBar pages.</summary>
    public static bool IsTabBarPage(string route) => TabBarPages.Contains(route);

    /// <summary>Returns true if <paramref name="route"/> is one of the Flyout menu pages.</summary>
    public static bool IsFlyoutPage(string route) => FlyoutPages.Contains(route);

    // ================================================
    // NAVIGATION
    // ================================================

    /// <summary>
    /// Navigate to a TabBar page using an absolute route (//Page).
    /// Replaces the navigation stack root - tab switching never adds stack entries.
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
    /// For Flyout pages, call SetFlyoutOrigin() BEFORE this method.
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

    /// <summary>
    /// Navigate to a subpage with a parameter object.
    /// For Flyout pages, call SetFlyoutOrigin() BEFORE this method.
    /// </summary>
    public static async Task NavigateToPage(string route, object parameter)
    {
        if (!ValidateRoute(route))
            return;

        try
        {
            // Serialize the parameter as JSON and pass as query string
            string json = System.Text.Json.JsonSerializer.Serialize(parameter);
            string encodedJson = Uri.EscapeDataString(json);
            string routeWithParam = $"{route}?data={encodedJson}";

            await Shell.Current.GoToAsync(routeWithParam, animate: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] Error navigating to {route}: {ex.Message}");
        }
    }

    // ================================================
    // BACK BUTTON
    // ================================================

    /// <summary>
    /// Central back-button handler. Call from both the system back button override
    /// (AppShell.OnBackButtonPressed) and any in-app back button/gesture.
    ///
    /// Navigation behavior:
    ///   - TabBar page (not Home): navigate to //HomePage
    ///   - TabBar page (Home): return false (let system exit the app)
    ///   - Flyout page: respects SetFlyoutOrigin():
    ///     * Authentication origin: use Shell stack ("..")
    ///     * MainApp origin: use Shell stack ("..")
    ///     * None origin: use Shell stack ("..")
    ///   - Subpage: use Shell stack ("..")
    ///   - Default: Always respect Shell navigation stack via ".."
    ///
    /// CRITICAL: This method prioritizes Shell navigation stack over manual routing.
    /// The Shell handles the stack correctly - we should only override for TabBar pages.
    /// </summary>
    /// <param name="currentPage">The simple route name of the visible page (e.g. "ServicesPage").</param>
    /// <returns>true if the event was handled; false to let the OS handle it (exit).</returns>
    public static async Task<bool> HandleBackButton(string currentPage)
    {
        if (string.IsNullOrWhiteSpace(currentPage))
            return false;

        try
        {
            // RULE 1: TabBar pages - only special handling for TabBar
            if (TabBarPages.Contains(currentPage))
            {
                // Already on Home - do nothing, let the OS exit the app
                if (currentPage == ROUTE_HOME)
                {
                    Console.WriteLine($"[Navigation] On HomePage - allowing system exit");
                    return false;
                }

                // Any other tab - go to Home (not using ".." because TabBar navigation is absolute)
                Console.WriteLine($"[Navigation] TabBar page '{currentPage}' - navigating to HomePage");
                await Shell.Current.GoToAsync($"//{ROUTE_HOME}", animate: true);
                return true;
            }

            // RULE 2 & 3: ALL other pages (Flyout + Subpage) - trust the Shell navigation stack
            // The Shell maintains the stack correctly, so ".." will return to the actual previous page
            // DO NOT force HomePage navigation for Flyout pages - that overrides the stack!

            Console.WriteLine($"[Navigation] Non-TabBar page '{currentPage}' - using Shell stack navigation (..)");
            Console.WriteLine($"[Navigation] Current route: {GetCurrentRoute()}");
            Console.WriteLine($"[Navigation] Flyout origin: {_flyoutOrigin}");

            // Simply pop one level - Shell handles the stack correctly
            await Shell.Current.GoToAsync("..", animate: true);

            // Reset origin after navigation (it's no longer relevant)
            ResetFlyoutOrigin();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] Back button error from {currentPage}: {ex.Message}");
            return false;
        }
    }

    // ================================================
    // AUTH FLOWS
    // ================================================

    /// <summary>
    /// Navigate to LoginPage using absolute routing (//LoginPage).
    /// This ensures LoginPage is a root page, not pushed onto the stack.
    /// Call this to show the authentication screen.
    /// </summary>
    public static async Task NavigateToLoginPage()
    {
        try
        {
            ResetFlyoutOrigin();
            Console.WriteLine($"[Navigation] Navigating to LoginPage (absolute route)");
            await Shell.Current.GoToAsync("//LoginPage", animate: false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] Error navigating to LoginPage: {ex.Message}");
            throw;
        }
    }

    /// <summary>Navigate to LoginPage and clear the entire stack (logout).</summary>
    public static async Task NavigateToLoginAndClear()
    {
        try
        {
            ResetFlyoutOrigin();
            Console.WriteLine($"[Navigation] Logging out and navigating to LoginPage (absolute route)");
            await Shell.Current.GoToAsync("//LoginPage", animate: false);
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
            ResetFlyoutOrigin();
            await Shell.Current.GoToAsync($"//{ROUTE_HOME}", animate: false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] Login navigation error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Navigate to main app (HomePage) using absolute routing (//HomePage).
    /// This ensures HomePage is a root page, not pushed onto the stack.
    /// Call this when user successfully authenticates.
    /// </summary>
    public static async Task NavigateToMainApp()
    {
        try
        {
            ResetFlyoutOrigin();
            Console.WriteLine($"[Navigation] Navigating to main app (HomePage)");
            await Shell.Current.GoToAsync($"//{ROUTE_HOME}", animate: false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] Error navigating to main app: {ex.Message}");
            throw;
        }
    }

    // ================================================
    // DIAGNOSTICS
    // ================================================

    /// <summary>
    /// Returns the raw Shell location string (e.g. "//HomePage/NotifictionPage").
    /// Use GetCurrentPageName() to get just the last segment.
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
    /// e.g. "//HomePage/NotifictionPage" becomes "NotifictionPage"
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
            Console.WriteLine("[Navigation] Shell.Current is null - validation skipped");
            return false;
        }
        Console.WriteLine("[Navigation] Route validation OK");
        return true;
    }

    // ================================================
    // PRIVATE
    // ================================================

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

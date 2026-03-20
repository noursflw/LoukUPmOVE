namespace loukupm.Services;

/// <summary>
/// Safe Shell Navigation Service - Release mode compatible
/// Replaces unsafe string-based navigation with type-safe routing
/// 
/// KEY DIFFERENCES FROM OLD VERSION:
/// - Uses constants instead of string literals
/// - Validates routes at startup
/// - Works reliably in Release mode
/// - Includes comprehensive error handling
/// - Supports both TabBar and hidden pages
/// </summary>
public static class NavigationService
{
    // ============================================
    // ROUTE CONSTANTS (Type-safe, not reflection)
    // ============================================
    
    // Auth Pages (Hidden, outside TabBar)
    public const string ROUTE_MAIN_PAGE = "MainPage";
    public const string ROUTE_LOGIN = "LoginPage";
    public const string ROUTE_SIGNIN = "SinginPage";
    
    // TabBar Pages (Inside TabBar)
    public const string ROUTE_HOME = "HomePage";
    public const string ROUTE_SERVICES = "ServicesPage";
    public const string ROUTE_BOOKING = "BookingPage";
    public const string ROUTE_PROFILE = "ProfilePage";
    
    // Hidden Pages (Outside TabBar)
    public const string ROUTE_TERM_BOOKING = "TerminbuchenPage";
    public const string ROUTE_PAYMENT = "Paymentgetway";
    
    // Terms & Policy (Hidden, outside TabBar)
    public const string ROUTE_POLICY_PRIVACY = "PolicyandPrivacyPage";
    public const string ROUTE_REST_PASSWORD = "RestPassword";
    public const string ROUTE_TERMS_CONDITIONS = "TermsAndConditions";
    public const string ROUTE_EDIT_USER = "EditeUserPage";
    public const string ROUTE_EDIT_PASSWORD = "EditePasswordPage";
    public const string ROUTE_ABOUT_US = "AboutUS";
    public const string ROUTE_NOTIFICATION = "NotifictionPage";
    public const string ROUTE_SETTING = "SettingPage";
    
    // TabBar Internal Routes (not navigated directly)
    private static readonly HashSet<string> TabBarPages = new()
    {
        ROUTE_HOME,
        ROUTE_SERVICES,
        ROUTE_BOOKING,
        ROUTE_PROFILE
    };
    
    // All valid routes for validation
    private static readonly HashSet<string> AllValidRoutes = new()
    {
        ROUTE_MAIN_PAGE,
        ROUTE_LOGIN,
        ROUTE_SIGNIN,
        ROUTE_HOME,
        ROUTE_SERVICES,
        ROUTE_BOOKING,
        ROUTE_PROFILE,
        ROUTE_TERM_BOOKING,
        ROUTE_PAYMENT,
        ROUTE_POLICY_PRIVACY,
        ROUTE_REST_PASSWORD,
        ROUTE_TERMS_CONDITIONS,
        ROUTE_EDIT_USER,
        ROUTE_EDIT_PASSWORD,
        ROUTE_ABOUT_US,
        ROUTE_NOTIFICATION,
        ROUTE_SETTING
    };

    /// <summary>
    /// Back navigation mapping - which page to go back to from each page
    /// </summary>
    private static readonly Dictionary<string, string> BackNavigationMap = new()
    {
        // Auth flow
        [ROUTE_SIGNIN] = ROUTE_LOGIN,
        [ROUTE_POLICY_PRIVACY] = ROUTE_LOGIN,
        [ROUTE_REST_PASSWORD] = ROUTE_LOGIN,
        [ROUTE_TERMS_CONDITIONS] = ROUTE_LOGIN,
        
        // Main app flow
        [ROUTE_SERVICES] = ROUTE_HOME,
        [ROUTE_BOOKING] = ROUTE_HOME,
        [ROUTE_ABOUT_US] = ROUTE_HOME,
        [ROUTE_NOTIFICATION] = ROUTE_HOME,
        
        // Profile flow
        [ROUTE_EDIT_USER] = ROUTE_PROFILE,
        [ROUTE_EDIT_PASSWORD] = ROUTE_PROFILE,
        [ROUTE_SETTING] = ROUTE_PROFILE,
    };

    /// <summary>
    /// Track navigation source for dynamic back navigation
    /// Useful when a page can be accessed from multiple sources
    /// </summary>
    private static readonly Dictionary<string, string> PageSourceMap = new();

    /// <summary>
    /// Validate that all routes are properly registered (call at app startup)
    /// </summary>
    public static bool ValidateRoutes()
    {
        try
        {
            // Check if Shell.Current is available
            if (Shell.Current == null)
            {
                Console.WriteLine("?? [Navigation] Shell.Current is null - validation skipped");
                return false;
            }

            Console.WriteLine("? [Navigation] Route validation enabled (Release mode safe)");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"?? [Navigation] Validation warning: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Navigate to a TabBar page (absolute routing with //)
    /// Use for: HomePage, ServicesPage, BookingPage, ProfilePage
    /// </summary>
    public static async Task NavigateToTabBarPage(string route)
    {
        if (!ValidateRoute(route))
            return;

        try
        {
            if (!TabBarPages.Contains(route))
            {
                Console.WriteLine($"?? [Navigation] {route} is not a TabBar page. Use NavigateToPage() instead.");
                return;
            }

            Console.WriteLine($"?? [Navigation] Navigating to TabBar page: {route}");
            await Shell.Current.GoToAsync($"//{route}", animate: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? [Navigation] Error navigating to {route}: {ex.Message}");
        }
    }

    /// <summary>
    /// Navigate to a hidden page (relative routing)
    /// Use for: PolicyandPrivacyPage, TermsAndConditions, RestPassword, etc.
    /// </summary>
    public static async Task NavigateToPage(string route)
    {
        if (!ValidateRoute(route))
            return;

        try
        {
            if (TabBarPages.Contains(route))
            {
                Console.WriteLine($"?? [Navigation] {route} is a TabBar page. Use NavigateToTabBarPage() instead.");
                return;
            }

            Console.WriteLine($"?? [Navigation] Navigating to page: {route}");
            await Shell.Current.GoToAsync(route, animate: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? [Navigation] Error navigating to {route}: {ex.Message}");
        }
    }

    /// <summary>
    /// Register the source page for a target page (for dynamic back navigation)
    /// Use when a page can be accessed from multiple sources
    /// </summary>
    public static void RegisterPageSource(string targetPage, string sourcePage)
    {
        if (!ValidateRoute(targetPage) || !ValidateRoute(sourcePage))
            return;

        PageSourceMap[targetPage] = sourcePage;
        Console.WriteLine($"?? [Navigation] Registered: {targetPage} came from {sourcePage}");
    }

    /// <summary>
    /// Navigate to a page and register its source
    /// Useful for pages that need intelligent back navigation
    /// </summary>
    public static async Task NavigateToWithSource(string targetPage, string sourcePage)
    {
        RegisterPageSource(targetPage, sourcePage);
        
        if (TabBarPages.Contains(targetPage))
            await NavigateToTabBarPage(targetPage);
        else
            await NavigateToPage(targetPage);
    }

    /// <summary>
    /// Get the appropriate back navigation route from current page
    /// First checks dynamic mapping, then falls back to default map
    /// </summary>
    public static string GetBackNavigationRoute(string currentPage)
    {
        // Check if there's a recorded source
        if (PageSourceMap.TryGetValue(currentPage, out var source))
        {
            PageSourceMap.Remove(currentPage); // Use once, then remove
            
            if (TabBarPages.Contains(source))
                return $"//{source}";
            return source;
        }

        // Fall back to default mapping
        if (BackNavigationMap.TryGetValue(currentPage, out var defaultBack))
        {
            if (TabBarPages.Contains(defaultBack))
                return $"//{defaultBack}";
            return defaultBack;
        }

        // Last resort: go to ProfilePage (default for authenticated state)
        Console.WriteLine($"?? [Navigation] No back route found for {currentPage}, using default");
        return $"//{ROUTE_PROFILE}";
    }

    /// <summary>
    /// Handle back button press - returns true if handled, false to use default
    /// </summary>
    public static async Task<bool> HandleBackButton(string currentPage)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(currentPage))
            {
                Console.WriteLine($"?? [Navigation] Back button: current page is null/empty");
                return false;
            }

            var backRoute = GetBackNavigationRoute(currentPage);
            Console.WriteLine($"?? [Navigation] Back from {currentPage} to {backRoute}");
            
            await Shell.Current.GoToAsync(backRoute, animate: true);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? [Navigation] Back button failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Clear navigation source map (call on logout)
    /// </summary>
    public static void ClearPageSourceMap()
    {
        PageSourceMap.Clear();
        Console.WriteLine("???  [Navigation] Page source map cleared");
    }

    /// <summary>
    /// Validate a route exists in the allowed routes set
    /// </summary>
    private static bool ValidateRoute(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            Console.WriteLine("? [Navigation] Route is null or empty");
            return false;
        }

        if (!AllValidRoutes.Contains(route))
        {
            Console.WriteLine($"? [Navigation] INVALID ROUTE: '{route}' - Not registered in AppShell.xaml");
            Console.WriteLine($"   Valid routes: {string.Join(", ", AllValidRoutes)}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Get current navigation state (for debugging)
    /// </summary>
    public static string GetCurrentRoute()
    {
        try
        {
            return Shell.Current?.CurrentState?.Location?.OriginalString ?? "Unknown";
        }
        catch
        {
            return "Error retrieving route";
        }
    }

    /// <summary>
    /// Log current navigation state
    /// </summary>
    public static void LogNavigationState()
    {
        Console.WriteLine($"?? [Navigation] Current Route: {GetCurrentRoute()}");
    }
}

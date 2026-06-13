using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using loukupm.View;

namespace loukupm.Services;


public static class NavigationService
{
   

    // Auth pages (hidden, outside TabBar)
    public const string ROUTE_MAIN_PAGE = "MainPage";
    public const string ROUTE_LOGIN = "LoginPage";
    public const string ROUTE_SIGNIN = "SinginPage";
    public const string ROUTE_OTP = "OTPSINGIN";
    public const string ROUTE_POLICY_PRIVACY_AUTH = "PolicyandPrivacyPageatAthun";
    public const string ROUTE_TermsAndConditions_Athun = "TermsAndConditionsAthun";
    // TabBar pages
    public const string ROUTE_SPLASH = "LoadingPage";
    public const string ROUTE_HOME = "HomePage";
    public const string ROUTE_SERVICES = "ServicesPage";
    public const string ROUTE_BOOKING = "BookingPage";
    public const string ROUTE_PROFILE = "ProfilePage";

    // Subpages (outside TabBar � push onto the stack)
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

    // ?????????????????????????????????????????????
    // SETS
    // ?????????????????????????????????????????????
    private static readonly HashSet<string> TerminalPages = new()
{
    ROUTE_LOGIN,ROUTE_SPLASH

};
    /// <summary>All four TabBar pages.</summary>
    private static readonly HashSet<string> TabBarPages = new()
    {
        ROUTE_HOME,
        ROUTE_SERVICES,
        ROUTE_BOOKING,
        ROUTE_PROFILE
    };

    /// <summary>Flyout menu pages (About Us, Privacy Policy, Terms and Conditions).</summary>
    private static readonly HashSet<string> FlyoutPages = new()
    {
        ROUTE_ABOUT_US,
        ROUTE_POLICY_PRIVACY,
        ROUTE_TERMS_CONDITIONS, ROUTE_SETTING,ROUTE_IMPRESSUM
    };
    private static readonly HashSet<string> AuthPages = new()
{
    ROUTE_SIGNIN,
    ROUTE_REST_PASSWORD,
    ROUTE_POLICY_PRIVACY_AUTH,
    ROUTE_TermsAndConditions_Athun,
    ROUTE_OTP
};

    private static readonly HashSet<string> AllValidRoutes = new()
    {
        ROUTE_MAIN_PAGE, ROUTE_SIGNIN, ROUTE_OTP,
        ROUTE_HOME, ROUTE_SERVICES, ROUTE_BOOKING, ROUTE_PROFILE,
        ROUTE_TERM_BOOKING, ROUTE_PAYMENT,
        ROUTE_POLICY_PRIVACY, ROUTE_REST_PASSWORD, ROUTE_TERMS_CONDITIONS,
        ROUTE_EDIT_USER, ROUTE_EDIT_PASSWORD, ROUTE_EDIT_PASSWORD_VERIFICATION, ROUTE_CHACKOUT,
        ROUTE_ABOUT_US, ROUTE_NOTIFICATION,ROUTE_IMPRESSUM, ROUTE_POLICY_PRIVACY_AUTH,ROUTE_TermsAndConditions_Athun,
    };

    // ?????????????????????????????????????????????
    // PUBLIC HELPERS
    // ?????????????????????????????????????????????

    /// <summary>Returns true if <paramref name="route"/> is one of the four TabBar pages.</summary>
    public static bool IsTabBarPage(string route) => TabBarPages.Contains(route);

    /// <summary>Returns true if <paramref name="route"/> is one of the Flyout menu pages.</summary>
    public static bool IsFlyoutPage(string route) => FlyoutPages.Contains(route);

   
    public static async Task NavigateToTabBarPage(string route)
    {
        if (!ValidateRoute(route) || !TabBarPages.Contains(route))
            return;

        try
        {
            var shell = Shell.Current ?? Application.Current?.MainPage as Shell;
            if (shell != null)
            {
                await shell.GoToAsync($"//{route}", animate: true);
            }
            else
            {
                Console.WriteLine($"[Navigation] Shell not available - cannot navigate to tab {route}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] Error navigating to tab {route}: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a page instance from a route name. Used for NavigationPage fallback.
    /// </summary>
    private static Page GetPageForRoute(string route)
    {
        return route switch
        {
            ROUTE_SIGNIN => new SinginPage(),
            ROUTE_OTP => new OTPSINGIN(),
            ROUTE_POLICY_PRIVACY_AUTH => new PolicyandPrivacyPageatAthun(),
            ROUTE_TermsAndConditions_Athun => new TermsAndConditionsAthun(),
            ROUTE_REST_PASSWORD => new RestPassword(),
            ROUTE_POLICY_PRIVACY => new PolicyandPrivacyPage(),
            ROUTE_TERMS_CONDITIONS => new TermsAndConditions(),
            ROUTE_EDIT_USER => new EditeUserPage(),
            ROUTE_EDIT_PASSWORD => new EditePasswordPage(),
            ROUTE_EDIT_PASSWORD_VERIFICATION => new EditPasswordVerification(),
            ROUTE_TERM_BOOKING => new TerminbuchenPage(),
            ROUTE_PAYMENT => new Paymentgetway(),
            ROUTE_CHACKOUT => new ChackoutPage(),
            ROUTE_ABOUT_US => new AboutUS(),
            ROUTE_NOTIFICATION => new NotifictionPage(),
            ROUTE_IMPRESSUM => new ImpressumPage(),
            ROUTE_SETTING => new SettingPage(),
            _ => throw new InvalidOperationException($"Unknown route: {route}")
        };
    }

    public static async Task NavigateToPage(string route)
    {
        if (!ValidateRoute(route) || TabBarPages.Contains(route))
            return;

        try
        {
            var shell = Shell.Current ?? Application.Current?.MainPage as Shell;
            if (shell != null)
            {
                await shell.GoToAsync(route, animate: true);
            }
            else
            {
                // Fallback: Use NavigationPage if Shell is not available (e.g., during auth flow)
                var navPage = Application.Current?.MainPage as NavigationPage;
                if (navPage != null)
                {
                    var page = GetPageForRoute(route);
                    if (page != null)
                    {
                        await navPage.PushAsync(page);
                        Console.WriteLine($"[Navigation] Successfully navigated to {route} using NavigationPage");
                    }
                }
                else
                {
                    Console.WriteLine($"[Navigation] Shell and NavigationPage not available - cannot navigate to page {route}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] Error navigating to {route}: {ex.Message}");
        }
    }
    public static async Task NavigateToPage(string route, object parameter)
    {
        if (!ValidateRoute(route))
            return;

        try
        {
            string json = System.Text.Json.JsonSerializer.Serialize(parameter);
            string encodedJson = Uri.EscapeDataString(json);
            string routeWithParam = $"{route}?data={encodedJson}";

            var shell = Shell.Current ?? Application.Current?.MainPage as Shell;
            if (shell != null)
            {
                await shell.GoToAsync(routeWithParam, animate: true);
            }
            else
            {
                // Fallback: Use NavigationPage if Shell is not available
                var navPage = Application.Current?.MainPage as NavigationPage;
                if (navPage != null)
                {
                    var page = GetPageForRoute(route);
                    if (page != null)
                    {
                        await navPage.PushAsync(page);
                        Console.WriteLine($"[Navigation] Successfully navigated to {route} with param using NavigationPage");
                    }
                }
                else
                {
                    Console.WriteLine($"[Navigation] Shell and NavigationPage not available - cannot navigate to page {route} with param");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] Error navigating to {route}: {ex.Message}");
        }
    }

    /// </summary>
    /// <param name="currentPage">The simple route name of the visible page (e.g. "ServicesPage").</param>
    /// <returns>true if the event was handled; false to let the OS handle it (exit).</returns>
    public static async Task<bool> HandleBackButton(string currentPage)
    {
        if (string.IsNullOrWhiteSpace(currentPage))
            return false;

        try
        {
            // Add null check for Shell.Current at the start
            var shell = Shell.Current ?? Application.Current?.MainPage as Shell;
            if (shell == null)
            {
                Console.WriteLine($"[Navigation] Shell context is null - cannot handle back button from {currentPage}");
                return false;
            }

            if (TerminalPages.Contains(currentPage))
            {
                
                bool shouldAllowExit = !BackButtonTracker.RegisterBackPress(currentPage);
                return !shouldAllowExit; 
            }

            // 📌 Auth pages
            if (AuthPages.Contains(currentPage))
            {
                var navPage = Application.Current?.MainPage as NavigationPage;

                if (navPage != null)
                {
                    // LoginPage موجودة تحتها بالـ Stack
                    if (navPage.Navigation.NavigationStack.Count > 1)
                    {
                        await navPage.PopAsync(true);
                        return true;
                    }
                }

                // fallback
                if (shell != null)
                {
                    await shell.GoToAsync($"//{ROUTE_LOGIN}", animate: true);
                    return true;
                }

                return true;
            }
            // 📌 Tab pages
            if (TabBarPages.Contains(currentPage))
            {
                if (currentPage == ROUTE_HOME)
                    return false;

                await shell.GoToAsync($"//{ROUTE_HOME}", animate: true);
                return true;
            }

            // 📌 Flyout pages
            if (FlyoutPages.Contains(currentPage))
            {
                await shell.GoToAsync($"//{ROUTE_HOME}", animate: true);
                return true;
            }

            // 📌 Sub pages
            // Sub pages: navigate up one level
            await shell.GoToAsync("..", animate: true);
            return true;

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] Back button error from {currentPage}: {ex.Message}");
            Console.WriteLine($"[Navigation] Stack trace: {ex.StackTrace}");
            return false;
        }
    }
    public static async Task NavigateToLoginAndClear()
    {
        try
        {
         
            if (Shell.Current != null)
                Shell.Current.FlyoutIsPresented = false;

           
            Application.Current!.MainPage = new AppShell();

         
            await Task.Delay(50);

           
            await Shell.Current.GoToAsync($"//{ROUTE_LOGIN}", animate: false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] Logout hard reset error: {ex.Message}");
        }
    }

    //public static async Task NavigateToLoginAndClear()
    //{
    //    try
    //    {
    //        await Shell.Current.GoToAsync("LoginPage", animate: false);
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"[Navigation] Logout navigation error: {ex.Message}");
    //        throw;
    //    }
    //}


    //public static async Task ForceNavigateToLogin()
    //{
    //    try
    //    {
    //        Shell.Current.FlyoutIsPresented = false;

    //        await Shell.Current.GoToAsync($"//{ROUTE_LOGIN}", animate: false);
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"[Navigation] Force login error: {ex.Message}");
    //    }
    //}


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

    public static string GetCurrentPageName()
    {
        var route = GetCurrentRoute();
        var segments = route.TrimStart('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.LastOrDefault() ?? string.Empty;
    }

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

 
    public static async Task NavigateUpOrToLogin()
    {
        try
        {
            var shell = Shell.Current ?? Application.Current?.MainPage as Shell;
            if (shell != null)
            {
                if (shell.Navigation.NavigationStack.Count > 1)
                {
                    await shell.GoToAsync("..", animate: true);
                    return;
                }

                await shell.GoToAsync($"//{ROUTE_LOGIN}", animate: false);
                return;
            }

            if (Application.Current?.MainPage is NavigationPage nav)
            {
                if (nav.Navigation.NavigationStack.Count > 1)
                {
                    await nav.PopAsync(true);
                    return;
                }

                await nav.PopToRootAsync(false);
                await nav.PushAsync(new LoginPage(), false);
                return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] NavigateUpOrToLogin error: {ex.Message}");
        }
    }


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
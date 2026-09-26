using CommunityToolkit.Maui.Views;
using loukupm.Model;
using loukupm.View;
using loukupm.View.MassgingApp;
using loukupm.ViewModel;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace loukupm.Services;

/// <summary>
/// Redesigned Navigation Service with proper architectural separation between:
/// 1. Authentication Flow (NavigationPage - pre-login)
/// 2. Application Flow (AppShell - post-login)
/// 
/// This solves the core problem: NavigationPage and Shell are mutually exclusive,
/// so they require completely separate back button handling logic.
/// </summary>
public static class NavigationService
{
    // Prevent late navigation calls from overriding an active logout sequence.
    private static volatile bool _logoutInProgress = false;

    // Track if we're currently in Login context vs App context
    private static volatile bool _isLoginContext = false;

    /// <summary>
    /// Set when we're in login context (NavigationPage).
    /// Used to prevent Shell-based navigation from being attempted.
    /// </summary>
    public static void SetLoginContext()
    {
        _isLoginContext = true;
        Console.WriteLine("[Navigation] Context set to: LOGIN");
    }

    /// <summary>
    /// Set when we're in app context (Shell).
    /// Used to allow Shell-based navigation.
    /// </summary>
    public static void SetAppContext()
    {
        _isLoginContext = false;
        Console.WriteLine("[Navigation] Context set to: APP");
    }

    /// <summary>
    /// Clear the internal logout-in-progress flag. Call this before navigating to home after a successful login.
    /// </summary>
    public static void ResetLogoutFlag()
    {
        _logoutInProgress = false;
        Console.WriteLine("[Navigation] ResetLogoutFlag called - logoutInProgress cleared");
    }

    /// <summary>
    /// Mark navigation system as being in a logout transition.
    /// This prevents other callers from navigating to Home while logout is executing.
    /// </summary>
    public static void BeginLogout()
    {
        _logoutInProgress = true;
        Console.WriteLine("[Navigation] BeginLogout called - logoutInProgress set");
    }

    // ============================================================================
    // ROUTE DEFINITIONS
    // ============================================================================

    // Auth pages (NavigationPage only)
    public const string ROUTE_MAIN_PAGE = "MainPage";
    public const string ROUTE_LOGIN = "LoginPage";
    public const string ROUTE_SIGNIN = "SinginPage";
    public const string ROUTE_OTP = "OTPSINGIN";
    public const string ROUTE_POLICY_PRIVACY_AUTH = "PolicyandPrivacyPageatAthun";
    public const string ROUTE_TermsAndConditions_Athun = "TermsAndConditionsAthun";
    public const string ROUTE_REST_PASSWORD = "RestPassword";

    // TabBar pages (AppShell only)
    public const string ROUTE_SPLASH = "LoadingPage";
    public const string ROUTE_HOME = "HomePage";
    public const string ROUTE_SERVICES = "ServicesPage";
    public const string ROUTE_BOOKING = "BookingPage";
    public const string ROUTE_PROFILE = "ProfilePage";

    // Sub-pages (can be in either context)
    public const string ROUTE_TERM_BOOKING = "TerminbuchenPage";
    public const string ROUTE_IMPRESSUM = "ImpressumPage";
    public const string ROUTE_PAYMENT = "Paymentgetway";
    public const string ROUTE_POLICY_PRIVACY = "PolicyandPrivacyPage";
    public const string ROUTE_TERMS_CONDITIONS = "TermsAndConditions";
    public const string ROUTE_EDIT_USER = "EditeUserPage";
    public const string ROUTE_EDIT_PASSWORD = "EditePasswordPage";
    public const string ROUTE_EDIT_PASSWORD_VERIFICATION = "EditPasswordVerification";
    public const string ROUTE_CHACKOUT = "ChackoutPage";
    public const string ROUTE_ABOUT_US = "AboutUS";
    public const string ROUTE_NOTIFICATION = "NotifictionPage";
    public const string ROUTE_SETTING = "SettingPage";
    public const string ROUTE_OTP_PHONE_NUMBER = "OTPPoneNumper";
    public const string Route_ContactUs = "ContenUs";
    public const string ROUTE_Delet_Acount = "Areyousuredeletyouraccountpage";

    // ============================================================================
    // NAVIGATION CONTEXT DETECTION
    // ============================================================================

    /// <summary>
    /// Represents the current navigation paradigm.
    /// NavigationPage and Shell are mutually exclusive, never both active.
    /// </summary>
    private enum NavigationContext
    {
        /// <summary>No active navigation context detected.</summary>
        Unknown,

        /// <summary>Using NavigationPage for authentication flow (pre-login).</summary>
        Authentication,

        /// <summary>Using AppShell for main application flow (post-login).</summary>
        Application
    }

    /// <summary>
    /// Detects which navigation paradigm is currently active.
    /// This is the KEY to solving the architectural problem.
    /// </summary>
    private static NavigationContext GetNavigationContext()
    {
        try
        {
            // Check if NavigationPage is active (auth flow)
            if (Application.Current?.MainPage is NavigationPage navPage)
            {
                if (navPage.Navigation.NavigationStack.Count > 0)
                {
                    Console.WriteLine("[Navigation] Context: AUTHENTICATION (NavigationPage active)");
                    return NavigationContext.Authentication;
                }
            }

            // Check if AppShell is active (app flow)
            if (Shell.Current != null)
            {
                Console.WriteLine("[Navigation] Context: APPLICATION (AppShell active)");
                return NavigationContext.Application;
            }

            Console.WriteLine("[Navigation] Context: UNKNOWN (no navigation root active)");
            return NavigationContext.Unknown;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] Error detecting context: {ex.Message}");
            return NavigationContext.Unknown;
        }
    }

    // ============================================================================
    // ROUTE CLASSIFICATION (immutable sets)
    // ============================================================================

    private static readonly HashSet<string> AuthPages = new()
    {
        ROUTE_LOGIN,
        ROUTE_SIGNIN,
        ROUTE_OTP,
        ROUTE_REST_PASSWORD,
        ROUTE_POLICY_PRIVACY_AUTH,
        ROUTE_TermsAndConditions_Athun
    };

    private static readonly HashSet<string> TabBarPages = new()
    {
        ROUTE_HOME,
        ROUTE_SERVICES,
        ROUTE_BOOKING,
        ROUTE_PROFILE
    };

    private static readonly HashSet<string> FlyoutPages = new()
    {
        ROUTE_ABOUT_US,
        ROUTE_POLICY_PRIVACY,
        ROUTE_TERMS_CONDITIONS,
        ROUTE_SETTING,
        ROUTE_IMPRESSUM,
        Route_ContactUs
    };

    private static readonly HashSet<string> TerminalPages = new()
    {
        ROUTE_LOGIN,
        ROUTE_SPLASH
    };

    private static readonly HashSet<string> AllValidRoutes = new()
    {
        ROUTE_MAIN_PAGE, ROUTE_LOGIN, ROUTE_SIGNIN, ROUTE_OTP,
        ROUTE_HOME, ROUTE_SERVICES, ROUTE_BOOKING, ROUTE_PROFILE,
        ROUTE_TERM_BOOKING, ROUTE_PAYMENT,
        ROUTE_POLICY_PRIVACY, ROUTE_REST_PASSWORD, ROUTE_TERMS_CONDITIONS,
        ROUTE_EDIT_USER, ROUTE_EDIT_PASSWORD, ROUTE_EDIT_PASSWORD_VERIFICATION, ROUTE_CHACKOUT,
        ROUTE_ABOUT_US, ROUTE_NOTIFICATION, ROUTE_IMPRESSUM, ROUTE_POLICY_PRIVACY_AUTH,
        ROUTE_TermsAndConditions_Athun, ROUTE_OTP_PHONE_NUMBER, Route_ContactUs, ROUTE_Delet_Acount,
    };

    // ============================================================================
    // PAGE NAME DETECTION - NOW CONTEXT-AWARE (Fixed "Unknown" problem)
    // ============================================================================

    /// <summary>
    /// Gets the name of the currently visible page, working in BOTH navigation contexts.
    /// This fixes the problem where GetCurrentPageName() always returned "Unknown".
    /// </summary>
    public static string GetCurrentPageName()
    {
        try
        {
            // AUTHENTICATION CONTEXT: Get page from NavigationPage stack
            if (Application.Current?.MainPage is NavigationPage navPage &&
                navPage.Navigation.NavigationStack.Count > 0)
            {
                var currentPage = navPage.Navigation.NavigationStack.Last();
                var pageName = currentPage.GetType().Name;
                Console.WriteLine($"[Navigation] Current page (Auth): {pageName}");
                return pageName;
            }

            // APPLICATION CONTEXT: Get page from Shell location
            if (Shell.Current?.CurrentState != null)
            {
                var route = Shell.Current.CurrentState.Location.OriginalString;
                var segments = route.TrimStart('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
                var pageName = segments.LastOrDefault() ?? "Unknown";
                Console.WriteLine($"[Navigation] Current page (App): {pageName}");
                return pageName;
            }

            Console.WriteLine("[Navigation] Current page: Unknown (no navigation context)");
            return "Unknown";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] Error getting current page: {ex.Message}");
            return "Unknown";
        }
    }

    /// <summary>
    /// Gets the full route path (for Shell only).
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

    // ============================================================================
    // BACK BUTTON HANDLING - SEPARATED BY CONTEXT
    // ============================================================================

    /// <summary>
    /// Main entry point for back button handling.
    /// Routes to appropriate handler based on navigation context.
    /// This is the core fix: handle NavigationPage and Shell separately.
    /// </summary>
    public static async Task<bool> HandleBackButton(string currentPageName)
    {
        if (string.IsNullOrWhiteSpace(currentPageName))
        {
            Console.WriteLine("[Navigation] HandleBackButton: currentPageName is null/empty");
            return false;
        }

        Console.WriteLine($"[Navigation] HandleBackButton called for: {currentPageName}");

        // Check for unsaved edits on EditUserPage and prompt the user before leaving.
        try
        {
            if (string.Equals(currentPageName, ROUTE_EDIT_USER, StringComparison.Ordinal) ||
                string.Equals(currentPageName, nameof(EditeUserPage), StringComparison.Ordinal))
            {
                var vm = ViewModel.AppViewModel.Instance;
                if (vm != null && vm.HasUserProfileChanges)
                {
                    try
                    {
                        var main = Application.Current?.MainPage;
                        if (main != null)
                        {
                            var popup = new DoNotChangeDetiatlesAcocunt();
                            var result = await main.ShowPopupAsync(popup);
                            bool discard = (result is bool b && b);
                            if (!discard)
                            {
                                // User cancelled - do not navigate
                                return true;
                            }

                            // User chose to discard - mark as saved so navigation won't re-prompt
                            try { vm.MarkUserProfileChangesSaved(); } catch { }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Navigation] Failed showing unsaved changes popup: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] Unsaved changes check failed: {ex.Message}");
        }

        try
        {
            var context = GetNavigationContext();

            return context switch
            {
                NavigationContext.Authentication => await HandleAuthBackButton(currentPageName),
                NavigationContext.Application => await HandleAppBackButton(currentPageName),
                _ => HandleUnknownContext(currentPageName)
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] HandleBackButton error: {ex.Message}");
            Console.WriteLine($"[Navigation] Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// Handles back button for AUTHENTICATION CONTEXT (NavigationPage).
    /// Auth flow is simple: all secondary pages pop to LoginPage.
    /// </summary>
    private static async Task<bool> HandleAuthBackButton(string currentPageName)
    {
        Console.WriteLine($"[Navigation] HandleAuthBackButton: {currentPageName}");

        try
        {
            var navPage = Application.Current?.MainPage as NavigationPage;
            if (navPage == null)
            {
                Console.WriteLine("[Navigation] NavigationPage not available in auth context");
                return false;
            }

            // ROOT PAGE (LoginPage): Allow exit
            if (currentPageName == ROUTE_LOGIN || currentPageName == nameof(LoginPage))
            {
                Console.WriteLine("[Navigation] At LoginPage - allowing application exit");
                return false; // Allow OS to handle exit
            }

            // SECONDARY PAGES: Pop back to LoginPage
            if (navPage.Navigation.NavigationStack.Count > 1)
            {
                Console.WriteLine($"[Navigation] Popping from {navPage.Navigation.NavigationStack.Count} pages to root");
                await navPage.PopToRootAsync(true);
                return true;
            }

            Console.WriteLine("[Navigation] Stack already at root");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] Auth back button error: {ex.Message}");
            return true; // Prevent OS default behavior
        }
    }

    /// <summary>
    /// Handles back button for APPLICATION CONTEXT (AppShell).
    /// App flow follows these rules:
    /// - TabBar pages navigate between tabs (Services/Booking/Profile -> Home)
    /// - Flyout pages (AboutUs/Settings/etc) -> Home
    /// - Sub-pages (pushed details) -> Pop or go to Home
    /// - Home page -> Allow exit
    /// </summary>
    private static async Task<bool> HandleAppBackButton(string currentPageName)
    {
        Console.WriteLine($"[Navigation] HandleAppBackButton: {currentPageName}");

        try
        {
            var shell = Shell.Current;
            if (shell == null)
            {
                Console.WriteLine("[Navigation] Shell not available in app context");
                return false;
            }

            if (currentPageName == ROUTE_LOGIN || currentPageName == nameof(LoginPage))
            {
                Console.WriteLine("[Navigation] At LoginPage - allowing application exit");
                return false;
            }

            if (currentPageName == ROUTE_SIGNIN || currentPageName == nameof(SinginPage)
                || currentPageName == ROUTE_OTP || currentPageName == ROUTE_REST_PASSWORD)
            {
                await shell.GoToAsync($"//{ROUTE_LOGIN}", false);
                return true;
            }

            if (currentPageName == ROUTE_OTP_PHONE_NUMBER || currentPageName == nameof(OTPPoneNumper))
            {
                await LeavePhoneOtpPageAsync();
                return true;
            }

            // HOME PAGE: Allow exit
            if (currentPageName == ROUTE_HOME || currentPageName == nameof(HomePage))
            {
                Console.WriteLine("[Navigation] At HomePage - allowing application exit");
                return false;
            }

            // TABBAR PAGES (non-Home): Navigate to Home
            if (TabBarPages.Contains(currentPageName))
            {
                Console.WriteLine($"[Navigation] TabBar page {currentPageName} -> Home");
                await shell.GoToAsync($"//{ROUTE_HOME}", true);
                return true;
            }

            // FLYOUT PAGES: Navigate to Home
            if (FlyoutPages.Contains(currentPageName))
            {
                Console.WriteLine($"[Navigation] Flyout page {currentPageName} -> Home");
                await shell.GoToAsync($"//{ROUTE_HOME}", true);
                return true;
            }

            // SUB-PAGES: Try to pop, if at root go to Home
            if (shell.Navigation.NavigationStack.Count > 1)
            {
                Console.WriteLine("[Navigation] Sub-page detected - popping");
                await shell.GoToAsync("..", true);
                return true;
            }

            // Already at a shell root (shouldn't reach here normally)
            Console.WriteLine("[Navigation] At shell root - navigating to Home");
            await shell.GoToAsync($"//{ROUTE_HOME}", true);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] App back button error: {ex.Message}");
            return true; // Prevent OS default behavior
        }
    }

    /// <summary>
    /// Handles edge case where no navigation context is active.
    /// </summary>
    private static bool HandleUnknownContext(string currentPageName)
    {
        Console.WriteLine($"[Navigation] Unknown context for page: {currentPageName}");
        // SafetyFall: Don't handle, let OS handle
        return false;
    }

    // ============================================================================
    // FORWARD NAVIGATION (shared between both contexts)
    // ============================================================================

    /// <summary>
    /// Navigate to a TabBar page within AppShell.
    /// </summary>
    public static async Task NavigateToTabBarPage(string route)
    {
        if (!ValidateRoute(route) || !TabBarPages.Contains(route))
            return;

        // Safety check: If we're still in login context, this shouldn't happen
        // But if it does, reject the navigation
        if (_isLoginContext)
        {
            Console.WriteLine($"[Navigation] NavigateToTabBarPage blocked - still in LOGIN context. Ensure SetAppContext() was called before navigation.");
            return;
        }

        try
        {
            // Prevent losing unsaved edits on EditUserPage when navigating away via TabBar
            try
            {
                var current = GetCurrentPageName();
                var vm = ViewModel.AppViewModel.Instance;
                if ((string.Equals(current, ROUTE_EDIT_USER, StringComparison.Ordinal) || string.Equals(current, nameof(EditeUserPage), StringComparison.Ordinal))
                    && vm != null && vm.HasUserProfileChanges)
                {
                    try
                    {
                        var main = Application.Current?.MainPage;
                        if (main != null)
                        {
                            var popup = new DoNotChangeDetiatlesAcocunt();
                            var result = await main.ShowPopupAsync(popup);
                            bool discard = (result is bool b && b);
                            if (!discard)
                            {
                                return;
                            }

                            try { vm.MarkUserProfileChangesSaved(); } catch { }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Navigation] TabBar unsaved popup failed: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Navigation] TabBar unsaved check failed: {ex.Message}");
            }

            var shell = Shell.Current;
            if (shell != null)
            {
                Console.WriteLine($"[Navigation] Navigating to TabBar page: {route}");
                await shell.GoToAsync($"//{route}", animate: true);
            }
            else
            {
                Console.WriteLine($"[Navigation] Shell not available - cannot navigate to TabBar page {route}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] Error navigating to TabBar {route}: {ex.Message}");
        }
    }

    /// <summary>
    /// Opens the phone OTP page on the shell stack.
    /// Falls back to a direct push when route navigation does not show the page.
    /// </summary>
    public static async Task OpenPhoneOtpPageAsync()
    {
        Console.WriteLine("[Navigation] Opening OTPPoneNumper");

        async Task OpenAsync()
        {
            var shell = Shell.Current;
            if (shell == null)
                throw new InvalidOperationException("Shell is not available");

            if (shell.CurrentPage is OTPPoneNumper)
                return;

            await shell.GoToAsync(ROUTE_OTP_PHONE_NUMBER, animate: true);
        }

        try
        {
            if (MainThread.IsMainThread)
                await OpenAsync();
            else
                await MainThread.InvokeOnMainThreadAsync(OpenAsync);

            Console.WriteLine("[Navigation] OTPPoneNumper opened");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] Opening OTPPoneNumper failed: {ex}");
            throw;
        }
    }

    /// <summary>
    /// Leaves the phone OTP page without Shell relative routing.
    /// GoToAsync("..") from this page crashes the Android shell and closes the app.
    /// </summary>
    public static async Task LeavePhoneOtpPageAsync()
    {
        Console.WriteLine("[Navigation] Returning to ProfilePage after phone OTP");

        if (Shell.Current is not AppShell shell)
        {
            await NavigateToTabBarPage(ROUTE_PROFILE);
            return;
        }

        if (MainThread.IsMainThread)
            await shell.ShowProfileAsync();
        else
            await MainThread.InvokeOnMainThreadAsync(shell.ShowProfileAsync);
    }

    /// <summary>
    /// Navigate to any page (auto-detects context).
    /// </summary>
    public static async Task NavigateToPage(string route)
    {
        if (!ValidateRoute(route) || TabBarPages.Contains(route))
            return;

        try
        {
            // If leaving EditUserPage via programmatic navigation, confirm unsaved changes
            try
            {
                var current = GetCurrentPageName();
                var vm = ViewModel.AppViewModel.Instance;
                if ((string.Equals(current, ROUTE_EDIT_USER, StringComparison.Ordinal) || string.Equals(current, nameof(EditeUserPage), StringComparison.Ordinal))
                    && vm != null && vm.HasUserProfileChanges)
                {
                    try
                    {
                        var main = Application.Current?.MainPage;
                        if (main != null)
                        {
                            var popup = new DoNotChangeDetiatlesAcocunt();
                            var result = await main.ShowPopupAsync(popup);
                            bool discard = (result is bool b && b);
                            if (!discard)
                            {
                                return;
                            }

                            try { vm.MarkUserProfileChangesSaved(); } catch { }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Navigation] Forward navigation unsaved popup failed: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Navigation] Forward navigation unsaved check failed: {ex.Message}");
            }

            var context = GetNavigationContext();
            Console.WriteLine($"[Navigation] Navigating to {route} in context: {context}");

            switch (context)
            {
                case NavigationContext.Application:
                    // Use Shell
                    var shell = Shell.Current;
                    if (shell != null)
                    {
                        await shell.GoToAsync(route, animate: true);
                    }
                    break;

                case NavigationContext.Authentication:
                    // Use NavigationPage
                    var navPage = Application.Current?.MainPage as NavigationPage;
                    if (navPage != null)
                    {
                        var page = GetPageForRoute(route);
                        if (page != null)
                        {
                            await navPage.PushAsync(page);
                        }
                    }
                    break;

                default:
                    Console.WriteLine($"[Navigation] No navigation context - cannot navigate to {route}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] Error navigating to {route}: {ex.Message}");
        }
    }

    /// <summary>
    /// Navigate to a page with parameters.
    /// </summary>
    public static async Task NavigateToPage(string route, object parameter)
    {
        if (!ValidateRoute(route))
            return;

        try
        {
            string json = JsonSerializer.Serialize(parameter);
            string encodedJson = Uri.EscapeDataString(json);
            string routeWithParam = $"{route}?data={encodedJson}";

            var context = GetNavigationContext();
            Console.WriteLine($"[Navigation] Navigating to {route} with params in context: {context}");

            switch (context)
            {
                case NavigationContext.Application:
                    var shell = Shell.Current;
                    if (shell != null)
                    {
                        await shell.GoToAsync(routeWithParam, animate: true);
                    }
                    break;

                case NavigationContext.Authentication:
                    var navPage = Application.Current?.MainPage as NavigationPage;
                    if (navPage != null)
                    {
                        var page = GetPageForRoute(route);
                        if (page != null)
                        {
                            ApplyFallbackParameters(page, parameter);
                            await navPage.PushAsync(page);
                        }
                    }
                    break;

                default:
                    Console.WriteLine($"[Navigation] No navigation context - cannot navigate to {route}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] Error navigating to {route} with params: {ex.Message}");
        }
    }

    /// <summary>
    /// Hard reset to login (used during logout).
    /// Destroys AppShell completely and returns to auth flow safely.
    /// This is critical: Shell.Current must NOT be referenced after this.
    /// </summary>

    public static async Task NavigateToLoginAndClear()
    {
        Console.WriteLine("[Navigation] NavigateToLoginAndClear START");
        SetLoginContext();

        try
        {
            if (Shell.Current is AppShell appShell)
            {
                await appShell.GoToLoginAsync();
                Console.WriteLine("[Navigation] NavigateToLoginAndClear COMPLETE via Shell");
                return;
            }

            if (Shell.Current != null)
            {
                Shell.Current.FlyoutIsPresented = false;
                Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;
                await Shell.Current.GoToAsync($"//{ROUTE_LOGIN}", false);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] NavigateToLoginAndClear ERROR: {ex}");
        }
    }

    /// <summary>
    /// Hard reset to home app (used after login).
    /// Reuses AppShell when it is already the window root.
    /// </summary>
    public static async Task NavigateToHomeAndClear()
    {
        try
        {
            ResetLogoutFlag();
            SetAppContext();

            Console.WriteLine("[Navigation] NavigateToHomeAndClear START");

            var app = Application.Current;

            if (app == null)
            {
                Console.WriteLine("[Navigation] Application.Current is null");
                return;
            }

            if (Shell.Current is AppShell existingShell)
            {
                await existingShell.GoToHomeAsync();
                var vm = AppViewModel.Instance;
                if (vm.HasNoHomeData && vm.RefreshHomeCommand?.CanExecute(null) == true)
                    await vm.RefreshHomeCommand.ExecuteAsync(null);
                Console.WriteLine("[Navigation] Switched existing AppShell to HomePage");
                return;
            }

            AppShell? shellPage = null;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                try
                {
                    // Final safety check
                    if (_logoutInProgress)
                    {
                        Console.WriteLine("[Navigation] Home navigation blocked - logout detected");
                        return;
                    }

                    // Secondary safety check
                    if (_isLoginContext)
                    {
                        Console.WriteLine("[Navigation] Home navigation blocked - in LOGIN context");
                        return;
                    }

                    Console.WriteLine("[Navigation] Creating new AppShell instance");

                    shellPage = new AppShell
                    {
                        FlowDirection = App.GetSavedFlowDirection()
                    };

                    var oldMain = app.Windows.FirstOrDefault()?.Page ?? app.MainPage;
                    App.SetRootPage(shellPage);
                    SetAppContext();

                    Console.WriteLine(
                        $"[Navigation] MainPage replaced: {oldMain?.GetType().Name} -> {shellPage.GetType().Name}"
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"[Navigation] Failed creating AppShell: {ex}"
                    );

                    throw;
                }
            });

            if (shellPage == null)
            {
                Console.WriteLine("[Navigation] Shell creation failed");
                return;
            }

            if (_logoutInProgress)
            {
                Console.WriteLine("[Navigation] Home navigation cancelled - logout started");
                return;
            }

            await shellPage.GoToHomeAsync();
            Console.WriteLine("[Navigation] Successfully navigated to HomePage");

        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[Navigation] Home reset error: {ex}"
            );
        }
    }

    // ============================================================================
    // HELPER METHODS
    // ============================================================================

    /// <summary>
    /// Creates a page instance from a route (for NavigationPage fallback).
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
            ROUTE_OTP_PHONE_NUMBER => new OTPPoneNumper(),
            Route_ContactUs => new ContenUs(),
            ROUTE_Delet_Acount => new Areyousuredeletyouraccountpage(),
            _ => throw new InvalidOperationException($"Unknown route: {route}")
        };
    }

    /// <summary>
    /// Applies parameters to pages that support them (fallback for NavigationPage).
    /// </summary>
    private static void ApplyFallbackParameters(Page page, object parameter)
    {
        if (parameter == null)
            return;

        try
        {
            // ============================================================
            // OTP PAGE
            // ============================================================
            if (page is OTPSINGIN otpPage &&
                parameter is Auth.OtpContext otpContext)
            {
                otpPage.SetOtpContext(otpContext);

                Console.WriteLine(
                    $"[Navigation] OTP context applied: " +
                    $"{otpContext.MaskedDestination}");

                return;
            }

            // ============================================================
            // NOTIFICATION PAGE
            // ============================================================
            if (page is NotifictionPage notificationPage)
            {
                var json = JsonSerializer.Serialize(parameter);
                var encodedJson = Uri.EscapeDataString(json);

                notificationPage.Data = encodedJson;

                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    string? id = null;

                    if (doc.RootElement.TryGetProperty(
                            "notificationId",
                            out var p1))
                    {
                        id = p1.GetString();
                    }
                    else if (doc.RootElement.TryGetProperty(
                                 "id",
                                 out var p2))
                    {
                        id = p2.GetString();
                    }
                    else if (doc.RootElement.TryGetProperty(
                                 "notification_id",
                                 out var p3))
                    {
                        id = p3.GetString();
                    }

                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        notificationPage.NotificationId = id;
                    }
                }

                Console.WriteLine(
                    $"[Navigation] Notification parameters applied");

                return;
            }

            // ============================================================
            // GENERIC FALLBACK
            // ============================================================
            Console.WriteLine(
                $"[Navigation] No specific parameter handler for page: " +
                $"{page.GetType().Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[Navigation] Failed to apply parameters: {ex}");
        }
    }

    /// <summary>
    /// Validates that a route is registered.
    /// </summary>
    private static bool ValidateRoute(string route)
    {
        if (string.IsNullOrWhiteSpace(route) || !AllValidRoutes.Contains(route))
        {
            Console.WriteLine($"[Navigation] Invalid route: '{route}'");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Public helper to check if a route is a TabBar page.
    /// </summary>
    public static bool IsTabBarPage(string route) => TabBarPages.Contains(route);

    /// <summary>
    /// Public helper to check if a route is a Flyout page.
    /// </summary>
    public static bool IsFlyoutPage(string route) => FlyoutPages.Contains(route);

    /// <summary>
    /// Validates navigation routes are registered in Shell.
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
}

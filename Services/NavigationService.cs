using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace loukupm.Services
{
    public class NavigationService
    {
        private static readonly string[] TerminalPages = { "LoginPage", "SplashPage" };
        private readonly BackButtonTracker _backTracker = new BackButtonTracker();

        // Safe resolver for Shell
        private Shell GetShell()
        {
            try
            {
                return Shell.Current ?? Application.Current?.MainPage as Shell;
            }
            catch
            {
                return Application.Current?.MainPage as Shell;
            }
        }

        // Centralized executor that supports both Shell and NavigationPage.
        private async Task ExecuteNavigation(Func<Shell, Task> shellAction, Func<NavigationPage, Task> navAction)
        {
            var shell = GetShell();
            if (shell != null)
            {
                if (shellAction != null)
                    await shellAction(shell).ConfigureAwait(false);
                return;
            }

            var nav = Application.Current?.MainPage as NavigationPage;
            if (nav != null)
            {
                if (navAction != null)
                    await navAction(nav).ConfigureAwait(false);
                return;
            }

            // Last resort: try to operate on MainPage if it's a Page
            throw new InvalidOperationException("No Shell or NavigationPage available for navigation.");
        }

        // Helper: find the current page name using Shell state or NavigationPage
        public string GetCurrentPageName()
        {
            var shell = GetShell();
            if (shell != null)
            {
                try
                {
                    var loc = shell.CurrentState?.Location?.OriginalString;
                    if (!string.IsNullOrEmpty(loc))
                    {
                        // Normalize and get last meaningful segment
                        var trimmed = loc.Trim('/');
                        if (trimmed.Contains('/'))
                            trimmed = trimmed.Split('/').Last();
                        // If route contains query params, strip them
                        var qIdx = trimmed.IndexOf('?');
                        if (qIdx >= 0) trimmed = trimmed.Substring(0, qIdx);
                        return trimmed;
                    }
                }
                catch
                {
                    // Fall through to navigationpage fallback
                }
            }

            try
            {
                var nav = Application.Current?.MainPage as NavigationPage;
                var page = nav?.CurrentPage ?? Application.Current?.MainPage as Page;
                return page?.GetType().Name;
            }
            catch
            {
                return null;
            }
        }

        // Public navigation methods
        public Task NavigateToTabBarPage(string route)
        {
            // route expected like "//HomePage" or "//SomeShellRoute". Use Shell when available.
            return ExecuteNavigation(
                async shell =>
                {
                    if (string.IsNullOrWhiteSpace(route)) return;
                    await shell.GoToAsync(route).ConfigureAwait(false);
                },
                async nav =>
                {
                    // Fallback: strip leading slashes and try to create page and replace stack
                    var name = NormalizeRouteToName(route);
                    var page = CreatePageForRoute(name);
                    if (page == null) return;
                    await nav.PopToRootAsync().ConfigureAwait(false);
                    await nav.PushAsync(page).ConfigureAwait(false);
                });
        }

        public Task NavigateToPage(string route)
        {
            return ExecuteNavigation(
                async shell =>
                {
                    if (string.IsNullOrWhiteSpace(route)) return;
                    await shell.GoToAsync(route).ConfigureAwait(false);
                },
                async nav =>
                {
                    var name = NormalizeRouteToName(route);
                    var page = CreatePageForRoute(name);
                    if (page == null) return;
                    await nav.PushAsync(page).ConfigureAwait(false);
                });
        }

        // Force navigate to login and clear stack correctly
        public Task ForceNavigateToLogin()
        {
            return ExecuteNavigation(
                async shell =>
                {
                    // Absolute route to login
                    await shell.GoToAsync("//LoginPage").ConfigureAwait(false);
                },
                async nav =>
                {
                    await nav.PopToRootAsync().ConfigureAwait(false);
                    var login = CreatePageForRoute("LoginPage");
                    if (login != null)
                        await nav.PushAsync(login).ConfigureAwait(false);
                });
        }

        // Back button handler: returns true if back was handled (do not exit app), false to allow default behavior (exit)
        public async Task<bool> HandleBackButton()
        {
            var shell = GetShell();
            var currentName = GetCurrentPageName();

            // Terminal pages block navigation and implement double-back-to-exit
            if (!string.IsNullOrEmpty(currentName) && TerminalPages.Contains(currentName))
            {
                if (_backTracker.ShouldExit())
                {
                    // allow default behavior (exit)
                    return false;
                }

                // handled: block the back and show prompt via tracker (platform-specific feedback can be added)
                _backTracker.NotifyBackAttempt();
                return true;
            }

            if (shell != null)
            {
                // TabBar pages and Flyout pages: normalize to go to Home
                try
                {
                    var loc = shell.CurrentState?.Location?.OriginalString ?? string.Empty;

                    // If location is not the root home, go home
                    if (!string.IsNullOrEmpty(loc) && !loc.Contains("//HomePage", StringComparison.OrdinalIgnoreCase))
                    {
                        // Always redirect to HomePage for TabBar and Flyout scenarios
                        await shell.GoToAsync("//HomePage").ConfigureAwait(false);
                        return true;
                    }
                    // If already at //HomePage, allow exit
                    return false;
                }
                catch
                {
                    // Fall back to shell back navigation
                    try
                    {
                        await shell.GoToAsync("..", true).ConfigureAwait(false);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            // NavigationPage fallback
            var nav = Application.Current?.MainPage as NavigationPage;
            if (nav != null)
            {
                var stack = nav.Navigation.NavigationStack;
                var current = nav.CurrentPage;
                var name = current?.GetType().Name;

                if (!string.IsNullOrEmpty(name) && TerminalPages.Contains(name))
                {
                    if (_backTracker.ShouldExit())
                        return false;
                    _backTracker.NotifyBackAttempt();
                    return true;
                }

                // If more pages in stack, pop one
                if (stack != null && stack.Count > 1)
                {
                    await nav.PopAsync().ConfigureAwait(false);
                    return true;
                }

                // If at root and not HomePage, navigate to HomePage
                var rootName = stack?.FirstOrDefault()?.GetType().Name;
                if (!string.Equals(rootName, "HomePage", StringComparison.OrdinalIgnoreCase))
                {
                    // Try to go to HomePage by clearing and pushing
                    await nav.PopToRootAsync().ConfigureAwait(false);
                    var home = CreatePageForRoute("HomePage");
                    if (home != null)
                        await nav.PushAsync(home).ConfigureAwait(false);
                    return true;
                }

                // Already at HomePage root — allow exit
                return false;
            }

            // No handler available, allow default
            return false;
        }

        // Helpers
        private static string NormalizeRouteToName(string route)
        {
            if (string.IsNullOrEmpty(route)) return route;
            var trimmed = route.Trim('/');
            if (trimmed.Contains('/')) trimmed = trimmed.Split('/').Last();
            var q = trimmed.IndexOf('?');
            if (q >= 0) trimmed = trimmed.Substring(0, q);
            return trimmed;
        }

        private static Page CreatePageForRoute(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            // Search loaded assemblies for a Page type with matching name
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = null;
                try
                {
                    t = asm.GetTypes().FirstOrDefault(x => x.IsClass && !x.IsAbstract && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && typeof(Page).IsAssignableFrom(x));
                }
                catch (ReflectionTypeLoadException)
                {
                    continue;
                }

                if (t != null)
                {
                    try
                    {
                        var page = Activator.CreateInstance(t) as Page;
                        return page;
                    }
                    catch
                    {
                        // ignore create failures
                    }
                }
            }

            return null;
        }
    }

    // Simple tracker for double-back-to-exit behavior
    internal class BackButtonTracker
    {
        private DateTime _lastBack;
        private readonly TimeSpan _threshold = TimeSpan.FromSeconds(2);

        // Call when a back attempt occurs and you want to notify user to press again
        public void NotifyBackAttempt()
        {
            _lastBack = DateTime.UtcNow;
            // Platform-specific prompt (Toast/snackbar) should be triggered by caller if desired
        }

        // Returns true if enough time has passed (second press) to allow exit
        public bool ShouldExit()
        {
            var now = DateTime.UtcNow;
            if (now - _lastBack <= _threshold)
            {
                // reset
                _lastBack = DateTime.MinValue;
                return true;
            }
            return false;
        }
    }
}

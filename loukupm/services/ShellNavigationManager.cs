using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using loukupm.View;
namespace loukupm.Services
{
    /// <summary>
    /// Manages Shell navigation with proper stack clearing.
    /// Replaces the deprecated Navigation.PopToRootAsync() functionality.
    /// </summary>
    public static class ShellNavigationManager
    {
        /// <summary>
        /// Navigate to a route while clearing all previous pages from the stack.
        /// This is equivalent to Flutter's pushAndRemoveUntil.
        /// </summary>
        /// <param name="route">The route to navigate to (e.g., "HomePage", "LoginPage")
        /// <param name="animate">Whether to animate the transition (default: false)</param>
        public static async Task ClearStackAndNavigate(string route)
        {
            try
            {
                Console.WriteLine($"🔄 [Navigation] Clearing stack and navigating to: {route}");

                // Verify Shell.Current is available
                var shell = Shell.Current ?? Application.Current?.MainPage as Shell;
                if (shell == null)
                {
                    Console.WriteLine($"❌ [Navigation] Shell context is null - cannot navigate {route}");
                    // Fallback: attempt to set MainPage directly when possible
                    if (Application.Current?.MainPage is NavigationPage nav && !route.StartsWith("//"))
                    {
                        await nav.PushAsync(new ContentPage());
                        return;
                    }
                    throw new InvalidOperationException("Shell context is not available");
                }

                // For Shell navigation, using absolute routes with // will replace the entire stack
                string absoluteRoute = route.StartsWith("//") ? route : $"//{route}";

                // Navigate with animation disabled for cleaner transition
                await shell.GoToAsync(absoluteRoute, animate: false);

                Console.WriteLine($"✅ [Navigation] Successfully navigated to: {route}");
            }
            catch (InvalidOperationException iex)
            {
                Console.WriteLine($"❌ [Navigation] Invalid operation navigating to {route}: {iex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [Navigation] Error navigating to {route}: {ex.Message}");
                Console.WriteLine($"   Type: {ex.GetType().Name}");
                throw;
            }
        }

        /// <summary>
        /// Navigate to LoginPage and clear the entire stack (for logout scenarios).
        /// </summary>
        /// <summary>
        /// Navigate to LoginPage and clear the entire stack (for logout scenarios).
        /// </summary>
        public static async Task NavigateToLoginAndClear()
        {
            try
            {
                Console.WriteLine($"🔄 [NavigationManager] Initiating hard logout sequence");

                // تحويل التحكم مباشرة إلى الخدمة المركزية التي تقوم بتدمير الـ Shell وبناء الـ NavigationPage الجديد
                await NavigationService.NavigateToLoginAndClear();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [NavigationManager] Error during logout navigation: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Navigate to the main TabBar (typically HomePage) and clear the entire stack (for login scenarios).
        /// </summary>
        public static async Task NavigateToHomeAndClear()
        {
            try
            {
                Console.WriteLine($"🔄 [NavigationManager] Delegating NavigateToHomeAndClear to NavigationService");
                await NavigationService.NavigateToHomeAndClear();
                Console.WriteLine($"✅ [NavigationManager] NavigationService completed NavigateToHomeAndClear");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [NavigationManager] Error during NavigateToHomeAndClear delegation: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get the current route information for debugging purposes.
        /// </summary>
        public static string GetCurrentRoute()
        {
            try
            {
                return Shell.Current.CurrentState?.Location?.OriginalString ?? "Unknown";
            }
            catch
            {
                return "Error retrieving current route";
            }
        }

        /// <summary>
        /// Log the current navigation state for debugging.
        /// </summary>
        public static void LogNavigationState()
        {
            Console.WriteLine($"?? [Navigation] Current Route: {GetCurrentRoute()}");
        }
    }
}
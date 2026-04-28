using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// <param name="route">The route to navigate to (e.g., "HomePage", "LoginPage")</param>
        /// <param name="animate">Whether to animate the transition (default: false)</param>
        public static async Task ClearStackAndNavigate(string route)
        {
            try
            {
                Console.WriteLine($"?? [Navigation] Clearing stack and navigating to: {route}");

                // For Shell navigation, using absolute routes with // will replace the entire stack
                string absoluteRoute = route.StartsWith("//") ? route : $"//{route}";

                // Navigate with animation disabled for cleaner transition
                await Shell.Current.GoToAsync(absoluteRoute, animate: false);

                Console.WriteLine($"? [Navigation] Successfully navigated to: {route}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? [Navigation] Error navigating to {route}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Navigate to LoginPage and clear the entire stack (for logout scenarios).
        /// </summary>
        public static async Task NavigateToLoginAndClear()
        {
            try
            {
                Console.WriteLine($"?? [Navigation] Logging out and clearing stack");

                // Use relative route for LoginPage (global route)
                await Shell.Current.GoToAsync("LoginPage", animate: false);

                Console.WriteLine($"? [Navigation] Successfully logged out to LoginPage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? [Navigation] Error during logout navigation: {ex.Message}");
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
                Console.WriteLine($"?? [Navigation] Logging in and navigating to home");

                // Use absolute route for TabBar pages
                await Shell.Current.GoToAsync("//HomePage", animate: false);

                Console.WriteLine($"? [Navigation] Successfully logged in to HomePage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? [Navigation] Error during login navigation: {ex.Message}");
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
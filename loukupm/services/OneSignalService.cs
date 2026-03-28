using OneSignalSDK.DotNet;
using System;
using System.Threading.Tasks;

namespace loukupm.Services
{
    public static class OneSignalService
    {
        private static readonly string _appId =
            "68c49ad8-113c-4160-91cc-5eb9d2c908d5";

        private static bool _initialized = false;

        public static async Task Init()
        {
            if (_initialized)
                return;

            try
            {
                if (string.IsNullOrWhiteSpace(_appId))
                {
                    Console.WriteLine("⚠️ OneSignal: AppId not configured!");
                    return;
                }

                OneSignal.Initialize(_appId);
                await OneSignal.Notifications.RequestPermissionAsync(true);

                // Setup notification handlers for tap/click events
                SetupNotificationHandlers();

                _initialized = true;
                Console.WriteLine("✅ OneSignal initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ OneSignal Init Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets up handlers for notification events.
        /// Supports foreground, background, and terminated app states.
        /// 
        /// NOTE: OneSignal SDK 5.2.2 has limited direct event handling.
        /// Platform-specific code in AppShell, MainActivity, and AppDelegate
        /// will call HandleNotificationTapped() when notifications are tapped.
        /// </summary>
        private static void SetupNotificationHandlers()
        {
            try
            {
                // OneSignal SDK 5.2.2 notification system is now ready
                // Notification taps will be detected by:
                // 1. AppShell.xaml.cs for foreground/background states
                // 2. MainActivity.cs (Android) for terminated state
                // 3. AppDelegate.cs (iOS) for terminated state
                //
                // Those platform handlers will call HandleNotificationTapped() 
                // when a notification is tapped.

                Console.WriteLine("✅ OneSignal notification system ready");
                Console.WriteLine("ℹ️  Platform-specific handlers will route notification taps to NotificationPage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error setting up notification handlers: {ex.Message}");
            }
        }

        /// <summary>
        /// Public method to navigate to the NotificationPage.
        /// Call this from AppShell.xaml.cs or platform-specific notification handlers
        /// when a notification is tapped.
        /// </summary>
        public static async Task HandleNotificationTapped()
        {
            try
            {
                await NavigateToNotificationPageAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error handling notification tap: {ex.Message}");
            }
        }

        /// <summary>
        /// Navigates to the NotificationPage using the project's NavigationService.
        /// </summary>
        private static async Task NavigateToNotificationPageAsync()
        {
            try
            {
                // Ensure main thread execution for UI operations
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await NavigationService.NavigateToPage(NavigationService.ROUTE_NOTIFICATION);
                    Console.WriteLine("📍 Navigated to NotificationPage");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error navigating to NotificationPage: {ex.Message}");
            }
        }

        /// <param name="userId">معرف المستخدم</param>
        public static void RegisterUser(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    Console.WriteLine("⚠️ Cannot register: UserId is null or empty");
                    return;
                }

                OneSignal.Login(userId);
                OneSignal.User.AddTag("user_no", userId);

                Console.WriteLine($"✅ User {userId} registered with OneSignal");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ RegisterUser Error: {ex.Message}");
            }
        }

        public static void Logout()
        {
            try
            {
                OneSignal.Logout();

                OneSignal.User.RemoveTag("user_id");
                OneSignal.User.RemoveTag("user_no");
                OneSignal.User.RemoveTag("email");
                OneSignal.User.RemoveTag("name");
                OneSignal.User.RemoveTag("signup_type");
                OneSignal.User.RemoveTag("login_type");
                OneSignal.User.RemoveTag("signup_date");
                OneSignal.User.RemoveTag("display_name");

                Console.WriteLine("✅ OneSignal logout completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ OneSignal logout failed: {ex.Message}");
            }
        }

        public static void AddTag(string key, string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key) ||
                    string.IsNullOrWhiteSpace(value))
                {
                    Console.WriteLine("⚠️ AddTag: key or value is null or empty");
                    return;
                }

                OneSignal.User.AddTag(key, value);
                Console.WriteLine($"✅ Tag added: {key} = {value}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ AddTag Error: {ex.Message}");
            }
        }

        public static void RemoveTag(string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    Console.WriteLine("⚠️ RemoveTag: key is null or empty");
                    return;
                }

                OneSignal.User.RemoveTag(key);
                Console.WriteLine($"✅ Tag removed: {key}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ RemoveTag Error: {ex.Message}");
            }
        }
    }
}

using OneSignalSDK.DotNet;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

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

                // Apply saved notification preference after initialization
                try
                {
                    bool isNotificationsEnabled = Preferences.Get("NotificationsEnabled", true);
                    if (isNotificationsEnabled)
                    {
                        OneSignal.User.PushSubscription.OptIn();
                        Console.WriteLine("✅ OneSignal: Notifications OptIn applied from saved preference");
                    }
                    else
                    {
                        OneSignal.User.PushSubscription.OptOut();
                        Console.WriteLine("🔕 OneSignal: Notifications OptOut applied from saved preference");
                    }
                }
                catch (Exception prefEx)
                {
                    Console.WriteLine($"⚠️ OneSignal: Error applying notification preference: {prefEx.Message}");
                }

                _initialized = true;
                Console.WriteLine("✅ OneSignal initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ OneSignal Init Error: {ex.Message}");
            }
        }

        public static async Task HandleNotificationTapped(string? notificationId = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(notificationId))
                {
                    await NavigateToNotificationPageAsync(notificationId);
                }
                else
                {
                    await NavigateToNotificationPageAsync(null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error handling notification tap: {ex.Message}");
            }
        }


        private static async Task NavigateToNotificationPageAsync(string? notificationId)
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(notificationId))
                        {
                            await NavigationService.NavigateToPage(NavigationService.ROUTE_NOTIFICATION, new { notificationId = notificationId });
                            Console.WriteLine($"📍 Navigated to NotificationPage with id={notificationId}");
                        }
                        else
                        {
                            await NavigationService.NavigateToPage(NavigationService.ROUTE_NOTIFICATION);
                            Console.WriteLine("📍 Navigated to NotificationPage");
                        }
                    }
                    catch (Exception navEx)
                    {
                        Console.WriteLine($"❌ Navigation error: {navEx.Message}");
                        Console.WriteLine($"   Stack: {navEx.StackTrace}");
                    }
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

        // Non-blocking logout. Runs OneSignal SDK calls on a background thread so the UI thread is not blocked.
        public static Task LogoutAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    // The SDK may perform network or heavy work internally; run it off the main thread.
                    OneSignal.Logout();

                    OneSignal.User.RemoveTag("user_id");
                    OneSignal.User.RemoveTag("user_no");
                    OneSignal.User.RemoveTag("email");
                    OneSignal.User.RemoveTag("name");
                    OneSignal.User.RemoveTag("signup_type");
                    OneSignal.User.RemoveTag("login_type");
                    OneSignal.User.RemoveTag("signup_date");
                    OneSignal.User.RemoveTag("display_name");

                    Console.WriteLine("✅ OneSignal logout completed (background)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ OneSignal logout failed: {ex.Message}");
                }
            });
        }

        // Backwards-compatible synchronous wrapper that triggers the async implementation without blocking.
        public static void Logout()
        {
            _ = LogoutAsync();
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

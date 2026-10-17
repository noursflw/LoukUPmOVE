using OneSignalSDK.DotNet;
using OneSignalSDK.DotNet.Core.User.Subscriptions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace loukupm.Services
{
    public static class OneSignalService
    {
        private static readonly string _appId =
            "68c49ad8-113c-4160-91cc-5eb9d2c908d5";

        private const string SubscriptionPreferenceKey = "onesignal_subscription_id";
        private const string RegisteredPairPreferenceKey = "onesignal_registered_pair";

        private static bool _initialized = false;
        private static int _session;
        private static int _watcherAttached;
        private static string? _activeUserId;

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

        /// <param name="userId">معرف المستخدم في السيرفر</param>
        public static void RegisterUser(string userId)
        {
            LinkUser(userId);
        }

        /// <summary>
        /// Links the backend user to this device in OneSignal, then stores the subscription id via /register-device.
        /// </summary>
        public static void LinkUser(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    Console.WriteLine("⚠️ Cannot register: UserId is null or empty");
                    return;
                }

                var session = Interlocked.Increment(ref _session);
                _activeUserId = userId;

                OneSignal.Login(userId);
                OneSignal.User.AddTag("user_no", userId);
                OneSignal.User.PushSubscription.OptIn();
                EnsureSubscriptionWatcher();

                _ = PublishDeviceAsync(session, userId);
                Console.WriteLine($"✅ User {userId} linked with OneSignal");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ RegisterUser Error: {ex.Message}");
            }
        }

        // Removes this device from the server while the access token is still valid, then logs out of OneSignal.
        public static async Task LogoutAsync(bool deregisterOnServer = true)
        {
            if (deregisterOnServer)
                await DeregisterCurrentDeviceAsync();

            await Task.Run(() =>
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

                    Console.WriteLine("✅ OneSignal logout completed (background)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ OneSignal logout failed: {ex.Message}");
                }
            });
        }

        private static void EnsureSubscriptionWatcher()
        {
            if (Interlocked.Exchange(ref _watcherAttached, 1) == 1)
                return;

            OneSignal.User.PushSubscription.Changed += (_, args) =>
            {
                var subscriptionId = args.State.Current.Id;
                var userId = _activeUserId;
                var session = Volatile.Read(ref _session);
                if (string.IsNullOrWhiteSpace(subscriptionId) || string.IsNullOrWhiteSpace(userId))
                    return;

                _ = PublishDeviceAsync(session, userId, subscriptionId);
            };
        }

        private static async Task PublishDeviceAsync(int session, string userId, string? knownSubscriptionId = null)
        {
            try
            {
                var subscriptionId = knownSubscriptionId;
                if (string.IsNullOrWhiteSpace(subscriptionId))
                    subscriptionId = await WaitForSubscriptionIdAsync(TimeSpan.FromSeconds(15));

                if (session != Volatile.Read(ref _session) ||
                    !string.Equals(_activeUserId, userId, StringComparison.Ordinal))
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(subscriptionId))
                {
                    Console.WriteLine("⚠️ OneSignal subscription id is not ready. Device was not sent to the server.");
                    return;
                }

                Preferences.Set(SubscriptionPreferenceKey, subscriptionId);

                var pair = userId + ":" + subscriptionId;
                if (Preferences.Get(RegisteredPairPreferenceKey, string.Empty) == pair)
                    return;

                var registered = await new ApiServices().RegisterDeviceAsync(subscriptionId);
                var stillCurrent = session == Volatile.Read(ref _session) &&
                    string.Equals(_activeUserId, userId, StringComparison.Ordinal);

                if (registered && !stillCurrent)
                {
                    await new ApiServices().DeregisterDeviceAsync(subscriptionId);
                    return;
                }

                if (!registered || !stillCurrent)
                    return;

                Preferences.Set(RegisteredPairPreferenceKey, pair);
                Console.WriteLine($"✅ Device registered on server for user {userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Device register failed: {ex.Message}");
            }
        }

        private static async Task<string?> WaitForSubscriptionIdAsync(TimeSpan timeout)
        {
            var existing = OneSignal.User.PushSubscription.Id;
            if (!string.IsNullOrWhiteSpace(existing))
                return existing;

            var ready = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
            EventHandler<PushSubscriptionChangedEventArgs>? handler = null;
            handler = (_, args) =>
            {
                var id = args.State.Current.Id;
                if (!string.IsNullOrWhiteSpace(id))
                    ready.TrySetResult(id);
            };

            OneSignal.User.PushSubscription.Changed += handler;
            try
            {
                var completed = await Task.WhenAny(ready.Task, Task.Delay(timeout));
                if (completed == ready.Task)
                    return await ready.Task;

                return OneSignal.User.PushSubscription.Id;
            }
            finally
            {
                OneSignal.User.PushSubscription.Changed -= handler;
            }
        }

        private static async Task DeregisterCurrentDeviceAsync()
        {
            Interlocked.Increment(ref _session);
            _activeUserId = null;

            var subscriptionId = Preferences.Get(SubscriptionPreferenceKey, string.Empty);
            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                try
                {
                    subscriptionId = OneSignal.User.PushSubscription.Id ?? string.Empty;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Could not read OneSignal subscription id: {ex.Message}");
                }
            }

            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                Console.WriteLine("⚠️ No OneSignal subscription id to deregister");
                return;
            }

            try
            {
                var removed = await new ApiServices().DeregisterDeviceAsync(subscriptionId);
                if (removed)
                {
                    Preferences.Remove(SubscriptionPreferenceKey);
                    Preferences.Remove(RegisteredPairPreferenceKey);
                    Console.WriteLine("✅ Device deregistered on server");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Device deregister failed: {ex.Message}");
            }
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

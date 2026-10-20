using Foundation;
using System.Text.Json;
using UserNotifications;
using UIKit;
using loukupm.Services;

namespace loukupm
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate, IUNUserNotificationCenterDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        // ─────────────────────────────────────────────────────────────
        // COLD START NOTIFICATION HANDLER
        // 
        // Called when app is launched from a terminated state by tapping
        // a remote notification. launchOptions contains the notification.
        // ─────────────────────────────────────────────────────────────
        public override bool FinishedLaunching(UIApplication application, 
                                               NSDictionary launchOptions)
        {
            try
            {
                if (launchOptions != null)
                {
                    // Check if app was opened via remote notification
                    if (launchOptions.ContainsKey(UIApplication.LaunchOptionsRemoteNotificationKey))
                    {
                        Console.WriteLine("🔔 [iOS] App launched from terminated state via notification");

                        var notification = launchOptions[UIApplication.LaunchOptionsRemoteNotificationKey] 
                                          as NSDictionary;

                        if (notification != null)
                        {
                            Console.WriteLine($"   Notification keys: {string.Join(", ", notification.Keys)}");

                            // ─────────────────────────────────────────────────────
                            // DEFERRED NAVIGATION
                            // Delay to allow MAUI app to fully initialize
                            // ─────────────────────────────────────────────────────
                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                try
                                {
                                    // Wait for app and Shell initialization
                                    await Task.Delay(1000);

                                    // Attempt to extract notificationId from payload and navigate with it
                                    string? nid = null;
                                    try
                                    {
                                        nid = ExtractNotificationId(notification);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"❌ [iOS] Failed extracting notificationId: {ex.Message}");
                                    }

                                    // DEFENSIVE: Check Shell is ready
                                    if (Shell.Current != null)
                                    {
                                        await OneSignalService.HandleNotificationTapped(nid);
                                        Console.WriteLine("✅ [iOS] Cold start notification navigation completed");
                                    }
                                    else
                                    {
                                        Console.WriteLine("⚠️ [iOS] Shell.Current null, cannot navigate from cold start");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"❌ [iOS] Error navigating from cold start: {ex.Message}");
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [iOS] Error in FinishedLaunching: {ex.Message}");
            }

            // Continue normal app initialization
            return base.FinishedLaunching(application, launchOptions);
        }

        // ─────────────────────────────────────────────────────────────
        // FOREGROUND/BACKGROUND NOTIFICATION TAP HANDLER
        // 
        // Called when user taps a notification while the app is running
        // (foreground or background state)
        // ─────────────────────────────────────────────────────────────
        [Export("userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:")]
        public void DidReceiveNotificationResponse(UNUserNotificationCenter center, 
                                                    UNNotificationResponse response, 
                                                    Action completionHandler)
        {
            try
            {
                Console.WriteLine("🔔 [iOS] Notification tapped while app is running");

                string? nid = null;
                if (response?.Notification?.Request?.Content != null)
                {
                    var userInfo = response.Notification.Request.Content.UserInfo;
                    if (userInfo != null)
                    {
                        var keyCount = userInfo.Keys.Length;
                        Console.WriteLine($"   Notification data: {keyCount} items");
                        try
                        {
                            nid = ExtractNotificationId(userInfo);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ [iOS] Failed extracting notificationId: {ex.Message}");
                        }
                    }
                }

                // ─────────────────────────────────────────────────────
                // ROUTE TO NOTIFICATION PAGE
                // ─────────────────────────────────────────────────────
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        // DEFENSIVE: Check Shell is ready
                        if (Shell.Current != null)
                        {
                            await OneSignalService.HandleNotificationTapped(nid);
                            Console.WriteLine("✅ [iOS] Notification navigation completed");
                        }
                        else
                        {
                            Console.WriteLine("⚠️ [iOS] Shell.Current null, cannot navigate");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ [iOS] Navigation error: {ex.Message}");
                    }
                });

                // Always call completion handler
                completionHandler();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [iOS] Error in DidReceiveNotificationResponse: {ex.Message}");
                completionHandler();
            }
        }

        private static string? ExtractNotificationId(NSDictionary payload)
        {
            if (payload == null || payload.Count == 0)
                return null;

            string? directId = GetValueIgnoreCase(payload, "notificationId", "notification_id", "id");
            if (!string.IsNullOrWhiteSpace(directId))
                return directId;

            var customJson = GetValueIgnoreCase(payload, "custom");
            var fromCustom = ExtractFromJson(customJson);
            if (!string.IsNullOrWhiteSpace(fromCustom))
                return fromCustom;

            var dataJson = GetValueIgnoreCase(payload, "data");
            var fromData = ExtractFromJson(dataJson);
            if (!string.IsNullOrWhiteSpace(fromData))
                return fromData;

            return null;
        }

        private static string? GetValueIgnoreCase(NSDictionary payload, params string[] keys)
        {
            foreach (var keyObject in payload.Keys)
            {
                var keyText = keyObject?.ToString();
                if (string.IsNullOrWhiteSpace(keyText))
                    continue;

                foreach (var wanted in keys)
                {
                    if (!string.Equals(keyText, wanted, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var valueObject = payload.ObjectForKey(keyObject);
                    return valueObject?.ToString();
                }
            }

            return null;
        }

        private static string? ExtractFromJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                    return null;

                if (TryGetString(root, "notificationId", out var id) ||
                    TryGetString(root, "notification_id", out id) ||
                    TryGetString(root, "id", out id))
                {
                    return id;
                }

                if (root.TryGetProperty("a", out var additional) && additional.ValueKind == JsonValueKind.Object)
                {
                    if (TryGetString(additional, "notificationId", out id) ||
                        TryGetString(additional, "notification_id", out id) ||
                        TryGetString(additional, "id", out id))
                    {
                        return id;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ [iOS] Failed parsing notification payload JSON: {ex.Message}");
            }

            return null;
        }

        private static bool TryGetString(JsonElement element, string propertyName, out string? value)
        {
            value = null;
            if (!element.TryGetProperty(propertyName, out var property))
                return false;

            value = property.ValueKind == JsonValueKind.String
                ? property.GetString()
                : property.ToString();

            return !string.IsNullOrWhiteSpace(value);
        }

        // ─────────────────────────────────────────────────────────────
        // FOREGROUND NOTIFICATION PRESENTATION
        // 
        // iOS by default does NOT show notifications while app is in
        // foreground. This method lets us customize the presentation.
        // ─────────────────────────────────────────────────────────────
        [Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
        public void WillPresentNotification(UNUserNotificationCenter center, 
                                            UNNotification notification, 
                                            Action<UNNotificationPresentationOptions> completionHandler)
        {
            try
            {
                Console.WriteLine("🔔 [iOS] Notification received while app is in foreground");

                // Show notification banner, sound, and badge while app is running
                var presentationOptions = UNNotificationPresentationOptions.Banner 
                                        | UNNotificationPresentationOptions.Sound 
                                        | UNNotificationPresentationOptions.Badge;

                completionHandler(presentationOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [iOS] Error in WillPresentNotification: {ex.Message}");
                completionHandler(UNNotificationPresentationOptions.None);
            }
        }
    }
}

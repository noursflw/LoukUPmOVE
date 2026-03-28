using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Activity;
using loukupm.Services;

namespace loukupm
{
    [Activity(
         Theme = "@style/Maui.SplashTheme",
         MainLauncher = true,
         LaunchMode = LaunchMode.SingleTop,
         ConfigurationChanges = ConfigChanges.ScreenSize
                              | ConfigChanges.Orientation
                              | ConfigChanges.UiMode
                              | ConfigChanges.ScreenLayout
                              | ConfigChanges.SmallestScreenSize
                              | ConfigChanges.Density,
         WindowSoftInputMode = SoftInput.AdjustPan)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // تحديد لون شريط الحالة
            Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#202020"));

            // ─────────────────────────────────────────────────────────
            // Register back-navigation handler using the modern Android API.
            //
            // OnBackPressed() is deprecated on Android 13+ (API 33).
            // OnBackPressedDispatcher is the correct approach and works on
            // all API levels. Shell.OnBackButtonPressed() is NOT reliable
            // in Release APK builds due to IL trimming — this is the fix.
            // ─────────────────────────────────────────────────────────
            OnBackPressedDispatcher.AddCallback(this, new AppBackPressedCallback(this));
        }

        // ─────────────────────────────────────────────────────────────
        // NOTIFICATION TAP HANDLER - Cold Start & Resume
        // 
        // Called when:
        // 1. App is running and notification is tapped (OnNewIntent fires)
        // 2. App is terminated and launched via notification tap
        // 
        // KEY: LaunchMode.SingleTop ensures this is called in both cases
        // ─────────────────────────────────────────────────────────────
        protected override void OnNewIntent(Android.Content.Intent intent)
        {
            base.OnNewIntent(intent);

            try
            {
                // DEFENSIVE: Check if intent has extras (notification payload)
                if (intent != null && intent.Extras != null)
                {
                    Console.WriteLine("🔔 [Android] Notification intent received");
                    Console.WriteLine($"   Action: {intent.Action}");
                    Console.WriteLine($"   Extras count: {intent.Extras.KeySet().Count}");

                    // ─────────────────────────────────────────────────────
                    // DEFERRED NAVIGATION
                    // Schedule on main thread to ensure Shell is ready
                    // ─────────────────────────────────────────────────────
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        try
                        {
                            // Small delay for Shell initialization
                            await Task.Delay(500);

                            // DEFENSIVE: Check Shell is ready before navigating
                            if (Shell.Current != null)
                            {
                                await OneSignalService.HandleNotificationTapped();
                                Console.WriteLine("✅ [Android] Notification navigation completed");
                            }
                            else
                            {
                                Console.WriteLine("⚠️ [Android] Shell.Current null, retrying after 1 second...");

                                // Retry after longer delay
                                await Task.Delay(1000);

                                if (Shell.Current != null)
                                {
                                    await OneSignalService.HandleNotificationTapped();
                                    Console.WriteLine("✅ [Android] Notification navigation completed (retry)");
                                }
                                else
                                {
                                    Console.WriteLine("❌ [Android] Shell still null, unable to navigate");
                                }
                            }
                        }
                        catch (Exception navEx)
                        {
                            Console.WriteLine($"❌ [Android] Navigation error: {navEx.Message}");
                        }
                    });
                }
                else
                {
                    Console.WriteLine("ℹ️ [Android] OnNewIntent called without notification extras");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [Android] Error in OnNewIntent: {ex.Message}");
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Back navigation callback — implements the two navigation rules:
        //
        //   Tab page (not Home) → navigate to //HomePage
        //   HomePage            → send app to background (MoveTaskToBack)
        //   Subpage             → pop one level (..)
        // ─────────────────────────────────────────────────────────────
        private sealed class AppBackPressedCallback : OnBackPressedCallback
        {
            private readonly MainActivity _activity;

            public AppBackPressedCallback(MainActivity activity) : base(enabled: true)
            {
                _activity = activity;
            }

            public override void HandleOnBackPressed()
            {
                var currentPage = NavigationService.GetCurrentPageName();

                if (NavigationService.IsTabBarPage(currentPage))
                {
                    if (currentPage == NavigationService.ROUTE_HOME)
                    {
                        // Standard Android behaviour: send the app to background
                        _activity.MoveTaskToBack(true);
                        return;
                    }

                    // Any other tab → go to Home
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await NavigationService.NavigateToTabBarPage(NavigationService.ROUTE_HOME);
                    });
                    return;
                }

                // Subpage → pop one level off the Shell stack
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await NavigationService.HandleBackButton(currentPage);
                });
            }
        }
    }
}
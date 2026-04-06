using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Activity;
using AndroidX.Core.View;
using loukupm.Services;

namespace loukupm
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ApplyStatusBarColor();
        }

        protected override void OnResume()
        {
            base.OnResume();
            ApplyStatusBarColor();
        }

        private void ApplyStatusBarColor()
        {
            // فرض لون شريط الحالة #202020
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#202020"));
            }

            // API 29+ - استخدم الطريقة الحديثة
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                WindowCompat.SetDecorFitsSystemWindows(Window, true);
            }
        }

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
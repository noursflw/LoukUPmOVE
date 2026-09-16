using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Activity;
using AndroidX.Core.View;
using loukupm.Services;
using Microsoft.Maui.Controls.PlatformConfiguration;

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

            if (Window != null)
            {
                // 1. تعيين اللون الأسود لشريط الحالة
                Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#000000"));

                // 2. استخدام المظهر الداكن لشريط الحالة (لتصبح الأيقونات بيضاء)
                // نستخدم WindowCompat مباشرة لإحضار المتحكم
                var insetsController = WindowCompat.GetInsetsController(Window, Window.DecorView);
                if (insetsController != null)
                {
                    // false تعني أيقونات بيضاء (تناسب الخلفية السوداء)
                    insetsController.AppearanceLightStatusBars = false;
                }

                // 3. إجبار التطبيق على احترام أبعاد شريط الحالة وعدم التمدد خلفه إجبارياً في أندرويد 15+
                WindowCompat.SetDecorFitsSystemWindows(Window, true);
            }

            OnBackPressedDispatcher.AddCallback(this, new AppBackPressedCallback(this));
        }




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
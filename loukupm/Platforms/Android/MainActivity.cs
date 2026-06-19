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
            Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#000000"));


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
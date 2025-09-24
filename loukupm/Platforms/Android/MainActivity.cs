using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

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

        }
    }
}
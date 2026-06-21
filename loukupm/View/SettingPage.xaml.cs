namespace loukupm.View;

using loukupm.Services;
using loukupm.Langue;
using System.Globalization;
using System.Windows.Input;
using System.ComponentModel;
using OneSignalSDK.DotNet;
using loukupm.ViewModel;

public partial class SettingPage : ContentPage, INotifyPropertyChanged
{
    private readonly SettingsViewModel _vm;

    public SettingPage()
    {
        InitializeComponent();

        var vm = new SettingsViewModel(new ApiServices());

        BindingContext = vm;
        _vm = vm;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// معالج زر العودة - يستخدم نظام الملاحة المركزي
    /// يتبع القاعدة: صفحات تدفق الملف الشخصي → //ProfilePage مباشرة
    /// </summary>
    protected override bool OnBackButtonPressed()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await NavigationService.HandleBackButton(NavigationService.ROUTE_HOME);
        });
        return true;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        await NavigationService.HandleBackButton(NavigationService.ROUTE_SETTING);
    }

    /// <summary>
    /// OnAppearing - Called when the page is about to appear.
    /// Loads the saved notification preference and sets the Switch state.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            bool isNotificationsEnabled = Preferences.Get("NotificationsEnabled", true);
            NotificationsSwitch.IsToggled = isNotificationsEnabled;

            await _vm.LoadDataCommand.ExecuteAsync(null);

            Console.WriteLine($"🔔 Loaded notification preference: {isNotificationsEnabled}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SettingPage] Error loading notification preference: {ex.Message}");
        }
    }

    /// <summary>
    /// OnNotificationsSwitchToggled - Called when the notifications switch is toggled.
    /// Saves the preference and applies the OptIn/OptOut state to OneSignal.
    /// </summary>
    private async void OnNotificationsSwitchToggled(object sender, ToggledEventArgs e)
    {
        try
        {
            bool isEnabled = e.Value;

            Preferences.Set("NotificationsEnabled", isEnabled);
            Console.WriteLine($"💾 Notification preference saved: {isEnabled}");

            if (isEnabled)
            {
                OneSignal.User.PushSubscription.OptIn();
                Console.WriteLine("✅ Notifications OptIn triggered");
            }
            else
            {
                OneSignal.User.PushSubscription.OptOut();
                Console.WriteLine("🔕 Notifications OptOut triggered");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SettingPage] Error toggling notifications: {ex.Message}");
            NotificationsSwitch.IsToggled = !e.Value;
        }
    }

}
using loukupm.ViewModel;
using loukupm.Services;

namespace loukupm.View;

public partial class EditePasswordPage : ContentPage
{
    public EditePasswordPage()
    {
        InitializeComponent();
        BindingContext = new AppViewModel();
    }

    /// <summary>
    /// معالج زر العودة - يستخدم نظام الملاحة المركزي
    /// يتبع القاعدة: صفحات تدفق الملف الشخصي → //ProfilePage مباشرة
    /// </summary>
    protected override bool OnBackButtonPressed()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await NavigationService.HandleBackButton(NavigationService.ROUTE_EDIT_PASSWORD);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EditePasswordPage] Back button error: {ex.Message}");
                    Console.WriteLine($"[EditePasswordPage] Exception: {ex.GetType().Name}");
                }
            });
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EditePasswordPage] OnBackButtonPressed crash: {ex.Message}");
            return true; // Prevent crash
        }
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            await NavigationService.HandleBackButton(NavigationService.ROUTE_EDIT_PASSWORD);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EditePasswordPage] Button click navigation error: {ex.Message}");
        }
    }
}
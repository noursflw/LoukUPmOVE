using loukupm.ViewModel;
using loukupm.Services;

namespace loukupm.View;

public partial class EditPasswordVerification : ContentPage
{
    public EditPasswordVerification()
    {
        InitializeComponent();
        BindingContext = new AppViewModel();
    }

    /// <summary>
    /// معالج زر العودة - يستخدم نظام الملاحة المركزي
    /// يتبع القاعدة: جميع الصفحات الأخرى → pop one level
    /// </summary>
    protected override bool OnBackButtonPressed()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await NavigationService.HandleBackButton(NavigationService.ROUTE_EDIT_PASSWORD_VERIFICATION);
        });
        return true;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        // العودة إلى الصفحة السابقة باستخدام الملاحة المركزية
        await NavigationService.HandleBackButton(NavigationService.ROUTE_EDIT_PASSWORD_VERIFICATION);
    }       
}

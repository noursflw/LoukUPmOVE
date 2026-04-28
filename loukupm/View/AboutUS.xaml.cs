using loukupm.Services;

namespace loukupm.View;

public partial class AboutUS : ContentPage
{
	public AboutUS()
	{
		InitializeComponent();
	}

	/// <summary>
	/// معالج زر العودة - يستخدم نظام التنقل الموحد
	/// يتبع القاعدة: جميع الصفحات الأخرى → pop one level
	/// </summary>
	protected override bool OnBackButtonPressed()
	{
		MainThread.BeginInvokeOnMainThread(async () =>
		{
			await NavigationService.HandleBackButton(NavigationService.ROUTE_ABOUT_US);
		});
		return true;
	}

	private async void Button_Clicked(object sender, EventArgs e)
	{
		await NavigationService.NavigateToTabBarPage(NavigationService.ROUTE_SERVICES);
	}
}
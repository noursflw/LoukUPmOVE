using loukupm.Services;

namespace loukupm.View;

public partial class ChackoutPage : ContentPage
{
	public ChackoutPage()
	{
		InitializeComponent();
		Shell.SetNavBarIsVisible(this, false);
	}

	/// <summary>
	/// معالج زر العودة - يستخدم نظام الملاحة المركزي
	/// يتبع القاعدة: جميع الصفحات الأخرى → pop one level
	/// </summary>
	protected override bool OnBackButtonPressed()
	{
		MainThread.BeginInvokeOnMainThread(async () =>
		{
			await NavigationService.HandleBackButton(NavigationService.ROUTE_CHACKOUT);
		});
		return true;
	}

	private async void Button_Clicked(object sender, EventArgs e)
	{
		await ShellNavigationManager.NavigateToLoginAndClear();	
	}
}
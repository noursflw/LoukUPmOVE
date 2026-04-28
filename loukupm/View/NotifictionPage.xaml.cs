using loukupm.ViewModel;
using loukupm.Services;

namespace loukupm.View;

public partial class NotifictionPage : ContentPage
{
	private NotificationViewModel _viewModel;

	public NotifictionPage()
	{
		InitializeComponent();
		_viewModel = new NotificationViewModel();
		this.BindingContext = _viewModel;
	}

	/// <summary>
	/// معالج زر العودة - يستخدم نظام الملاحة المركزي
	/// يتبع القاعدة: جميع الصفحات الأخرى → pop one level
	/// </summary>
	protected override bool OnBackButtonPressed()
	{
		MainThread.BeginInvokeOnMainThread(async () =>
		{
			await NavigationService.HandleBackButton(NavigationService.ROUTE_NOTIFICATION);
		});
		return true;
	}

	private async void Button_Clicked(object sender, EventArgs e)
	{
		await NavigationService.HandleBackButton(NavigationService.ROUTE_NOTIFICATION);
	}
}
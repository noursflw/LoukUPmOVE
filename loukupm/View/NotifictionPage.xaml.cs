using loukupm.ViewModel;

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

	private async void Button_Clicked(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}
}
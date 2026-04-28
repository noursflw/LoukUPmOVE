using loukupm.Model;
using loukupm.Services;
using loukupm.ViewModel;
using System.Threading.Tasks;

namespace loukupm.View;

public partial class BookingPage : ContentPage
{
	public BookingPage()
	{
		InitializeComponent();
		this.BindingContext = AppViewModel.Instance;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		
		var viewModel = BindingContext as AppViewModel;
		if (viewModel?.LoadAppointmentsCommand.IsRunning == false)
		{
			await viewModel.LoadAppointmentsCommand.ExecuteAsync(null);
		}
	}

	private void Button_Clicked(object sender, EventArgs e)
	{

	}

	private async void Button_Clicked_1(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}
	protected override bool OnBackButtonPressed()
	{
		// TabBar page: Delegate to centralized back button logic
		MainThread.BeginInvokeOnMainThread(async () =>
		{
			await NavigationService.HandleBackButton(NavigationService.ROUTE_BOOKING);
		});
		return true;
	}

	private async void Button_Clicked_2(object sender, EventArgs e)
	{
		await NavigationService.NavigateToTabBarPage(NavigationService.ROUTE_SERVICES);
	}
}
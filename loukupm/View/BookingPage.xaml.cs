using loukupm.Model;
using loukupm.Services;
using loukupm.ViewModel;
using Microsoft.Maui.Graphics;
using System.Threading.Tasks;

namespace loukupm.View;

public partial class BookingPage : ContentPage
{
	private int currentTabIndex = 0;

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

	// Tab Navigation Handlers
	private void OnTab1Clicked(object sender, EventArgs e)
	{
		SelectTab(0);
	}

	private void OnTab2Clicked(object sender, EventArgs e)
	{
		SelectTab(1);
	}

	private void OnTab3Clicked(object sender, EventArgs e)
	{
		SelectTab(2);
	}

	private void SelectTab(int tabIndex)
	{
		currentTabIndex = tabIndex;

		// Update Tab Headers
		var tab1Button = this.FindByName<Button>("Tab1Button");
		var tab2Button = this.FindByName<Button>("Tab2Button");
		var tab3Button = this.FindByName<Button>("Tab3Button");

		var tab1Content = this.FindByName<ScrollView>("Tab1Content");
		var tab2Content = this.FindByName<ScrollView>("Tab2Content");
		var tab3Content = this.FindByName<ScrollView>("Tab3Content");

		// Reset all tabs
		if (tab1Button != null) tab1Button.TextColor = new Color(153, 153, 153);
		if (tab2Button != null) tab2Button.TextColor = new Color(153, 153, 153);
		if (tab3Button != null) tab3Button.TextColor = new Color(153, 153, 153);

		if (tab1Content != null) tab1Content.IsVisible = false;
		if (tab2Content != null) tab2Content.IsVisible = false;
		if (tab3Content != null) tab3Content.IsVisible = false;

		// Highlight selected tab
		var b1 = this.FindByName<BoxView>("b1");
		var b2 = this.FindByName<BoxView>("b2");
		var b3 = this.FindByName<BoxView>("b3");

		if (b1 != null) b1.BackgroundColor = Colors.Transparent;
		if (b2 != null) b2.BackgroundColor = Colors.Transparent;
		if (b3 != null) b3.BackgroundColor = Colors.Transparent;

		if (tabIndex == 0)
		{
			if (tab1Button != null) tab1Button.TextColor = new Color(255, 215, 0);
			if (tab1Content != null) tab1Content.IsVisible = true;
			if (b1 != null) b1.BackgroundColor = new Color(255, 215, 0);
		}
		else if (tabIndex == 1)
		{
			if (tab2Button != null) tab2Button.TextColor = new Color(255, 215, 0);
			if (tab2Content != null) tab2Content.IsVisible = true;
			if (b2 != null) b2.BackgroundColor = new Color(255, 215, 0);
		}
		else if (tabIndex == 2)
		{
			if (tab3Button != null) tab3Button.TextColor = new Color(255, 215, 0);
			if (tab3Content != null) tab3Content.IsVisible = true;
			if (b3 != null) b3.BackgroundColor = new Color(255, 215, 0);
        }
	}
}
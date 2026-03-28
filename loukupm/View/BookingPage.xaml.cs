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

    private void Button_Clicked(object sender, EventArgs e)
    {

    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("//HomePage");
         return true;

    }

    private async void Button_Clicked_2(object sender, EventArgs e)
    {
        await NavigationService.NavigateToTabBarPage(NavigationService.ROUTE_SERVICES);
    }
}
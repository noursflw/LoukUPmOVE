using CommunityToolkit.Maui.Views;
using loukupm.Services;
using loukupm.ViewModel;

namespace loukupm.View.MassgingApp;

public partial class CanyouAcpetPhonenumber : Popup
{
	public CanyouAcpetPhonenumber()
	{
		InitializeComponent();
        this.BindingContext= AppViewModel.Instance;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await NavigationService.NavigateToTabBarPage(
            NavigationService.ROUTE_PROFILE
        );
        this.Close();
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        // Clear selection UI + booking state before leaving so ServicesPage
        // does not keep cards visually selected ("Not right now").
        AppViewModel.Instance.ClearBookingData();
        await NavigationService.NavigateToTabBarPage(
            NavigationService.ROUTE_HOME
        );
        this.Close();
    }

}
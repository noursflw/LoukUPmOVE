using CommunityToolkit.Maui.Views;
using loukupm.Services;
namespace loukupm.View.MassgingApp;

public partial class SuccessfullyBookingNow : Popup
{
	public SuccessfullyBookingNow()
	{
		InitializeComponent();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await NavigationService.NavigateToTabBarPage(
           NavigationService.ROUTE_BOOKING
       );
        this.Close();
    }
}
using CommunityToolkit.Maui.Views;
using loukupm.Services;
using loukupm.ViewModel;
namespace loukupm.View.MassgingApp;

public partial class Itsphonenumber : Popup
{
	public Itsphonenumber()
	{
		InitializeComponent();
        this.BindingContext = AppViewModel.Instance;
    }

   
    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_EDIT_USER);
        this.Close();
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        this.Close();
    }
}
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using loukupm.Model;
using loukupm.ViewModel;

namespace loukupm.View;

public partial class ServicesPage : ContentPage
{
	public ServicesPage()
	{
		InitializeComponent();
        this.BindingContext = AppViewModel.Instance;


    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//HomePage");
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        var service = (sender as Button)?.BindingContext as Servies;
        if (service == null) return;

        var vm = AppViewModel.Instance;
       
        vm.CurrentBooking.ServiceName = service.NameServies;
        vm.CurrentBooking.ServiceType = service.Catgery;
        await Navigation.PushAsync(new TerminbuchenPage());


    }

    private DateTime _lastBackPressed;

    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("//HomePage");
        return true;
    }  
}
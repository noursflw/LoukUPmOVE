namespace loukupm.View;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using FFImageLoading.Maui;
using loukupm.Model;
using loukupm.ViewModel;
using loukupm.Services;
using System.Globalization;
using System.Windows.Input;
using Microsoft.Maui.Controls;

public partial class HomePage : ContentPage
{
	

	public HomePage()
	{
		InitializeComponent();
		this.BindingContext= new AppViewModel();

		
		this.InitializeLanguageTracking();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		// Explicitly trigger AboutUs data loading when page appears
		if (BindingContext is AppViewModel appVM && appVM.AboutUsVM?.LoadAboutUsDataCommand.CanExecute(null) == true)
		{
			appVM.AboutUsVM.LoadAboutUsDataCommand.Execute(null);
		}

		// Trigger Home Sliders data loading when page appears
		if (BindingContext is AppViewModel appVm && appVm.HomeSliderVM?.LoadSlidersCommand.CanExecute(null) == true)
		{
			appVm.HomeSliderVM.LoadSlidersCommand.Execute(null);
		}
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
	
		GC.SuppressFinalize(this);
	}

	

	private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        await NavigationService.NavigateToTabBarPage(NavigationService.ROUTE_SERVICES);   
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_NOTIFICATION);
    }

    private void OnFlyoutMenuClicked(object sender, EventArgs e)
    {
        Shell.Current.FlyoutIsPresented = !Shell.Current.FlyoutIsPresented;
    }

   

    
    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        var service = (sender as Button)?.BindingContext as Servies;
        if (service == null) return;
        var vm = BindingContext as AppViewModel;
        if (vm?.SelectServiceButtonCommand is ICommand command && command.CanExecute(service))
        {
            command.Execute(service);
            await NavigationService.NavigateToPage(NavigationService.ROUTE_TERM_BOOKING);
        }
    }

    private DateTime _lastBackPressed = DateTime.MinValue;
    protected override bool OnBackButtonPressed()
    {
        var currentTime = DateTime.Now;

        if ((currentTime - _lastBackPressed).TotalSeconds <= 2)
        {
#if ANDROID
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#endif
            return true;
        }
 
        _lastBackPressed = currentTime;
        ShowToast("ÇÖÛØ ãÑÉ ÃÎÑì ááÎÑæÌ");
        return true; 
    }

    private async void ShowToast(string message)
    {
        var toast = Toast.Make(message, ToastDuration.Short);
        await toast.Show();
    }

 

    

    private void Button_Clicked_2(object sender, EventArgs e)
    {

    }
}

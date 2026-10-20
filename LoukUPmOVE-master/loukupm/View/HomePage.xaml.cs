namespace loukupm.View;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using FFImageLoading.Maui;
using loukupm.Model;
using loukupm.services;
using loukupm.Services;
using loukupm.ViewModel;
using Microsoft.Maui.Controls;
using System.Globalization;
using System.Windows.Input;

using System.ComponentModel;

public partial class HomePage : ContentPage
{
	public HomePage()
	{
		InitializeComponent();
		this.InitializeLanguageTracking();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		try
		{
			// ? ?????? Singleton Instance ?????? - ???? ????? ??? ?? ???????
			var vm = AppViewModel.Instance;

			Console.WriteLine($"?? [HomePage] OnAppearing - Using Singleton Instance: {vm.GetHashCode()}");

			// ??? ???? BindingContext ??? ???? ??????
			if (BindingContext == vm)
			{
				System.Diagnostics.Debug.WriteLine($"?? HomePage.OnAppearing - BindingContext already set to Instance: {vm.GetHashCode()}");
			}
			else
			{
				// ????? ???????? ??? ?????
				await vm.InitializeNotificationsAsync();
				this.BindingContext = vm;
				System.Diagnostics.Debug.WriteLine($"? HomePage.OnAppearing - set BindingContext to Singleton Instance: {vm.GetHashCode()} with NotificationCount={vm.NotificationCount}");
			}

			// ????? ???????? ????????
			if (vm.AboutUsVM?.LoadAboutUsDataCommand.CanExecute(null) == true)
			{
				vm.AboutUsVM.LoadAboutUsDataCommand.Execute(null);
			}

			if (vm.HomeSliderVM?.LoadSlidersCommand.CanExecute(null) == true)
			{
				vm.HomeSliderVM.LoadSlidersCommand.Execute(null);
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"? HomePage.OnAppearing error: {ex.Message}");
			Console.WriteLine($"? [HomePage] Error: {ex.StackTrace}");
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
        _ = vm.LoadNotificationCountAsync();
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

    private readonly NotificationService _notificationService = new();

   
    


    private void Button_Clicked_2(object sender, EventArgs e)
    {

    }
}

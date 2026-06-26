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
			// Resolve a ViewModel instance from DI if available; fallback to parameterless construction
			var mauiContext = Handler?.MauiContext ?? Application.Current?.Handler?.MauiContext;
			var vm = mauiContext?.Services.GetService(typeof(ViewModel.AppViewModel)) as ViewModel.AppViewModel;
			if (vm == null)
			{
				vm = new ViewModel.AppViewModel();
			}

			// If BindingContext is already set to same instance, no-op
			if (BindingContext == vm)
			{
				System.Diagnostics.Debug.WriteLine($"HomePage.OnAppearing - existing BindingContext hash: {BindingContext?.GetHashCode()}");
			}
			else
			{
				// Ensure notifications are loaded before setting BindingContext so UI shows correct values immediately
				await vm.InitializeNotificationsAsync();
				this.BindingContext = vm;
				System.Diagnostics.Debug.WriteLine($"HomePage.OnAppearing - set BindingContext to VM hash: {vm.GetHashCode()} with NotificationCount={vm.NotificationCount}");
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"HomePage.OnAppearing error resolving VM: {ex.Message}");
		}

		// Explicitly trigger AboutUs and HomeSlider data loading after BindingContext established
		if (BindingContext is ViewModel.AppViewModel appVM && appVM.AboutUsVM?.LoadAboutUsDataCommand.CanExecute(null) == true)
		{
			appVM.AboutUsVM.LoadAboutUsDataCommand.Execute(null);
		}

		if (BindingContext is ViewModel.AppViewModel appVm && appVm.HomeSliderVM?.LoadSlidersCommand.CanExecute(null) == true)
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

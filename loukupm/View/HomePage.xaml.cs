namespace loukupm.View;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using FFImageLoading.Maui;
using loukupm.Model;
using loukupm.services;
using loukupm.Services;
using loukupm.Langue;
using loukupm.ViewModel;
using Microsoft.Maui.Controls;
using System.Globalization;
using System.Windows.Input;

using System.ComponentModel;

public partial class HomePage : ContentPage
{
	private IDispatcherTimer? _sliderTimer;
	private HomeSliderViewModel? _hookedSlider;
	private bool _sliderAdvanceInProgress;

	public HomePage()
	{
		InitializeComponent();
		this.InitializeLanguageTracking();
		LocalizationResourcesManager.Instanse.LanguageChanged += OnHomeLanguageChanged;
		Unloaded += (_, _) =>
		{
			LocalizationResourcesManager.Instanse.LanguageChanged -= OnHomeLanguageChanged;
			StopSliderTimer();
			if (_hookedSlider != null)
				_hookedSlider.PropertyChanged -= OnSliderVmChanged;
		};
		LockSliderDirection();
	}

	private void OnHomeLanguageChanged(CultureInfo culture)
	{
		MainThread.BeginInvokeOnMainThread(async () =>
		{
			StopSliderTimer();
			try
			{
				await Task.Delay(150);
				LockSliderDirection();
				StartSliderTimer();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[HomeSlider] language lock skipped: {ex.Message}");
			}
		});
	}

	private void LockSliderDirection()
	{
		try
		{
			if (HomeSlider != null)
				HomeSlider.FlowDirection = FlowDirection.LeftToRight;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[HomeSlider] direction lock skipped: {ex.Message}");
		}
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

			if (vm.HasNoHomeData && vm.RefreshHomeCommand?.CanExecute(null) == true)
			{
				_ = vm.RefreshHomeCommand.ExecuteAsync(null);
			}

			HookSlider(vm);
			LockSliderDirection();
			StartSliderTimer();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"? HomePage.OnAppearing error: {ex.Message}");
			Console.WriteLine($"? [HomePage] Error: {ex.StackTrace}");
		}
	}

	protected override void OnDisappearing()
	{
		StopSliderTimer();
		base.OnDisappearing();
	
		GC.SuppressFinalize(this);
	}

	private void HookSlider(AppViewModel vm)
	{
		var slider = vm?.HomeSliderVM;
		if (slider == null || ReferenceEquals(_hookedSlider, slider))
			return;

		if (_hookedSlider != null)
			_hookedSlider.PropertyChanged -= OnSliderVmChanged;

		_hookedSlider = slider;
		_hookedSlider.PropertyChanged += OnSliderVmChanged;
	}

	private void OnSliderVmChanged(object sender, PropertyChangedEventArgs e)
	{
		if (sender is not HomeSliderViewModel slider)
			return;

		if (e.PropertyName == nameof(HomeSliderViewModel.IsLoading))
		{
			if (slider.IsLoading)
				StopSliderTimer();
			else
				MainThread.BeginInvokeOnMainThread(() =>
				{
					SafeResetSliderPosition(slider);
					StartSliderTimer();
				});
		}
		else if (e.PropertyName == nameof(HomeSliderViewModel.Items))
		{
			MainThread.BeginInvokeOnMainThread(() => SafeResetSliderPosition(slider));
		}
	}

	private void SafeResetSliderPosition(HomeSliderViewModel slider)
	{
		try
		{
			if (HomeSlider == null)
				return;

			var count = slider.Items?.Count ?? 0;
			if (count == 0)
			{
				slider.SetCurrentIndex(0);
				return;
			}

			if (HomeSlider.Position != 0)
				HomeSlider.Position = 0;

			slider.SetCurrentIndex(0);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[HomeSlider] reset skipped: {ex.Message}");
		}
	}

	private void OnHomeSliderPositionChanged(object sender, PositionChangedEventArgs e)
	{
		if (_sliderAdvanceInProgress)
			return;

		if (BindingContext is AppViewModel vm)
			vm.HomeSliderVM?.SetCurrentIndex(e.CurrentPosition);

		if (e.CurrentPosition != e.PreviousPosition)
			RestartSliderTimer();
	}

	private void EnsureSliderTimer()
	{
		if (_sliderTimer != null)
			return;

		_sliderTimer = Dispatcher.CreateTimer();
		_sliderTimer.Interval = TimeSpan.FromSeconds(5);
		_sliderTimer.IsRepeating = true;
		_sliderTimer.Tick += OnSliderTick;
	}

	private void StartSliderTimer()
	{
		EnsureSliderTimer();
		if (_sliderTimer is { IsRunning: false })
			_sliderTimer.Start();
	}

	private void StopSliderTimer()
	{
		if (_sliderTimer?.IsRunning == true)
			_sliderTimer.Stop();
	}

	private void RestartSliderTimer()
	{
		StopSliderTimer();
		StartSliderTimer();
	}

	private void OnSliderTick(object sender, EventArgs e)
	{
		if (HomeSlider?.Handler == null || HomeSlider.Window == null || HomeSliderHost?.IsVisible != true)
			return;

		if (BindingContext is AppViewModel vm && vm.HomeSliderVM?.IsLoading == true)
			return;

		if (HomeSlider.ItemsSource is not System.Collections.ICollection items || items.Count < 2)
			return;

		var next = HomeSlider.Position + 1;
		if (next >= items.Count)
			next = 0;

		try
		{
			_sliderAdvanceInProgress = true;
			HomeSlider.ScrollTo(next, animate: next != 0);
			if (BindingContext is AppViewModel current)
				current.HomeSliderVM?.SetCurrentIndex(next);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[HomeSlider] auto scroll skipped: {ex.Message}");
		}
		finally
		{
			_sliderAdvanceInProgress = false;
		}
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
        ShowToast("���� ��� ���� ������");
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

    private void OnTeamListScrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        if (BindingContext is not AppViewModel vm || sender is not CollectionView list)
            return;

        var dots = vm.TeamPagerDots?.Count ?? 0;
        var cards = vm.WorkTeams?.Count ?? 0;
        if (dots == 0 || cards == 0 || list.Width <= 0)
            return;

        const double cardWidth = 130;
        const double spacing = 12;
        var contentWidth = cards * cardWidth + Math.Max(0, cards - 1) * spacing;
        var maxOffset = Math.Max(0, contentWidth - list.Width);
        var offset = Math.Max(0, e.HorizontalOffset);

        int index;
        if (dots == 1 || maxOffset <= 1)
            index = 0;
        else if (offset >= maxOffset - 12)
            index = dots - 1;
        else
            index = (int)Math.Round(offset / maxOffset * (dots - 1));

        vm.TeamPagerIndex = Math.Clamp(index, 0, dots - 1);
    }
}

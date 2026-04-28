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

/// <summary>
/// «·’›Õ… «·—∆Ì”Ì…
///   ÕœÌÀ « Ã«ÂÂ«  ·ﬁ«∆Ì« ⁄‰œ  €ÌÌ— «··€…
/// </summary>
public partial class HomePage : ContentPage
{
	private System.Timers.Timer _carouselTimer;
	private int _currentCarouselIndex = 0;

	public HomePage()
	{
		InitializeComponent();
		this.BindingContext= AppViewModel.Instance;

		//  ÂÌ∆…   »⁄ «··€… Ê«·« Ã«Â «· ·ﬁ«∆Ì
		this.InitializeLanguageTracking();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		StartCarouselAutoScroll();
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		StopCarouselAutoScroll();
	}

	private void StartCarouselAutoScroll()
	{
		if (_carouselTimer != null)
			return;

		_carouselTimer = new System.Timers.Timer(2000); // 5 seconds interval
		_carouselTimer.Elapsed += (s, e) =>
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				try
				{
					if (carouselView != null)
					{
						// 6 images in the carousel
						_currentCarouselIndex = (_currentCarouselIndex + 1) % 6;
						carouselView.Position = _currentCarouselIndex;
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"[HomePage] Carousel scroll error: {ex.Message}");
				}
			});
		};
		_carouselTimer.Start();
	}

	private void StopCarouselAutoScroll()
	{
		if (_carouselTimer != null)
		{
			_carouselTimer.Stop();
			_carouselTimer.Dispose();
			_carouselTimer = null;
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
        ShowToast("«÷€ÿ „—… √Œ—Ï ··Œ—ÊÃ");
        return true; 
    }

    private async void ShowToast(string message)
    {
        var toast = Toast.Make(message, ToastDuration.Short);
        await toast.Show();
    }

 

    private Frame _lastSelectedFrame;

    private async void OnCategoryTapped(object sender, TappedEventArgs e)
    {
        if (sender is Frame tappedFrame && tappedFrame.BindingContext is Category selectedCategory)
        {
            var vm = BindingContext as AppViewModel;
            vm?.FilterServices(selectedCategory);

            if (_lastSelectedFrame != null)
                _lastSelectedFrame.BorderColor = Color.FromArgb("#444444");

            tappedFrame.BorderColor = Color.FromArgb("#EBD750");
            _lastSelectedFrame = tappedFrame;

         
            tappedFrame.AnchorX = 0.5;
            tappedFrame.AnchorY = 0.5;

         
            await tappedFrame.ScaleTo(1.05, 100, Easing.CubicOut);
            await tappedFrame.ScaleTo(1, 100, Easing.CubicIn);
        }
    }

   
}

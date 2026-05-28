using loukupm.Services;
using loukupm.ViewModel;
using loukupm.Langue;

namespace loukupm.View;

public partial class AboutUS : ContentPage
{
	private AboutUsViewModel _viewModel;
	private CancellationTokenSource _autoScrollCts;
	private CancellationTokenSource _featuresAutoScrollCts;
	private Task _autoScrollTask;
	private Task _featuresAutoScrollTask;

	public AboutUS()
	{
		InitializeComponent();
		_viewModel = new AboutUsViewModel();
		BindingContext = _viewModel;
	}
	protected override async void OnAppearing()
	{
		base.OnAppearing();

		// Load AboutUs data when page appears
		if (_viewModel.AboutUsData == null)
		{
			await _viewModel.LoadAboutUsDataCommand.ExecuteAsync(null);
		}

		// Connect IndicatorView to CarouselView
		heroIndicatorView.SetBinding(IndicatorView.ItemsSourceProperty, "AboutUsData.Hero.Images");
		heroIndicatorView.SetBinding(IndicatorView.PositionProperty, new Binding("Position", source: heroCarousel));

		// Subscribe to language change events
		// When user changes language in SettingsPage, this refreshes all collections
		LocalizationResourcesManager.Instanse.LanguageChanged += OnLanguageChanged;
		CollectionRefreshService.Instance.CollectionsNeedRefresh += OnCollectionsNeedRefresh;

		// Start auto-scroll for both carousels
		StartAutoScroll();
		StartFeaturesAutoScroll();
	}

    protected override bool OnBackButtonPressed()
	{
		StopAutoScroll();
		MainThread.BeginInvokeOnMainThread(async () =>
		{
			await NavigationService.HandleBackButton(NavigationService.ROUTE_ABOUT_US);
		});
		return true;
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		StopAutoScroll();
		StopFeaturesAutoScroll();

		// Unsubscribe from language change events to prevent memory leaks
		LocalizationResourcesManager.Instanse.LanguageChanged -= OnLanguageChanged;
		CollectionRefreshService.Instance.CollectionsNeedRefresh -= OnCollectionsNeedRefresh;
	}

	/// <summary>
	/// Handles language change events from LocalizationResourcesManager.
	/// Refreshes all collections to force CollectionView/CarouselView to re-render items.
	/// </summary>
	private void OnLanguageChanged(System.Globalization.CultureInfo culture)
	{
		MainThread.BeginInvokeOnMainThread(() =>
		{
			Console.WriteLine($"🌍 AboutUS.OnLanguageChanged triggered for culture: {culture?.DisplayName}");
			_viewModel?.RefreshCollectionsForLanguageChange();
		});
	}

	/// <summary>
	/// Handles collection refresh signal from CollectionRefreshService.
	/// Alternative trigger for collection refresh (not currently used but available for future use).
	/// </summary>
	private void OnCollectionsNeedRefresh()
	{
		Console.WriteLine("📋 AboutUS.OnCollectionsNeedRefresh triggered");
		_viewModel?.RefreshCollectionsForLanguageChange();
	}

	private void StartAutoScroll()
	{
		if (_autoScrollCts != null)
		{
			return; // Already running
		}

		_autoScrollCts = new CancellationTokenSource();
		_autoScrollTask = AutoScrollCarousel(_autoScrollCts.Token);
	}

	private void StopAutoScroll()
	{
		_autoScrollCts?.Cancel();
		_autoScrollCts = null;
	}

	private void StartFeaturesAutoScroll()
	{
		if (_featuresAutoScrollCts != null)
		{
			return; // Already running
		}

		_featuresAutoScrollCts = new CancellationTokenSource();
		_featuresAutoScrollTask = AutoScrollFeaturesCarousel(_featuresAutoScrollCts.Token);
	}

	private void StopFeaturesAutoScroll()
	{
		_featuresAutoScrollCts?.Cancel();
		_featuresAutoScrollCts = null;
	}

	private async Task AutoScrollCarousel(CancellationToken cancellationToken)
	{
		try
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				await Task.Delay(2000, cancellationToken); // 2 seconds interval

				if (heroCarousel?.ItemsSource is not null)
				{
					var itemCount = ((System.Collections.ICollection)heroCarousel.ItemsSource).Count;
					if (itemCount > 0)
					{
						var currentPosition = heroCarousel.Position;
						var nextPosition = (currentPosition + 1) % itemCount;
						heroCarousel.ScrollTo(nextPosition, animate: true);
					}
				}
			}
		}
		catch (OperationCanceledException)
		{
			// Expected when stopping
		}
	}

	private async Task AutoScrollFeaturesCarousel(CancellationToken cancellationToken)
	{
		try
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				await Task.Delay(3000, cancellationToken); // 3 seconds interval for features

				if (featuresCarousel?.ItemsSource is not null)
				{
					var itemCount = ((System.Collections.ICollection)featuresCarousel.ItemsSource).Count;
					if (itemCount > 0)
					{
						var currentPosition = featuresCarousel.Position;
						var nextPosition = (currentPosition + 1) % itemCount;
						featuresCarousel.ScrollTo(nextPosition, animate: true);
					}
				}
			}
		}
		catch (OperationCanceledException)
		{
			// Expected when stopping
		}
	}
  

    private async void Button_Clicked(object sender, EventArgs e)
	{
		await NavigationService.NavigateToTabBarPage(NavigationService.ROUTE_SERVICES);
	}
}


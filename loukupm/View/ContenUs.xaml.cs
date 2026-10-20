using loukupm.Langue;
using loukupm.Services;
using loukupm.ViewModel;

namespace loukupm.View;

public partial class ContenUs : ContentPage
{
    private AboutUsViewModel _viewModel;
	public ContenUs()
	{
		InitializeComponent();
		this.InitializeLanguageTracking();
		_viewModel = new AboutUsViewModel();
		BindingContext = _viewModel;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LocalizationResourcesManager.Instanse.LanguageChanged -= OnLanguageChanged;
        LocalizationResourcesManager.Instanse.LanguageChanged += OnLanguageChanged;

        if (_viewModel.AboutUsData == null || _viewModel.NeedsLanguageReload)
            await _viewModel.LoadAboutUsDataCommand.ExecuteAsync(null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        LocalizationResourcesManager.Instanse.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(System.Globalization.CultureInfo culture)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            Console.WriteLine($"🌍 ContenUs.OnLanguageChanged triggered for culture: {culture?.DisplayName}");
            if (_viewModel?.LoadAboutUsDataCommand.CanExecute(null) == true)
                await _viewModel.LoadAboutUsDataCommand.ExecuteAsync(null);
        });
    }
    private void OnCollectionsNeedRefresh()
    {
        Console.WriteLine("📋 AboutUS.OnCollectionsNeedRefresh triggered");
        _viewModel?.RefreshCollectionsForLanguageChange();
    }
    protected override bool OnBackButtonPressed()
    {
        
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await NavigationService.HandleBackButton(NavigationService.Route_ContactUs);
        });
        return true;
    }

}
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
            LocalizationResourcesManager.Instanse.LanguageChanged += OnLanguageChanged;
            CollectionRefreshService.Instance.CollectionsNeedRefresh += OnCollectionsNeedRefresh;
        }
    }
    private void OnLanguageChanged(System.Globalization.CultureInfo culture)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Console.WriteLine($"🌍 AboutUS.OnLanguageChanged triggered for culture: {culture?.DisplayName}");
            _viewModel?.RefreshCollectionsForLanguageChange();
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
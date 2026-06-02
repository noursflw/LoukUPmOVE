using loukupm.Services;
using loukupm.ViewModel;

namespace loukupm.View;

public partial class ImpressumPage : ContentPage
{
    private ImpressumViewModel _viewModel;

    public ImpressumPage()
    {
        InitializeComponent();

        // Initialize ViewModel
        _viewModel = new ImpressumViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Load Impressum data when page appears
        if (_viewModel != null && _viewModel.CmsData == null)
        {
            await _viewModel.LoadImpressumCommand.ExecuteAsync(null);
        }
    }
    protected override bool OnBackButtonPressed()
    {
        _ = NavigationService.HandleBackButton(NavigationService.ROUTE_IMPRESSUM);
        return true;
    }
}

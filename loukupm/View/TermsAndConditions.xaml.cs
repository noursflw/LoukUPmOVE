namespace loukupm.View;

using System.Globalization;
using loukupm.Services;
using loukupm.ViewModel;

/// <summary>
/// Terms and Conditions page with dynamic CMS content loading
/// </summary>
public partial class TermsAndConditions : ContentPage
{
    private TermsAndConditionsViewModel _viewModel;

    public TermsAndConditions()
    {
        InitializeComponent();

        // Initialize language tracking
        this.InitializeLanguageTracking();

        // Create and set the ViewModel
        _viewModel = new TermsAndConditionsViewModel();
        this.BindingContext = _viewModel;
    }

    /// <summary>
    /// Load content when page appears
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel?.CmsData == null)
        {
            await _viewModel.LoadTermsAndConditionsCommand.ExecuteAsync(null);
        }
    }

    /// <summary>
    /// Clean up when page disappears
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Console.WriteLine("📄 TermsAndConditions page disappearing");
    }

    /// <summary>
    /// Handle back button navigation
    /// ????? ?? ?????? - ???? ????? ?????? Traditional Stack Navigation
    /// </summary>
    protected override bool OnBackButtonPressed()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await NavigationService.HandleBackButton(NavigationService.ROUTE_TERMS_CONDITIONS);
        });
        return true;
    }
}

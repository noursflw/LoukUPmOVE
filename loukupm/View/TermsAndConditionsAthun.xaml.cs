namespace loukupm.View;

using System.Globalization;
using loukupm.Services;
using loukupm.ViewModel;


public partial class TermsAndConditionsAthun : ContentPage
{
    private TermsAndConditionsViewModel _viewModel;
    public TermsAndConditionsAthun()
	{
		InitializeComponent();
        this.InitializeLanguageTracking();
        _viewModel = new TermsAndConditionsViewModel();
        this.BindingContext = _viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await Task.Yield();
        

        try
        {
            // Load Terms and Conditions from CMS API
            if (_viewModel != null)
            {
                Console.WriteLine("📄 TermsAndConditions page appearing - triggering data load");
                await _viewModel.LoadTermsAndConditionsCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in OnAppearing: {ex.Message}");
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
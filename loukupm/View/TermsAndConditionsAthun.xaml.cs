namespace loukupm.View;

using System.Globalization;
using loukupm.Services;
using loukupm.ViewModel;


public partial class TermsAndConditionsAthun : ContentPage
{
	private TermsAndConditionsViewModel _viewModel;
	public TermsAndConditionsAthun()
	{
        Console.WriteLine("1");
        InitializeComponent();
        Console.WriteLine("2");

        this.InitializeLanguageTracking();
        Console.WriteLine("3");

        _viewModel = new TermsAndConditionsViewModel();
        Console.WriteLine("4");

        BindingContext = _viewModel;
        Console.WriteLine("5");
    }
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
            await NavigationService.HandleBackButton(NavigationService.ROUTE_TermsAndConditions_Athun);
        });
        return true;
    }

}
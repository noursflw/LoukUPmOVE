using loukupm.ViewModel;
using loukupm.services;
using System.Globalization;
using loukupm.Services;

namespace loukupm.View;

public partial class PolicyandPrivacyPage : ContentPage
{
    private PrivacyPolicyViewModel _viewModel;

    public PolicyandPrivacyPage()
    {
        InitializeComponent();

        // Create and bind ViewModel
        _viewModel = new PrivacyPolicyViewModel();
        this.BindingContext = _viewModel;

        // Subscribe to language change event for automatic reload
        Langue.LocalizationResourcesManager.Instanse.LanguageChanged += OnLanguageChanged;

        // Cleanup on page unload
        Unloaded += (s, e) =>
        {
            Langue.LocalizationResourcesManager.Instanse.LanguageChanged -= OnLanguageChanged;
        };
    }

    /// <summary>
    /// Load Privacy Policy when page appears
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Console.WriteLine($"📄 Privacy Policy page appearing");

        // Load privacy policy content from CMS
        if (_viewModel != null)
        {
            await _viewModel.LoadPrivacyPolicyCommand.ExecuteAsync(null);
        }
    }

    /// <summary>
    /// Handle back button navigation
    /// </summary>
    protected override bool OnBackButtonPressed()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await NavigationService.HandleBackButton(NavigationService.ROUTE_POLICY_PRIVACY);
        });
        return true;
    }

    /// <summary>
    /// Reload Privacy Policy when language changes
    /// </summary>
    private void OnLanguageChanged(CultureInfo culture)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            Console.WriteLine($"🔄 Language changed to {culture.DisplayName}, reloading Privacy Policy");
            if (_viewModel != null)
            {
                await _viewModel.LoadPrivacyPolicyCommand.ExecuteAsync(null);
            }
        });
    }
}

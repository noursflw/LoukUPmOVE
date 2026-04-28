using loukupm.ViewModel;
using loukupm.Services;
using System.Globalization;

namespace loukupm.View;

public partial class PolicyandPrivacyPage : ContentPage
{
	public PolicyandPrivacyPage()
	{
		InitializeComponent();
		this.BindingContext = AppViewModel.Instance;

		// Load policy and privacy data
		_ = AppViewModel.Instance.LoadPolicyandPrivacyAsync();

		// Set initial flow direction based on current language
		UpdateFlowDirection(Langue.LocalizationResourcesManager.Instanse.CurrentCulture);

		// Subscribe to language change event
		Langue.LocalizationResourcesManager.Instanse.LanguageChanged += OnLanguageChanged;

		// Cleanup on page unload
		Unloaded += (s, e) =>
		{
			Langue.LocalizationResourcesManager.Instanse.LanguageChanged -= OnLanguageChanged;
		};
	}

	/// <summary>
	/// ????? ?? ?????? - ???? ????? ?????? Traditional Stack Navigation
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
	/// Ì „ «” œ⁄«¡ Â–Â «·œ«·… ⁄‰œ  €ÌÌ— «··€…
	/// </summary>
	private void OnLanguageChanged(CultureInfo culture)
	{
		MainThread.BeginInvokeOnMainThread(() =>
		{
			UpdateFlowDirection(culture);
		});
	}

	/// <summary>
	///  ÕœÌÀ « Ã«Â «·‰’ »‰«¡ ⁄·Ï «··€… «·Õ«·Ì…
	/// «·⁄—»Ì… ? RightToLeft (RTL)
	/// «·√·„«‰Ì… Ê«·≈‰Ã·Ì“Ì… ? LeftToRight (LTR)
	/// </summary>
	private void UpdateFlowDirection(CultureInfo culture)
	{
		if (culture == null) return;

		//  Õﬁﬁ „‰ —„“ «··€… (ar ··⁄—»Ì…)
		string languageCode = culture.TwoLetterISOLanguageName.ToLower();

		if (languageCode == "ar")
		{
			this.FlowDirection = FlowDirection.RightToLeft;
			Console.WriteLine($"? Page Flow Direction Changed to RTL (Arabic)");
		}
		else
		{
			this.FlowDirection = FlowDirection.LeftToRight;
			Console.WriteLine($"? Page Flow Direction Changed to LTR ({culture.DisplayName})");
		}
	}
}

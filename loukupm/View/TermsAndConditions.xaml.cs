namespace loukupm.View;
using System.Globalization;
using loukupm.Services;

/// <summary>
/// ÕİÍÉ ÇáÔÑæØ æÇáÃÍßÇã
/// ÊÊÍÏË ÇÊÌÇååÇ ÊáŞÇÆíÇğ ÚäÏ ÊÛííÑ ÇááÛÉ
/// </summary>
public partial class TermsAndConditions : ContentPage
{
	public TermsAndConditions()
	{
		InitializeComponent();

		// ÊåíÆÉ ÊÊÈÚ ÇááÛÉ æÇáÇÊÌÇå ÇáÊáŞÇÆí
		this.InitializeLanguageTracking();
	}

	/// <summary>
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

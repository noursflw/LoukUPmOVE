namespace loukupm.View;
using System.Globalization;

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
}

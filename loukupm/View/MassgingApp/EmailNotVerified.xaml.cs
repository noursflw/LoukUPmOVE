using CommunityToolkit.Maui.Views;
using loukupm.Langue;

namespace loukupm.View.MassgingApp;

public partial class EmailNotVerified : Popup
{
	public EmailNotVerified()
	{
		InitializeComponent();
		DialogRoot.FlowDirection = LocalizationResourcesManager.Instanse.GetFlowDirection();
	}

	private void CloseClicked(object? sender, EventArgs e)
	{
		Close(false);
	}

	private void VerifyClicked(object? sender, EventArgs e)
	{
		Close(true);
	}
}

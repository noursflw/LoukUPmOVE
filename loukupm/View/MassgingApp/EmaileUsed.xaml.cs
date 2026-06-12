using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using loukupm.Services;

namespace loukupm.View.MassgingApp;

public partial class EmaileUsed : Popup
{
	public EmaileUsed()
	{
		InitializeComponent();
        this.Color = Colors.Transparent;
	}
	private async void CancelClicked(object? sender, EventArgs e)
	{
		await ShellNavigationManager.NavigateToLoginAndClear();
    }
}
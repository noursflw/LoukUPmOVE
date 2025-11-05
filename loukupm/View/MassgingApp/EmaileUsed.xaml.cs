using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;

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
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
using CommunityToolkit.Maui.Views;
namespace loukupm.View;

public partial class CompletedLogin : Popup
{
	public CompletedLogin()
	{
		InitializeComponent();
        this.Color = Colors.Transparent;
    }

    private void CancelClicked(object? sender, EventArgs e)
    {

        Close(false);
    }
}
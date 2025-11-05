using CommunityToolkit.Maui.Views;

namespace loukupm.View.MassgingApp;

public partial class NoServerResponse : Popup
{
	public NoServerResponse()
	{
		InitializeComponent();
        this.Color = Colors.Transparent;
    }

    private void CancelClicked(object? sender, EventArgs e)
    {

        Close(false);
    }

}
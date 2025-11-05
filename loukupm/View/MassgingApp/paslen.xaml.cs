using CommunityToolkit.Maui.Views;

namespace loukupm.View.MassgingApp;

public partial class paslen : Popup
{
	public paslen()
	{
		InitializeComponent();
        this.Color = Colors.Transparent;
    }

    private void CancelClicked(object? sender, EventArgs e)
    {

        Close(false);
    }

}
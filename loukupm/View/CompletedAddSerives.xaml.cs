using CommunityToolkit.Maui.Views;

namespace loukupm.View;

public partial class CompletedAddSerives : Popup
{
	public CompletedAddSerives()
	{
		InitializeComponent();
        this.Color = Colors.Transparent;
    }

    private void CancelClicked(object? sender, EventArgs e)
    {
        Close(false);
    }

}
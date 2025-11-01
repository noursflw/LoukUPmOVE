using CommunityToolkit.Maui.Views;

namespace loukupm.View;

public partial class ConfermChange : Popup
{
	public ConfermChange()
	{
		InitializeComponent();
        this.Color = Colors.Transparent;
    }

    private void CancelClicked(object? sender, EventArgs e)
    {
        Close(false);
    }
}
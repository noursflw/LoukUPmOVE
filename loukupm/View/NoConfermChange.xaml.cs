using CommunityToolkit.Maui.Views;

namespace loukupm.View;

public partial class NoConfermChange : Popup
{
	public NoConfermChange()
	{
		InitializeComponent();
        this.Color = Colors.Transparent;
    }
    private void CancelClicked(object? sender, EventArgs e)
    {
        Close(false);
    }
}
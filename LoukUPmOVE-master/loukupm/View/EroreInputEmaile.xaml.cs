
using CommunityToolkit.Maui.Views;

namespace loukupm.View;


public partial class EroreInputEmaile : Popup
{
	public EroreInputEmaile()
	{
		InitializeComponent();
        this.Color = Colors.Transparent;
    }

    private void CancelClicked(object? sender, EventArgs e)
    {
        Close(false);
    }
}
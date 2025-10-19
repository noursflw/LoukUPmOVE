
using Microsoft.Maui.Storage;
using CommunityToolkit.Maui.Views;
namespace loukupm.View;
public partial class DisplayAlretCoustm : Popup
{
	public DisplayAlretCoustm()
	{
		InitializeComponent();
        this.Color = Colors.Transparent;
    }

    private void CancelClicked(object? sender, EventArgs e)
    {
        Close(false);
    }

}
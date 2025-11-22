using CommunityToolkit.Maui.Views;
using Microsoft.Maui.ApplicationModel.Communication;

namespace loukupm.View;

public partial class NoEqaulData : Popup
{
	public NoEqaulData()
	{
		InitializeComponent();
        this.Color = Colors.Transparent;

    }

    private void CancelClicked(object? sender, EventArgs e)
    {
      
        Close(false);
    }
}
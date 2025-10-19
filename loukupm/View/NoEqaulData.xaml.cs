using CommunityToolkit.Maui.Views;
using Microsoft.Maui.ApplicationModel.Communication;
using Stripe.Climate;
namespace loukupm.View;

public partial class NoEqaulData : Popup
{
	public NoEqaulData()
	{
		InitializeComponent();
	}

    private void CancelClicked(object? sender, EventArgs e)
    {
      
        Close(false);
    }
}
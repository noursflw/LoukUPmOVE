using CommunityToolkit.Maui.Views;
namespace loukupm.View.MassgingApp;

public partial class ErorRemoveMyAccount : Popup
{
	public ErorRemoveMyAccount()
	{
		InitializeComponent();
	}

    private void Button_Clicked_1(object sender, EventArgs e)
    {
        Close(true);
    }
}
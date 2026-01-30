using CommunityToolkit.Maui.Views;

namespace loukupm.View.MassgingApp;

public partial class SuccessfullyVerified : Popup
{
	public SuccessfullyVerified()
	{
		InitializeComponent();
	}
    private void Button_Clicked_1(object sender, EventArgs e)
    {
        Close(true);
    }
}
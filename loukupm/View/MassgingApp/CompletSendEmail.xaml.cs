using CommunityToolkit.Maui.Views;

namespace loukupm.View.MassgingApp;

public partial class CompletSendEmail : Popup
{
	public CompletSendEmail()
	{
		InitializeComponent();
	}
    private void Button_Clicked_1(object sender, EventArgs e)
    {
        Close(true);
    }
}
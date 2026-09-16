using CommunityToolkit.Maui.Views;

namespace loukupm.View.MassgingApp;

public partial class EmaileIsNotFound : Popup
{
	public EmaileIsNotFound()
	{
		InitializeComponent();
	}
    private void Button_Clicked_1(object sender, EventArgs e)
    {
        Close(true);
    }
}
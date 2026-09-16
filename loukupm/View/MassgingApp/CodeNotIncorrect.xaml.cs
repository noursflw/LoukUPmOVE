using CommunityToolkit.Maui.Views;

namespace loukupm.View.MassgingApp;

public partial class CodeNotIncorrect : Popup
{
	public CodeNotIncorrect()
	{
		InitializeComponent();
	}
    private void Button_Clicked_1(object sender, EventArgs e)
    {
        Close(true);
    }
}
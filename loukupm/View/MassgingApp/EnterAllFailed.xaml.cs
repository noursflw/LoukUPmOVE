using CommunityToolkit.Maui.Views;
namespace loukupm.View.MassgingApp;


public partial class EnterAllFailed : Popup
{
	public EnterAllFailed()
	{
		InitializeComponent();
        
	}

    private void Button_Clicked_1(object sender, EventArgs e)
    {
        Close(true);
    }

}
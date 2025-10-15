using CommunityToolkit.Maui.Views;

namespace loukupm.View;

public partial class MassegBoxLogout : Popup
{
	public MassegBoxLogout()
	{
		InitializeComponent();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("LoginPage");
        Close(true);
    }

    private void Button_Clicked_1(object sender, EventArgs e)
    {
        Close(true);
    }
}
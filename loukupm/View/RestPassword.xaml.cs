namespace loukupm.View;

public partial class RestPassword : ContentPage
{
	public RestPassword()
	{
		InitializeComponent();
        Shell.SetNavBarIsVisible(this, false);
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Verificationpage());
   
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
    }
}
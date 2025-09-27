namespace loukupm.View;

public partial class ChackoutPage : ContentPage
{
	public ChackoutPage()
	{
		InitializeComponent();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
	  await	Navigation.PushAsync(new LoginPage());	
    }
}
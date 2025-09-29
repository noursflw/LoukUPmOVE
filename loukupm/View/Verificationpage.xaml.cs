namespace loukupm.View;

public partial class Verificationpage : ContentPage
{
	public Verificationpage()
	{
		InitializeComponent();
        Shell.SetNavBarIsVisible(this, false);
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
		await Navigation.PushAsync(new EditPasswordVerification());
    }
}
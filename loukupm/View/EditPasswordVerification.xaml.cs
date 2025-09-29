namespace loukupm.View;

public partial class EditPasswordVerification : ContentPage
{
	public EditPasswordVerification()
	{
		InitializeComponent();
        Shell.SetNavBarIsVisible(this, false);
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
		await Navigation.PushAsync(new ChackoutPage());
    }
}
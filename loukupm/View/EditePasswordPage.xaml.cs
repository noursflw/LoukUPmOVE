namespace loukupm.View;

public partial class EditePasswordPage : ContentPage
{
	public EditePasswordPage()
	{
		InitializeComponent();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
		await Navigation.PopAsync();
    }
}
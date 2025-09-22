namespace loukupm.View;

public partial class EditeUserPage : ContentPage
{
	public EditeUserPage()
	{
		InitializeComponent();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
		await Navigation.PopAsync();	
    }
}
using loukupm.services;

namespace loukupm.View;

public partial class LoginPage : ContentPage
{
    public LoginPage()
	{
		InitializeComponent();
	}

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new SinginPage());

    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {

        await Navigation.PushAsync(new RestPassword());


    }

    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("//MainPage");
        return true;
    }
    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HomePage());
    }
}
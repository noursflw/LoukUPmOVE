using loukupm.Model;
using loukupm.services;

namespace loukupm.View;

public partial class SinginPage : ContentPage
{
    public SinginPage()
	{
		InitializeComponent();
	}

    private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("LoginPage");
    }

    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("LoginPage");
        return true;
    }


}
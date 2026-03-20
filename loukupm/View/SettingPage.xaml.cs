namespace loukupm.View;

public partial class SettingPage : ContentPage
{
	public SettingPage()
	{
		InitializeComponent();
	}

    private async void TapGestureRecognizer_Tapped_6(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new PolicyandPrivacyPage());
    }
    private async void Button_Clicked_11(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PolicyandPrivacyPage());
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new TermsAndConditions());
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TermsAndConditions());
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ProfilePage");
    }
}
using CommunityToolkit.Maui.Views;         // PopupOptions, ShowPopupAsync
using loukupm.Services;
using loukupm.ViewModel;
using Microsoft.Maui.Controls.Shapes;     // RoundRectangle
using Microsoft.Maui.Graphics;

using System.Threading.Tasks;

namespace loukupm.View;

public partial class ProfilePage : ContentPage
{
	public ProfilePage()
	{
		InitializeComponent();
        this.BindingContext = AppViewModel.Instance;
        Shell.SetNavBarIsVisible(this, false);
	}

    private void Button_Clicked(object sender, EventArgs e)
    {
		Shell.Current.GoToAsync("//HomePage");
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new EditeUserPage());
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new EditeUserPage());
    }

    private async void Button_Clicked_2(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new EditePasswordPage());
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new EditePasswordPage());
    }

    private async void Button_Clicked_3(object sender, EventArgs e)
    {
        await Navigation.PushAsync (new BookingPage());   
    }

    private async void TapGestureRecognizer_Tapped_2(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new BookingPage());
    }

    private async void Button_Clicked_4(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new NotifictionPage());    
    }

    private async void TapGestureRecognizer_Tapped_3(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new NotifictionPage());
    }

    private async void Button_Clicked_5(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingPage());    
    }

    private async void TapGestureRecognizer_Tapped_4(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new SettingPage());
    }

    private async void Button_Clicked_6(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AboutUS());
    }

    private async void TapGestureRecognizer_Tapped_5(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new AboutUS());
    }

    private async void Button_Clicked_7(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new EditeUserPage());
    }
    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("//ProfilePage");
        return true;
    }

    private async void Button_Clicked_8(object sender, EventArgs e)
    {
        OneSignalService.Logout();  // ✨ استخدم async
        SecureStorage.Remove("auth_token");
        SecureStorage.Remove("refresh_token");
        
        var popup = new MassegBoxLogout();
        await this.ShowPopupAsync(popup);  
    }

    private async void Button_Clicked_9(object sender, EventArgs e)
    {

        var popup = new RemoveUserPopup();
         OneSignalService.Logout();

        await this.ShowPopupAsync(popup);

    }

    private async void TapGestureRecognizer_Tapped_6(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new PolicyandPrivacyPage());
    }

    
    private async void Button_Clicked_11(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PolicyandPrivacyPage());
    }
}
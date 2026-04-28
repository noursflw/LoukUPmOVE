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

    private async void Button_Clicked(object sender, EventArgs e)
    {
        NavigationService.NavigateToTabBarPage(NavigationService.ROUTE_HOME);
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_EDIT_USER);
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_EDIT_USER);
    }

    private async void Button_Clicked_2(object sender, EventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_EDIT_PASSWORD);
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_EDIT_PASSWORD);
    }

    private async void Button_Clicked_3(object sender, EventArgs e)
    {
        await NavigationService.NavigateToTabBarPage(NavigationService.ROUTE_BOOKING);   
    }

    private async void TapGestureRecognizer_Tapped_2(object sender, TappedEventArgs e)
    {
        await NavigationService.NavigateToTabBarPage(NavigationService.ROUTE_BOOKING);
    }

    private async void Button_Clicked_4(object sender, EventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_NOTIFICATION);    
    }

    private async void TapGestureRecognizer_Tapped_3(object sender, TappedEventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_NOTIFICATION);
    }

    private async void Button_Clicked_5(object sender, EventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_SETTING);    
    }

    private async void TapGestureRecognizer_Tapped_4(object sender, TappedEventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_SETTING);
    }

    private async void Button_Clicked_6(object sender, EventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_ABOUT_US);
    }

    private async void TapGestureRecognizer_Tapped_5(object sender, TappedEventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_ABOUT_US);
    }

    private async void Button_Clicked_7(object sender, EventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_EDIT_USER);
    }
    protected override bool OnBackButtonPressed()
    {
        // TabBar page: Delegate to centralized back button logic
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await NavigationService.HandleBackButton(NavigationService.ROUTE_PROFILE);
        });
        return true;
    }

    private async void Button_Clicked_8(object sender, EventArgs e)
    {
        OneSignalService.Logout();  // ✨ استخدم async
        SecureStorage.Remove("auth_token");
        SecureStorage.Remove("refresh_token");
        
        // مسح خريطة التنقل
        //NavigationService.ClearPageSourceMap();
        
        // Reset authentication check flag to allow re-authentication
        App.ResetAuthenticationCheck();
        
        var popup = new MassegBoxLogout();
        await this.ShowPopupAsync(popup);  
    }

    private async void Button_Clicked_9(object sender, EventArgs e)
    {
        var popup = new RemoveUserPopup();
        OneSignalService.Logout();
        
        // مسح خريطة التنقل قبل حذف الحساب
       // NavigationService.ClearPageSourceMap();
        
        // Reset authentication check flag
        App.ResetAuthenticationCheck();

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
namespace loukupm.View;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using loukupm.ViewModel;
using System.Globalization;



   public partial class HomePage : ContentPage
{
	public HomePage()
	{
		InitializeComponent();
    	this.BindingContext= AppViewModel.Instance;

    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
		await Navigation.PushAsync(new ServicesPage());   

    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new NotifictionPage());
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AboutUS());
    }

    private DateTime _lastBackPressed;

    protected override bool OnBackButtonPressed()
    {
        // ≈–« ﬂ«‰ Â‰«ﬂ ’›Õ«  ”«»ﬁ…
        if (Navigation.NavigationStack.Count > 1)
        {
            Shell.Current.GoToAsync("//HomePage");
            return true; // „‰⁄ «·≈Ã—«¡ «·«› —«÷Ì
        }
        else
        {
            var currentTime = DateTime.Now;
            if ((currentTime - _lastBackPressed).TotalSeconds <= 2)
            {
                // Œ—ÊÃ „‰ «· ÿ»Ìﬁ »⁄œ «·÷€ÿ „— Ì‰
                System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
            }
            else
            {
                _lastBackPressed = currentTime;
                // ⁄—÷ Toast
                ShowToast("«÷€ÿ „—… √Œ—Ï ··Œ—ÊÃ");
            }
            return true; // „‰⁄ «·≈Ã—«¡ «·«› —«÷Ì
        }
    }
    private async void ShowToast(string message)
    {
        var toast = Toast.Make(message, ToastDuration.Short);
        await toast.Show();
    }


    
    private void Button_Clicked_2(object sender, EventArgs e)
    {
        var current = Langue.LocalizationResourcesManager.Instanse.CurrentCulture.TwoLetterISOLanguageName;
        CultureInfo newCulture;

        switch (current)
        {
            case "en":
                newCulture = new CultureInfo("de-DE");
                FlowDirection= FlowDirection.LeftToRight;
                break;
            case "de":
                newCulture = new CultureInfo("ar-AR");
                FlowDirection= FlowDirection.RightToLeft;
                break;
            default:
                newCulture = new CultureInfo("de-DE");
                FlowDirection = FlowDirection.LeftToRight;
                break;
        }
        Preferences.Set("AppLanguage", newCulture.Name);
        Langue.LocalizationResourcesManager.Instanse.SetCulture(newCulture);
    }
}
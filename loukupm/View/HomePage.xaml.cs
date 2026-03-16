namespace loukupm.View;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using FFImageLoading.Maui;
using loukupm.Model;
using loukupm.ViewModel;
using System.Globalization;

/// <summary>
/// «·’›Õ… «·—∆Ì”Ì…
///   ÕœÌÀ « Ã«ÂÂ«  ·ﬁ«∆Ì« ⁄‰œ  €ÌÌ— «··€…
/// </summary>
public partial class HomePage : ContentPage
{
	public HomePage()
	{
		InitializeComponent();
    	this.BindingContext= AppViewModel.Instance;
    	
    	//  ÂÌ∆…   »⁄ «··€… Ê«·« Ã«Â «· ·ﬁ«∆Ì
    	this.InitializeLanguageTracking();
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
        var service = (sender as Button)?.BindingContext as Servies;
        if (service == null) return;

        if (AppViewModel.Instance.CurrentBooking.SelectedServices == null)
            AppViewModel.Instance.CurrentBooking.SelectedServices = new List<Servies>();

        if (!AppViewModel.Instance.CurrentBooking.SelectedServices.Contains(service))
        {
            AppViewModel.Instance.CurrentBooking.SelectedServices.Add(service);
            await Toast.Make(Langue.AppResource.CompletedAddServies).Show();
        }
        else
        {
            AppViewModel.Instance.CurrentBooking.SelectedServices.Remove(service);
            await Toast.Make(Langue.AppResource.theserviewasdone).Show();
        }
    }

    private DateTime _lastBackPressed = DateTime.MinValue;
    protected override bool OnBackButtonPressed()
    {
        var currentTime = DateTime.Now;

        if ((currentTime - _lastBackPressed).TotalSeconds <= 2)
        {
#if ANDROID
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#endif
            return true;
        }
 
        _lastBackPressed = currentTime;
        ShowToast("«÷€ÿ „—… √Œ—Ï ··Œ—ÊÃ");
        return true; 
    }

    private async void ShowToast(string message)
    {
        var toast = Toast.Make(message, ToastDuration.Short);
        await toast.Show();
    }

    /// <summary>
    /// “—  €ÌÌ— «··€…
    /// «·œÊ—…: English ? German ? Arabic
    /// «· ÕœÌÀ «· ·ﬁ«∆Ì ··« Ã«Â Ì „ „‰ ﬁ»· LocalizationResourcesManager Ê«·‹ PageLanguageHelper
    /// </summary>
    private void Button_Clicked_2(object sender, EventArgs e)
    {
        var current = Langue.LocalizationResourcesManager.Instanse.CurrentCulture.TwoLetterISOLanguageName;
        CultureInfo newCulture;

        switch (current)
        {
            case "en":
                newCulture = new CultureInfo("de-DE");
                break;
            case "de":
                newCulture = new CultureInfo("ar-AR");
                break;
            default:
                newCulture = new CultureInfo("de-DE");
                break;
        }

        // Save language preference
        Preferences.Set("AppLanguage", newCulture.Name);
        
        // Update language and flow direction
        // All subscribed pages will update automatically via the LanguageChanged event
        Langue.LocalizationResourcesManager.Instanse.SetCulture(newCulture);

        Console.WriteLine($"? Language Changed to {newCulture.DisplayName}");
    }

    private Frame _lastSelectedFrame;

    private async void OnCategoryTapped(object sender, TappedEventArgs e)
    {
        if (sender is Frame tappedFrame && tappedFrame.BindingContext is Category selectedCategory)
        {
            var vm = BindingContext as AppViewModel;
            vm?.FilterServices(selectedCategory);

            if (_lastSelectedFrame != null)
                _lastSelectedFrame.BorderColor = Color.FromArgb("#444444");

            tappedFrame.BorderColor = Color.FromArgb("#EBD750");
            _lastSelectedFrame = tappedFrame;

            // ?? ÷»ÿ AnchorX/AnchorY
            tappedFrame.AnchorX = 0.5;
            tappedFrame.AnchorY = 0.5;

            //  √ÀÌ— Scale
            await tappedFrame.ScaleTo(1.05, 100, Easing.CubicOut);
            await tappedFrame.ScaleTo(1, 100, Easing.CubicIn);
        }
    }

    private async void ImageButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AboutUS());
    }
}

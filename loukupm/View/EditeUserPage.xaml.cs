

using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using loukupm.Langue;
using loukupm.Services;
using loukupm.ViewModel;

namespace loukupm.View;

public partial class EditeUserPage : ContentPage
{
    public EditeUserPage()
    {
        InitializeComponent();
        this.InitializeLanguageTracking();
        this.BindingContext = AppViewModel.Instance;
    }

    protected override bool OnBackButtonPressed()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await NavigationService.HandleBackButton(NavigationService.ROUTE_EDIT_USER);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EditeUserPage] Back button error: {ex.Message}");
                }
            });
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EditeUserPage] OnBackButtonPressed crash: {ex.Message}");
            return true;
        }
    }

    
    private async void Button_Clicked(object sender, EventArgs e)
    {
       await NavigationService.NavigateToTabBarPage(NavigationService.ROUTE_PROFILE);
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        try
        {
         
            bool hasPermission = await RequestPhotoPermissionAsync();

            if (!hasPermission)
            {
                await ShowPermissionDeniedAlert();
                return;
            }

           
            await PickAndSetPhotoAsync();
        }
        catch (Exception ex)
        {
            await Toast.Make(AppResource.image_upload_failed, ToastDuration.Short).Show();
        }
    }
    private async Task<bool> RequestPhotoPermissionAsync()
    {
        PermissionStatus status = PermissionStatus.Unknown;

#if ANDROID
        // Android 13 وما فوق
        if (DeviceInfo.Version.Major >= 13)
        {
            status = await Permissions.CheckStatusAsync<Permissions.Media>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.Media>();
        }
        else
        {
            // Android 12 وأقل
            status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.StorageRead>();
        }
#elif IOS
        status = await Permissions.CheckStatusAsync<Permissions.Photos>();
        if (status != PermissionStatus.Granted)
            status = await Permissions.RequestAsync<Permissions.Photos>();
#endif

        return status == PermissionStatus.Granted;
    }

    private async Task PickAndSetPhotoAsync()
    {
        try
        {
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = AppResource.select_image_from_gallery
            });

            if (result != null)
            {
                var viewModel = BindingContext as AppViewModel;
                if (viewModel != null)
                {
                    
                    viewModel.SelectedImagePath = result.FullPath;

                    
                    viewModel.Avatar = result.FullPath;

                   
                    await Toast.Make(AppResource.image_upload_completed_success).Show();
                }
            }
        }
        catch (OperationCanceledException)
        {
            await Toast.Make(AppResource.operation_cancelled, ToastDuration.Short).Show();
        }
        catch (Exception ex)
        {
            
            await Toast.Make(AppResource.image_upload_failed, ToastDuration.Short).Show();
        }
    }

    
    private async Task ShowPermissionDeniedAlert()
    {
        bool result = await DisplayAlert(
            "أذن مطلوبة",
            "يجب السماح بالوصول للصور لاختيار صورة شخصية.\n\nهل تريد فتح إعدادات التطبيق؟",
            "نعم",
            "لا"
        );

        if (result)
        {
            AppInfo.ShowSettingsUI();
        }
    }

        private async void Button_Clicked_9(object sender, EventArgs e)
    {
        var popup = new RemoveUserPopup();

        // Trigger OneSignal logout in background to avoid blocking the UI thread.
        _ = OneSignalService.LogoutAsync();
        App.ResetAuthenticationCheck();

        await this.ShowPopupAsync(popup);
    }
    private async void Button_Clicked_2(object sender, EventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_EDIT_PASSWORD);
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_EDIT_PASSWORD);
    }

   


}
using loukupm.ViewModel;

namespace loukupm.View;

public partial class EditeUserPage : ContentPage
{
	public EditeUserPage()
	{
		InitializeComponent();
	    this.BindingContext= new AppViewModel();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
		await Navigation.PopAsync();	
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        try
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
        // iOS Photos
        status = await Permissions.CheckStatusAsync<Permissions.Photos>();
        if (status != PermissionStatus.Granted)
            status = await Permissions.RequestAsync<Permissions.Photos>();
#endif

            // 🔹 تحقق من النتيجة
            if (status != PermissionStatus.Granted)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "تنبيه",
                    "يجب منح إذن الوصول للصور للمتابعة",
                    "حسناً"
                );
                return;
            }

            // ✅ إذا وافق المستخدم
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "اختر صورة من المعرض"
            });

            if (result != null)
            {
                var filePath = result.FullPath;
                ((AppViewModel)BindingContext).ImageUser = filePath;
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("خطأ", ex.Message, "حسناً");
        }
    }





}
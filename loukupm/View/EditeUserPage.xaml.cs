// EditeUserPage.xaml.cs - الكود المحسّن النهائي

using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using loukupm.Services;
using loukupm.ViewModel;

namespace loukupm.View;

public partial class EditeUserPage : ContentPage
{
    public EditeUserPage()
    {
        InitializeComponent();
        this.BindingContext = AppViewModel.Instance;
    }

    /// <summary>
    /// معالج زر العودة - يستخدم نظام الملاحة المركزي
    /// يتبع القاعدة: صفحات تدفق الملف الشخصي → //ProfilePage مباشرة
    /// </summary>
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

    /// <summary>
    /// زر العودة للصفحة السابقة
    /// </summary>
    private async void Button_Clicked(object sender, EventArgs e)
    {
       await NavigationService.NavigateToTabBarPage(NavigationService.ROUTE_PROFILE);
    }

    /// <summary>
    /// معالج زر تغيير الصورة - يطلب الأذن ثم يختار صورة
    /// ✅ يطلب أذن الوصول للصور
    /// ✅ يفتح المعرج
    /// ✅ يحدّث الصورة في الحساب
    /// </summary>
    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        try
        {
            // 🔹 الخطوة 1: طلب الأذن
            bool hasPermission = await RequestPhotoPermissionAsync();

            if (!hasPermission)
            {
                await ShowPermissionDeniedAlert();
                return;
            }

            // ✅ الخطوة 2: اختيار الصورة
            await PickAndSetPhotoAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("خطأ", $"حدث خطأ: {ex.Message}", "حسناً");
        }
    }

    /// <summary>
    /// طلب أذن الوصول للصور
    /// ✅ يدعم Android 13+ (Permissions.Media)
    /// ✅ يدعم Android 12 وأقل (Permissions.StorageRead)
    /// ✅ يدعم iOS (Permissions.Photos)
    /// </summary>
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

    /// <summary>
    /// اختيار صورة من المعرج وتحديث الملف الشخصي
    /// ✅ تخزين مسار الصورة المختارة
    /// ✅ تحديث Avatar لعرضها مباشرة
    /// ✅ معالجة شاملة للأخطاء
    /// </summary>
    private async Task PickAndSetPhotoAsync()
    {
        try
        {
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "اختر صورة من المعرج"
            });

            if (result != null)
            {
                var viewModel = BindingContext as AppViewModel;
                if (viewModel != null)
                {
                    // 🔹 تخزين مسار الصورة المختارة مؤقتًا
                    viewModel.SelectedImagePath = result.FullPath;

                    // 🎨 تحديث Avatar لعرض الصورة مباشرة
                    viewModel.Avatar = result.FullPath;

                    Console.WriteLine($"📸 Image selected: {result.FullPath}");
                    await Toast.Make(
                        "تم تحميل الصورة بنجاح",
                        ToastDuration.Short
                    ).Show();
                }
            }
        }
        catch (OperationCanceledException)
        {
            await Toast.Make("تم الإلغاء", ToastDuration.Short).Show();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error selecting image: {ex.Message}");
            await DisplayAlert("خطأ", $"فشل تحميل الصورة: {ex.Message}", "حسناً");
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
        OneSignalService.Logout();

        // مسح خريطة التنقل قبل حذف الحساب
        //NavigationService.ClearPageSourceMap();

        // Reset authentication check flag
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
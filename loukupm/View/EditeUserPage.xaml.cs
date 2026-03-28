// EditeUserPage.xaml.cs - الكود المحسّن النهائي

using loukupm.ViewModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace loukupm.View;

public partial class EditeUserPage : ContentPage
{
    public EditeUserPage()
    {
        InitializeComponent();
        this.BindingContext = AppViewModel.Instance;
    }

    /// <summary>
    /// زر العودة للصفحة السابقة
    /// </summary>
    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
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

    /// <summary>
    /// عرض رسالة عند رفض المستخدم للأذن
    /// ✅ خيار فتح الإعدادات
    /// ✅ السماح للمستخدم بمنح الأذن يدويًا
    /// </summary>
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
}
// 📝 ملف مرجعي: كود تطبيقي كامل للتعديلات

namespace loukupm.ViewModel
{
    public partial class AppViewModel : ObservableObject
    {
        // ==================== الخصائص الجديدة ====================

        /// <summary>
        /// متغير مؤقت لتخزين مسار الصورة المختارة
        /// يتم مسحه بعد التحديث الناجح
        /// </summary>
        [ObservableProperty] private string? selectedImagePath;

        // ==================== الدوال المحدثة ====================

        /// <summary>
        /// تحميل بيانات المستخدم الحالي من السيرفر
        /// ✅ معالجة الأخطاء
        /// ✅ إدارة حالة التحميل
        /// ✅ Logging شامل
        /// </summary>
        private async Task LoadUser()
        {
            IsLoadUser = true;
            try
            {
                currentUser = await _apiServices.GetUserAsync();
                if (currentUser != null)
                {
                    UserName = currentUser.UserName;
                    Email = currentUser.Email;
                    FullName = currentUser.FullName;
                    ImageUser = currentUser.ProfileImageUrl ?? "default_avatar.png";
                    Console.WriteLine($"✅ User loaded: {UserName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading user: {ex.Message}");
            }
            finally
            {
                IsLoadUser = false;
            }
        }

        /// <summary>
        /// تحديث بيانات المستخدم مع رفع الصورة
        /// 
        /// العملية:
        /// 1. التحقق من البيانات المطلوبة
        /// 2. إنشاء MultipartFormDataContent
        /// 3. إضافة اسم المستخدم
        /// 4. إضافة الصورة إذا تم تغييرها
        /// 5. إرسال POST request
        /// 6. إعادة تحميل البيانات عند النجاح
        /// 7. عرض رسالة نجاح/فشل
        /// 
        /// ✅ استخدام MultipartFormDataContent
        /// ✅ إعادة تحميل البيانات بعد النجاح
        /// ✅ إدارة حالة التحميل
        /// ✅ معالجة الأخطاء الشاملة
        /// </summary>
        private async Task UpdateUserInfo()
        {
            try
            {
                IsLoadUser = true;
                Console.WriteLine("🔄 Starting user update...");

                // التحقق من البيانات المطلوبة
                if (string.IsNullOrWhiteSpace(UserName))
                {
                    await Toast.Make("يرجى إدخال الاسم", ToastDuration.Short).Show();
                    IsLoadUser = false;
                    return;
                }

                // إعداد البيانات للإرسال
                using (var content = new MultipartFormDataContent())
                {
                    // ✅ إضافة اسم المستخدم
                    content.Add(new StringContent(UserName, Encoding.UTF8), "name");

                    // ✅ إضافة الصورة إذا تم تغييرها
                    if (!string.IsNullOrWhiteSpace(SelectedImagePath) && File.Exists(SelectedImagePath))
                    {
                        Console.WriteLine($"📸 Adding image: {SelectedImagePath}");
                        var fileContent = new StreamContent(File.OpenRead(SelectedImagePath));
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                        content.Add(fileContent, "profile_image", Path.GetFileName(SelectedImagePath));
                    }

                    // 🌐 إرسال الطلب إلى API
                    await SetAuthorizationHeaderAsync();
                    string url = "https://test.center-yazan.com/api/users/profile/update";

                    Console.WriteLine($"📡 Sending request to: {url}");
                    var response = await _httpClient.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"✅ User updated successfully");

                        // 🔄 إعادة تحميل بيانات المستخدم من السيرفر
                        await LoadUser();

                        // ✨ مسح الصورة المختارة مؤقتًا
                        SelectedImagePath = null;

                        // 🎉 عرض رسالة النجاح
                        await Toast.Make("تم تحديث البيانات بنجاح", ToastDuration.Short).Show();
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"❌ Update failed: {response.StatusCode} - {errorContent}");
                        await Toast.Make("فشل تحديث البيانات. الرجاء المحاولة مجددًا", ToastDuration.Short).Show();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception during user update: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                await Toast.Make($"حدث خطأ: {ex.Message}", ToastDuration.Short).Show();
            }
            finally
            {
                IsLoadUser = false;
            }
        }
    }
}

// ==================== في EditeUserPage.xaml.cs ====================

namespace loukupm.View
{
    public partial class EditeUserPage : ContentPage
    {
        public EditeUserPage()
        {
            InitializeComponent();
            // ✅ استخدام Singleton pattern
            this.BindingContext = AppViewModel.Instance;
        }

        /// <summary>
        /// معالج زر اختيار الصورة
        /// 1. طلب الأذن
        /// 2. فتح معرج الصور
        /// 3. تخزين مسار الصورة مؤقتًا
        /// 4. تحديث Avatar لعرضها فوراً
        /// </summary>
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
                await DisplayAlert("خطأ", $"حدث خطأ: {ex.Message}", "حسناً");
            }
        }

        /// <summary>
        /// اختيار صورة من المعرج
        /// ✅ تخزين مسار الصورة المختارة
        /// ✅ تحديث Avatar للعرض الفوري
        /// ✅ Logging كامل
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

        private async Task<bool> RequestPhotoPermissionAsync()
        {
            PermissionStatus status = PermissionStatus.Unknown;

#if ANDROID
            if (DeviceInfo.Version.Major >= 13)
            {
                status = await Permissions.CheckStatusAsync<Permissions.Media>();
                if (status != PermissionStatus.Granted)
                    status = await Permissions.RequestAsync<Permissions.Media>();
            }
            else
            {
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
}

// ==================== في EditeUserPage.xaml ====================

/*
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="loukupm.View.EditeUserPage"
             Shell.NavBarIsVisible="False"
             Shell.TabBarIsVisible="False"
             FlowDirection="LeftToRight"
             xmlns:Loc="clr-namespace:loukupm.Langue"
             xmlns:material="http://schemas.enisn-projects.io/dotnet/maui/uraniumui/material"
             xmlns:converters="clr-namespace:loukupm.Converter"
             BackgroundColor="#252525"
             Title="EditeUserPage">

    <ContentPage.Resources>
        <ResourceDictionary>
            <converters:InverseBoolConverter x:Key="InverseBoolConverter" />
        </ResourceDictionary>
    </ContentPage.Resources>

    <!-- ... UI Elements ... -->

    <!-- الزر مع Binding للأمر والحالة -->
    <Button 
        Command="{Binding UpdateUserCommand}"
        IsEnabled="{Binding IsLoadUser, Converter={StaticResource InverseBoolConverter}}"
        Text="حفظ التغييرات" 
        FontSize="22" 
        HeightRequest="50" />

    <!-- زر اختيار الصورة معطل أثناء التحميل -->
    <Button 
        Text="تغيير الصورة"
        IsEnabled="{Binding IsLoadUser, Converter={StaticResource InverseBoolConverter}}"
        Clicked="Button_Clicked_1" />

    <!-- مؤشر التحميل -->
    <ActivityIndicator 
        IsRunning="{Binding IsLoadUser}" 
        IsVisible="{Binding IsLoadUser}" />
</ContentPage>
*/

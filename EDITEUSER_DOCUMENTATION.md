# 📋 توثيق تحديثات نظام تعديل بيانات المستخدم

## 🎯 ملخص التحديثات

تم تطوير نظام شامل لتحديث بيانات المستخدم مع دعم رفع الصور باستخدام **MultipartFormDataContent**، مع إعادة تحميل البيانات الفورية وإدارة حالة التحميل بشكل احترافي.

---

## ✨ المميزات الرئيسية

### 1️⃣ **تحديث بيانات المستخدم مع رفع الصور**

```csharp
// في UpdateUserInfo():
using (var content = new MultipartFormDataContent())
{
    // ✅ إضافة اسم المستخدم
    content.Add(new StringContent(UserName, Encoding.UTF8), "name");

    // ✅ إضافة الصورة إذا تم تغييرها
    if (!string.IsNullOrWhiteSpace(SelectedImagePath) && File.Exists(SelectedImagePath))
    {
        var fileContent = new StreamContent(File.OpenRead(SelectedImagePath));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "profile_image", Path.GetFileName(SelectedImagePath));
    }

    // 🌐 إرسال الطلب إلى API
    var response = await _httpClient.PostAsync(url, content);
}
```

### 2️⃣ **إدارة الصورة المختارة**

#### خاصية جديدة:
```csharp
[ObservableProperty] private string? selectedImagePath;
```

#### عند اختيار صورة:
```csharp
private async Task PickAndSetPhotoAsync()
{
    var result = await MediaPicker.PickPhotoAsync(...);

    if (result != null)
    {
        // 🔹 تخزين مسار الصورة المختارة مؤقتًا
        viewModel.SelectedImagePath = result.FullPath;

        // 🎨 تحديث Avatar لعرض الصورة مباشرة
        viewModel.Avatar = result.FullPath;
    }
}
```

### 3️⃣ **إعادة تحميل البيانات بعد التحديث**

```csharp
private async Task UpdateUserInfo()
{
    try
    {
        // ... إرسال البيانات ...

        if (response.IsSuccessStatusCode)
        {
            // 🔄 إعادة تحميل بيانات المستخدم من السيرفر
            await LoadUser();

            // ✨ مسح الصورة المختارة مؤقتًا
            SelectedImagePath = null;
        }
    }
    finally
    {
        IsLoadUser = false;
    }
}
```

### 4️⃣ **إدارة حالة التحميل**

#### في XAML:
```xaml
<!-- الزر معطل أثناء التحميل -->
<Button Command="{Binding UpdateUserCommand}" 
        IsEnabled="{Binding IsLoadUser, Converter={StaticResource InverseBoolConverter}}"
        Text="حفظ التغييرات" />

<!-- ActivityIndicator يعمل أثناء التحميل -->
<ActivityIndicator IsRunning="{Binding IsLoadUser}" 
                   IsVisible="{Binding IsLoadUser}" />
```

#### في ViewModel:
```csharp
private async Task UpdateUserInfo()
{
    try
    {
        IsLoadUser = true;  // ✅ تفعيل الحالة
        // ... العملية ...
    }
    finally
    {
        IsLoadUser = false;  // ✅ إيقاف الحالة
    }
}
```

---

## 📁 الملفات المعدلة

### 1. **AppViewModel.cs**

#### الخصائص المضافة:
```csharp
[ObservableProperty] private string? selectedImagePath;
```

#### الدوال المحدثة:
- `LoadUser()` - إضافة معالجة الأخطاء والـ Logging
- `UpdateUserInfo()` - إعادة كتابة شاملة باستخدام `MultipartFormDataContent`

#### الـ Commands:
```csharp
UpdateUserCommand = new AsyncRelayCommand(UpdateUserInfo);
```

### 2. **EditeUserPage.xaml**

#### إضافة Resources:
```xaml
<ContentPage.Resources>
    <ResourceDictionary>
        <converters:InverseBoolConverter x:Key="InverseBoolConverter" />
    </ResourceDictionary>
</ContentPage.Resources>
```

#### تحديث الأزرار:
```xaml
<!-- جميع الأزرار معطلة أثناء التحميل -->
<Button IsEnabled="{Binding IsLoadUser, Converter={StaticResource InverseBoolConverter}}" />
```

### 3. **EditeUserPage.xaml.cs**

#### تحديث BindingContext:
```csharp
this.BindingContext = AppViewModel.Instance;  // ✅ Singleton pattern
```

#### تحديث PickAndSetPhotoAsync():
```csharp
private async Task PickAndSetPhotoAsync()
{
    var result = await MediaPicker.PickPhotoAsync(...);

    if (result != null)
    {
        // 🔹 تخزين مسار الصورة المختارة
        viewModel.SelectedImagePath = result.FullPath;

        // 🎨 تحديث Avatar لعرضها مباشرة
        viewModel.Avatar = result.FullPath;
    }
}
```

---

## 🔄 سير العملية الكاملة

```
1. المستخدم يضغط على "تغيير الصورة"
   ↓
2. اختيار صورة من المعرج
   ↓
3. تحديث SelectedImagePath و Avatar فوراً (معاينة)
   ↓
4. المستخدم يعدل الاسم إذا أراد
   ↓
5. المستخدم يضغط على "حفظ التغييرات"
   ↓
6. IsLoadUser = true (تعطيل الأزرار، عرض Spinner)
   ↓
7. إنشاء MultipartFormDataContent بالاسم والصورة
   ↓
8. إرسال POST request إلى API
   ↓
9. إذا نجح:
   - استدعاء LoadUser() لإعادة تحميل البيانات
   - مسح SelectedImagePath
   - عرض رسالة نجاح
   ↓
10. IsLoadUser = false (تفعيل الأزرار)
```

---

## 🛡️ معالجة الأخطاء

```csharp
private async Task UpdateUserInfo()
{
    try
    {
        // ✅ التحقق من البيانات المطلوبة
        if (string.IsNullOrWhiteSpace(UserName))
        {
            await Toast.Make("يرجى إدخال الاسم", ToastDuration.Short).Show();
            return;
        }

        // ... العملية ...
    }
    catch (Exception ex)
    {
        // ❌ عرض رسالة خطأ واضحة
        Console.WriteLine($"❌ Exception: {ex.Message}");
        await Toast.Make($"حدث خطأ: {ex.Message}", ToastDuration.Short).Show();
    }
    finally
    {
        IsLoadUser = false;  // ✅ التأكد من إيقاف حالة التحميل
    }
}
```

---

## 📡 API Integration

### Endpoint:
```
POST https://test.center-yazan.com/api/users/profile/update
```

### Request Headers:
```
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

### Request Body:
```
name: "اسم المستخدم"
profile_image: [ملف الصورة] (اختياري)
```

### Response:
```json
{
  "success": true,
  "message": "تم تحديث البيانات بنجاح",
  "data": {
    "id": 26,
    "name": "أحمد محمد",
    "profile_image_url": "https://...",
    ...
  }
}
```

---

## ✅ قائمة التحقق

- ✅ تحديث UserName و Avatar معاً
- ✅ رفع الصورة باستخدام MultipartFormDataContent
- ✅ إرسال الصورة فقط عند تغييرها
- ✅ إعادة تحميل البيانات بعد النجاح
- ✅ إدارة حالة التحميل (IsLoadUser)
- ✅ تعطيل الأزرار أثناء التحميل
- ✅ عرض رسائل نجاح وفشل
- ✅ معالجة شاملة للأخطاء
- ✅ Singleton pattern للـ ViewModel
- ✅ معاينة الصورة فوراً عند الاختيار

---

## 🚀 كيفية الاستخدام

### 1. من الصفحة:
```xaml
<Button Command="{Binding UpdateUserCommand}" 
        Text="حفظ التغييرات" />
```

### 2. من البرنامج:
```csharp
var viewModel = AppViewModel.Instance;
viewModel.UserName = "أحمد";
viewModel.SelectedImagePath = "/path/to/image.jpg";
await viewModel.UpdateUserCommand.ExecuteAsync(null);
```

---

## 📝 ملاحظات مهمة

1. **Singleton Pattern**: استخدام `AppViewModel.Instance` يضمن توحيد البيانات عبر الصفحات
2. **MultipartFormDataContent**: يدعم رفع الملفات بشكل آمن وفعال
3. **تخزين مسار الصورة**: `SelectedImagePath` يُمسح بعد التحديث الناجح
4. **معاينة فورية**: تحديث `Avatar` يسمح بعرض الصورة قبل الحفظ
5. **إدارة حالة القفل**: `IsLoadUser` يضمن عدم الضغط على الزر أثناء التحميل

---

## 🔗 المراجع

- [MAUI MultipartFormDataContent](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.multipartformdatacontent)
- [MVVM Toolkit - ObservableProperty](https://learn.microsoft.com/en-us/windows/communitytoolkit/mvvm/)
- [MAUI MediaPicker](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/file-picker)

---

**آخر تحديث**: 2025-03-26
**الحالة**: ✅ جاهز للإنتاج

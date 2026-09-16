# 🚀 مرجع سريع للمطورين

## 📍 موقع الملفات المعدلة

```
loukupm/
├── ViewModel/
│   └── AppViweModel.cs          ← ✅ معدل
├── View/
│   ├── EditeUserPage.xaml       ← ✅ معدل
│   └── EditeUserPage.xaml.cs    ← ✅ معدل
```

---

## 🔑 الكلمات المفتاحية الجديدة

| الكلمة | الموقع | الوصف |
|------|--------|-------|
| `selectedImagePath` | ViewModel | متغير مؤقت للصورة |
| `SelectedImagePath` | ViewModel | خاصية Binding |
| `MultipartFormDataContent` | UpdateUserInfo | رفع الملفات |
| `InverseBoolConverter` | XAML | تعطيل الأزرار |
| `IsEnabled` | XAML | حالة الزر |
| `AsyncRelayCommand` | Constructor | أمر async |

---

## 📱 أماكن الـ Binding

### في XAML:
```xaml
<!-- الاسم -->
<material:TextField Text="{Binding UserName}" ... />

<!-- الصورة -->
<Image Source="{Binding Avatar}" ... />

<!-- مؤشر التحميل -->
<ActivityIndicator IsRunning="{Binding IsLoadUser}" ... />

<!-- الأزرار -->
<Button Command="{Binding UpdateUserCommand}" 
        IsEnabled="{Binding IsLoadUser, Converter={...}}" />
```

### في ViewModel:
```csharp
[ObservableProperty] private string userName;
[ObservableProperty] private string imageUser;
[ObservableProperty] private bool isLoadUser;
[ObservableProperty] private string selectedImagePath;

public string Avatar 
{ 
    get => ImageUser; 
    set => ImageUser = value; 
}
```

---

## 🔄 سير البيانات

```
المستخدم
    ↓
UIEvent (Clicked / Changed)
    ↓
Command / EventHandler
    ↓
ViewModel Method
    ↓
API Request (POST MultipartFormData)
    ↓
Server
    ↓
Response (Success/Error)
    ↓
UpdateUI (Toast, Reload)
    ↓
Display Updated Data
```

---

## 🧠 الكود الحساس

### 1. إنشاء MultipartFormDataContent:
```csharp
using (var content = new MultipartFormDataContent())
{
    content.Add(new StringContent(UserName, Encoding.UTF8), "name");

    if (!string.IsNullOrWhiteSpace(SelectedImagePath) && File.Exists(SelectedImagePath))
    {
        var fileContent = new StreamContent(File.OpenRead(SelectedImagePath));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "profile_image", Path.GetFileName(SelectedImagePath));
    }

    var response = await _httpClient.PostAsync(url, content);
}
```

### 2. إعادة التحميل:
```csharp
if (response.IsSuccessStatusCode)
{
    await LoadUser();  // ✅ مهم جداً
    SelectedImagePath = null;  // مسح المؤقت
}
```

### 3. إدارة الحالة:
```csharp
try
{
    IsLoadUser = true;  // تفعيل
    // ... العملية ...
}
finally
{
    IsLoadUser = false;  // إيقاف
}
```

---

## 🐛 Debugging Tips

### 1. في Console:
```csharp
Console.WriteLine($"🔍 Debug: {variable}");
Debug.WriteLine($"📝 Trace: {message}");
```

### 2. في XAML:
```xaml
<!-- اختبر الـ Binding -->
<Label Text="{Binding UserName}" />
<Label Text="{Binding IsLoadUser}" />
<Label Text="{Binding SelectedImagePath}" />
```

### 3. في Breakpoint:
```csharp
#if DEBUG
    System.Diagnostics.Debugger.Break();
#endif
```

---

## ⚡ الأوامر المهمة

### تشغيل:
```powershell
dotnet maui run -f net10.0-android
dotnet maui run -f net10.0-ios
```

### بناء:
```powershell
dotnet build
dotnet clean && dotnet build
```

### تنظيف:
```powershell
dotnet clean
rm -r bin obj
```

---

## 📊 النقاط الحرجة

| النقطة | الأهمية | ملاحظة |
|------|--------|-------|
| MultipartFormDataContent | ⭐⭐⭐ | بدون هذا لن تُرفع الصور |
| LoadUser() بعد النجاح | ⭐⭐⭐ | بدونها البيانات لن تُحدث |
| IsLoadUser = false finally | ⭐⭐⭐ | يجب تنفيذها دائماً |
| SelectedImagePath = null | ⭐⭐ | لتنظيف المؤقت |
| Try-Catch شامل | ⭐⭐⭐ | لمعالجة الأخطاء |

---

## 🧪 اختبار سريع

```csharp
// 1. التحقق من الـ Binding
Assert.IsNotNull(viewModel.UpdateUserCommand);
Assert.IsTrue(viewModel.UpdateUserCommand is AsyncRelayCommand);

// 2. التحقق من الخصائص
viewModel.UserName = "Test";
Assert.AreEqual("Test", viewModel.UserName);

// 3. التحقق من الحالة
viewModel.IsLoadUser = true;
Assert.IsTrue(viewModel.IsLoadUser);
```

---

## 📋 قائمة مراجعة سريعة

قبل الإطلاق:
- [ ] البناء ناجح
- [ ] لا توجد تحذيرات
- [ ] الاختبارات نجحت
- [ ] الـ Logging واضح
- [ ] معالجة الأخطاء تعمل
- [ ] رسائل النجاح تظهر
- [ ] الصور تُرفع بنجاح
- [ ] البيانات تُحدث صحيح
- [ ] الأزرار تتعطل/تفعل صحيح
- [ ] التطبيق يعمل على جميع الأجهزة

---

## 🎯 الهدف النهائي

```
✅ ViewModel يدير البيانات
✅ Commands تنفذ العمليات
✅ API يتصل بالسيرفر
✅ UI تعرض النتائج
✅ Users سعداء 😊
```

---

## 📚 المراجع السريعة

- [MAUI Docs](https://learn.microsoft.com/en-us/dotnet/maui/)
- [MVVM Toolkit](https://learn.microsoft.com/en-us/windows/communitytoolkit/mvvm/)
- [AsyncRelayCommand](https://learn.microsoft.com/en-us/windows/communitytoolkit/mvvm/observableobject)
- [HttpClient](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient)

---

**استخدم هذا الملف كمرجع سريع أثناء التطوير!** 🚀

*آخر تحديث: 2025-03-26*

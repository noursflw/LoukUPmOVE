# 🚀 دليل سريع - ملخص المتطلبات والحلول

## 📌 المتطلبات الأصلية ✅ جميعها مُنفذة

### 1️⃣ تعديل UpdateUserCommand ✅
- ✅ تنفيذ API باستخدام `POST`
- ✅ إرسال البيانات باستخدام `MultipartFormDataContent`
- ✅ إرسال `UserName`
- ✅ إرسال `Avatar` فقط إذا تم تغييره

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
}
```

---

### 2️⃣ إدارة الصورة ✅
- ✅ اختيار صورة من الجهاز
- ✅ تخزينها مؤقتًا في `SelectedImagePath`
- ✅ تحديث `Avatar` لعرضها مباشرة

```csharp
viewModel.SelectedImagePath = result.FullPath;  // التخزين المؤقت
viewModel.Avatar = result.FullPath;             // المعاينة الفورية
```

---

### 3️⃣ إعادة تحميل البيانات بعد التحديث ✅
- ✅ استدعاء API لجلب بيانات المستخدم
- ✅ تحديث `UserName`
- ✅ تحديث `Avatar`

```csharp
if (response.IsSuccessStatusCode)
{
    await LoadUser();  // ✅ إعادة تحميل
    SelectedImagePath = null;  // مسح المؤقت
}
```

---

### 4️⃣ إدارة حالة التحميل ✅
- ✅ `IsLoadUser = true` أثناء الرفع والجلب
- ✅ `IsLoadUser = false` بعد الانتهاء

```csharp
try
{
    IsLoadUser = true;  // ✅ تفعيل
    // ... العملية ...
}
finally
{
    IsLoadUser = false;  // ✅ إيقاف
}
```

---

### 5️⃣ تحسين الـ Binding ✅
- ✅ جميع الخصائص تستخدم `[ObservableProperty]`
- ✅ الزر معطل أثناء التحميل: `IsEnabled="{Binding IsLoadUser, Converter={StaticResource InverseBoolConverter}}"`

```xaml
<Button IsEnabled="{Binding IsLoadUser, Converter={StaticResource InverseBoolConverter}}" />
```

---

### 6️⃣ تجربة المستخدم ✅
- ✅ رسالة نجاح عند التحديث: `"تم تحديث البيانات بنجاح"`
- ✅ رسالة خطأ عند الفشل: `"فشل تحديث البيانات"`

```csharp
await Toast.Make("تم تحديث البيانات بنجاح", ToastDuration.Short).Show();
// أو
await Toast.Make("فشل تحديث البيانات", ToastDuration.Short).Show();
```

---

## 📂 الملفات المعدلة

| الملف | التعديلات |
|------|----------|
| `loukupm/ViewModel/AppViweModel.cs` | أضيفت خاصية `selectedImagePath` وأعادة كتابة `UpdateUserInfo()` |
| `loukupm/View/EditeUserPage.xaml` | أضيفت Converter و `IsEnabled` للأزرار |
| `loukupm/View/EditeUserPage.xaml.cs` | استخدام Singleton و تحديث `PickAndSetPhotoAsync()` |

---

## 🎯 الأوامر الرئيسية

### الأمر الرئيسي:
```csharp
UpdateUserCommand = new AsyncRelayCommand(UpdateUserInfo);
```

### الاستدعاء من XAML:
```xaml
<Button Command="{Binding UpdateUserCommand}" 
        Text="حفظ التغييرات" />
```

---

## 🔗 API Integration

```
📡 Endpoint: POST https://test.center-yazan.com/api/users/profile/update

📤 Request:
   - name: "اسم المستخدم" (نصي)
   - profile_image: ملف الصورة (اختياري)

📥 Response:
   {
     "success": true,
     "message": "تم تحديث البيانات بنجاح",
     "data": { ... }
   }
```

---

## 🔍 Debugging

### Logging يمكنك مراقبته:
```csharp
🔄 Starting user update...
📸 Adding image: /path/to/image.jpg
📡 Sending request to: https://...
✅ User updated successfully
✅ User loaded: محمد أحمد
```

### في Console:
```csharp
Debug > Windows > Output
// أو
View > Debug Output
```

---

## ⚙️ الإعدادات الأساسية

### بيانات المستخدم:
```csharp
[ObservableProperty] private string userName;
[ObservableProperty] private string imageUser;
[ObservableProperty] private string selectedImagePath;  // ✅ جديد
```

### Avatar property (للتوافق):
```csharp
public string Avatar 
{ 
    get => ImageUser; 
    set => ImageUser = value; 
}
```

---

## 🧪 اختبار سريع

```
1. ✅ فتح الصفحة → بيانات تحمّل
2. ✅ تغيير الصورة → معاينة فورية
3. ✅ تعديل الاسم → يكتب الاسم
4. ✅ حفظ → الأزرار تتعطّل
5. ✅ النجاح → رسالة "تم التحديث"
6. ✅ البيانات تحدث من السيرفر
```

---

## 📊 حالات الاستخدام

### ✅ حالة 1: تحديث الاسم فقط
```
- لا تختر صورة
- عدّل الاسم فقط
- انتظر → اسم يتحدث
```

### ✅ حالة 2: تحديث الصورة فقط
```
- اختر صورة جديدة
- لا تعدّل الاسم
- انتظر → صورة تتحدث
```

### ✅ حالة 3: تحديث الاثنين معاً
```
- اختر صورة جديدة
- عدّل الاسم
- انتظر → الاثنان يتحدثان
```

---

## 🎓 معلومات مهمة

### MultipartFormDataContent:
```csharp
// لرفع الملفات مع البيانات
using (var content = new MultipartFormDataContent())
{
    // نصوص
    content.Add(new StringContent("value"), "field_name");

    // ملفات
    content.Add(new StreamContent(stream), "file_field", "filename.jpg");
}
```

### Singleton Pattern:
```csharp
// واحد في كل مكان
BindingContext = AppViewModel.Instance;
```

### Toast Notifications:
```csharp
await Toast.Make("الرسالة", ToastDuration.Short).Show();
```

---

## ✅ النتيجة النهائية

| المتطلب | الحالة |
|--------|-------|
| تعديل UpdateUserCommand | ✅ مُنفذ |
| إدارة الصورة | ✅ مُنفذ |
| إعادة تحميل البيانات | ✅ مُنفذ |
| إدارة حالة التحميل | ✅ مُنفذ |
| تحسين الـ Binding | ✅ مُنفذ |
| تجربة المستخدم | ✅ مُنفذ |
| **البناء** | ✅ **ناجح** |

---

## 📞 أسئلة شائعة

**س: لماذا استخدام Singleton؟**
ج: للبيانات الموحدة عبر جميع الصفحات

**س: متى تُمسح `SelectedImagePath`؟**
ج: بعد التحديث الناجح فقط

**س: هل يمكن رفع صورة بدون اسم؟**
ج: لا، الاسم مطلوب (Validation)

**س: كم الحد الأقصى لحجم الصورة؟**
ج: يعتمد على السيرفر (تحقق من API docs)

---

**الحالة**: ✅ مُكتمل وجاهز
**التاريخ**: 2025-03-26
**النسخة**: 1.0.0

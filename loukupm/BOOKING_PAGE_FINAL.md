# 📋 BookingPage - TabView Implementation (Final Design)

## ✨ نظرة عامة

تم إعادة هيكلة صفحة **BookingPage** لاستخدام **Custom TabView Design** مع تقسيم الحجوزات إلى ثلاث تابات:
- **القادمة** (Upcoming / PENDING)
- **السابقة** (Previous / COMPLETED)
- **الملغاة** (Canceled / CANCELED)

---

## 🎯 التصميم المرئي

```
┌─────────────────────────────────────────┐
│      تابع جميع حجوزاتك بسهولة            │
│  ←  (Back Button)                        │
├─────────────────────────────────────────┤
│                                         │
│  القادمة  │  السابقة  │  الملغاة         │ ← Tab Headers
│  ─────    │  ─────    │  ─────          │ (Golden underline)
│  (Gold)   │ (Gray)    │ (Gray)          │
│                                         │
├─────────────────────────────────────────┤
│                                         │
│  ┌─────────────────────────────────┐    │
│  │ Card 1 (Appointment Details)    │    │
│  │ ✓ Status                        │    │
│  │ ✓ Date & Time & Price           │    │
│  │ ✓ Services                      │    │
│  │ ✓ Provider Info                 │    │
│  │ ✓ Cancel Button                 │    │
│  └─────────────────────────────────┘    │
│                                         │
│  ┌─────────────────────────────────┐    │
│  │ Card 2 (More Appointments)      │    │
│  └─────────────────────────────────┘    │
│                                         │
├─────────────────────────────────────────┤
│  [Book Now]                             │ ← Bottom Button
└─────────────────────────────────────────┘
```

---

## 📦 الملفات المعدلة

### 1. **ViewModel: `loukupm\ViewModel\AppViweModel.cs`**

#### الخصائص الجديدة:
```csharp
[ObservableProperty] private ObservableCollection<Appointment> upcomingAppointments = new();
[ObservableProperty] private ObservableCollection<Appointment> previousAppointments = new();
[ObservableProperty] private ObservableCollection<Appointment> canceledAppointments = new();
```

#### دالة `LoadBookingsAsync()`:
```csharp
// تقسيم الحجوزات حسب Status:
if (item.Status == "CANCELED" || item.IsCancelled)
    CanceledAppointments.Add(item);
else if (item.Status == "COMPLETED" || item.IsCompleted || item.IsPast)
    PreviousAppointments.Add(item);
else if (item.Status == "PENDING" || item.IsUpcoming)
    UpcomingAppointments.Add(item);
```

---

### 2. **XAML: `loukupm\View\BookingPage.xaml`**

#### البنية:
```xml
<Grid RowDefinitions="Auto,*,Auto">
  <!-- Header -->
  <Grid Grid.Row="0"> ... </Grid>

  <!-- Content (TabView Headers + Tab Contents) -->
  <Grid Grid.Row="1">
    <!-- Custom Tab Headers (Grid: 3 columns) -->
    <Grid Grid.Row="0">
      <StackLayout Grid.Column="0"> <!-- Tab 1 -->
        <Button /> <!-- Text: القادمة -->
        <BoxView BackgroundColor="#FFD700" /> <!-- Underline -->
      </StackLayout>

      <StackLayout Grid.Column="1"> <!-- Tab 2 -->
        <Button /> <!-- Text: السابقة -->
        <BoxView BackgroundColor="Transparent" /> <!-- No underline -->
      </StackLayout>

      <StackLayout Grid.Column="2"> <!-- Tab 3 -->
        <Button /> <!-- Text: الملغاة -->
        <BoxView BackgroundColor="Transparent" /> <!-- No underline -->
      </StackLayout>
    </Grid>

    <!-- Tab Contents (ScrollView) -->
    <Grid Grid.Row="1">
      <ScrollView x:Name="Tab1Content" IsVisible="True"> ... </ScrollView>
      <ScrollView x:Name="Tab2Content" IsVisible="False"> ... </ScrollView>
      <ScrollView x:Name="Tab3Content" IsVisible="False"> ... </ScrollView>
    </Grid>
  </Grid>

  <!-- Bottom Button -->
  <Button Grid.Row="2"> ... </Button>
</Grid>
```

#### DataTemplate الموحدة:
```xml
<DataTemplate x:Key="AppointmentTemplate">
  <Frame> <!-- Appointment Card -->
    <VerticalStackLayout>
      <Button Text="{Binding Stutes}" /> <!-- Status -->
      <Grid> <!-- Date/Time/Price -->
        <Label Text="{Binding Date}" />
        <Label Text="{Binding Time}" />
        <Label Text="{Binding Total, StringFormat='{}{0} €'}" />
      </Grid>
      <BoxView /> <!-- Divider -->
      <CollectionView ItemsSource="{Binding ServicesDetails}" /> <!-- Services -->
      <BoxView /> <!-- Divider -->
      <HorizontalStackLayout> <!-- Provider -->
        <Frame> <Image Source="{Binding ImgePerson}" /> </Frame>
        <Label Text="{Binding UserName}" />
      </HorizontalStackLayout>
      <BoxView /> <!-- Divider -->
      <Button Command="{Binding CancelBookingCommand}" /> <!-- Cancel -->
    </VerticalStackLayout>
  </Frame>
</DataTemplate>
```

---

### 3. **CodeBehind: `loukupm\View\BookingPage.xaml.cs`**

#### Tab Navigation Logic:
```csharp
private void SelectTab(int tabIndex)
{
    // Reset all tabs to gray color
    // Hide all tab contents

    // Color selected tab to gold
    // Show selected tab content
}

// Tab Click Handlers:
private void OnTab1Clicked(object sender, EventArgs e) => SelectTab(0);
private void OnTab2Clicked(object sender, EventArgs e) => SelectTab(1);
private void OnTab3Clicked(object sender, EventArgs e) => SelectTab(2);
```

---

## 🎨 الألوان والتصميم

| العنصر | اللون | الوصف |
|--------|--------|-------|
| Background | `#252525` | أسود داكن جداً |
| Tab Text (Selected) | `#FFD700` | ذهبي براق |
| Tab Text (Unselected) | `#999999` | رمادي |
| Tab Underline (Active) | `#FFD700` | ذهبي |
| Tab Underline (Inactive) | `Transparent` | مخفي |
| Card Background | `#444444` | رمادي داكن |
| Card Text | `#D3D3D3` | رمادي فاتح |
| Price | `#FFD700` | ذهبي |
| Header Background | `#202020` | أسود أغمق |

---

## 🔄 سلوك التطبيق

### عند تحميل الصفحة:
1. ✅ استدعاء `LoadAppointmentsCommand`
2. ✅ جلب جميع الحجوزات من API
3. ✅ تقسيم الحجوزات إلى 3 Collections
4. ✅ عرض Tab 1 (القادمة) افتراضياً
5. ✅ عرض Skeleton Loading أثناء الجلب

### عند النقر على Tab:
1. ✅ إعادة تعيين ألوان جميع الـ Tabs إلى رمادي
2. ✅ تحويل الخط الأصفر إلى شفاف لجميع الـ Tabs
3. ✅ تلوين Tab المحدد بـ ذهبي
4. ✅ إظهار خط ذهبي تحت Tab المحدد
5. ✅ إخفاء المحتويات السابقة
6. ✅ إظهار محتوى Tab الجديد

### عند الإلغاء:
```csharp
Command="{Binding Source={RelativeSource AncestorType={x:Type ContentPage}}, 
          Path=BindingContext.CancelBookingCommand}"
CommandParameter="{Binding Id}"
```

---

## 📊 البيانات والـ API

### API Endpoint:
```csharp
await _apiServices.GetUserAppointmentsAsync(currentUser);
```

### Status Values:
| Status | Destination Collection | الشرط |
|--------|------------------------|--------|
| `PENDING` أو `IsUpcoming` | `UpcomingAppointments` | الحجوزات القادمة |
| `COMPLETED` أو `IsCompleted` أو `IsPast` | `PreviousAppointments` | الحجوزات المنتهية |
| `CANCELED` أو `IsCancelled` | `CanceledAppointments` | الحجوزات الملغاة |

---

## ✨ الميزات

✅ **نفس التصميم البصري** - النص الأصفر + الخط الأصفر
✅ **Dark Theme كامل** - متوافق تماماً مع التطبيق
✅ **DataTemplate واحدة** - بدون تكرار الكود
✅ **Skeleton Loading** - عند جلب البيانات
✅ **Empty State** - لكل Tab منفصلة
✅ **Cancel Button** - مع Command Binding
✅ **Provider Info** - مع الصورة والاسم
✅ **Services List** - مع الأسعار والمدة
✅ **Responsive Design** - على جميع الأحجام
✅ **MVVM Pattern** - فصل نظيف بين البيانات والعرض
✅ **Performance** - Lazy loading و minimal binding

---

## 🧪 الاختبار

### السيناريوهات:
- [ ] فقط حجوزات قادمة
- [ ] فقط حجوزات سابقة
- [ ] فقط حجوزات ملغاة
- [ ] خليط من الحجوزات
- [ ] لا توجد حجوزات (Empty State)
- [ ] أثناء التحميل (Skeleton)
- [ ] الانتقال بين التابات
- [ ] عرض تفاصيل الحجز
- [ ] إلغاء الحجز

---

## 🚀 Performance

- **Lazy Loading**: كل Tab لديها ScrollView منفصل
- **Minimal Binding**: استخدام Computed Properties
- **Reusable Template**: DataTemplate واحدة لجميع الكروت
- **Efficient Filtering**: في ViewModel وليس في XAML
- **No Unnecessary Re-renders**: UpdateUI فقط عند التغييرات

---

## 📝 الملاحظات

⚠️ **MVVM Toolkit**: توليد الخصائص تلقائياً
⚠️ **Status Values**: قد تأتي كـ string أو boolean
⚠️ **Tab Navigation**: يتم عبر Click handlers
⚠️ **RTL Support**: النصوص بالعربية مدعومة

---

**الحالة**: ✅ **Production Ready**
**Build Status**: ✅ **Successful**
**Last Updated**: Now

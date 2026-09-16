# ⚡ Quick Reference Guide

## 🎯 نقاط مهمة

### 1. الـ Collections الثلاث في ViewModel:
```csharp
public ObservableCollection<Appointment> UpcomingAppointments { get; set; }
public ObservableCollection<Appointment> PreviousAppointments { get; set; }
public ObservableCollection<Appointment> CanceledAppointments { get; set; }
```

### 2. تقسيم البيانات في LoadBookingsAsync:
```csharp
if (item.Status == "CANCELED" || item.IsCancelled)
    CanceledAppointments.Add(item);
else if (item.Status == "COMPLETED" || item.IsCompleted || item.IsPast)
    PreviousAppointments.Add(item);
else // PENDING or IsUpcoming
    UpcomingAppointments.Add(item);
```

### 3. Tab Navigation في CodeBehind:
```csharp
private void SelectTab(int tabIndex)
{
    // 1. Reset all to gray
    tab1Button.TextColor = Gray;
    tab2Button.TextColor = Gray;
    tab3Button.TextColor = Gray;

    // 2. Hide all content
    Tab1Content.IsVisible = false;
    Tab2Content.IsVisible = false;
    Tab3Content.IsVisible = false;

    // 3. Highlight selected
    if (tabIndex == 0)
    {
        tab1Button.TextColor = Gold;
        Tab1Content.IsVisible = true;
    }
    // ... similar for other tabs
}
```

---

## 🎨 الألوان

| الاستخدام | اللون | Code |
|-----------|-------|------|
| Tab Selected Text | Gold | #FFD700 |
| Tab Unselected Text | Gray | #999999 |
| Tab Underline (Active) | Gold | #FFD700 |
| Card Background | Dark Gray | #444444 |
| Main Background | Black | #252525 |
| Header Background | Darker Black | #202020 |
| Price Text | Gold | #FFD700 |
| Regular Text | Light Gray | #D3D3D3 |

---

## 📱 XAML Structure

```xml
<Grid RowDefinitions="Auto,*,Auto">
  <!-- Row 0: Header -->

  <!-- Row 1: Tabs + Content -->
  <Grid>
    <!-- Row 0: Tab Headers (3 Columns) -->
    <Grid.Row="0" ColumnDefinitions="*,*,*">
      <StackLayout Grid.Column="0"> <!-- Tab 1 -->
        <Button Clicked="OnTab1Clicked" />
        <BoxView BackgroundColor="#FFD700" /> <!-- Underline -->
      </StackLayout>
    </Grid.Row>

    <!-- Row 1: Tab Contents -->
    <Grid.Row="1">
      <ScrollView IsVisible="True"> <!-- Tab 1 Content -->
        <CollectionView ItemsSource="{Binding UpcomingAppointments}"
                        ItemTemplate="{StaticResource AppointmentTemplate}" />
      </ScrollView>
    </Grid.Row>
  </Grid>

  <!-- Row 2: Footer Button -->
</Grid>
```

---

## 🔄 Event Flow

```
Page Appears
    ↓
OnAppearing() 
    ↓
LoadAppointmentsCommand.Execute()
    ↓
LoadBookingsAsync()
    ↓
ViewModel:
  - Clear all collections
  - Fetch from API
  - Categorize appointments
  - Set HasNoAppointments = false
    ↓
UI Updates:
  - Tab Headers appear
  - Tab 1 content shows (default)
  - Rest are hidden
```

---

## 💡 Common Tasks

### إضافة Tab جديد:
1. أضف `[ObservableProperty]` في ViewModel
2. أضف عمود جديد في Tab Headers Grid
3. أضف StackLayout مع Button + BoxView
4. أضف ScrollView جديد في Tab Contents
5. أضف event handler و case في SelectTab()

### تغيير الألوان:
```xml
<!-- In XAML -->
TextColor="#FFD700"  <!-- Change this -->
BackgroundColor="#444444"  <!-- Or this -->

<!-- Or in C# -->
button.TextColor = new Color(255, 215, 0);  // Gold
```

### إضافة Empty State لـ Tab:
```xml
<StackLayout IsVisible="{Binding UpcomingAppointments.Count, 
             Converter={StaticResource InverseBoolConverter}}">
    <Label Text="No appointments" />
</StackLayout>
```

---

## 🧪 Testing Checklist

- [ ] Load Page → Tab 1 shows, Tab 1 header gold
- [ ] Click Tab 2 → Tab 2 shows, header gold
- [ ] Click Tab 3 → Tab 3 shows, header gold
- [ ] No appointments → Empty state
- [ ] Loading → Skeleton appears
- [ ] Cards → Proper layout
- [ ] Services → Show with prices
- [ ] Provider → Avatar + name
- [ ] Cancel → Button clickable
- [ ] Dark theme → Intact

---

## 🐛 Troubleshooting

| المشكلة | الحل |
|--------|------|
| Tab content doesn't change | Check SelectTab() logic |
| Colors not updating | Ensure new Color() used in code |
| Collections empty | Check API response & Status values |
| Skeleton appears forever | Check IsloadBooking flag |
| Empty state never shows | Check .Count binding |
| Tab headers not showing | Check HasNoAppointments binding |
| Provider image broken | Check ImgePerson property |
| Services not showing | Check ServicesDetails collection |

---

## 📞 Quick Support

**Q: How to add more appointments to Tab?**
A: They're automatically added via ViewModel binding

**Q: How to change tab colors?**
A: Update Color values in SelectTab() method

**Q: How to add tab content?**
A: Follow same pattern as Tab 1/2/3

**Q: How to customize card design?**
A: Edit AppointmentTemplate in XAML

---

## 📊 API Integration

```csharp
// API Call
var appointments = await _apiServices.GetUserAppointmentsAsync(currentUser);

// Expected Response Format
{
  "id": 1,
  "status": "PENDING",  // or "COMPLETED", "CANCELED"
  "appointment_date": "2024-01-15",
  "start_time": "10:00",
  "end_time": "11:00",
  "total_amount": 100.00,
  "provider": {
    "full_name": "Dr. Smith",
    "avatar_url": "https://..."
  },
  "services_details": [
    {
      "service_name": "Haircut",
      "formatted_price": "50.00"
    }
  ]
}
```

---

## 🎯 Performance Tips

✅ Use static DataTemplate (avoid duplication)
✅ Bind Collections in ViewModel (not XAML)
✅ Use scrollable content (each tab)
✅ Lazy load when tabs clicked (future feature)
✅ Cache images from provider
✅ Use Item virtualization in CollectionView

---

## 📚 Reference Files

- Main: `BookingPage.xaml`, `BookingPage.xaml.cs`
- ViewModel: `AppViweModel.cs`
- Model: `Appointment.cs`
- API: `ApiServices.cs`
- Styles: Color scheme in `App.xaml`

---

**Version**: 1.0
**Last Updated**: Now
**Status**: Ready for Production

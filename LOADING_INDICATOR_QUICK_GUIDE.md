# ? ใแฮี วแสอฯํห - ใฤิั วแสอใํแ

## ? ใว สใ สÛํํัๅ:

### **XAML - ลึวÝษ ใฤิั วแสอใํแ**

```xml
<!-- วแาั ใฺ ใฤิั วแสอใํแ -->
<Grid RowDefinitions="Auto" Margin="12,50,25,25" Padding="0">
    <!-- วแาั วแัฦํำํ -->
    <Button 
        x:Name="SendOtpButton"
        Clicked="Button_Clicked"
        IsEnabled="True" />

    <!-- ใฤิั วแสอใํแ -->
    <ActivityIndicator 
        x:Name="LoadingIndicator"
        IsRunning="False"
        IsVisible="False"
        Color="#EBD750" />
</Grid>
```

### **C# - สÝฺํแ ใฤิั วแสอใํแ**

```csharp
private void ShowLoadingIndicator(bool show)
{
    MainThread.BeginInvokeOnMainThread(() =>
    {
        // ฺัึ/ลฮÝวม ใฤิั วแสอใํแ
        LoadingIndicator.IsVisible = show;
        LoadingIndicator.IsRunning = show;

        // สฺุํแ/สÝฺํแ วแาั
        SendOtpButton.IsEnabled = !show;
        SendOtpButton.Opacity = show ? 0.6 : 1.0;
    });
}
```

---

## ?? วแไสวฦฬ:

| วแอวแษ | วแาั | วแใฤิั | วแิÝวÝํษ |
|--------|------|--------|----------|
| **Þศแ วแลัำวแ** | ? ใÝฺ๘แ | ? ใฮÝํ | 100% |
| **รหไวม วแลัำวแ** | ? ใฺุ๘แ | ? ํฺใแ | 60% |
| **ศฺฯ วแวไสๅวม** | ? ใÝฺ๘แ | ? ใฮÝํ | 100% |

---

## ?? วแูๅๆั วแศีัํ:

```
รหไวม วแสอใํแ:
???????????????????????
?  SEND OTP BUTTON    ? (ใฺสใ)
?     ? Loading...    ? (ะๅศํ)
???????????????????????
```

---

## ? วแอวแษ:

```
Build:       ? ไฬอ
XAML:        ? ใอฯ๘ห
C#:          ? ใอฯ๘ห
Testing:     ? ฬวๅา
Deployment:  ?? ฬวๅา
```

---

**ฬวๅา วแยไ! ??**

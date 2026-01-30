using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using loukupm.Services;

namespace loukupm.View;

public partial class NotificationSettingsPage : ContentPage
{
    private bool _isToggling = false;

    public NotificationSettingsPage()
    {
        InitializeComponent();
        LoadNotificationSettings();
    }

    /// <summary>
    /// تحميل حالة الإشعارات الحالية
    /// </summary>
    private void LoadNotificationSettings()
    {
        try
        {
            // الحصول على حالة الموافقة السابقة
            bool? consent = App.GetNotificationConsent();
            
            if (consent.HasValue)
            {
                NotificationToggle.IsToggled = consent.Value;
                StatusLabel.Text = consent.Value 
                    ? "✅ الإشعارات مفعّلة" 
                    : "❌ الإشعارات معطّلة";
                StatusLabel.TextColor = consent.Value 
                    ? Color.FromArgb("#4CAF50")
                    : Color.FromArgb("#F44336");
            }
            else
            {
                StatusLabel.Text = "⏳ لم يتم الموافقة بعد";
                StatusLabel.TextColor = Color.FromArgb("#FF9800");
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("خطأ", $"خطأ في تحميل الإعدادات: {ex.Message}", "حسناً");
        }
    }

    /// <summary>
    /// معالج تبديل الإشعارات
    /// </summary>
    private async void OnNotificationToggled(object sender, ToggledEventArgs e)
    {
        if (_isToggling) return;

        _isToggling = true;
        try
        {
            bool enable = e.Value;

            // اطلب تأكيد من المستخدم
            bool confirmed = await DisplayAlert(
                "تأكيد",
                enable 
                    ? "هل تريد تفعيل الإشعارات؟"
                    : "هل تريد تعطيل الإشعارات؟",
                "نعم",
                "لا"
            );

            if (!confirmed)
            {
                // عد الحالة للقيمة السابقة
                NotificationToggle.IsToggled = !enable;
                _isToggling = false;
                return;
            }

            // غيّر الإعداد
            await App.ToggleNotificationsAsync(enable);

            // تحديث الواجهة
            StatusLabel.Text = enable 
                ? "✅ تم تفعيل الإشعارات بنجاح"
                : "✅ تم تعطيل الإشعارات بنجاح";
            StatusLabel.TextColor = enable
                ? Color.FromArgb("#4CAF50")
                : Color.FromArgb("#F44336");

            // عرض Toast
            await Toast.Make(
                enable ? "الإشعارات مفعّلة الآن" : "الإشعارات معطّلة الآن",
                ToastDuration.Short
            ).Show();
        }
        catch (Exception ex)
        {
            await DisplayAlert("خطأ", $"حدث خطأ: {ex.Message}", "حسناً");
            // عد الحالة
            NotificationToggle.IsToggled = !NotificationToggle.IsToggled;
        }
        finally
        {
            _isToggling = false;
        }
    }

    /// <summary>
    /// إعادة تعيين الموافقة للسؤال مجدداً
    /// </summary>
    private async void OnResetConsent(object sender, EventArgs e)
    {
        try
        {
            bool confirmed = await DisplayAlert(
                "تأكيد",
                "هل تريد إعادة تعيين خيار الإشعارات؟\nسيتم السؤال عند الدخول التالي",
                "نعم",
                "لا"
            );

            if (confirmed)
            {
                // حذف الخيار السابق
                Preferences.Remove("notification_consent");
                
                // تحديث الواجهة
                NotificationToggle.IsToggled = false;
                StatusLabel.Text = "⏳ سيتم السؤال عند الدخول التالي";
                StatusLabel.TextColor = Color.FromArgb("#FF9800");

                await Toast.Make("تم إعادة تعيين الخيار", ToastDuration.Short).Show();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("خطأ", ex.Message, "حسناً");
        }
    }
}
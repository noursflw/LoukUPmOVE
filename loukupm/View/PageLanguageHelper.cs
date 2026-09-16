using System.Globalization;
using loukupm.Langue;

namespace loukupm.View;

/// <summary>
/// فئة مساعدة لإضافة دعم تغيير اللغة والاتجاه تلقائياً لكل صفحة
/// استخدم هذا في كل صفحة content page
/// </summary>
public static class PageLanguageHelper
{
    /// <summary>
    /// تهيئة الصفحة لتحديث اتجاهها تلقائياً عند تغيير اللغة
    /// استدعِ هذا في المنشئ (constructor) من كل صفحة
    /// </summary>
    public static void InitializeLanguageTracking(this ContentPage page)
    {
        if (page == null) return;

        // إنشء handler محلي للاشتراك والإلغاء
        Action<CultureInfo> languageChangeHandler = (culture) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdatePageFlowDirection(page, culture);
            });
        };

        // الاشتراك في حدث تغيير اللغة
        LocalizationResourcesManager.Instanse.LanguageChanged += languageChangeHandler;

        // تحديث الاتجاه الأولي
        UpdatePageFlowDirection(page, LocalizationResourcesManager.Instanse.CurrentCulture);

        // إلغاء الاشتراك عند إغلاق الصفحة (CRITICAL!)
        page.Unloaded += (s, e) =>
        {
            LocalizationResourcesManager.Instanse.LanguageChanged -= languageChangeHandler;
            Console.WriteLine($"🧹 Language tracking cleaned up for {page.GetType().Name}");
        };
    }

    /// <summary>
    /// تحديث اتجاه الصفحة بناءً على اللغة
    /// </summary>
    private static void UpdatePageFlowDirection(ContentPage page, CultureInfo culture)
    {
        if (page == null || culture == null) return;

        string languageCode = culture.TwoLetterISOLanguageName.ToLower();

        page.FlowDirection = languageCode == "ar" 
            ? FlowDirection.RightToLeft 
            : FlowDirection.LeftToRight;

        Console.WriteLine($"✅ {page.GetType().Name} FlowDirection Updated to {page.FlowDirection}");
    }
}


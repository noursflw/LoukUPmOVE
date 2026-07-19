namespace loukupm.Langue
{
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.Versioning;

    public partial class LocalizationResourcesManager : INotifyPropertyChanged
    {
        [SupportedOSPlatform("IOS")]
        [SupportedOSPlatform("android")]
        [SupportedOSPlatform("windows")]
        public LocalizationResourcesManager()
        {
            AppResource.Culture = CultureInfo.CurrentCulture;
        }

        public static LocalizationResourcesManager Instanse { get; } = new();

        public event PropertyChangedEventHandler PropertyChanged;

        // 🔹 حدث جديد لإعلام التغييرات في اللغة
        public event Action<CultureInfo>? LanguageChanged;

        public object this[string resourceKey] =>
            AppResource.ResourceManager.GetObject(resourceKey, AppResource.Culture) ?? resourceKey;

        public CultureInfo CurrentCulture => AppResource.Culture;

        public void SetCulture(CultureInfo culture)
        {
            if (culture == null) return;

            AppResource.Culture = culture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));

            // 🔹 إعلام المشتركين بتغيير اللغة
            LanguageChanged?.Invoke(culture);
            
            // 🔹 تحديث اتجاه التطبيق بأكمله
            UpdateApplicationFlowDirection();
        }

        /// <summary>
        /// الحصول على اتجاه النص بناءً على اللغة الحالية
        /// العربية → RightToLeft
        /// الألمانية والإنجليزية → LeftToRight
        /// </summary>
        public FlowDirection GetFlowDirection()
        {
            string languageCode = CurrentCulture.TwoLetterISOLanguageName.ToLower();
            return languageCode == "ar" ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        /// <summary>
        /// الحصول على اتجاه النص للغة محددة
        /// </summary>
        public FlowDirection GetFlowDirection(CultureInfo culture)
        {
            if (culture == null) return FlowDirection.LeftToRight;
            
            string languageCode = culture.TwoLetterISOLanguageName.ToLower();
            return languageCode == "ar" ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        /// <summary>
        /// تحديث اتجاه النص لصفحة محددة
        /// </summary>
        public void UpdatePageFlowDirection(ContentPage page)
        {
            if (page == null) return;
            page.FlowDirection = GetFlowDirection();
        }

        /// <summary>
        /// تحديث اتجاه النص للتطبيق بالكامل وجميع الصفحات
        /// </summary>
        public void UpdateApplicationFlowDirection()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    var newDirection = GetFlowDirection();
                    Console.WriteLine($"🔄 Updating FlowDirection to {newDirection} for culture {CurrentCulture.DisplayName}");

                    // تحديث الصفحة الرئيسية الحالية بغض النظر عن نوعها (Shell / NavigationPage / ContentPage)
                    if (Application.Current?.MainPage is Page rootPage)
                    {
                        rootPage.FlowDirection = newDirection;
                        Console.WriteLine($"✅ MainPage Flow Direction Updated to {newDirection}");
                    }

                    // تحديث AppShell بشكل صريح عند استخدام Shell
                    if (Application.Current?.MainPage is AppShell appShell)
                    {
                        appShell.FlowDirection = newDirection;
                        Console.WriteLine($"✅ AppShell Flow Direction Updated to {newDirection}");
                    }

                    // تحديث جميع الصفحات في navigation stack
                    var navStack = Application.Current?.MainPage?.Navigation?.NavigationStack;
                    if (navStack != null)
                    {
                        foreach (var page in navStack)
                        {
                            if (page != null)
                            {
                                page.FlowDirection = newDirection;
                                Console.WriteLine($"✅ Updated {page.GetType().Name} FlowDirection to {newDirection}");
                            }
                        }
                    }

                    // تحديث جميع الصفحات المشروطة (modal pages)
                    var modalStack = Application.Current?.MainPage?.Navigation?.ModalStack;
                    if (modalStack != null)
                    {
                        foreach (var page in modalStack)
                        {
                            if (page != null)
                            {
                                page.FlowDirection = newDirection;
                                Console.WriteLine($"✅ Updated Modal {page.GetType().Name} FlowDirection to {newDirection}");
                            }
                        }
                    }

                    Console.WriteLine($"✅ Application Flow Direction Updated to {newDirection}");

                    // Small delay to ensure UI updates propagate
                    await Task.Delay(50);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error updating application flow direction: {ex.Message}");
                }
            });
        }
    }
}

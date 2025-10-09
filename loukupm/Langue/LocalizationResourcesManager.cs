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
        }
    }
}

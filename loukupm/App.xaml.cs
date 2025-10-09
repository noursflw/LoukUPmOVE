using loukupm.View;
using System.Globalization;
using Microsoft.Maui.Storage;

namespace loukupm
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            Langue.LocalizationResourcesManager.Instanse.LanguageChanged += (culture) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var direction = culture.TwoLetterISOLanguageName == "ar"
                        ? FlowDirection.RightToLeft
                        : FlowDirection.LeftToRight;

                    if (Application.Current.MainPage != null)
                        Application.Current.MainPage.FlowDirection = direction;
                });
            };



            // استرجاع اللغة المحفوظة
            var savedLang = Preferences.Get("AppLanguage", string.Empty);
            if (!string.IsNullOrEmpty(savedLang))
            {
                var culture = new CultureInfo(savedLang);
                Langue.LocalizationResourcesManager.Instanse.SetCulture(culture);

                // ضبط اتجاه الواجهة
                var direction = culture.TwoLetterISOLanguageName == "ar"
                    ? Microsoft.Maui.FlowDirection.RightToLeft
                    : Microsoft.Maui.FlowDirection.LeftToRight;

                // بعد تهيئة MainPage
                MainPage = new AppShell
                {
                    FlowDirection = direction
                };
            }
            else
            {
                MainPage = new AppShell();
            }

            // توجيه للصفحة الرئيسية
            MainPage.Dispatcher.Dispatch(async () =>
            {
                await Shell.Current.GoToAsync("MainPage");
            });
        }
    }
}

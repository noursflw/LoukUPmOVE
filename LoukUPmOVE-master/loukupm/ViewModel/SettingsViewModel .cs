using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using loukupm.Langue;
using loukupm.Model;
using loukupm.Services;
using Microsoft.Maui.Storage;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace loukupm.ViewModel
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly ApiServices _apiServices;

        public SettingsViewModel(ApiServices apiServices)
        {
            _apiServices = apiServices;

            InitializeLanguages();
            LoadSavedLanguage();
        }

        // =========================
        // EXISTING LOGIC (UNCHANGED)
        // =========================

        [ObservableProperty]
        private bool smsEnabled;

        [ObservableProperty]
        private bool emailEnabled;

        [ObservableProperty]
        private bool isBusy;

        private List<SettingItem> _settingsCache = new();
        partial void OnSmsEnabledChanged(bool value)
        {
            Console.WriteLine($"📱 sms UI changed to: {value}");

            _ = UpdateSetting("reminder_sms_enabled", value);
        }

        partial void OnEmailEnabledChanged(bool value)
        {
            Console.WriteLine($"📱 nEmai UI changed to: {value}");
            _ = UpdateSetting("reminder_email_enabled", value);
        }

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            // Prevent duplicate concurrent loads
            if (IsBusy) return;

            // If we already loaded settings once, skip reloading to preserve UI state across page navigations.
            if (_settingsCache != null && _settingsCache.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("[SettingsViewModel] LoadDataAsync skipped - already loaded");
                return;
            }

            try
            {
                IsBusy = true;

                _settingsCache = await _apiServices.GetSettingsAsync();

                var sms = _settingsCache.FirstOrDefault(x => x.Key == "reminder_sms_enabled");
                var email = _settingsCache.FirstOrDefault(x => x.Key == "reminder_email_enabled");

                SmsEnabled = sms?.Value?.ToString()?.ToLower() == "true";
                EmailEnabled = email?.Value?.ToString()?.ToLower() == "true";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task ToggleSmsAsync()
        {
            await UpdateSetting("reminder_sms_enabled", SmsEnabled);
        }

        [RelayCommand]
        public async Task ToggleEmailAsync()
        {
            await UpdateSetting("reminder_email_enabled", EmailEnabled);
        }

        private async Task UpdateSetting(string key, bool value)
        {
            try
            {
                IsBusy = true;

                var success = await _apiServices.UpdateSettingAsync(key, value);

                if (!success)
                {
                    if (key == "reminder_sms_enabled")
                        SmsEnabled = !value;
                    else
                        EmailEnabled = !value;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        // =========================
        // LANGUAGE ADDITION (NEW)
        // =========================

        public class LanguageOption
        {
            public string Name { get; set; }
            public string CultureName { get; set; }

            public override string ToString() => Name;
        }

        public List<LanguageOption> Languages { get; set; }

        private LanguageOption selectedLanguage;

        public LanguageOption SelectedLanguage
        {
            get => selectedLanguage;
            set
            {
                if (SetProperty(ref selectedLanguage, value) && value != null)
                {
                    ChangeLanguage(value.CultureName);
                }
            }
        }

        private void InitializeLanguages()
        {
            Languages = new List<LanguageOption>
            {
                new LanguageOption { Name = "Deutsch", CultureName = "de-DE" },
                new LanguageOption { Name = "العربية", CultureName = "ar" }
            };
        }

        private void LoadSavedLanguage()
        {
            var saved = Preferences.Get("AppLanguage", "de-DE");

            // Handle both "ar" and "ar-*" formats
            if (saved.StartsWith("ar"))
            {
                SelectedLanguage = Languages.FirstOrDefault(x => x.CultureName == "ar");
            }
            else
            {
                SelectedLanguage = Languages.FirstOrDefault(x => x.CultureName == saved);
            }

            Console.WriteLine($"🌍 LoadSavedLanguage: Loaded {SelectedLanguage?.Name} ({saved})");
        }

        private void ChangeLanguage(string cultureName)
        {
            try
            {
                var culture = new CultureInfo(cultureName);

                Preferences.Set("AppLanguage", culture.Name);
                Console.WriteLine($"💾 AppLanguage saved as: {culture.Name}");

                LocalizationResourcesManager.Instanse.SetCulture(culture);

                Console.WriteLine($"🌍 Language changed to {culture.DisplayName} (Code: {culture.TwoLetterISOLanguageName})");
                Console.WriteLine($"↔️ Flow Direction: {LocalizationResourcesManager.Instanse.GetFlowDirection()}");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"❌ Error changing language: {ex.Message}");
            }
        }
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using loukupm.Langue;
using loukupm.Model;
using loukupm.Services;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private bool isBusy;

        /// <summary>
        /// Toggles rendered from GET /api/settings. Keys are never hardcoded in the UI,
        /// so settings added on the backend show up without an app release.
        /// </summary>
        public ObservableCollection<SettingToggleViewModel> Settings { get; } = new();

        private List<SettingItem> _settingsCache = new();

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            if (IsBusy) return;

            if (_settingsCache != null && _settingsCache.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("[SettingsViewModel] LoadDataAsync skipped - already loaded");
                return;
            }

            try
            {
                IsBusy = true;

                _settingsCache = await _apiServices.GetSettingsAsync();

                Settings.Clear();
                foreach (var item in _settingsCache)
                {
                    if (!string.Equals(item.Type, "boolean", StringComparison.OrdinalIgnoreCase))
                        continue;

                    Settings.Add(new SettingToggleViewModel(item, UpdateSetting));
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<bool> UpdateSetting(string key, bool value)
        {
            try
            {
                return await _apiServices.UpdateSettingAsync(key, value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SettingsViewModel] Failed to update {key}: {ex.Message}");
                return false;
            }
        }

        // =========================
        // LANGUAGE ADDITION (NEW)
        // =========================

        public class LanguageOption
        {
            public string Name { get; set; }
            public string CultureName { get; set; }
            public string Icon { get; set; }

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
                new LanguageOption { Name = "Deutsch", CultureName = "de-DE", Icon = "flag_de" },
                new LanguageOption { Name = "العربية", CultureName = "ar", Icon = "flag_ar" }
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

                RefreshSettingTexts();

                // Notification bodies are built from the stored profile locale, not from request headers.
                _ = SyncLocaleWithBackendAsync(culture.TwoLetterISOLanguageName);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"❌ Error changing language: {ex.Message}");
            }
        }

        private async Task SyncLocaleWithBackendAsync(string languageCode)
        {
            try
            {
                await _apiServices.UpdateUserLocaleAsync(languageCode);

                // Labels and reminder texts are localized server-side, so refetch both.
                _settingsCache = new List<SettingItem>();
                await LoadDataAsync();
                await AppViewModel.Instance.RefreshReminderOptionsAsync();
                RefreshSettingTexts();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"❌ Error syncing locale: {ex.Message}");
            }
        }

        private void RefreshSettingTexts()
        {
            foreach (var setting in Settings)
                setting.RefreshLocalizedTexts();
        }
    }
}
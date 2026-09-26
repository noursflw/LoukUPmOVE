using CommunityToolkit.Mvvm.ComponentModel;
using loukupm.Langue;
using loukupm.Model;
using System;
using System.Threading.Tasks;

namespace loukupm.ViewModel
{
    /// <summary>
    /// Wraps a single setting returned by GET /api/settings so the UI can render
    /// whatever keys the backend sends without hardcoding them.
    /// Labels and descriptions use Arabic/German app resources for known keys.
    /// </summary>
    public partial class SettingToggleViewModel : ObservableObject
    {
        private readonly Func<string, bool, Task<bool>> _updateAsync;
        private readonly string _apiLabel;
        private readonly string _apiDescription;
        private bool _suppressUpdate;

        public SettingToggleViewModel(SettingItem item, Func<string, bool, Task<bool>> updateAsync)
        {
            _updateAsync = updateAsync;

            Key = item.Key;
            Type = item.Type;
            Group = item.Group;
            _apiLabel = item.Label ?? string.Empty;
            _apiDescription = item.Description ?? string.Empty;

            RefreshLocalizedTexts();

            _suppressUpdate = true;
            IsOn = ParseValue(item.Value);
            _suppressUpdate = false;
        }

        public string Key { get; }
        public string Type { get; }
        public string Group { get; }

        [ObservableProperty]
        private string label = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;

        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

        [ObservableProperty]
        private bool isOn;

        public void RefreshLocalizedTexts()
        {
            Label = ResolveLabel(Key, _apiLabel);
            Description = ResolveDescription(Key, _apiDescription);
        }

        partial void OnDescriptionChanged(string value)
        {
            OnPropertyChanged(nameof(HasDescription));
        }

        partial void OnIsOnChanged(bool value)
        {
            if (_suppressUpdate || _updateAsync == null)
                return;

            _ = ApplyAsync(value);
        }

        private async Task ApplyAsync(bool value)
        {
            var success = await _updateAsync(Key, value);
            if (success)
                return;

            _suppressUpdate = true;
            IsOn = !value;
            _suppressUpdate = false;
        }

        private static string ResolveLabel(string key, string fallback)
        {
            if (IsSmsKey(key, fallback) && !string.IsNullOrWhiteSpace(AppResource.SMSNotifications))
                return AppResource.SMSNotifications;

            if (IsEmailKey(key, fallback) && !string.IsNullOrWhiteSpace(AppResource.EmailNotifications))
                return AppResource.EmailNotifications;

            if (IsPushKey(key, fallback) && !string.IsNullOrWhiteSpace(AppResource.PushNotifications))
                return AppResource.PushNotifications;

            var fromResources = AppResource.ResourceManager.GetString(key, AppResource.Culture);
            return string.IsNullOrWhiteSpace(fromResources) ? fallback : fromResources;
        }

        private static string ResolveDescription(string key, string fallback)
        {
            if (IsSmsKey(key, fallback) && !string.IsNullOrWhiteSpace(AppResource.DesSms))
                return AppResource.DesSms;

            if (IsEmailKey(key, fallback) && !string.IsNullOrWhiteSpace(AppResource.DesEmai))
                return AppResource.DesEmai;

            if (IsPushKey(key, fallback) && !string.IsNullOrWhiteSpace(AppResource.DesPush))
                return AppResource.DesPush;

            var fromResources = AppResource.ResourceManager.GetString($"{key}_desc", AppResource.Culture);
            return string.IsNullOrWhiteSpace(fromResources) ? fallback : fromResources;
        }

        private static bool IsSmsKey(string key, string fallback)
        {
            return ContainsIgnoreCase(key, "sms")
                || ContainsIgnoreCase(fallback, "SMS");
        }

        private static bool IsEmailKey(string key, string fallback)
        {
            return ContainsIgnoreCase(key, "email")
                || ContainsIgnoreCase(key, "mail")
                || ContainsIgnoreCase(fallback, "E-Mail")
                || ContainsIgnoreCase(fallback, "Email");
        }

        private static bool IsPushKey(string key, string fallback)
        {
            return ContainsIgnoreCase(key, "push")
                || ContainsIgnoreCase(fallback, "Push appointment")
                || ContainsIgnoreCase(fallback, "in-app notification")
                || ContainsIgnoreCase(fallback, "Push-Erinnerung")
                || ContainsIgnoreCase(fallback, "Push reminders");
        }

        private static bool ContainsIgnoreCase(string value, string fragment)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.Contains(fragment, StringComparison.OrdinalIgnoreCase);
        }

        private static bool ParseValue(object value)
        {
            return value switch
            {
                bool b => b,
                System.Text.Json.JsonElement json => json.ValueKind == System.Text.Json.JsonValueKind.True
                    || (json.ValueKind == System.Text.Json.JsonValueKind.String && bool.TryParse(json.GetString(), out var parsed) && parsed),
                string s => bool.TryParse(s, out var result) && result,
                _ => false
            };
        }
    }
}

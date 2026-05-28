using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace loukupm.Model
{
    /// <summary>
    /// API response wrapper for Home Slider data
    /// </summary>
    public partial class HomeSliderResponse : ObservableObject
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public ObservableCollection<HomeSliderItem> Data { get; set; } = new();

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    /// <summary>
    /// Individual slider item with multilingual support
    /// </summary>
    public partial class HomeSliderItem : ObservableObject
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; } = string.Empty;

        [JsonPropertyName("action_type")]
        public string ActionType { get; set; } = string.Empty;

        [JsonPropertyName("action_value")]
        public string ActionValue { get; set; } = string.Empty;

        [JsonPropertyName("sort_order")]
        public int SortOrder { get; set; }

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }

        [JsonPropertyName("title")]
        public MultiLanguageText Title { get; set; } = new();

        [JsonPropertyName("subtitle")]
        public MultiLanguageText Subtitle { get; set; } = new();

        [JsonPropertyName("translations")]
        public Dictionary<string, Dictionary<string, string>> Translations { get; set; } = new();
    }
}

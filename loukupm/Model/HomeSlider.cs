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
        public HomeSliderData Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    /// <summary>
    /// Container for home slider items with grouping key
    /// </summary>
    public partial class HomeSliderData : ObservableObject
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = "home";

        [JsonPropertyName("items")]
        public ObservableCollection<HomeSliderItem> Items { get; set; } = new();
    }

    /// <summary>
    /// Individual slider item with promotional content
    /// </summary>
    public partial class HomeSliderItem : ObservableObject
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("sort_order")]
        public int SortOrder { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("subtitle")]
        public string Subtitle { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; } = string.Empty;

        [JsonPropertyName("starts_at")]
        public string StartsAt { get; set; }

        [JsonPropertyName("ends_at")]
        public string EndsAt { get; set; }

        [JsonPropertyName("is_permanent")]
        public bool IsPermanent { get; set; }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace loukupm.Model
{
    /// <summary>
    /// Root response wrapper for Privacy Policy API
    /// </summary>
    public partial class PrivacyPolicyResponse : ObservableObject
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public PrivacyPolicyData Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    /// <summary>
    /// Main Privacy Policy data container
    /// </summary>
    public partial class PrivacyPolicyData : ObservableObject
    {
        [JsonPropertyName("slug")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string Slug { get; set; }

        [JsonPropertyName("language")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string Language { get; set; }

        [JsonPropertyName("fallback_language")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string FallbackLanguage { get; set; }

        [JsonPropertyName("direction")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string Direction { get; set; }

        [JsonPropertyName("blocks")]
        public ObservableCollection<PrivacyPolicyCmsBlock> Blocks { get; set; } = new ObservableCollection<PrivacyPolicyCmsBlock>();
    }

    /// <summary>
    /// Individual CMS block (heading, paragraph, divider, unordered_list, title_paragraph)
    /// </summary>
    public partial class PrivacyPolicyCmsBlock : ObservableObject
    {
        [JsonPropertyName("type")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string Type { get; set; }

        [JsonPropertyName("id")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string Id { get; set; }

        [JsonPropertyName("props")]
        public PrivacyPolicyCmsBlockProps Props { get; set; }

        [JsonPropertyName("content")]
        public PrivacyPolicyCmsBlockContent Content { get; set; }
    }

    /// <summary>
    /// Block properties (heading level, alignment, etc.)
    /// Uses FlexibleNullableIntConverter for level to handle int/"h1"/"h2"/null formats
    /// </summary>
    public partial class PrivacyPolicyCmsBlockProps : ObservableObject
    {
        [JsonPropertyName("level")]
        [JsonConverter(typeof(FlexibleNullableIntConverter))]
        public int? Level { get; set; }

        [JsonPropertyName("textAlignment")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string TextAlignment { get; set; }

        [JsonPropertyName("color")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string Color { get; set; }

        [JsonPropertyName("backgroundColor")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string BackgroundColor { get; set; }
    }

    /// <summary>
    /// Block content structure (text, items, children, etc.)
    /// </summary>
    public partial class PrivacyPolicyCmsBlockContent : ObservableObject
    {
        [JsonPropertyName("text")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string Text { get; set; }

        [JsonPropertyName("title")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string Title { get; set; }

        [JsonPropertyName("items")]
        public ObservableCollection<string> Items { get; set; } = new ObservableCollection<string>();

        [JsonPropertyName("children")]
        public ObservableCollection<PrivacyPolicyCmsBlockContentChild> Children { get; set; } = new ObservableCollection<PrivacyPolicyCmsBlockContentChild>();
    }

    /// <summary>
    /// Nested content structure for complex blocks
    /// </summary>
    public partial class PrivacyPolicyCmsBlockContentChild : ObservableObject
    {
        [JsonPropertyName("type")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string Type { get; set; }

        [JsonPropertyName("text")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string Text { get; set; }
    }

    /// <summary>
    /// Legacy model - kept for backward compatibility
    /// This should not be used for new Privacy Policy loading
    /// </summary>
    [Obsolete("Use PrivacyPolicyData instead")]
    public partial class PolicyandPrivacyS : ObservableObject
    {
        public int id { get; set; }
        public string LastUpate { get; set; }
        [ObservableProperty]
        public string description;
    }
}

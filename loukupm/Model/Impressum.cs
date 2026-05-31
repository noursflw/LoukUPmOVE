using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace loukupm.Model
{
    /// <summary>
    /// Root response wrapper for Impressum API
    /// </summary>
    public partial class ImpressumResponse : ObservableObject
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public ImpressumData Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    /// <summary>
    /// Main Impressum data container
    /// </summary>
    public partial class ImpressumData : ObservableObject
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
        public ObservableCollection<ImpressumCmsBlock> Blocks { get; set; } = new ObservableCollection<ImpressumCmsBlock>();
    }

    /// <summary>
    /// Individual CMS block (heading, paragraph, divider, unordered_list, title_paragraph, warning_box)
    /// </summary>
    public partial class ImpressumCmsBlock : ObservableObject
    {
        [JsonPropertyName("type")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string Type { get; set; }

        [JsonPropertyName("id")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string Id { get; set; }

        [JsonPropertyName("props")]
        public ImpressumCmsBlockProps Props { get; set; }

        [JsonPropertyName("content")]
        public ImpressumCmsBlockContent Content { get; set; }
    }

    /// <summary>
    /// Block properties (heading level, alignment, color, backgroundColor, etc.)
    /// Uses FlexibleNullableIntConverter for level to handle int/"h1"/"h2"/null formats
    /// </summary>
    public partial class ImpressumCmsBlockProps : ObservableObject
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

        [JsonPropertyName("alignment")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string Alignment { get; set; }

        [JsonPropertyName("size")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string Size { get; set; }

        [JsonPropertyName("style")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string Style { get; set; }

        [JsonPropertyName("orientation")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string Orientation { get; set; }
    }

    /// <summary>
    /// Block content structure (text, items, children, title, etc.)
    /// Supports all block types: heading, paragraph, title_paragraph, unordered_list, divider, warning_box
    /// </summary>
    public partial class ImpressumCmsBlockContent : ObservableObject
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
        public ObservableCollection<ImpressumCmsBlock> Children { get; set; } = new ObservableCollection<ImpressumCmsBlock>();

        [JsonPropertyName("description")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string Description { get; set; }

        [JsonPropertyName("level")]
        [JsonConverter(typeof(FlexibleNullableIntConverter))]
        public int? Level { get; set; }
    }
}

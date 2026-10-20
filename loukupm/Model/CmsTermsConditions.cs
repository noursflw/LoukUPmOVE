using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace loukupm.Model
{
    /// <summary>
    /// Custom JsonConverter for flexible int? deserialization
    /// Handles: int, string "1", string "h1", null, empty string
    /// Falls back to null on conversion failure
    /// </summary>
    public class FlexibleNullableIntConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.Null:
                        return null;

                    case JsonTokenType.Number:
                        if (reader.TryGetInt32(out int intValue))
                        {
                            return intValue;
                        }
                        // If it's a number but not int32, try int64 then convert
                        if (reader.TryGetInt64(out long longValue))
                        {
                            return (int)longValue;
                        }
                        return null;

                    case JsonTokenType.String:
                        string strValue = reader.GetString();
                        if (string.IsNullOrWhiteSpace(strValue))
                        {
                            return null;
                        }

                        // Try to parse string "1", "2", "3"
                        if (int.TryParse(strValue, out int parsedInt))
                        {
                            return parsedInt;
                        }

                        // Handle string format "h1", "h2" - extract number
                        if (strValue.StartsWith("h", StringComparison.OrdinalIgnoreCase))
                        {
                            string numberPart = strValue.Substring(1);
                            if (int.TryParse(numberPart, out int headingLevel))
                            {
                                return headingLevel;
                            }
                        }

                        // Unable to convert - return null
                        Console.WriteLine($"?? [FlexibleNullableIntConverter] Could not parse '{strValue}' as int or heading level");
                        return null;

                    default:
                        Console.WriteLine($"?? [FlexibleNullableIntConverter] Unexpected token type: {reader.TokenType}");
                        return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? [FlexibleNullableIntConverter] Exception: {ex.Message}");
                return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }

    /// <summary>
    /// Custom JsonConverter for flexible string deserialization
    /// Handles: string, null, empty string, non-string types
    /// Falls back to null on conversion failure
    /// </summary>
    public class FlexibleStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.Null:
                    case JsonTokenType.None:
                        return null;

                    case JsonTokenType.String:
                        return reader.GetString();

                    case JsonTokenType.Number:
                        // Convert number to string (e.g., 1 -> "1")
                        if (reader.TryGetInt32(out int intVal))
                        {
                            return intVal.ToString();
                        }
                        if (reader.TryGetInt64(out long longVal))
                        {
                            return longVal.ToString();
                        }
                        if (reader.TryGetDouble(out double doubleVal))
                        {
                            return doubleVal.ToString();
                        }
                        return null;

                    case JsonTokenType.True:
                    case JsonTokenType.False:
                        bool boolVal = reader.GetBoolean();
                        return boolVal.ToString();

                    default:
                        Console.WriteLine($"?? [FlexibleStringConverter] Unexpected token type: {reader.TokenType}");
                        return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? [FlexibleStringConverter] Exception: {ex.Message}");
                return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStringValue(value);
            }
        }
    }

    /// <summary>
    /// Root response wrapper for Terms & Conditions API
    /// </summary>
    public partial class TermsConditionsResponse : ObservableObject
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public TermsConditionsData Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    /// <summary>
    /// Main Terms & Conditions data container
    /// </summary>
    public partial class TermsConditionsData : ObservableObject
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
        public ObservableCollection<CmsBlock> Blocks { get; set; } = new ObservableCollection<CmsBlock>();
    }

    /// <summary>
    /// Individual CMS block (heading, paragraph, divider, unordered_list, warning_box)
    /// </summary>
    public partial class CmsBlock : ObservableObject
    {
        [JsonPropertyName("type")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string Type { get; set; }

        [JsonPropertyName("id")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string Id { get; set; }

        [JsonPropertyName("props")]
        public CmsBlockProps Props { get; set; }

        [JsonPropertyName("content")]
        public CmsBlockContent Content { get; set; }
    }

    /// <summary>
    /// Block properties (heading level, alignment, etc.)
    /// Uses FlexibleNullableIntConverter for level to handle int/"h1"/"h2"/null formats
    /// </summary>
    public partial class CmsBlockProps : ObservableObject
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
    /// Block content structure (text, items, etc.)
    /// </summary>
    public partial class CmsBlockContent : ObservableObject
    {
        [JsonPropertyName("text")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string Text { get; set; }

        [JsonPropertyName("items")]
        public ObservableCollection<string> Items { get; set; } = new ObservableCollection<string>();

        [JsonPropertyName("children")]
        public ObservableCollection<CmsBlockContentChild> Children { get; set; } = new ObservableCollection<CmsBlockContentChild>();
    }

    /// <summary>
    /// Nested content structure for complex blocks
    /// </summary>
    public partial class CmsBlockContentChild : ObservableObject
    {
        [JsonPropertyName("type")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string Type { get; set; }

        [JsonPropertyName("text")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string Text { get; set; }
    }
}

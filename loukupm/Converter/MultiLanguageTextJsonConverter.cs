using System.Text.Json;
using System.Text.Json.Serialization;
using loukupm.Model;

namespace loukupm.Converter
{
    /// <summary>
    /// Custom JsonConverter for MultiLanguageText that handles both:
    /// 1. Object format: {"de": "...", "ar": "...", "en": "..."}
    /// 2. String format: "Simple text"
    /// 
    /// When a string is provided, it's converted to a MultiLanguageText
    /// with all language properties set to the same value.
    /// </summary>
    public class MultiLanguageTextJsonConverter : JsonConverter<MultiLanguageText>
    {
        public override MultiLanguageText Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Create a new MultiLanguageText instance
            var multiLanguageText = new MultiLanguageText();

            // Check the token type
            if (reader.TokenType == JsonTokenType.String)
            {
                // Handle simple string format
                var stringValue = reader.GetString();
                if (!string.IsNullOrEmpty(stringValue))
                {
                    // Set all language properties to the same string value
                    multiLanguageText.German = stringValue;
                    multiLanguageText.Arabic = stringValue;
                    multiLanguageText.English = stringValue;
                }
                return multiLanguageText;
            }
            else if (reader.TokenType == JsonTokenType.Null)
            {
                // Handle null case
                return multiLanguageText;
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                // Handle object format
                using (var jsonDoc = JsonDocument.ParseValue(ref reader))
                {
                    var root = jsonDoc.RootElement;

                    // Read each language property if it exists
                    if (root.TryGetProperty("de", out var deElement))
                    {
                        multiLanguageText.German = deElement.GetString();
                    }

                    if (root.TryGetProperty("ar", out var arElement))
                    {
                        multiLanguageText.Arabic = arElement.GetString();
                    }

                    if (root.TryGetProperty("en", out var enElement))
                    {
                        multiLanguageText.English = enElement.GetString();
                    }
                }
                return multiLanguageText;
            }

            return multiLanguageText;
        }

        public override void Write(Utf8JsonWriter writer, MultiLanguageText value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            // Write as object format with language codes
            writer.WriteStartObject();

            if (!string.IsNullOrEmpty(value.German))
            {
                writer.WriteString("de", value.German);
            }
            else
            {
                writer.WriteNull("de");
            }

            if (!string.IsNullOrEmpty(value.Arabic))
            {
                writer.WriteString("ar", value.Arabic);
            }
            else
            {
                writer.WriteNull("ar");
            }

            if (!string.IsNullOrEmpty(value.English))
            {
                writer.WriteString("en", value.English);
            }
            else
            {
                writer.WriteNull("en");
            }

            writer.WriteEndObject();
        }
    }
}

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace loukupm.Model
{
    /// <summary>
    /// API response containing a list of settings.
    /// </summary>
    public class SettingsResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        [JsonPropertyName("data")]
        public List<SettingItem> Data { get; set; } = new();
    }

    /// <summary>
    /// Represents a single setting item from the backend.
    /// </summary>
    public class SettingItem
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = "boolean";

        [JsonPropertyName("group")]
        public string Group { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public object? Value { get; set; }

        [JsonPropertyName("is_default")]
        public bool IsDefault { get; set; }
    }

    /// <summary>
    /// Request body for updating a setting via PATCH endpoint.
    /// </summary>
    public class PatchSettingRequest
    {
        public bool Value { get; set; }
    }

    /// <summary>
    /// Response from PATCH /api/settings/{key}.
    /// </summary>
    public class PatchSettingResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public PatchData? Data { get; set; }
    }

    /// <summary>
    /// Data contained in PATCH response.
    /// </summary>
    public class PatchData
    {
        public string Key { get; set; } = string.Empty;
        public bool Value { get; set; }
    }

    /// <summary>
    /// Custom exception for settings service errors.
    /// </summary>
    public class SettingsServiceException : Exception
    {
        public int? StatusCode { get; set; }

        public SettingsServiceException(string message, int? statusCode = null, Exception? innerException = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}

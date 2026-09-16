using System.Collections.Generic;

namespace loukupm.Model
{
    /// <summary>
    /// API response containing a list of settings.
    /// </summary>
    public class SettingsResponse
    {
        public bool Success { get; set; }
        public List<SettingItem> Data { get; set; } = new();
    }

    /// <summary>
    /// Represents a single setting item from the backend.
    /// </summary>
    public class SettingItem
    {
        /// <summary>
        /// Unique identifier for the setting (e.g., "sms_notifications").
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Display label for the setting (e.g., "SMS Notifications").
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Description or help text for the setting.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Type of the setting: "boolean", "string", "integer", etc.
        /// </summary>
        public string Type { get; set; } = "boolean";

        /// <summary>
        /// Group name for organizing settings (e.g., "notifications", "privacy").
        /// </summary>
        public string Group { get; set; } = string.Empty;

        /// <summary>
        /// Current value of the setting (can be bool, string, int, etc.).
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Indicates if this is the default value.
        /// </summary>
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

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace loukupm.Model.ApiResponses
{
    /// <summary>
    /// Response payload returned by GET /api/notifications.
    /// </summary>
    public class NotificationApiResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public List<NotificationItem> Data { get; set; } = new();

        [JsonPropertyName("pagination")]
        public NotificationPagination? Pagination { get; set; }

        [JsonPropertyName("unread_count")]
        public int UnreadCount { get; set; }
    }

    /// <summary>
    /// Response payload returned by GET /api/notifications/unread-count.
    /// </summary>
    public class NotificationUnreadCountResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public NotificationUnreadCountData Data { get; set; } = new();
    }

    public class NotificationUnreadCountData
    {
        [JsonPropertyName("unread_count")]
        public int UnreadCount { get; set; }
    }

    /// <summary>
    /// Optional pagination metadata for cursor-based notification lists.
    /// </summary>
    public class NotificationPagination
    {
        [JsonPropertyName("per_page")]
        public int PerPage { get; set; }

        [JsonPropertyName("next_cursor")]
        public string? NextCursor { get; set; }

        [JsonPropertyName("prev_cursor")]
        public string? PrevCursor { get; set; }

        [JsonPropertyName("has_more_pages")]
        public bool HasMorePages { get; set; }
    }
}

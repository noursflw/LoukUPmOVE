using System.Collections.Generic;

namespace loukupm.Model.ApiResponses
{
    /// <summary>
    /// Wrapper for API responses with pagination support
    /// </summary>
    public class NotificationApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<Notification> Data { get; set; } = new();
        public NotificationPagination Pagination { get; set; }
        public int UnreadCount { get; set; }
    }

    /// <summary>
    /// Pagination metadata from API
    /// </summary>
    public class NotificationPagination
    {
        public int PerPage { get; set; }
        public string NextCursor { get; set; }
        public string PrevCursor { get; set; }
        public bool HasMorePages { get; set; }
    }
}

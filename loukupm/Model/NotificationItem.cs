using System;

namespace loukupm.Model
{
    /// <summary>
    /// NotificationItem model representing a single notification from the API
    /// With support for nullable read_at timestamp
    /// </summary>
    public class NotificationItem
    {
        /// <summary>
        /// Unique identifier for the notification (string)
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Notification title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Notification message content
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when notification was read (nullable)
        /// Null if notification has not been read yet
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// UTC timestamp when notification was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Whether the notification has been read
        /// </summary>
        public bool IsRead => ReadAt.HasValue;

        /// <summary>
        /// Formatted date and time display (e.g., "25/11/2024 14:30")
        /// </summary>
        public string FormattedDateTime => CreatedAt.ToString("dd/MM/yyyy HH:mm");

        /// <summary>
        /// Formatted date only (e.g., "25/11/2024")
        /// </summary>
        public string FormattedDate => CreatedAt.ToString("dd/MM/yyyy");

        /// <summary>
        /// Formatted time only (e.g., "14:30")
        /// </summary>
        public string FormattedTime => CreatedAt.ToString("HH:mm");

        /// <summary>
        /// Relative time display (e.g., "2 hours ago")
        /// </summary>
        public string RelativeTime
        {
            get
            {
                var diff = DateTime.UtcNow - CreatedAt;

                if (diff.TotalSeconds < 60)
                    return "just now";
                else if (diff.TotalMinutes < 60)
                    return $"{(int)diff.TotalMinutes}m ago";
                else if (diff.TotalHours < 24)
                    return $"{(int)diff.TotalHours}h ago";
                else if (diff.TotalDays < 7)
                    return $"{(int)diff.TotalDays}d ago";
                else
                    return FormattedDate;
            }
        }

        /// <summary>
        /// Read status display
        /// </summary>
        public string ReadStatus => IsRead ? "Read" : "Unread";
    }
}

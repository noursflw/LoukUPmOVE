using System;

namespace loukupm.Model
{
    /// <summary>
    /// Notification model representing a single notification from the API
    /// </summary>
    public class Notification
    {
        public int Id { get; set; }

        /// <summary>
        /// Notification title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Notification message content
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when notification was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Whether the notification has been read
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// Type of notification (optional - for future filtering)
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Formatted date and time display (e.g., "25 Nov 2024 14:30")
        /// </summary>
        public string FormattedDateTime
        {
            get => CreatedAt.ToString("dd MMM yyyy HH:mm");
        }

        /// <summary>
        /// Formatted date only (e.g., "25 Nov 2024")
        /// </summary>
        public string FormattedDate
        {
            get => CreatedAt.ToString("dd MMM yyyy");
        }

        /// <summary>
        /// Formatted time only (e.g., "14:30")
        /// </summary>
        public string FormattedTime
        {
            get => CreatedAt.ToString("HH:mm");
        }

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
    }
}

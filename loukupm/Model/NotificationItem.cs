using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Text.Json.Serialization;

namespace loukupm.Model
{
    /// <summary>
    /// Notification item returned by the backend API.
    /// </summary>
    public partial class NotificationItem : ObservableObject
    {
        [ObservableProperty]
        [JsonPropertyName("id")]
        private string id = string.Empty;

        [ObservableProperty]
        [JsonPropertyName("title")]
        private string title = string.Empty;

        [ObservableProperty]
        [JsonPropertyName("message")]
        private string message = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsRead))]
        [NotifyPropertyChangedFor(nameof(ReadStatus))]
        [JsonPropertyName("read_at")]
        private DateTime? readAt;

        [ObservableProperty]
        [JsonPropertyName("created_at")]
        private DateTime createdAt;

        public bool IsRead => ReadAt != null;

        public string FormattedDateTime => CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

        public string FormattedDate => CreatedAt.ToLocalTime().ToString("dd/MM/yyyy");

        public string FormattedTime => CreatedAt.ToLocalTime().ToString("HH:mm");

        public string RelativeTime
        {
            get
            {
                var createdAtUtc = CreatedAt.Kind == DateTimeKind.Utc ? CreatedAt : CreatedAt.ToUniversalTime();
                var diff = DateTime.UtcNow - createdAtUtc;

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

        public string ReadStatus => IsRead ? "Read" : "Unread";
    }
}

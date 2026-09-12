using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace loukupm.Model
{
    /// <summary>
    /// Represents a reminder duration option returned by the API.
    /// </summary>
    public class ReminderOption
    {
        [JsonPropertyName("offset_hours")]
        public int OffsetHours { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        public override string ToString() => Label;
    }

    /// <summary>
    /// Represents the reminder configuration options response from the API.
    /// </summary>
    public class ReminderOptionsResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public ReminderOptionsData Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Contains the reminder options and texts from the API.
    /// </summary>
    public class ReminderOptionsData
    {
        [JsonPropertyName("options")]
        public List<ReminderOption> Options { get; set; } = new();

        [JsonPropertyName("default_offset_hours")]
        public int DefaultOffsetHours { get; set; }

        [JsonPropertyName("texts")]
        public ReminderTexts Texts { get; set; }
    }

    /// <summary>
    /// Reminder UI texts from the API.
    /// </summary>
    public class ReminderTexts
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("subtitle")]
        public string Subtitle { get; set; } = string.Empty;

        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a single reminder for an appointment.
    /// </summary>
    public class Reminder
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("appointment_id")]
        public int AppointmentId { get; set; }

        [JsonPropertyName("remind_at")]
        public string RemindAt { get; set; }

        [JsonPropertyName("offset_hours")]
        public int? OffsetHours { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }

        [JsonPropertyName("sent_at")]
        public string SentAt { get; set; }

        [JsonPropertyName("cancelled_at")]
        public string CancelledAt { get; set; }

        [JsonPropertyName("delivered_channels")]
        public List<string> DeliveredChannels { get; set; } = new();

        [JsonPropertyName("channels")]
        public ReminderChannels Channels { get; set; }

        [JsonPropertyName("active_channels")]
        public List<string> ActiveChannels { get; set; } = new();

        [JsonPropertyName("has_active_channel")]
        public bool HasActiveChannel { get; set; }
    }

    /// <summary>
    /// Represents the status of notification channels for a reminder.
    /// </summary>
    public class ReminderChannels
    {
        [JsonPropertyName("push")]
        public ChannelStatus Push { get; set; }

        [JsonPropertyName("email")]
        public ChannelStatus Email { get; set; }

        [JsonPropertyName("sms")]
        public ChannelStatus SMS { get; set; }
    }

    /// <summary>
    /// Status of a specific notification channel.
    /// </summary>
    public class ChannelStatus
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("deliverable")]
        public bool Deliverable { get; set; }

        [JsonPropertyName("effective")]
        public bool Effective { get; set; }
    }

    /// <summary>
    /// Request to create or update a reminder.
    /// </summary>
    public class CreateReminderRequest
    {
        [JsonPropertyName("appointment_id")]
        public int AppointmentId { get; set; }

        [JsonPropertyName("offset_hours")]
        public int OffsetHours { get; set; }
    }

    /// <summary>
    /// Response from reminder API operations.
    /// </summary>
    public class ReminderResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public Reminder Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("errors")]
        public Dictionary<string, List<string>> Errors { get; set; }

        [JsonPropertyName("error_type")]
        public string ErrorType { get; set; }
    }
}

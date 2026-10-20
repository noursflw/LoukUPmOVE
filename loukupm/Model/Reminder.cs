using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
    /// Includes channel information and active channel status.
    /// </summary>
    public class ReminderOptionsData
    {
        [JsonPropertyName("options")]
        public List<ReminderOption> Options { get; set; } = new();

        [JsonPropertyName("default_offset_hours")]
        public int DefaultOffsetHours { get; set; }

        [JsonPropertyName("texts")]
        public ReminderTexts Texts { get; set; }

        /// <summary>
        /// Indicates if user has at least one active notification channel.
        /// If false, reminder is saved but user won't receive notifications.
        /// </summary>
        [JsonPropertyName("has_active_channel")]
        public bool HasActiveChannel { get; set; }

        /// <summary>
        /// Raw channels payload (API may return either an array of channel objects or an object with named keys).
        /// Use GetChannels() to obtain a normalized list of ChannelInfo.
        /// </summary>
        [JsonPropertyName("channels")]
        public JsonElement ChannelsRaw { get; set; }

        /// <summary>
        /// Normalize channels payload into a list of ChannelInfo regardless of original shape.
        /// </summary>
        public List<ChannelInfo> GetChannels()
        {
            var list = new List<ChannelInfo>();
            try
            {
                if (ChannelsRaw.ValueKind == JsonValueKind.Array)
                {
                    list = JsonSerializer.Deserialize<List<ChannelInfo>>(ChannelsRaw.GetRawText());
                }
                else if (ChannelsRaw.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in ChannelsRaw.EnumerateObject())
                    {
                        try
                        {
                            var c = prop.Value;
                            var info = new ChannelInfo
                            {
                                Type = prop.Name,
                                Enabled = c.TryGetProperty("enabled", out var en) && en.GetBoolean(),
                                Deliverable = c.TryGetProperty("deliverable", out var dl) && dl.GetBoolean(),
                                Effective = c.TryGetProperty("effective", out var ef) && ef.GetBoolean()
                            };
                            if (!info.Effective)
                                info.Effective = info.Enabled && info.Deliverable;
                            list.Add(info);
                        }
                        catch { }
                    }
                }
            }
            catch { }

            return list ?? new List<ChannelInfo>();
        }

        public bool ResolveHasActiveChannel()
        {
            var channels = GetChannels();
            if (channels.Count > 0)
                return channels.Any(c => c.Effective || (c.Enabled && c.Deliverable));
            return HasActiveChannel;
        }
    }

    /// <summary>
    /// Information about a notification channel (push, SMS, email).
    /// </summary>
    public class ChannelInfo
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("deliverable")]
        public bool Deliverable { get; set; }

        [JsonPropertyName("effective")]
        public bool Effective { get; set; }
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
        public List<string> DeliveredChannels { get; set; }

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

        [JsonPropertyName("channels")]
        public ReminderChannels Channels { get; set; }

        public string FirstErrorMessage()
        {
            if (Errors != null)
            {
                foreach (var pair in Errors)
                {
                    if (pair.Value != null && pair.Value.Count > 0 && !string.IsNullOrWhiteSpace(pair.Value[0]))
                        return pair.Value[0];
                }
            }

            return Message ?? string.Empty;
        }
    }

    /// <summary>
    /// Holds the most recent options returned by GET /api/appointments/reminders/options
    /// so screens can render server labels instead of hardcoded ones.
    /// </summary>
    public static class ReminderOptionsCache
    {
        private static List<ReminderOption> _options = new();

        public static IReadOnlyList<ReminderOption> Options => _options;

        public static void Set(IEnumerable<ReminderOption> options)
        {
            _options = options?.ToList() ?? new List<ReminderOption>();
        }

        public static string LabelFor(int offsetHours)
        {
            return _options.FirstOrDefault(o => o.OffsetHours == offsetHours)?.Label ?? string.Empty;
        }
    }

    public static class ReminderChannelHelper
    {
        public static List<string> BuildWarnings(ReminderChannels channels, bool hasActiveChannel)
        {
            var warnings = new List<string>();
            var lang = GetLanguageCode();

            if (!hasActiveChannel)
                warnings.Add(NoActiveChannelText(lang));

            if (channels == null)
                return warnings;

            if (channels.Push != null && channels.Push.Enabled && !channels.Push.Deliverable)
                warnings.Add(PushUndeliverableText(lang));
            if (channels.Email != null && channels.Email.Enabled && !channels.Email.Deliverable)
                warnings.Add(EmailUndeliverableText(lang));
            if (channels.SMS != null && channels.SMS.Enabled && !channels.SMS.Deliverable)
                warnings.Add(SmsUndeliverableText(lang));

            return warnings;
        }

        public static List<string> BuildWarnings(IEnumerable<ChannelInfo> channels, bool hasActiveChannel)
        {
            var warnings = new List<string>();
            var lang = GetLanguageCode();

            if (!hasActiveChannel)
                warnings.Add(NoActiveChannelText(lang));

            if (channels == null)
                return warnings;

            foreach (var channel in channels)
            {
                if (channel == null || !channel.Enabled || channel.Deliverable)
                    continue;

                switch ((channel.Type ?? string.Empty).ToLowerInvariant())
                {
                    case "push":
                        warnings.Add(PushUndeliverableText(lang));
                        break;
                    case "sms":
                        warnings.Add(SmsUndeliverableText(lang));
                        break;
                    case "email":
                        warnings.Add(EmailUndeliverableText(lang));
                        break;
                }
            }

            return warnings;
        }

        public static string GetLanguageCode()
        {
            try
            {
                var saved = Microsoft.Maui.Storage.Preferences.Get("AppLanguage", "en-US");
                var code = (saved ?? "en").Split('-')[0].ToLowerInvariant();
                return code is "ar" or "de" or "en" ? code : "en";
            }
            catch
            {
                return "en";
            }
        }

        public static string TurnOffText() => GetLanguageCode() switch
        {
            "ar" => "إيقاف التذكير",
            "de" => "Erinnerung ausschalten",
            _ => "Turn reminder off"
        };

        public static string DismissText() => GetLanguageCode() switch
        {
            "ar" => "إلغاء",
            "de" => "Abbrechen",
            _ => "Cancel"
        };

        private static string NoActiveChannelText(string lang) => lang switch
        {
            "ar" => "تم حفظ التذكير، لكن لا توجد قنوات إشعار فعّالة. فعّل قناة من الإعدادات.",
            "de" => "Erinnerung gespeichert, aber kein aktiver Benachrichtigungskanal. Aktiviere einen Kanal in den Einstellungen.",
            _ => "Reminder saved, but no active notification channels are available. Enable a channel in Settings."
        };

        private static string PushUndeliverableText(string lang) => lang switch
        {
            "ar" => "تذكير الدفع مفعّل، لكن الجهاز غير مسجّل. فعّل الإشعارات في إعدادات النظام.",
            "de" => "Push-Erinnerungen sind aktiv, aber kein Gerät registriert. Bitte Benachrichtigungen erlauben.",
            _ => "Push reminders are enabled, but this device is not registered. Enable notifications and register the device."
        };

        private static string EmailUndeliverableText(string lang) => lang switch
        {
            "ar" => "تذكير البريد مفعّل، لكن الحساب بلا بريد إلكتروني.",
            "de" => "E-Mail-Erinnerungen sind aktiv, aber dem Konto fehlt eine E-Mail-Adresse.",
            _ => "Email reminders are enabled, but your account lacks an email address."
        };

        private static string SmsUndeliverableText(string lang) => lang switch
        {
            "ar" => "تذكير الرسائل مفعّل، لكن الحساب بلا رقم هاتف. أضف رقماً لاستلام الرسائل.",
            "de" => "SMS-Erinnerungen sind aktiv, aber dem Konto fehlt eine Telefonnummer.",
            _ => "SMS reminders are enabled, but your account lacks a phone number. Add a phone number to receive SMS reminders."
        };
    }
}

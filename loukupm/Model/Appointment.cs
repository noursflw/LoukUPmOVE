using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace loukupm.Model
{
    public class AppointmentResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public AppointmentDataWrapper Data { get; set; }
    }

    public class AppointmentDataWrapper
    {
        [JsonPropertyName("data")]
        public List<Appointment> Data { get; set; }
    }

    public class Appointment
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("number")]
        public string Number { get; set; }

        [JsonPropertyName("appointment_date")]
        public string AppointmentDate { get; set; }

        [JsonPropertyName("formatted_date")]
        public string FormattedDate { get; set; }

        [JsonPropertyName("start_time")]
        public string StartTime { get; set; }

        [JsonPropertyName("end_time")]
        public string EndTime { get; set; }

        [JsonPropertyName("time_range")]
        public string TimeRange { get; set; }

        [JsonPropertyName("duration_minutes")]
        public int DurationMinutes { get; set; }

        [JsonPropertyName("formatted_duration")]
        public string FormattedDuration { get; set; }

        [JsonPropertyName("subtotal")]
        public decimal Subtotal { get; set; }

        [JsonPropertyName("tax_amount")]
        public decimal TaxAmount { get; set; }

        [JsonPropertyName("total_amount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("status_value")]
        public int StatusValue { get; set; }

        [JsonPropertyName("status_label")]
        public string StatusLabel { get; set; }

        [JsonPropertyName("payment_status")]
        public string PaymentStatus { get; set; }

        [JsonPropertyName("payment_status_value")]
        public int PaymentStatusValue { get; set; }

        [JsonPropertyName("payment_status_label")]
        public string PaymentStatusLabel { get; set; }

        [JsonPropertyName("payment_method")]
        public string PaymentMethod { get; set; }

        [JsonPropertyName("cancellation_reason")]
        public string CancellationReason { get; set; }

        [JsonPropertyName("cancelled_at")]
        public string CancelledAt { get; set; }

        [JsonPropertyName("provider")]
        public Provider Provider { get; set; }

        [JsonPropertyName("services_details")]
        public List<ServiceDetail> ServicesDetails { get; set; }

        [JsonPropertyName("notes")]
        public string Notes { get; set; }

        [JsonPropertyName("note")]
        public string Note { get; set; }

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public string UpdatedAt { get; set; }

        [JsonPropertyName("is_upcoming")]
        public bool IsUpcoming { get; set; }

        [JsonPropertyName("is_past")]
        public bool IsPast { get; set; }

        [JsonPropertyName("is_cancelled")]
        public bool IsCancelled { get; set; }

        [JsonPropertyName("is_completed")]
        public bool IsCompleted { get; set; }

        [JsonPropertyName("can_cancel")]
        public bool CanCancel { get; set; }

        [JsonPropertyName("reminder")]
        public Reminder Reminder { get; set; }

        public string Stutes =>
            StatusLabel ?? Status ?? "Unknown";

        public string Date =>
            FormattedDate ?? AppointmentDate ?? "N/A";

        public string Time =>
            TimeRange ?? $"{StartTime} - {EndTime}";

        public string PriceBooking =>
            $"{FormattedDuration ?? ""}";

        public string UserName =>
            Provider?.DisplayName ?? "Unknown";

        public string ImgePerson
        {
            get
            {
                if (Provider == null)
                    return "imagesafe.png";

                string processedUrl = Provider.ImageUrl;

                if (string.IsNullOrWhiteSpace(processedUrl))
                    return "imagesafe.png";

                if (processedUrl.Contains("'"))
                {
                    processedUrl = processedUrl.Replace("'", "%27");
                }

                if (processedUrl.Contains("\""))
                {
                    processedUrl = processedUrl.Replace("\"", "%22");
                }

                if (processedUrl.Contains(" "))
                {
                    processedUrl = processedUrl.Replace(" ", "%20");
                }

                return processedUrl;
            }
        }

        public string Total =>
            $"{TotalAmount:F2}";

        public string TimePrice =>
            $"{FormattedDuration ?? ""}";

        public string ServiceName =>
            ServicesDetails?.FirstOrDefault()?.ServiceName ?? "N/A";
    }

    public class Provider
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("full_name")]
        public string FullName { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonPropertyName("profile_image_url")]
        public string ProfileImageUrl { get; set; }

        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }

        public string DisplayName =>
            !string.IsNullOrWhiteSpace(FullName)
                ? FullName
                : !string.IsNullOrWhiteSpace(Name)
                    ? Name
                    : "N/A";

        public string ImageUrl
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ProfileImageUrl))
                    return ProfileImageUrl;

                if (!string.IsNullOrWhiteSpace(AvatarUrl))
                    return AvatarUrl;

                if (!string.IsNullOrWhiteSpace(Avatar))
                    return Avatar;

                return "imagesafe.png";
            }
        }
    }

    public class ServiceDetail
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("service_id")]
        public int ServiceId { get; set; }

        [JsonPropertyName("service_name")]
        public string ServiceName { get; set; }

        [JsonPropertyName("duration_minutes")]
        public int DurationMinutes { get; set; }

        [JsonPropertyName("formatted_duration")]
        public string FormattedDuration { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("formatted_price")]
        public string FormattedPrice { get; set; }

        [JsonPropertyName("sequence_order")]
        public int SequenceOrder { get; set; }

        [JsonPropertyName("provider")]
        public Provider Provider { get; set; }

        public string ProviderName =>
            Provider?.DisplayName ?? "N/A";

        public string ProviderImage
        {
            get
            {
                if (Provider == null)
                    return "imagesafe.png";

                string processedUrl = Provider.ImageUrl;

                if (string.IsNullOrWhiteSpace(processedUrl))
                    return "imagesafe.png";

                if (processedUrl.Contains("'"))
                {
                    processedUrl = processedUrl.Replace("'", "%27");
                }

                if (processedUrl.Contains("\""))
                {
                    processedUrl = processedUrl.Replace("\"", "%22");
                }

                if (processedUrl.Contains(" "))
                {
                    processedUrl = processedUrl.Replace(" ", "%20");
                }

                return processedUrl;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace loukupm.Model
{
    // «·«” Ã«»… «·⁄«„…
    public class AvailabilityResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public AvailabilityData Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    // »Ì«‰«  «· Ê›—
    public class AvailabilityData
    {
        [JsonPropertyName("provider")]
        public ProviderInfo Provider { get; set; }

        [JsonPropertyName("service")]
        public ServiceInfo Service { get; set; }

        [JsonPropertyName("pricing")]
        public PricingInfo Pricing { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("day_name")]
        public string DayName { get; set; }

        [JsonPropertyName("formatted_date")]
        public string FormattedDate { get; set; }

        [JsonPropertyName("total_slots")]
        public int TotalSlots { get; set; }

        [JsonPropertyName("available_slots")]
        public List<AvailableSlot> AvailableSlots { get; set; }
    }

    // „⁄·Ê„«  «·»—Ê›«Ìœ—
    public class ProviderInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("branch")]
        public BranchInfo Branch { get; set; }
    }

    // „⁄·Ê„«  «·›—⁄
    public class BranchInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("coordinates")]
        public CoordinatesInfo Coordinates { get; set; }
    }

    // «·≈Õœ«ÀÌ«  «·Ã€—«›Ì…
    public class CoordinatesInfo
    {
        [JsonPropertyName("latitude")]
        public string Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public string Longitude { get; set; }
    }

    // „⁄·Ê„«  «·Œœ„…
    public class ServiceInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("duration_minutes")]
        public int DurationMinutes { get; set; }

        [JsonPropertyName("formatted_duration")]
        public string FormattedDuration { get; set; }
    }

    // „⁄·Ê„«  «· ”⁄Ì—
    public class PricingInfo
    {
        [JsonPropertyName("original_price")]
        public decimal OriginalPrice { get; set; }

        [JsonPropertyName("effective_price")]
        public decimal EffectivePrice { get; set; }

        [JsonPropertyName("has_discount")]
        public bool HasDiscount { get; set; }

        [JsonPropertyName("discount_amount")]
        public decimal DiscountAmount { get; set; }

        [JsonPropertyName("discount_percentage")]
        public decimal DiscountPercentage { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("formatted_price")]
        public string FormattedPrice { get; set; }
    }

    // «·‹ Slot «·Ê«Õœ
    public class AvailableSlot
    {
        [JsonPropertyName("start_time")]
        public string StartTime { get; set; }

        [JsonPropertyName("end_time")]
        public string EndTime { get; set; }

        [JsonPropertyName("start_time_formatted")]
        public string StartTimeFormatted { get; set; }

        [JsonPropertyName("end_time_formatted")]
        public string EndTimeFormatted { get; set; }

        [JsonPropertyName("display_time")]
        public string DisplayTime { get; set; }

        [JsonPropertyName("duration_minutes")]
        public int DurationMinutes { get; set; }
    }
}

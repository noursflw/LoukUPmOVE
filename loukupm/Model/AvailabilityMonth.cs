using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace loukupm.Model
{
    public class AvailabilityMonthResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public AvailabilityMonthData Data { get; set; }
    }

    public class AvailabilityMonthData
    {
        [JsonPropertyName("month")]
        public string Month { get; set; }

        [JsonPropertyName("previous_month")]
        public string PreviousMonth { get; set; }

        [JsonPropertyName("next_month")]
        public string NextMonth { get; set; }

        [JsonPropertyName("days")]
        public List<AvailabilityMonthDay> Days { get; set; } = new();
    }

    public class AvailabilityMonthDay
    {
        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("is_available")]
        public bool IsAvailable { get; set; }

        [JsonPropertyName("day_number")]
        public int DayNumber { get; set; }
    }
}

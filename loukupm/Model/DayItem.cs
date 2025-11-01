using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace loukupm.Model
{
   public class DayItem
    {
        public DayItem() { }
        public string Date { get; set; }
        public string Day { get; set; }
        public bool IsAvailable { get; set; } = false;
        public string BorderColor { get; set; } = "#444444";
        public DateTime FullDate { get; set; }
    }


    public class SlotItem
    {
        [JsonPropertyName("start_time")]
        public string StartTime { get; set; }

        [JsonPropertyName("end_time")]
        public string EndTime { get; set; }

        [JsonPropertyName("display_time")]
        public string DisplayTime { get; set; }

        [JsonPropertyName("duration_minutes")]
        public int DurationMinutes { get; set; }
    }
    public class AvailabilityResponseWrapper
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public AvailabilityResponse Data { get; set; }
    }

    public class AvailabilityResponse
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("day_name")]
        public string DayName { get; set; }

        [JsonPropertyName("available_slots")]
        public List<SlotItem> AvailableSlots { get; set; }
    }

}

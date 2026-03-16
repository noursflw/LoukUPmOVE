using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace loukupm.Model
{
    public partial class SlotItem : ObservableObject
    {
        [JsonPropertyName("start_time")]
        public string StartTime { get; set; }

        [JsonPropertyName("end_time")]
        public string EndTime { get; set; }

        [JsonPropertyName("display_time")]
        public string DisplayTime { get; set; }

        [JsonPropertyName("duration_minutes")]
        public int DurationMinutes { get; set; }

        /// <summary>
        /// property لتتبع إذا كان الوقت مختار
        /// </summary>
        [ObservableProperty]
        private bool isSelected = false;
    }
}


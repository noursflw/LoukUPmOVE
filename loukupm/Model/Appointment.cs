using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace loukupm.Model
{
    // الاستجابة العامة من الـ API
    public class AppointmentResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public AppointmentDataContainer Data { get; set; }
    }

    // تحتوي على قائمة الحجوزات فقط
    public class AppointmentDataContainer
    {
        [JsonPropertyName("data")]
        public List<Appointment> Data { get; set; }
    }

    // تمثيل عنصر الحجز الواحد
    public class Appointment
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; }

        [JsonPropertyName("user_name")]
        public string UserName { get; set; }

        [JsonPropertyName("service_name")]
        public string ServiceName { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}

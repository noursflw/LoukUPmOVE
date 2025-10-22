using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace loukupm.Model
{
    // كلاس فريق العمل
    public class WorkTeam
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("full_name")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public string? Description { get; set; } // يمكنك تعديل المعنى إذا أردت لاحقًا

        [JsonPropertyName("phone")]
        public string? Job { get; set; } // مبدئيًا نربطه كـ Job لعدم وجود حقل مطابق

        [JsonPropertyName("avatar_url")]
        public string? Image { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime WorkTime { get; set; }

        public WorkTeam() { }

        public WorkTeam(int id, string name, string description, string job, string image, DateTime workTime)
        {
            Id = id;
            Name = name;
            Description = description;
            Job = job;
            Image = image;
            WorkTime = workTime;
        }
    }

    // كلاس الغلاف (الـ wrapper)
    public class WorkTeamWrapper
    {
        [JsonPropertyName("data")]
        public List<WorkTeam> Data { get; set; }
    }
}

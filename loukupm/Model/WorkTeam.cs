using loukupm.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace loukupm.Model
    {
    //كلاس فريق العمل
    public class WorkTeam
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("firstname")]
        public string Name { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("job")]
        public string? job { get; set; }
        [JsonPropertyName("image")]
        public string? Image { get; set; }
        [JsonPropertyName("workTime")]
        public DateTime WorkTime { get; set; }
        public WorkTeam() { }
       public WorkTeam(int id, string name, string description, string job, string image, DateTime workTime)
        {
            Id = id;
            Name = name;
            Description = description;
            job = job;
            Image = image;
            WorkTime = workTime;
        }
    }
}
public class Root
{
    [JsonPropertyName("workers")]
    public List<WorkTeam> Workers { get; set; }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loukupm.Model
    {
    //كلاس فريق العمل
    public class WorkTeam
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? job { get; set; }
        public string Image { get; set; }
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

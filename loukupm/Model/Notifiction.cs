using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loukupm.Model
{
    public class Notifiction
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string TextNotifiction { get; set; }
        public DateTime TimeandMonth { get; set; }
        public DateTime Time { get; set; }  
        public Notifiction() { }
    }
}

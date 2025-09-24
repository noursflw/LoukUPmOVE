using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loukupm.Model
{
    //كلاس المواعيد
    public class Booking
    {
        public int Id { get; set; }
        public string Monthbooking { get; set; }    
        public string TimeBooking { get; set; }
        public DateTime PriceBooking { get; set; }
        public int Total { get; set; }
        public DateTime TimePrice { get; set; }
        public string? ImagePerson { get; set; }
        public string NamePerson { get; set; }
        public Booking() { }    
    }
}

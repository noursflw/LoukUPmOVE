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
        public string ServiceType { get; set; }
        public string ServiceName { get; set; }
        public string ProviderId { get; set; }
        public string ProviderName { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string PaymentMethod { get; set; }
    }
}

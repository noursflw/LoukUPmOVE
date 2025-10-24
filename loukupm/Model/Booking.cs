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
        public List<Servies> SelectedServices { get; set; } = new();

        public string ProviderId { get; set; }
        public string ProviderName { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }



        // الدفع
        public string PaymentMethod { get; set; }   // "Cash" أو "Card"
        public string CardHolderName { get; set; }      // من واجهة الدفع
        public string CardNumber { get; set; }          // يمكن إخفاؤه جزئياً
        public string ExpirationDate { get; set; }      // MM/YYYY
        public string CVV { get; set; }                 // لا يُرسل إن لم يُستخدم Stripe
        public string StripePaymentId { get; set; }
    }
}

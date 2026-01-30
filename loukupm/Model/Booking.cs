using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loukupm.Model
{
  
    public class Booking
    {
        public string ServiceType { get; set; }
        public string ServiceName { get; set; }
        public List<Servies> SelectedServices { get; set; } = new();

        public string ProviderId { get; set; }
        public string ProviderName { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }



        
        public string PaymentMethod { get; set; } 
        public string CardHolderName { get; set; }      
        public string CardNumber { get; set; }     
        public string ExpirationDate { get; set; }    
        public string CVV { get; set; }                 
        public string StripePaymentId { get; set; }
    }
}

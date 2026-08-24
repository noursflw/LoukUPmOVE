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

        // Convert current Booking instance into a payload matching API /api/bookings
        // Supports multiple services. Caller must ensure SelectedServices contains distinct services
        public object ToCreateBookingPayload()
        {
            var servicesPayload = new List<object>();

            foreach (var s in SelectedServices)
            {
                // Each selected service must provide a provider id and a start time.
                // The app-level Booking model stores ProviderId/Time at top-level for simple flows,
                // but when multiple services are used, each service should carry its own provider and start_time.
                // Here we assume ProviderId and Time are set on the Booking for single-service flow.
                servicesPayload.Add(new
                {
                    service_id = s.Id,
                    provider_id = int.TryParse(ProviderId, out var pid) ? pid : 0,
                    start_time = Time.ToString("hh\\:mm")
                });
            }

            return new
            {
                date = Date.ToString("yyyy-MM-dd"),
                payment_method = string.IsNullOrWhiteSpace(PaymentMethod) ? "cash" : PaymentMethod,
                services = servicesPayload
            };
        }
    }
}

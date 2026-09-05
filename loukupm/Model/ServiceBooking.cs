using System;

namespace loukupm.Model
{
    /// <summary>
    /// Represents a single service booking with its own provider and slot information.
    /// Used for service-based booking where each service can have an independent provider.
    /// </summary>
    public class ServiceBooking
    {
        /// <summary>
        /// The service being booked (required)
        /// </summary>
        public Servies Service { get; set; }

        /// <summary>
        /// The provider offering this service (required)
        /// </summary>
        public WorkTeam Provider { get; set; }

        /// <summary>
        /// The slot for this service booking (required)
        /// </summary>
        public SlotItem Slot { get; set; }

        /// <summary>
        /// Service duration in minutes (if different from default)
        /// </summary>
        public int DurationMinutes { get; set; } = 0;

        /// <summary>
        /// Calculated price for this service
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Booking date for this service
        /// </summary>
        public DateTime BookingDate { get; set; }

        /// <summary>
        /// Creates a service booking payload for API submission
        /// </summary>
        /// <returns>Object representing service booking in API format</returns>
        public object ToServicePayload()
        {
            var startTime = Slot?.StartTime ?? "00:00";

            return new
            {
                service_id = Service?.Id ?? 0,
                provider_id = Provider?.Id ?? 0,
                start_time = startTime
            };
        }

        /// <summary>
        /// Validates that all required fields are set
        /// </summary>
        /// <returns>True if all required fields are valid</returns>
        public bool IsValid()
        {
            return Service != null && 
                   Service.Id > 0 &&
                   Provider != null && 
                   Provider.Id > 0 &&
                   Slot != null && 
                   !string.IsNullOrWhiteSpace(Slot.StartTime) &&
                   BookingDate != default;
        }

        /// <summary>
        /// Creates a friendly display name for this booking
        /// </summary>
        /// <returns>String representation of the booking</returns>
        public override string ToString()
        {
            return $"{Service?.NameServies} with {Provider?.Name} on {BookingDate:yyyy-MM-dd} at {Slot?.StartTime}";
        }
    }
}

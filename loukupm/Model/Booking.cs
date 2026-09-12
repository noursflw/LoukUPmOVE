using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace loukupm.Model
{
    public class BookingServicePayloadItem
    {
        public int service_id { get; set; }
        public int provider_id { get; set; }
        public string start_time { get; set; }
    }

    public class Booking
    {
        public string ServiceType { get; set; }
        public string ServiceName { get; set; }
        public List<Servies> SelectedServices { get; set; } = new();

        public string ProviderId { get; set; }
        public string ProviderName { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }

        /// <summary>
        /// Mapping of service ID to provider for service-based booking.
        /// Key: Service.Id, Value: WorkTeam.Id
        /// </summary>
        public Dictionary<int, int> ServiceProviderMap { get; set; } = new();

        /// <summary>
        /// Mapping of service ID to slot start time (HH:mm).
        /// Key: Service.Id, Value: SlotItem.StartTime
        /// </summary>
        public Dictionary<int, string> ServiceSlotMap { get; set; } = new();

        /// <summary>
        /// Mapping of service ID to the date chosen for that service.
        /// Key: Service.Id, Value: DateTime (date portion)
        /// </summary>
        public Dictionary<int, DateTime> ServiceDateMap { get; set; } = new();

        /// <summary>
        /// Reminder offset in hours. If null or not set, no reminder is created.
        /// If positive integer, creates a reminder that many hours before the appointment.
        /// </summary>
        public int? ReminderOffsetHours { get; set; }

        public string PaymentMethod { get; set; }
        public string CardHolderName { get; set; }
        public string CardNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string CVV { get; set; }
        public string StripePaymentId { get; set; }

        public bool HasPerServiceSelections =>
            (ServiceProviderMap?.Count ?? 0) > 0
            || (ServiceDateMap?.Count ?? 0) > 0
            || (ServiceSlotMap?.Count ?? 0) > 0;

        public void EnsureMaps()
        {
            ServiceProviderMap ??= new Dictionary<int, int>();
            ServiceSlotMap ??= new Dictionary<int, string>();
            ServiceDateMap ??= new Dictionary<int, DateTime>();
            SelectedServices ??= new List<Servies>();
        }

        public void ClearServiceSelections()
        {
            EnsureMaps();
            ServiceProviderMap.Clear();
            ServiceSlotMap.Clear();
            ServiceDateMap.Clear();
            ProviderId = null;
            ProviderName = null;
            Date = default;
            Time = default;
            PaymentMethod = null;
        }

        public void RemoveServiceSelections(int serviceId)
        {
            EnsureMaps();
            ServiceProviderMap.Remove(serviceId);
            ServiceSlotMap.Remove(serviceId);
            ServiceDateMap.Remove(serviceId);
        }

        public static string FormatApiDate(DateTime date)
        {
            return date.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Normalizes a slot string to 24-hour HH:mm without timezone conversion.
        /// </summary>
        public static string FormatApiTime(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            raw = raw.Trim();

            if (TimeSpan.TryParse(raw, CultureInfo.InvariantCulture, out var ts))
                return $"{ts.Hours:D2}:{ts.Minutes:D2}";

            if (DateTime.TryParseExact(raw, new[] { "HH:mm", "H:mm", "HH:mm:ss", "h:mm tt", "hh:mm tt" },
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt.ToString("HH:mm", CultureInfo.InvariantCulture);

            return raw;
        }

        public bool TryGetProviderId(int serviceId, int? globalProviderId, bool allowGlobalFallback, out int providerId)
        {
            EnsureMaps();
            if (ServiceProviderMap.TryGetValue(serviceId, out providerId) && providerId > 0)
                return true;

            if (allowGlobalFallback && globalProviderId.HasValue && globalProviderId.Value > 0)
            {
                providerId = globalProviderId.Value;
                return true;
            }

            if (allowGlobalFallback && int.TryParse(ProviderId, out var parsed) && parsed > 0)
            {
                providerId = parsed;
                return true;
            }

            providerId = 0;
            return false;
        }

        public bool TryGetSlot(int serviceId, string globalSlot, bool allowGlobalFallback, out string startTime)
        {
            EnsureMaps();
            if (ServiceSlotMap.TryGetValue(serviceId, out var mapped) && !string.IsNullOrWhiteSpace(mapped))
            {
                startTime = FormatApiTime(mapped);
                return !string.IsNullOrWhiteSpace(startTime);
            }

            if (allowGlobalFallback && !string.IsNullOrWhiteSpace(globalSlot))
            {
                startTime = FormatApiTime(globalSlot);
                return !string.IsNullOrWhiteSpace(startTime);
            }

            if (allowGlobalFallback && Time != default)
            {
                startTime = $"{Time.Hours:D2}:{Time.Minutes:D2}";
                return true;
            }

            startTime = null;
            return false;
        }

        public bool TryGetServiceDate(int serviceId, DateTime? globalDate, bool allowGlobalFallback, out DateTime date)
        {
            EnsureMaps();
            if (ServiceDateMap.TryGetValue(serviceId, out var mapped) && mapped != default)
            {
                date = mapped.Date;
                return true;
            }

            if (allowGlobalFallback && globalDate.HasValue && globalDate.Value != default)
            {
                date = globalDate.Value.Date;
                return true;
            }

            if (allowGlobalFallback && Date != default)
            {
                date = Date.Date;
                return true;
            }

            date = default;
            return false;
        }

        /// <summary>
        /// Root-level API date. Backend contract uses a single booking date, not per-service dates.
        /// Returns false when per-service dates disagree.
        /// </summary>
        public bool TryResolveRootDate(IEnumerable<int> serviceIds, DateTime? globalDate, bool allowGlobalFallback, out DateTime date, out bool datesConflict)
        {
            datesConflict = false;
            date = default;
            EnsureMaps();

            var ids = serviceIds?.ToList() ?? new List<int>();
            var perServiceDates = new List<DateTime>();
            foreach (var id in ids)
            {
                if (ServiceDateMap.TryGetValue(id, out var mapped) && mapped != default)
                    perServiceDates.Add(mapped.Date);
            }

            if (perServiceDates.Count > 0)
            {
                var distinct = perServiceDates.Distinct().ToList();
                if (distinct.Count > 1)
                {
                    datesConflict = true;
                    return false;
                }

                if (perServiceDates.Count == ids.Count)
                {
                    date = distinct[0];
                    return true;
                }
            }

            if (allowGlobalFallback && globalDate.HasValue && globalDate.Value != default)
            {
                date = globalDate.Value.Date;
                return true;
            }

            if (allowGlobalFallback && Date != default)
            {
                date = Date.Date;
                return true;
            }

            return false;
        }

        public object ToCreateBookingPayload(IList<BookingServicePayloadItem> services = null, string notes = null)
        {
            EnsureMaps();
            services ??= BuildServicesPayloadFromMaps(allowGlobalFallback: false);

            var rootDate = Date != default ? FormatApiDate(Date) : FormatApiDate(DateTime.Today);
            var paymentMethod = NormalizePaymentMethod(PaymentMethod);

            System.Diagnostics.Debug.WriteLine($"[ToCreateBookingPayload] Root date: {rootDate}");
            System.Diagnostics.Debug.WriteLine($"[ToCreateBookingPayload] Payment method: {paymentMethod}");
            System.Diagnostics.Debug.WriteLine($"[ToCreateBookingPayload] Services count: {services?.Count ?? 0}");
            if (services != null)
            {
                foreach (var svc in services)
                {
                    System.Diagnostics.Debug.WriteLine($"  Service {svc.service_id}: provider={svc.provider_id}, start_time={svc.start_time}");
                }
            }

            var payload = new Dictionary<string, object>
            {
                ["date"] = rootDate,
                ["payment_method"] = paymentMethod,
                ["services"] = services
            };

            if (!string.IsNullOrWhiteSpace(notes))
                payload["notes"] = notes;

            // Include reminder_offset_hours if reminder is enabled and value is set
            if (ReminderOffsetHours.HasValue && ReminderOffsetHours.Value > 0)
            {
                payload["reminder_offset_hours"] = ReminderOffsetHours.Value;
                System.Diagnostics.Debug.WriteLine($"[ToCreateBookingPayload] Reminder offset hours: {ReminderOffsetHours.Value}");
            }

            return payload;
        }

        public List<BookingServicePayloadItem> BuildServicesPayloadFromMaps(bool allowGlobalFallback)
        {
            EnsureMaps();
            var list = new List<BookingServicePayloadItem>();
            foreach (var s in SelectedServices ?? Enumerable.Empty<Servies>())
            {
                if (!TryGetProviderId(s.Id, null, allowGlobalFallback, out var providerId))
                    continue;
                if (!TryGetSlot(s.Id, null, allowGlobalFallback, out var startTime))
                    continue;

                list.Add(new BookingServicePayloadItem
                {
                    service_id = s.Id,
                    provider_id = providerId,
                    start_time = startTime
                });
            }
            return list;
        }

        public static string NormalizePaymentMethod(string paymentMethod)
        {
            if (string.IsNullOrWhiteSpace(paymentMethod))
                return "cash";

            var value = paymentMethod.Trim().ToLowerInvariant();
            if (value == "card")
                return "card";
            if (value == "cash")
                return "cash";
            return "cash";
        }

        public object ToCreateSingleServicePayload(Servies service)
        {
            if (service == null)
                return ToCreateBookingPayload();

            TryGetProviderId(service.Id, null, allowGlobalFallback: true, out var providerId);
            TryGetSlot(service.Id, null, allowGlobalFallback: true, out var startTime);

            return new
            {
                date = FormatApiDate(Date != default ? Date : DateTime.Today),
                payment_method = NormalizePaymentMethod(PaymentMethod),
                services = new[]
                {
                    new BookingServicePayloadItem
                    {
                        service_id = service.Id,
                        provider_id = providerId,
                        start_time = startTime ?? "00:00"
                    }
                }
            };
        }
    }
}

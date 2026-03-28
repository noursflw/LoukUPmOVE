using System;
using System.Threading.Tasks;
using loukupm.Model;
using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace loukupm.services
{
    /// <summary>
    /// Service to handle appointment reminders
    /// </summary>
    public class ReminderService
    {
        private readonly HttpClient _httpClient;
        private System.Timers.Timer _reminderTimer;
        private int _reminderIntervalMinutes = 60;
        private const string REMINDER_API_URL = "https://test.center-yazan.com/api/appointments/reminders";

        public ReminderService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Start the appointment reminder timer
        /// </summary>
        public void StartReminderTimer(int intervalMinutes, Func<Task> onTimerElapsed)
        {
            _reminderIntervalMinutes = intervalMinutes > 0 ? intervalMinutes : 60;

            if (_reminderTimer != null && _reminderTimer.Enabled)
            {
                StopReminderTimer();
            }

            _reminderTimer = new System.Timers.Timer();
            _reminderTimer.Interval = 60000; // Check every 1 minute
            _reminderTimer.Elapsed += async (sender, e) => await onTimerElapsed();
            _reminderTimer.AutoReset = true;
            _reminderTimer.Enabled = true;

            Console.WriteLine($"✅ Reminder timer started with interval: {_reminderIntervalMinutes} minutes");
        }

        /// <summary>
        /// Stop the reminder timer
        /// </summary>
        public void StopReminderTimer()
        {
            if (_reminderTimer != null)
            {
                _reminderTimer.Stop();
                _reminderTimer.Dispose();
                _reminderTimer = null;
                Console.WriteLine("🛑 Reminder timer stopped");
            }
        }

        /// <summary>
        /// Send appointment reminder to API
        /// </summary>
        public async Task SendReminderAsync(Appointment appointment, int userId, int minutesUntilAppointment)
        {
            try
            {
                var reminderData = new
                {
                    appointment_id = appointment.Id,
                    provider_id = appointment.Provider?.Id ?? 0,
                    user_id = userId,
                    reminder_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    appointment_date = appointment.AppointmentDate,
                    minutes_until_appointment = minutesUntilAppointment,
                    status = "pending"
                };

                var json = JsonSerializer.Serialize(reminderData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"📤 Sending reminder for appointment {appointment.Id}:");
                Console.WriteLine($"   Time until appointment: {minutesUntilAppointment} minutes");

                var response = await _httpClient.PostAsync(REMINDER_API_URL, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"✅ Reminder sent successfully: {responseBody}");
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Failed to send reminder: {response.StatusCode} - {errorBody}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error sending reminder: {ex.Message}");
            }
        }

        /// <summary>
        /// Get reminder interval in minutes
        /// </summary>
        public int GetReminderIntervalMinutes() => _reminderIntervalMinutes;

        /// <summary>
        /// Check if timer is running
        /// </summary>
        public bool IsTimerRunning => _reminderTimer?.Enabled ?? false;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using loukupm.Model;
using System.Text.Json;
using System.Threading.Tasks;

namespace loukupm.services
{
    public class ApiServices
    {

        private readonly HttpClient _httpClient;
        public ApiServices()
        {
           _httpClient = new HttpClient();
            
        }

        private void SetAuthorizationHeader()
        {
            string token = Preferences.Get("auth_token", string.Empty);

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
        public async Task<List<WorkTeam>> GetWorkTeamsAsync()
        {
            SetAuthorizationHeader();
            var response = await _httpClient.GetAsync("https://api.example.com/workteams");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<WorkTeam>>(json);
            }
            return new List<WorkTeam>();

        }
        public async Task<List<Notifiction>> GetNotifictionsAsync()
        {
            SetAuthorizationHeader();
            var response = await _httpClient.GetAsync("https://api.example.com/notifications");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Notifiction>>(json);
            }
            return new List<Notifiction>();
        }
        public async Task<List<Booking>> GetBookingsAsync()
        {
            SetAuthorizationHeader();
            var response = await _httpClient.GetAsync("https://api.example.com/notifications");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Booking>>(json);
            }
            return new List<Booking>();
        }
        public async Task<List<User>> GetUsersAsync()
        {
            SetAuthorizationHeader();
            var response = await _httpClient.GetAsync("https://api.example.com/users");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<User>>(json);
            }
            return new List<User>();
        }
        public async Task<List<Servies>> GetServiesasync()
        {
            SetAuthorizationHeader();
            var response = await _httpClient.GetAsync("https://api.example.com/services");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Servies>>(json);
            }
            return new List<Servies>();
        }
        public async Task<bool> SubmitBookingAsync(Booking booking)
        {
            SetAuthorizationHeader();
            var json = JsonSerializer.Serialize(booking);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.example.com/bookings", content);
            return response.IsSuccessStatusCode;
        }
        public async Task<List<string>> GetCategoriesAsync()
        {
            try
            {
              
                SetAuthorizationHeader();

                var url = "https://center-yazan.com/api/sandbox/categories";
               

                var response = await _httpClient.GetAsync(url);
              

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                 

                    var result = JsonSerializer.Deserialize<CategoriesResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var categories = result?.Categories ?? new List<string>();
                 
                

                    return categories;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    
                    return new List<string>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CATEGORIES EXCEPTION] {ex.Message}");
                Console.WriteLine($"[CATEGORIES EXCEPTION StackTrace]: {ex.StackTrace}");
                return new List<string>();
            }
        }
        public async Task<List<string>> SearchServiessAsync(string searchTerm)
        {
            SetAuthorizationHeader();

            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return new List<string>();

                var encoded = Uri.EscapeDataString(searchTerm.Trim());
                var url = $"https://center-yazan.com/api/sandbox/products?search={encoded}";

                var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[SEARCH ERROR] {response.StatusCode}: {json}");
                    return new List<string>();
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // لو رجع Array مباشرة
                try
                {
                    var products = JsonSerializer.Deserialize<List<Servies>>(json, options);
                    if (products != null)
                        return products.Select(p => p.NameServies).Where(n => !string.IsNullOrEmpty(n)).ToList();
                }
                catch (JsonException)
                {
                    // لو رجع { data: [...] } أو { servies: [...] }
                    var wrapper = JsonSerializer.Deserialize<ServiesResponse>(json, options);
                    var products = wrapper?.Data ?? wrapper?.Servies ?? new List<Servies>();
                    return products.Select(p => p.NameServies).Where(n => !string.IsNullOrEmpty(n)).ToList();
                }

                return new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SEARCH EXCEPTION] {ex.Message} -- {ex.StackTrace}");
                return new List<string>();
            }
        }





    }

}
public class ServiesResponse
{
    public List<Servies> Data { get; set; }
    public List<Servies> Servies { get; set; }
}
public class CategoriesResponse
{
    public List<string> Categories { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; }
}


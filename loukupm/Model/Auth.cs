using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace loukupm.Model
{
    public class Auth
    {
        public class LoginRequest
        {
            [JsonPropertyName("email")]
            public string Email { get; set; }

            [JsonPropertyName("password")]
            public string Password { get; set; }
        }

        public class LoginResponse
        {
            [JsonPropertyName("access_token")]
            public string Token { get; set; }
            public UserData User { get; set; }
        }

        public class UserData
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
        }

        // إنشاء حساب
        public class RegisterRequest
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("email")]
            public string Email { get; set; }

            [JsonPropertyName("password")]
            public string Password { get; set; }

            [JsonPropertyName("password_confirmation")]
            public string password_confirmation { get; set; }
        }

        public class RegisterResponse
        {
            [JsonPropertyName("access_token")]
            public string Token { get; set; }

            [JsonPropertyName("user")]
            public UserData User { get; set; }

            [JsonPropertyName("status")]
            public string Status { get; set; }
        }

        // نموذج لمعالجة أخطاء 422
        public class ErrorResponse
        {
            public string Message { get; set; }

            [JsonPropertyName("errors")]
            public Dictionary<string, string[]> Errors { get; set; }
        }


    }
}

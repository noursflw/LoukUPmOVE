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

            [JsonPropertyName("refresh_token")]
            public string RefreshToken { get; set; }
        }

        public class LoginResponse
        {
            [JsonPropertyName("access_token")]
            public string Token { get; set; }
            [JsonPropertyName("refresh_token")]
            public string Refresh_Token { get; set; }

            [JsonPropertyName("user")]
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
            [JsonPropertyName("first_name")]
            public string FirstName { get; set; }

            [JsonPropertyName("last_name")]
            public string LastName { get; set; }

            [JsonPropertyName("email")]
            public string Email { get; set; }

            [JsonPropertyName("phone")]
            public string Phone { get; set; }

            [JsonPropertyName("password")]
            public string Password { get; set; }

            [JsonPropertyName("password_confirmation")]
            public string PasswordConfirmation { get; set; }
        }


        public class RegisterResponse
        {
            [JsonPropertyName("user")]
            public UserData User { get; set; }

            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }

            [JsonPropertyName("access_expires_at")]
            public string AccessExpiresAt { get; set; }

            [JsonPropertyName("refresh_token")]
            public string RefreshToken { get; set; }

            [JsonPropertyName("refresh_expires_at")]
            public string RefreshExpiresAt { get; set; }

            [JsonPropertyName("token_type")]
            public string TokenType { get; set; }

            [JsonPropertyName("otp")]
            public string Otp { get; set; }
        }


        // نموذج لمعالجة أخطاء 422
        public class ErrorResponse
        {
            public string Message { get; set; }

            [JsonPropertyName("errors")]
            public Dictionary<string, string[]> Errors { get; set; }
        }

       
        public class OTP 
        { 
            public string Email { get; set; }
            public string Code { get; set; }
        }


    }

    public class AuthResponse
    {
        public string Token { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public User User { get; set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("access_expires_at")]
        public string AccessExpiresAt { get; set; }

        [JsonPropertyName("refresh_expires_at")]
        public string RefreshExpiresAt { get; set; }
    }

}

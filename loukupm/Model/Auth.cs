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

            [JsonPropertyName("registration_method")]
            public string RegistrationMethod { get; set; }

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
            [JsonPropertyName("registration_method")]
            public string RegistrationMethod { get; set; }
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
            // 🔥 ADD THIS
            [JsonPropertyName("masked_destination")]
            public string MaskedDestination { get; set; }
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

        // OTP Context passed to the OTPSINGIN page
        public class OtpContext
        {
            [JsonPropertyName("email")]
            public string Email { get; set; }

            [JsonPropertyName("phone")]
            public string Phone { get; set; }

            [JsonPropertyName("registration_method")]
            public string RegistrationMethod { get; set; }

            [JsonPropertyName("masked_destination")]
            public string MaskedDestination { get; set; }

            [JsonPropertyName("user_id")]
            public int? UserId { get; set; }
        }

        /// <summary>
        /// OTP Verification Request sent to the backend.
        /// 
        /// REQUIRED FIELDS:
        /// - Otp: The 6-digit OTP code sent to the user (NOT "code")
        /// - RegistrationMethod: Either "email" or "phone" indicating which method was used for registration
        /// 
        /// EMAIL vs PHONE:
        /// - If RegistrationMethod == "email": Backend validates using Email field, Phone can be null
        /// - If RegistrationMethod == "phone": Backend validates using Phone field, Email can be null
        /// - Always send the relevant field based on RegistrationMethod to avoid 422 errors
        /// 
        /// COMMON ISSUE: Sending "code" instead of "otp" will result in 422 Unprocessable Entity error
        /// </summary>
        public class OtpVerificationRequest
        {
            /// <summary>
            /// Email address (required if RegistrationMethod is "email", can be null otherwise)
            /// </summary>
            [JsonPropertyName("email")]
            public string Email { get; set; }

            /// <summary>
            /// Phone number (required if RegistrationMethod is "phone", can be null otherwise)
            /// </summary>
            [JsonPropertyName("phone")]
            public string Phone { get; set; }

            /// <summary>
            /// The 6-digit OTP code. IMPORTANT: Use "otp" field, NOT "code"
            /// </summary>
            [JsonPropertyName("otp")]
            public string Otp { get; set; }

            /// <summary>
            /// Registration method: "email" or "phone". Tells backend which field to validate
            /// </summary>
            [JsonPropertyName("registration_method")]
            public string RegistrationMethod { get; set; }
        }

        // OTP Verification Response
        public class OtpVerificationResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }

            [JsonPropertyName("refresh_token")]
            public string RefreshToken { get; set; }

            [JsonPropertyName("token_type")]
            public string TokenType { get; set; }

            [JsonPropertyName("access_expires_at")]
            public string AccessExpiresAt { get; set; }

            [JsonPropertyName("refresh_expires_at")]
            public string RefreshExpiresAt { get; set; }

            [JsonPropertyName("user")]
            public UserData User { get; set; }

            [JsonPropertyName("message")]
            public string Message { get; set; }
        }

        // Resend OTP Request
        public class ResendOtpRequest
        {
            [JsonPropertyName("email")]
            public string Email { get; set; }

            [JsonPropertyName("phone")]
            public string Phone { get; set; }

            [JsonPropertyName("registration_method")]
            public string RegistrationMethod { get; set; }
        }

        // Resend OTP Response
        public class ResendOtpResponse
        {
            [JsonPropertyName("message")]
            public string Message { get; set; }

            [JsonPropertyName("otp_sent")]
            public bool OtpSent { get; set; }

            [JsonPropertyName("resend_after")]
            public int? ResendAfter { get; set; }
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

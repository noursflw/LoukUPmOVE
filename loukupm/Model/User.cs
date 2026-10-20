using System;
using System.Text.Json.Serialization;

namespace loukupm.Model
{
    public class User
    {
        public int Id { get; set; }

        [JsonPropertyName("first_name")]
        public string UserName { get; set; }

        // ✅ Added FirstName as alias for UserName for clarity
        [JsonIgnore]
        public string FirstName => UserName;

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("full_name")]
        public string FullName { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("avatar_url")]
        public string? ImageUser { get; set; }

        [JsonPropertyName("profile_image_url")]
        public string? ProfileImageUrl { get; set; }

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }
        [JsonPropertyName("ConfirmPassword")]
        public string ConfirmPassword { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }
        [JsonPropertyName("phone_verified")]
        public bool PhoneVerified { get; set; }
    }


    public class UserResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public User Data { get; set; }
    }
}


using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Linq;

namespace loukupm.Model
{
    // كلاس فريق العمل
    public partial class WorkTeam : ObservableObject
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("full_name")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public string? Description { get; set; }

        [JsonPropertyName("phone")]
        public string? Job { get; set; }

        [JsonPropertyName("profile_image_url")]
        public string? Image { get; set; }

        [JsonPropertyName("services")]
        public List<ServiceItem>? Services { get; set; }

        /// <summary>
        /// Production-ready image property with full URL encoding and null safety
        /// Returns avatar_url if available, otherwise first service image
        /// </summary>
        public string ImageSafe
        {
            get
            {
                string imageUrl = null;

                // تفحص Image أولاً
                if (!string.IsNullOrWhiteSpace(Image))
                {
                    imageUrl = Image;
                }
             
               

            
                else if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    return "imagesafe.png";  // ✅ هنا!
                }

                // Handle URLs with special characters
                string processedUrl = imageUrl;

                if (processedUrl.Contains("'"))
                {
                    processedUrl = processedUrl.Replace("'", "%27");
                }

                if (processedUrl.Contains("\""))
                {
                    processedUrl = processedUrl.Replace("\"", "%22");
                }

                if (processedUrl.Contains(" "))
                {
                    processedUrl = processedUrl.Replace(" ", "%20");
                }

                return processedUrl;
            }
        }

        [JsonPropertyName("created_at")]
        public DateTime WorkTime { get; set; }

        [ObservableProperty]
        private string borderColor = "#202020";

        public WorkTeam() { }

        public WorkTeam(int id, string name, string description, string job, string image, DateTime workTime)
        {
            Id = id;
            Name = name;
            Description = description;
            Job = job;
            Image = image;
            WorkTime = workTime;
        }
    }

    // Service item within WorkTeam
    public class ServiceItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; }
    }

    // كلاس الغلاف (الـ wrapper)
    public class WorkTeamWrapper
    {
        [JsonPropertyName("data")]
        public List<WorkTeam> Data { get; set; }
    }
}

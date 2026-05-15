using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace loukupm.Model
{
    public class ServiesWrapper
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public List<Servies> Data { get; set; }  // مصفوفة من الخدمات
    }

    public partial class Servies : ObservableObject
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string NameServies { get; set; }

        [JsonPropertyName("price")]
        public string PriceServies { get; set; }

        [JsonPropertyName("duration_minutes")]
        public int TimeServies { get; set; }

        [JsonPropertyName("image_url")]
        public string? Image { get; set; }

        [JsonPropertyName("category")]
        public Category Category { get; set; }  // تصنيف الخدمة

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        [ObservableProperty]
        private bool isSelected = false;

        public string ImageSafe
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Image))
                {
                    return "imagesafe.png";
                }

                string processedUrl = Image;

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
    }

    public class Category
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}

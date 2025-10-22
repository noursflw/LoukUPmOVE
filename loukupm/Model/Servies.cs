using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace loukupm.Model
{
    public class ServiesWrapper
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public List<Servies> Data { get; set; }  // مصفوفة من الخدمات
    }

    public class Servies
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string NameServies { get; set; }

        [JsonPropertyName("price")]
        public string PriceServies { get; set; }

        [JsonPropertyName("duration_minutes")]
        public int TimeServies { get; set; }

        [JsonPropertyName("category")]
        public Category Category { get; set; }  // تصنيف الخدمة
    }

    public class Category
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}

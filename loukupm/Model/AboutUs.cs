using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace loukupm.Model
{
    /// <summary>
    /// API response wrapper for AboutUs data
    /// </summary>
    public partial class AboutUsResponse : ObservableObject
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public AboutUsData Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    /// <summary>
    /// Main AboutUs data container
    /// </summary>
    public partial class AboutUsData : ObservableObject
    {
        [JsonPropertyName("hero")]
        public HeroSection Hero { get; set; }

        [JsonPropertyName("contact")]
        public ContactSection Contact { get; set; }

        [JsonPropertyName("social")]
        public SocialSection Social { get; set; }

        [JsonPropertyName("legal")]
        public LegalSection Legal { get; set; }

        [JsonPropertyName("features")]
        public ObservableCollection<Feature> Features { get; set; }

        [JsonPropertyName("team")]
        public ObservableCollection<TeamMember> Team { get; set; }

        [JsonPropertyName("newsletter")]
        public NewsletterSection Newsletter { get; set; }

        [JsonPropertyName("meta")]
        public MetaInfo Meta { get; set; }
    }

    /// <summary>
    /// Hero section with title, subtitle, description
    /// </summary>
    public partial class HeroSection : ObservableObject
    {
        [JsonPropertyName("title")]
        public MultiLanguageText Title { get; set; }

        [JsonPropertyName("subtitle")]
        public MultiLanguageText Subtitle { get; set; }

        [JsonPropertyName("description")]
        public MultiLanguageText Description { get; set; }

        [JsonPropertyName("images")]
        public ObservableCollection<HeroImage> Images { get; set; }
    }

    /// <summary>
    /// Hero image model
    /// </summary>
    public partial class HeroImage : ObservableObject
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("sort_order")]
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Multi-language text support
    /// </summary>
    public partial class MultiLanguageText : ObservableObject
    {
        [JsonPropertyName("de")]
        public string German { get; set; }

        [JsonPropertyName("ar")]
        public string Arabic { get; set; }

        [JsonPropertyName("en")]
        public string English { get; set; }

        /// <summary>
        /// Get text based on current culture
        /// </summary>
        public string GetText(string culture = null)
        {
            culture ??= Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

            return culture switch
            {
                "ar" => Arabic ?? English ?? German,
                "de" => German ?? English ?? Arabic,
                "en" => English ?? German ?? Arabic,
                _ => English ?? Arabic ?? German
            };
        }
    }

    /// <summary>
    /// Contact information section
    /// </summary>
    public partial class ContactSection : ObservableObject
    {
        [JsonPropertyName("phone")]
        public ContactInfo Phone { get; set; }

        [JsonPropertyName("address")]
        public ContactInfo Address { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("opening_hours")]
        public MultiLanguageText OpeningHours { get; set; }
    }

    /// <summary>
    /// Individual contact info item with label and icon
    /// </summary>
    public partial class ContactInfo : ObservableObject
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("label")]
        public MultiLanguageText Label { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }
    }

    /// <summary>
    /// Social media links section
    /// </summary>
    public partial class SocialSection : ObservableObject
    {
        [JsonPropertyName("title")]
        public MultiLanguageText Title { get; set; }

        [JsonPropertyName("links")]
        public ObservableCollection<SocialLink> Links { get; set; }
    }

    /// <summary>
    /// Individual social media link
    /// </summary>
    public partial class SocialLink : ObservableObject
    {
        [JsonPropertyName("platform")]
        public string Platform { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        /// <summary>
        /// Get FontAwesome icon name from platform
        /// </summary>
        public string GetIconName()
        {
            return Platform?.ToLower() switch
            {
                "tiktok" => "f_tiktok",
                "facebook" => "f_facebook",
                "instagram" => "f_instagram",
                _ => "f_globe"
            };
        }
    }

    /// <summary>
    /// Legal links section
    /// </summary>
    public partial class LegalSection : ObservableObject
    {
        [JsonPropertyName("links")]
        public ObservableCollection<LegalLink> Links { get; set; }
    }

    /// <summary>
    /// Individual legal link
    /// </summary>
    public partial class LegalLink : ObservableObject
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("label")]
        public MultiLanguageText Label { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    /// <summary>
    /// Feature/benefit item with icon, title, description
    /// </summary>
    public partial class Feature : ObservableObject
    {
        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("title")]
        public MultiLanguageText Title { get; set; }

        [JsonPropertyName("description")]
        public MultiLanguageText Description { get; set; }

        public string IconToImage =>
            Icon switch
            {
                "heroicon-o-scissors" => "scissors.png",
                "heroicon-o-star" => "star.png",
                "heroicon-o-user" => "personyy.png",
                _ => "z.svg"
            };
    }

    /// <summary>
    /// Team member information
    /// </summary>
    public partial class TeamMember : ObservableObject
    {
        [JsonPropertyName("name")]
        public MultiLanguageText Name { get; set; }

        [JsonPropertyName("position")]
        public MultiLanguageText Position { get; set; }

        [JsonPropertyName("description")]
        public MultiLanguageText Description { get; set; }

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; }

        [JsonPropertyName("sort_order")]
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Newsletter subscription section
    /// </summary>
    public partial class NewsletterSection : ObservableObject
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("title")]
        public MultiLanguageText Title { get; set; }

        [JsonPropertyName("description")]
        public MultiLanguageText Description { get; set; }
    }

    /// <summary>
    /// Metadata about the data
    /// </summary>
    public partial class MetaInfo : ObservableObject
    {
        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }

        [JsonPropertyName("updated_at")]
        public string UpdatedAt { get; set; }
    }
}
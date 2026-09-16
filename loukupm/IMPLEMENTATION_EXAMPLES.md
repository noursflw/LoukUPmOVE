# Implementation Examples & Usage Guide

## Example 1: Using the Solution with Different JSON Responses

### Scenario: String Format API Response
```json
{
  "success": true,
  "data": {
	"hero": {
	  "title": "Welcome",
	  "subtitle": "Professional Barber Services",
	  "description": "Best haircuts in town",
	  "images": [...]
	}
  }
}
```

**What Happens:**
1. JsonConverter.Read() receives string: `"Professional Barber Services"`
2. Creates MultiLanguageText with all languages = `"Professional Barber Services"`
3. ViewModel property HeroSubtitle returns: `"Professional Barber Services"`
4. XAML Label displays: `Professional Barber Services`

---

### Scenario: Object Format API Response
```json
{
  "success": true,
  "data": {
	"hero": {
	  "title": "Welcome",
	  "subtitle": {
		"de": "Professionelle Friseurdienstleistungen",
		"ar": "خدمات حلاقة احترافية",
		"en": "Professional Barber Services"
	  },
	  "description": {
		"de": "Beste Haarschnitte in der Stadt",
		"ar": "أفضل قصات الشعر في المدينة",
		"en": "Best haircuts in town"
	  },
	  "images": [...]
	}
  }
}
```

**What Happens:**
1. JsonConverter.Read() receives object
2. Maps: German → German, Arabic → Arabic, English → English
3. ViewModel property HeroSubtitle calls GetText() with current culture
   - If culture is "de": returns `"Professionelle Friseurdienstleistungen"`
   - If culture is "ar": returns `"خدمات حلاقة احترافية"`
   - If culture is "en": returns `"Professional Barber Services"`
4. XAML Label displays appropriate language

---

## Example 2: Handling Language Changes

### Code Implementation
```csharp
// In AboutUsViewModel.cs
public class AboutUsViewModel : ObservableObject
{
	// ... other code ...

	public void SetLanguage(string languageCode)
	{
		CurrentLanguage = languageCode;
		OnPropertyChanged(nameof(AboutUsData));
		OnPropertyChanged(nameof(HeroSubtitle));    // ← Triggers UI refresh
		OnPropertyChanged(nameof(HeroDescription)); // ← Triggers UI refresh
	}
}

// In a Page or ViewModel that calls this
public async Task ChangeToGerman()
{
	viewModel.SetLanguage("de");
	// UI automatically updates with German text
}

public async Task ChangeToArabic()
{
	viewModel.SetLanguage("ar");
	// UI automatically updates with Arabic text
}

public async Task ChangeToEnglish()
{
	viewModel.SetLanguage("en");
	// UI automatically updates with English text
}
```

### XAML That Binds to Localized Properties
```xaml
<VerticalStackLayout>
	<!-- Language Selection Buttons -->
	<Button Text="Deutsch" Command="{Binding ChangeToGermanCommand}" />
	<Button Text="العربية" Command="{Binding ChangeToArabicCommand}" />
	<Button Text="English" Command="{Binding ChangeToEnglishCommand}" />

	<!-- Display Localized Content -->
	<Label Text="{Binding HeroSubtitle}" 
			FontSize="24" 
			TextColor="#EBD750" />

	<Label Text="{Binding HeroDescription}" 
			FontSize="14" 
			TextColor="#999999" 
			LineBreakMode="WordWrap" />
</VerticalStackLayout>
```

---

## Example 3: Adding a New Multilingual Field

### Step 1: Define the Model Property
```csharp
// In AboutUs.cs
public partial class HeroSection : ObservableObject
{
	[JsonPropertyName("title")]
	public string Title { get; set; }

	[JsonPropertyName("subtitle")]
	public MultiLanguageText Subtitle { get; set; }

	[JsonPropertyName("description")]
	public MultiLanguageText Description { get; set; }

	// NEW FIELD (automatically works with converter!)
	[JsonPropertyName("tagline")]
	public MultiLanguageText Tagline { get; set; }
}
```

### Step 2: Add ViewModel Property
```csharp
// In AboutUsViewModel.cs
public class AboutUsViewModel : ObservableObject
{
	// ... existing properties ...

	/// <summary>
	/// NEW: Localized hero tagline text property for XAML binding.
	/// </summary>
	public string HeroTagline => AboutUsData?.Hero?.Tagline?.GetText();
}
```

### Step 3: Update LoadAboutUsData Notifications
```csharp
if (response?.Success == true && response.Data != null)
{
	AboutUsData = response.Data;

	// Notify UI about ALL localized text properties
	OnPropertyChanged(nameof(HeroSubtitle));
	OnPropertyChanged(nameof(HeroDescription));
	OnPropertyChanged(nameof(HeroTagline));  // ← NEW

	Console.WriteLine($"✅ AboutUs data loaded successfully");
}
```

### Step 4: Update SetLanguage Method
```csharp
public void SetLanguage(string languageCode)
{
	CurrentLanguage = languageCode;
	OnPropertyChanged(nameof(AboutUsData));
	OnPropertyChanged(nameof(HeroSubtitle));
	OnPropertyChanged(nameof(HeroDescription));
	OnPropertyChanged(nameof(HeroTagline));  // ← NEW
}
```

### Step 5: Add XAML Binding
```xaml
<Label Text="{Binding HeroTagline}" 
		FontSize="16" 
		TextColor="#D3D3D3" />
```

**That's it!** No converter changes needed - it works automatically.

---

## Example 4: Handling Edge Cases

### Null Response
```csharp
// API returns:
{ "hero": { "subtitle": null } }

// ViewModel property:
public string HeroSubtitle => AboutUsData?.Hero?.Subtitle?.GetText();
// Returns: "" (empty string due to null coalescing)
```

### Partial Language Object
```csharp
// API returns:
{ "subtitle": { "en": "English", "de": null } }

// GetText() with current culture:
string de = subtitle.GetText("de");  // Returns: null (or falls back per logic)
string en = subtitle.GetText("en");  // Returns: "English"
// Fallback chain: de → en → ar → German
```

### Empty MultiLanguageText
```csharp
// API returns:
{ "subtitle": "" }

// Converter converts to:
MultiLanguageText {
  German = "",
  Arabic = "",
  English = ""
}

// GetText() returns: "" (empty string)
```

### Mixed Formats in Same API Response
```json
{
  "hero": {
	"subtitle": "Simple string - no language variants",
	"description": {
	  "de": "German description",
	  "ar": "Arabic description",
	  "en": "English description"
	}
  }
}
```

**Result:** Both are handled correctly by the converter!

---

## Example 5: API Upload/Save with Merged Content

### Scenario: Submitting Updated Data Back to API
```csharp
// When UpdateAboutUsAsync() is called, the converter converts back:
var aboutUsData = viewModel.AboutUsData;

// aboutUsData.Hero.Subtitle.German = "German text"
// aboutUsData.Hero.Subtitle.Arabic = "Arabic text"  
// aboutUsData.Hero.Subtitle.English = "English text"

// Converter.Write() serializes back to JSON:
var json = JsonSerializer.Serialize(aboutUsData);

// Result sent to API:
{
  "hero": {
	"subtitle": {
	  "de": "German text",
	  "ar": "Arabic text",
	  "en": "English text"
	}
  }
}
```

---

## Example 6: Testing Both Formats

### Unit Test Example
```csharp
public class MultiLanguageTextJsonConverterTests
{
	private readonly JsonSerializerOptions _options = new();

	[Test]
	public void DeserializeStringFormat_ShouldSetAllLanguages()
	{
		// Arrange
		string json = "\"Professional Barber\"";

		// Act
		var result = JsonSerializer.Deserialize<MultiLanguageText>(json, _options);

		// Assert
		Assert.AreEqual("Professional Barber", result.German);
		Assert.AreEqual("Professional Barber", result.Arabic);
		Assert.AreEqual("Professional Barber", result.English);
	}

	[Test]
	public void DeserializeObjectFormat_ShouldMapLanguages()
	{
		// Arrange
		string json = @"{""de"":""Friseur"",""ar"":""حلاق"",""en"":""Barber""}";

		// Act
		var result = JsonSerializer.Deserialize<MultiLanguageText>(json, _options);

		// Assert
		Assert.AreEqual("Friseur", result.German);
		Assert.AreEqual("حلاق", result.Arabic);
		Assert.AreEqual("Barber", result.English);
	}

	[Test]
	public void DeserializeNull_ShouldReturnEmptyObject()
	{
		// Arrange
		string json = "null";

		// Act
		var result = JsonSerializer.Deserialize<MultiLanguageText>(json, _options);

		// Assert
		Assert.IsNotNull(result);
		Assert.IsNull(result.German);
		Assert.IsNull(result.Arabic);
		Assert.IsNull(result.English);
	}
}
```

---

## Example 7: Error Recovery Pattern

### Safe Property Access in XAML
```xaml
<!-- This safely handles all null cases -->
<Label Text="{Binding HeroSubtitle}" />
<!-- If HeroSubtitle is null, displays empty string (MAUI default) -->

<!-- More explicit handling if needed -->
<Label Text="{Binding HeroSubtitle, StringFormat='Subtitle: {0}'}" />
```

### Safe Access in Code
```csharp
// Pattern 1: Null coalescing operator (used in ViewModel)
public string HeroSubtitle => AboutUsData?.Hero?.Subtitle?.GetText() ?? "";

// Pattern 2: Direct null check
public string GetCachedSubtitle()
{
	try
	{
		return AboutUsData?.Hero?.Subtitle?.GetText() ?? "No subtitle";
	}
	catch (Exception ex)
	{
		Console.WriteLine($"Error getting subtitle: {ex.Message}");
		return "Error loading subtitle";
	}
}
```

---

## Quick Reference: When to Use What

| Use Case | Implementation |
|----------|-----------------|
| Display localized text in XAML | Use ViewModel string properties: `{Binding HeroSubtitle}` |
| Change language at runtime | Call `viewModel.SetLanguage("de")` |
| Add new multilingual field | Define property in Model, add string property in ViewModel |
| Get text in code-behind | Call `multiLanguageText.GetText("de")` |
| Handle API response | JsonConverter handles automatically - no manual intervention |
| Serialize back to API | JsonConverter.Write() handles automatically |

---

## Summary

This implementation provides:
✅ **Transparent handling** of both JSON formats
✅ **Automatic deserialization** at the model level
✅ **Clean XAML bindings** to localized string properties
✅ **Easy language switching** with automatic UI refresh
✅ **Production-ready** error handling and null safety
✅ **Scalable pattern** for adding new multilingual fields

All without breaking existing code or requiring changes to the API!

/*
 * 🎯 أفضل الممارسات: عرض الصور من API في .NET MAUI
 * Best Practices for Image Loading from API in .NET MAUI
 */

// ============================================
// 1. ✅ التعامل مع الـ URLs الآمن
// ============================================

// ❌ خطأ:
Image image = new Image { Source = imageUrl }; // URL مباشر بدون معالجة

// ✅ صحيح:
string safeUrl = ImageLoaderService.Instance.ProcessImageUrl(imageUrl);
Image image = new Image { Source = safeUrl };


// ============================================
// 2. ✅ معالجة الـ null والـ Empty
// ============================================

// ❌ خطأ:
public string ImageUrl => Provider?.Avatar; // قد يرجع null

// ✅ صحيح:
public string ImageUrl
{
    get
    {
        if (string.IsNullOrWhiteSpace(Provider?.Avatar))
            return "profile_placeholder.png";
        return Provider.Avatar;
    }
}


// ============================================
// 3. ✅ استخدام Converters في XAML
// ============================================

// ❌ خطأ - XAML:
<Image Source="{Binding ImageUrl}" />

// ✅ صحيح - XAML:
<Image Source="{Binding ImageUrl, Converter={StaticResource ImageUriConverter}}" />


// ============================================
// 4. ✅ معالجة الـ URL Encoding
// ============================================

// ❌ خطأ:
var url = "https://api.com/images/Men's_Haircut.png"; // سيفشل

// ✅ صحيح:
var url = "https://api.com/images/Men%27s_Haircut.png"; // يعمل
// أو:
var url = EncodeUrl("https://api.com/images/Men's_Haircut.png");


// ============================================
// 5. ✅ HttpClient Configuration
// ============================================

// ❌ خطأ:
private readonly HttpClient _client = new HttpClient();

// ✅ صحيح:
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => 
    {
        #if DEBUG
        return true; // للتطوير فقط
        #else
        return errors == SslPolicyErrors.None; // للإنتاج
        #endif
    }
};

_client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };


// ============================================
// 6. ✅ Logging للتصحيح
// ============================================

// ❌ خطأ - بدون logging:
try
{
    var response = await _httpClient.GetAsync(url);
}
catch { }

// ✅ صحيح - مع logging:
try
{
    Console.WriteLine($"📡 Loading: {url}");
    var response = await _httpClient.GetAsync(url);
    if (response.IsSuccessStatusCode)
        Console.WriteLine($"✅ Loaded: {url}");
    else
        Console.WriteLine($"❌ Failed ({response.StatusCode}): {url}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
}


// ============================================
// 7. ✅ Placeholder Images
// ============================================

// ❌ خطأ - بدون placeholder:
<Image Source="{Binding ImageUrl}" HeightRequest="50" WidthRequest="50" />

// ✅ صحيح - مع placeholder fallback:
<Frame HeightRequest="52" WidthRequest="52" CornerRadius="26" 
       BackgroundColor="#E0E0E0" Padding="0">
    <Image Source="{Binding ImageUrl, Converter={StaticResource ImageUriConverter}}"
           HeightRequest="50" WidthRequest="50" 
           Aspect="AspectFill"
           IsOpaque="True" />
</Frame>


// ============================================
// 8. ✅ Performance Optimization
// ============================================

// ❌ خطأ:
<Image Source="{Binding ImageUrl}" /> <!-- مرات متعددة في CollectionView -->

// ✅ صحيح:
<Image Source="{Binding ImageUrl, Converter={StaticResource ImageUriConverter}}" 
       IsOpaque="True"           <!-- تحسين الأداء -->
       CacheMode="Cache" />      <!-- تخزين مؤقت -->


// ============================================
// 9. ✅ التعامل مع الأخطاء بأناقة
// ============================================

// ❌ خطأ:
public string ImageUrl { get; set; } // قد يكون null أو invalid

// ✅ صحيح:
private string _imageUrl = "profile_placeholder.png";
public string ImageUrl
{
    get => _imageUrl;
    set => SetProperty(ref _imageUrl, 
        !string.IsNullOrWhiteSpace(value) ? value : "profile_placeholder.png");
}


// ============================================
// 10. ✅ التحقق من الـ URLs قبل الاستخدام
// ============================================

// ❌ خطأ:
_appointments.AddRange(items); // بدون تحقق من الـ URLs

// ✅ صحيح:
foreach (var item in items)
{
    // تحقق من الـ URL
    if (!string.IsNullOrWhiteSpace(item.ImageUrl))
    {
        // معالجة الـ URL
        item.ImageUrl = ImageLoaderService.Instance.ProcessImageUrl(item.ImageUrl);
    }
    _appointments.Add(item);
}


// ============================================
// 📋 CHECKLIST: Image Loading Quality
// ============================================

/*
Before submitting your code, ensure:

[ ] جميع الـ URLs تمر عبر Converter
[ ] معالجة شاملة للـ null/empty values
[ ] fallback images موجودة ومحددة
[ ] Logging موجود للتصحيح
[ ] Timeout محدد (15-30 ثانية)
[ ] SSL validation آمن (#if DEBUG)
[ ] URLs مُرمّزة للحروف الخاصة
[ ] Performance optimizations مطبقة
[ ] Error handling شامل
[ ] Code reviewed و tested
*/


// ============================================
// 🚀 مثال عملي كامل
// ============================================

// ViewModel:
public partial class BookingViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<AppointmentItem> appointments = new();

    public async Task LoadAppointmentsAsync()
    {
        try
        {
            // تحميل البيانات
            var data = await _apiService.GetAppointmentsAsync();

            // معالجة كل عنصر
            foreach (var item in data)
            {
                // ✅ معالجة الصورة
                if (!string.IsNullOrWhiteSpace(item.ProviderAvatar))
                {
                    item.ProviderAvatar = ImageLoaderService.Instance
                        .ProcessImageUrl(item.ProviderAvatar);
                }
                else
                {
                    item.ProviderAvatar = "profile_placeholder.png";
                }

                Appointments.Add(item);
            }

            Console.WriteLine($"✅ Loaded {Appointments.Count} appointments");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }
}

// XAML:
<CollectionView ItemsSource="{Binding Appointments}">
    <CollectionView.ItemTemplate>
        <DataTemplate>
            <StackLayout>
                <!-- ✅ مع Converter والـ Placeholder -->
                <Frame CornerRadius="25" HeightRequest="50" WidthRequest="50">
                    <Image Source="{Binding ProviderAvatar, 
                                  Converter={StaticResource ImageUriConverter}}"
                           Aspect="AspectFill"
                           IsOpaque="True" />
                </Frame>

                <Label Text="{Binding ProviderName}" FontSize="16" />
                <Label Text="{Binding ServiceName}" FontSize="14" TextColor="Gray" />
            </StackLayout>
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>


// ============================================
// 📚 مراجع إضافية
// ============================================

/*
1. URL Encoding in URLs:
   https://en.wikipedia.org/wiki/Percent-encoding

2. MAUI Image Control:
   https://learn.microsoft.com/dotnet/maui/user-interface/controls/image

3. HttpClient Best Practices:
   https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient

4. SSL/TLS in .NET:
   https://learn.microsoft.com/dotnet/api/system.net.http.httpclienthandler

5. XAML Value Converters:
   https://learn.microsoft.com/dotnet/maui/fundamentals/data-binding/converters
*/

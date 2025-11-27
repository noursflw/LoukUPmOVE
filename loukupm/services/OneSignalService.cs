using OneSignalSDK.DotNet;
using System.Threading.Tasks;

namespace loukupm.Services
{
    public static class OneSignalService
    {
        // ⚠️ تحديث: استبدل "YOUR-APP-ID" بـ معرّف تطبيقك الحقيقي من OneSignal Dashboard
        private static readonly string _appId = "YOUR-APP-ID";

        public static async Task Init()
        {
            try
            {
                if (string.IsNullOrEmpty(_appId) || _appId == "YOUR-APP-ID")
                {
                    Console.WriteLine("⚠️ OneSignal: AppId not configured! Using placeholder.");
                    return;
                }

                OneSignal.Initialize(_appId);
                await OneSignal.Notifications.RequestPermissionAsync(true);
                
                Console.WriteLine("✅ OneSignal initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ OneSignal Init Error: {ex.Message}");
                // Log to remote service if needed
            }
        }

        
        /// <param name="userId">معرف المستخدم</param>
        public static void RegisterUser(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("⚠️ Cannot register: UserId is null or empty");
                    return;
                }

                OneSignal.Login(userId);
                OneSignal.User.AddTag("user_id", userId);
                
                Console.WriteLine($"✅ User {userId} registered with OneSignal");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ RegisterUser Error: {ex.Message}");
            }
        }

      
        public static async Task LogoutAsync()
        {
            try
            {
                OneSignal.Logout();
                OneSignal.User.RemoveTag("user_id");
                OneSignal.User.RemoveTag("user_no");
                OneSignal.User.RemoveTag("email");
                OneSignal.User.RemoveTag("name");
                OneSignal.User.RemoveTag("signup_type");
                OneSignal.User.RemoveTag("login_type");
                OneSignal.User.RemoveTag("signup_date");
                OneSignal.User.RemoveTag("display_name");
                
                Console.WriteLine("✅ OneSignal logout completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ OneSignal logout failed: {ex.Message}");
            }
        }

       
        public static void AddTag(string key, string value)
        {
            try
            {
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
                {
                    Console.WriteLine("⚠️ AddTag: key or value is null or empty");
                    return;
                }

                OneSignal.User.AddTag(key, value);
                Console.WriteLine($"✅ Tag added: {key} = {value}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ AddTag Error: {ex.Message}");
            }
        }

       
        public static void RemoveTag(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    Console.WriteLine("⚠️ RemoveTag: key is null or empty");
                    return;
                }

                OneSignal.User.RemoveTag(key);
                Console.WriteLine($"✅ Tag removed: {key}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ RemoveTag Error: {ex.Message}");
            }
        }
    }
}

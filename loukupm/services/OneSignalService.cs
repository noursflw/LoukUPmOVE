using OneSignalSDK.DotNet;
using System;
using System.Threading.Tasks;

namespace loukupm.Services
{
    public static class OneSignalService
    {
        private static readonly string _appId =
            "68c49ad8-113c-4160-91cc-5eb9d2c908d5";

        private static bool _initialized = false;

        public static async Task Init()
        {
            if (_initialized)
                return;

            try
            {
                if (string.IsNullOrWhiteSpace(_appId))
                {
                    Console.WriteLine("⚠️ OneSignal: AppId not configured!");
                    return;
                }

                OneSignal.Initialize(_appId);
                await OneSignal.Notifications.RequestPermissionAsync(true);

                _initialized = true;
                Console.WriteLine("✅ OneSignal initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ OneSignal Init Error: {ex.Message}");
            }
        }

        /// <param name="userId">معرف المستخدم</param>
        public static void RegisterUser(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    Console.WriteLine("⚠️ Cannot register: UserId is null or empty");
                    return;
                }

                OneSignal.Login(userId);
                OneSignal.User.AddTag("user_no", userId);

                Console.WriteLine($"✅ User {userId} registered with OneSignal");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ RegisterUser Error: {ex.Message}");
            }
        }

        public static void Logout()
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
                if (string.IsNullOrWhiteSpace(key) ||
                    string.IsNullOrWhiteSpace(value))
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
                if (string.IsNullOrWhiteSpace(key))
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

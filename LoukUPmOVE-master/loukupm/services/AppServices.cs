using Microsoft.Extensions.DependencyInjection;

namespace loukupm.Services
{
    internal static class AppServices
    {
        public static T Get<T>() where T : class
        {
            try
            {
                if (MauiProgram.Services != null)
                    return MauiProgram.Services.GetService<T>();

                return Application.Current?.Handler?.MauiContext?.Services?.GetService<T>();
            }
            catch
            {
                return null;
            }
        }
    }
}

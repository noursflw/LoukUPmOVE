using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using The49.Maui.BottomSheet;

using UraniumUI;

namespace loukupm
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("georgia.ttf", "georgia");
                    fonts.AddFont("georgia-bold.ttf", "georgia-bold");
                    fonts.AddFont("Oswald-VariableFont_wght.ttf ", "Oswald");
                })
                .UseUraniumUI()
                .UseUraniumUIMaterial()
                .UseBottomSheet()
                .UseMauiCommunityToolkit();




#if DEBUG
            builder.Logging.AddDebug();
#endif

         

            return builder.Build();
        }
    }
}

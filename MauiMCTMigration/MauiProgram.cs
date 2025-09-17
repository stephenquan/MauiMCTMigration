using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace MauiMCTMigration;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        //MCT11.CommunityToolkit.Maui;

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

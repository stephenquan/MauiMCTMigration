using Foundation;

namespace MauiMCTMigration;

#pragma warning disable CS1591 // Suppress missing XML comment warning


[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

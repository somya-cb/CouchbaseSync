using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace AppSync.Platforms.iOS;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
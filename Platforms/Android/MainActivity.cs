using Android.App;
using Android.Content.PM;
using Android.OS;

namespace AppSync;

[Activity(
    Label = "AppSync", 
    Theme = "@style/MainTheme",
    MainLauncher = true, 
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize
)]
public class MainActivity : MauiAppCompatActivity
{
}

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace wildflowerFrontend;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ScreenOrientation = ScreenOrientation.Portrait,
    ConfigurationChanges = ConfigChanges.ScreenSize |
        ConfigChanges.Orientation |
        ConfigChanges.UiMode |
        ConfigChanges.ScreenLayout |
        ConfigChanges.SmallestScreenSize |
        ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    internal static event EventHandler<AndroidActivityResultEventArgs>? ActivityResultReceived;

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        ActivityResultReceived?.Invoke(this, new AndroidActivityResultEventArgs(requestCode, resultCode, data));
    }
}

internal sealed record AndroidActivityResultEventArgs(int RequestCode, Result ResultCode, Intent? Data);

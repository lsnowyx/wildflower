using Android.Content;
using Android.OS;
using wildflower.Models;
using wildflowerFrontend.Services;
using AndroidApplication = global::Android.App.Application;

namespace wildflowerFrontend.Platforms.Android;

public sealed class AndroidPlaybackServiceController : IPlaybackServiceController
{
    private readonly Context context = AndroidApplication.Context;
    public void Update(PlayerSessionSnapshot snapshot)
    {
        if (AndroidPlaybackService.IsRunning || snapshot.CurrentTrack is null ||
            (snapshot.Status != PlayerStatus.Playing && snapshot.Status != PlayerStatus.Paused))
        {
            return;
        }

        var intent = new Intent(context, typeof(AndroidPlaybackService));
        if (OperatingSystem.IsAndroidVersionAtLeast(26))
            context.StartForegroundService(intent);
        else
            context.StartService(intent);
    }
}

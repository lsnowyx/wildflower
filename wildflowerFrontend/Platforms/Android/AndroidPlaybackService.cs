using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Microsoft.Extensions.DependencyInjection;
using wildflower.Models;
using wildflower.Services.Library;
using wildflower.Services.Session;
using wildflowerFrontend.Services;
using AndroidNotification = global::Android.App.Notification;
using AndroidPlaybackState = global::Android.Media.Session.PlaybackState;
using AndroidService = global::Android.App.Service;

namespace wildflowerFrontend.Platforms.Android;

[Service(
    Name = "com.companyname.wildflowerfrontend.AndroidPlaybackService",
    Exported = false,
    ForegroundServiceType = ForegroundService.TypeMediaPlayback)]
public sealed class AndroidPlaybackService : AndroidService
{
    private static int running;
    internal static bool IsRunning => Volatile.Read(ref running) != 0;

    internal const string ActionPlayPause = "wildflower.action.PLAY_PAUSE";
    internal const string ActionPrevious = "wildflower.action.PREVIOUS";
    internal const string ActionNext = "wildflower.action.NEXT";
    internal const string ActionStop = "wildflower.action.STOP";

    private const string ChannelId = "wildflower_playback";
    private const int NotificationId = 4107;
    private readonly SemaphoreSlim tickGate = new(1, 1);
    private readonly object metadataSync = new();
    private IPlayerSessionService? playerSession;
    private ITrackMetadataCache? trackMetadataCache;
    private IMetadataService? metadataService;
    private NotificationManager? notificationManager;
    private MediaSession? mediaSession;
    private SessionCallback? sessionCallback;
    private TrackInfo? displayTrack;
    private string? metadataRequestPath;
    private string? resolvedMetadataPath;
    private System.Threading.Timer? timer;
    private int ticksSinceSave;
    private bool taskRemovalStarted;

    public override void OnCreate()
    {
        base.OnCreate();
        IServiceProvider services = MainApplication.ServiceProvider
            ?? throw new InvalidOperationException("Wildflower services are not initialized.");
        playerSession = services.GetRequiredService<IPlayerSessionService>();
        trackMetadataCache = services.GetRequiredService<ITrackMetadataCache>();
        metadataService = services.GetRequiredService<IMetadataService>();
        notificationManager = GetSystemService(NotificationService) as NotificationManager;
        Volatile.Write(ref running, 1);

        CreateNotificationChannel();
        mediaSession = new MediaSession(this, "WildflowerPlayback");
        sessionCallback = new SessionCallback(this);
        mediaSession.SetCallback(sessionCallback);
        mediaSession.Active = true;
        playerSession.SnapshotChanged += PlayerSession_SnapshotChanged;

        PlayerSessionSnapshot snapshot = playerSession.GetSnapshot();
        TrackInfo? presentationTrack = GetPresentationTrack(snapshot);
        StartForeground(NotificationId, BuildNotification(snapshot, presentationTrack));
        UpdateMediaSession(snapshot, presentationTrack);
        EnsureMetadataLoaded(snapshot);
        timer = new System.Threading.Timer(Timer_Tick, null, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500));
    }

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        HandleAction(intent?.Action);
        if (playerSession is not null)
        {
            PlayerSessionSnapshot snapshot = playerSession.GetSnapshot();
            TrackInfo? presentationTrack = GetPresentationTrack(snapshot);
            StartForeground(NotificationId, BuildNotification(snapshot, presentationTrack));
            UpdateMediaSession(snapshot, presentationTrack);
            EnsureMetadataLoaded(snapshot);
        }

        return StartCommandResult.NotSticky;
    }

    public override IBinder? OnBind(Intent? intent) => null;

    public override void OnTaskRemoved(Intent? rootIntent)
    {
        _ = StopAfterTaskRemovalAsync();
        base.OnTaskRemoved(rootIntent);
    }

    public override void OnDestroy()
    {
        Volatile.Write(ref running, 0);
        timer?.Dispose();
        timer = null;
        if (playerSession is not null)
            playerSession.SnapshotChanged -= PlayerSession_SnapshotChanged;

        mediaSession?.SetCallback(null);
        if (mediaSession is not null)
            mediaSession.Active = false;
        sessionCallback?.Dispose();
        mediaSession?.Release();
        mediaSession?.Dispose();
        base.OnDestroy();
    }

    private async void Timer_Tick(object? state)
    {
        if (taskRemovalStarted || playerSession is null || !await tickGate.WaitAsync(0))
            return;

        try
        {
            await playerSession.AdvanceIfStoppedAsync();
            ticksSinceSave++;
            if (ticksSinceSave >= 60)
            {
                ticksSinceSave = 0;
                await playerSession.SavePlaybackStateAsync();
            }

            PlayerSessionSnapshot snapshot = playerSession.GetSnapshot();
            UpdateMediaSession(snapshot, GetPresentationTrack(snapshot));
        }
        catch
        {
            // A service tick must never terminate background playback.
        }
        finally
        {
            tickGate.Release();
        }
    }

    private void PlayerSession_SnapshotChanged(object? sender, PlayerSessionSnapshot snapshot)
    {
        TrackInfo? presentationTrack = GetPresentationTrack(snapshot);
        UpdateMediaSession(snapshot, presentationTrack);
        notificationManager?.Notify(NotificationId, BuildNotification(snapshot, presentationTrack));
        EnsureMetadataLoaded(snapshot);
    }

    private void HandleAction(string? action)
    {
        if (playerSession is null || string.IsNullOrWhiteSpace(action))
            return;

        switch (action)
        {
            case ActionPlayPause:
                playerSession.TogglePlayPause();
                break;
            case ActionPrevious:
                playerSession.PreviousTrack();
                break;
            case ActionNext:
                playerSession.NextTrack();
                break;
            case ActionStop:
                _ = StopFromControlAsync();
                break;
        }
    }

    private async Task StopFromControlAsync()
    {
        if (playerSession is null)
            return;

        taskRemovalStarted = true;
        timer?.Dispose();
        timer = null;
        await tickGate.WaitAsync();
        try
        {
            await playerSession.SavePlaybackStateAsync();
            playerSession.StopPlayback();
            RemoveForegroundNotification();
            StopSelf();
        }
        finally
        {
            tickGate.Release();
        }
    }

    private async Task StopAfterTaskRemovalAsync()
    {
        if (taskRemovalStarted || playerSession is null)
            return;

        taskRemovalStarted = true;
        timer?.Dispose();
        timer = null;
        await tickGate.WaitAsync();
        try
        {
            await playerSession.SavePlaybackStateAsync();
        }
        finally
        {
            playerSession.StopPlayback();
            RemoveForegroundNotification();
            StopSelf();
            tickGate.Release();
            global::Android.OS.Process.KillProcess(global::Android.OS.Process.MyPid());
        }
    }

    private AndroidNotification BuildNotification(PlayerSessionSnapshot snapshot, TrackInfo? presentationTrack)
    {
        PendingIntentFlags pendingIntentFlags = PendingIntentFlags.UpdateCurrent;
        if (OperatingSystem.IsAndroidVersionAtLeast(23))
            pendingIntentFlags |= PendingIntentFlags.Immutable;

        PendingIntent contentIntent = PendingIntent.GetActivity(
            this,
            0,
            new Intent(this, typeof(MainActivity)).SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop),
            pendingIntentFlags)!;

        var builder = OperatingSystem.IsAndroidVersionAtLeast(26)
            ? new AndroidNotification.Builder(this, ChannelId)
            : new AndroidNotification.Builder(this);

        int playPauseIcon = snapshot.IsPlaying
            ? global::Android.Resource.Drawable.IcMediaPause
            : global::Android.Resource.Drawable.IcMediaPlay;
        string playPauseLabel = snapshot.IsPlaying ? "Pause" : "Play";

#pragma warning disable CA1422
#pragma warning disable CS8602
        builder!
            .SetSmallIcon(global::Android.Resource.Drawable.IcMediaPlay)
            .SetContentTitle(presentationTrack?.Title ?? "Wildflower")
            .SetContentText(string.IsNullOrWhiteSpace(presentationTrack?.Artist) ? "Wildflower" : presentationTrack.Artist)
            .SetContentIntent(contentIntent)
            .SetOnlyAlertOnce(true)
            .SetOngoing(snapshot.IsPlaying)
            .SetVisibility(NotificationVisibility.Public)
            .SetCategory(AndroidNotification.CategoryTransport)
            .AddAction(global::Android.Resource.Drawable.IcMediaPrevious, "Previous", CreateServiceIntent(ActionPrevious, 1))
            .AddAction(playPauseIcon, playPauseLabel, CreateServiceIntent(ActionPlayPause, 2))
            .AddAction(global::Android.Resource.Drawable.IcMediaNext, "Next", CreateServiceIntent(ActionNext, 3));
#pragma warning restore CA1422
#pragma warning restore CS8602

        if (mediaSession is not null)
        {
            var style = new AndroidNotification.MediaStyle();
            style.SetMediaSession(mediaSession.SessionToken);
            style.SetShowActionsInCompactView(0, 1, 2);
            builder.SetStyle(style);
        }

        return builder.Build() ?? throw new InvalidOperationException("Android could not create the playback notification.");
    }

    private PendingIntent CreateServiceIntent(string action, int requestCode)
    {
        var intent = new Intent(this, typeof(AndroidPlaybackService)).SetAction(action);
        PendingIntentFlags pendingIntentFlags = PendingIntentFlags.UpdateCurrent;
        if (OperatingSystem.IsAndroidVersionAtLeast(23))
            pendingIntentFlags |= PendingIntentFlags.Immutable;

        return PendingIntent.GetService(
            this,
            requestCode,
            intent,
            pendingIntentFlags)!;
    }

    private void UpdateMediaSession(PlayerSessionSnapshot snapshot, TrackInfo? presentationTrack)
    {
        if (mediaSession is null)
            return;

        long actions = AndroidPlaybackState.ActionPlay |
            AndroidPlaybackState.ActionPause |
            AndroidPlaybackState.ActionPlayPause |
            AndroidPlaybackState.ActionSeekTo |
            AndroidPlaybackState.ActionStop;
        if (snapshot.CanGoPrevious)
            actions |= AndroidPlaybackState.ActionSkipToPrevious;
        if (snapshot.CanGoNext)
            actions |= AndroidPlaybackState.ActionSkipToNext;

        using (var stateBuilder = new AndroidPlaybackState.Builder())
        {
            stateBuilder.SetActions(actions);
            PlaybackStateCode state = snapshot.IsPlaying
                ? PlaybackStateCode.Playing
                : snapshot.Status == PlayerStatus.Paused
                    ? PlaybackStateCode.Paused
                    : PlaybackStateCode.Stopped;
            stateBuilder.SetState(state, Math.Max(0, (long)(snapshot.PositionSeconds * 1000)), snapshot.IsPlaying ? 1f : 0f);
            using AndroidPlaybackState playbackState = stateBuilder.Build()
                ?? throw new InvalidOperationException("Android could not create the media playback state.");
            mediaSession.SetPlaybackState(playbackState);
        }

        using var metadataBuilder = new MediaMetadata.Builder();
        metadataBuilder.PutString(MediaMetadata.MetadataKeyTitle, presentationTrack?.Title ?? "Wildflower");
        metadataBuilder.PutString(MediaMetadata.MetadataKeyArtist, presentationTrack?.Artist ?? string.Empty);
        metadataBuilder.PutLong(MediaMetadata.MetadataKeyDuration, Math.Max(0, (long)(snapshot.LengthSeconds * 1000)));
        using MediaMetadata metadata = metadataBuilder.Build()
            ?? throw new InvalidOperationException("Android could not create the media metadata.");
        mediaSession.SetMetadata(metadata);
    }

    private TrackInfo? GetPresentationTrack(PlayerSessionSnapshot snapshot)
    {
        TrackInfo? currentTrack = snapshot.CurrentTrack;
        if (currentTrack is null)
            return null;

        lock (metadataSync)
        {
            if (displayTrack is not null &&
                string.Equals(displayTrack.FilePath, currentTrack.FilePath, StringComparison.OrdinalIgnoreCase))
            {
                return displayTrack;
            }

            displayTrack = new TrackInfo(
                currentTrack.FilePath,
                GetDecodedFileName(currentTrack.FilePath),
                string.Empty);
            metadataRequestPath = null;
            resolvedMetadataPath = null;
            return displayTrack;
        }
    }

    private void EnsureMetadataLoaded(PlayerSessionSnapshot snapshot)
    {
        string? path = snapshot.CurrentTrack?.FilePath;
        if (string.IsNullOrWhiteSpace(path) || trackMetadataCache is null || metadataService is null)
            return;

        lock (metadataSync)
        {
            if (string.Equals(resolvedMetadataPath, path, StringComparison.OrdinalIgnoreCase))
                return;

            if (string.Equals(metadataRequestPath, path, StringComparison.OrdinalIgnoreCase))
                return;

            metadataRequestPath = path;
        }

        _ = ResolveMetadataAsync(path);
    }

    private async Task ResolveMetadataAsync(string path)
    {
        try
        {
            IReadOnlyDictionary<string, TrackInfo> cachedTracks = await trackMetadataCache!.LoadAsync();
            if (!cachedTracks.TryGetValue(path, out TrackInfo? metadata))
            {
                metadata = await Task.Run(() => metadataService!.GetTrackInfo(path));
                await trackMetadataCache.StoreAsync(new[] { metadata });
            }

            PlayerSessionSnapshot snapshot = playerSession?.GetSnapshot()
                ?? throw new InvalidOperationException("The playback session is unavailable.");
            if (taskRemovalStarted || !IsRunning ||
                !string.Equals(snapshot.CurrentTrack?.FilePath, path, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            lock (metadataSync)
            {
                if (!string.Equals(metadataRequestPath, path, StringComparison.OrdinalIgnoreCase))
                    return;

                displayTrack = metadata;
                metadataRequestPath = null;
                resolvedMetadataPath = path;
            }

            UpdateMediaSession(snapshot, metadata);
            notificationManager?.Notify(NotificationId, BuildNotification(snapshot, metadata));
        }
        catch
        {
            lock (metadataSync)
            {
                if (string.Equals(metadataRequestPath, path, StringComparison.OrdinalIgnoreCase))
                    metadataRequestPath = null;
            }
        }
    }

    private static string GetDecodedFileName(string trackIdentifier)
    {
        try
        {
            global::Android.Net.Uri? uri = global::Android.Net.Uri.Parse(trackIdentifier);
            string decoded = global::Android.Net.Uri.Decode(uri?.LastPathSegment ?? trackIdentifier)
                ?? trackIdentifier;
            int separator = decoded.LastIndexOf(':');
            if (separator >= 0 && separator < decoded.Length - 1)
                decoded = decoded[(separator + 1)..];

            string title = Path.GetFileNameWithoutExtension(decoded);
            return string.IsNullOrWhiteSpace(title) ? decoded : title;
        }
        catch
        {
            return trackIdentifier;
        }
    }

    private void CreateNotificationChannel()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(26) || notificationManager is null)
            return;

        var channel = new NotificationChannel(ChannelId, "Music playback", NotificationImportance.Low)
        {
            Description = "Wildflower playback controls"
        };
        notificationManager.CreateNotificationChannel(channel);
    }

    private void RemoveForegroundNotification()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(24))
            StopForeground(StopForegroundFlags.Remove);
        else
        {
#pragma warning disable CA1422
            StopForeground(true);
#pragma warning restore CA1422
        }
    }

    private sealed class SessionCallback : MediaSession.Callback
    {
        private readonly WeakReference<AndroidPlaybackService> owner;

        public SessionCallback(AndroidPlaybackService owner)
        {
            this.owner = new WeakReference<AndroidPlaybackService>(owner);
        }

        public override void OnPlay() => WithSession(session =>
        {
            if (!session.GetSnapshot().IsPlaying)
                session.TogglePlayPause();
        });

        public override void OnPause() => WithSession(session =>
        {
            if (session.GetSnapshot().IsPlaying)
                session.TogglePlayPause();
        });

        public override void OnSkipToNext() => WithSession(session => session.NextTrack());

        public override void OnSkipToPrevious() => WithSession(session => session.PreviousTrack());

        public override void OnSeekTo(long pos) => WithSession(session =>
            session.SeekToMilliseconds((int)Math.Clamp(pos, 0, int.MaxValue)));

        public override void OnStop()
        {
            if (owner.TryGetTarget(out AndroidPlaybackService? service))
                _ = service.StopFromControlAsync();
        }

        private void WithSession(Action<IPlayerSessionService> action)
        {
            if (owner.TryGetTarget(out AndroidPlaybackService? service) && service.playerSession is not null)
                action(service.playerSession);
        }
    }
}

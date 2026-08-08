using Android.Content;
using Android.Media;
using Android.OS;
using wildflower.Models;
using wildflower.Services.Playback;
using AndroidApplication = global::Android.App.Application;
using AndroidUri = global::Android.Net.Uri;

namespace wildflowerFrontend.Platforms.Android;

/// <summary>
/// Plays Storage Access Framework document URIs directly through Android MediaPlayer.
/// Persisted positions are milliseconds on this platform; byte offsets are intentionally
/// unavailable because Android does not expose a stable encoded-stream byte position.
/// </summary>
public sealed class AndroidPlaybackEngine : IPlaybackEngine
{
    private readonly object syncRoot = new();
    private readonly Context context = AndroidApplication.Context;
    private readonly AudioFocusListener audioFocusListener;
    private AudioManager? audioManager;
    private AudioAttributes? audioAttributes;
    private AudioFocusRequestClass? audioFocusRequest;
    private MediaPlayer? mediaPlayer;
    private bool initialized;
    private bool prepared;
    private bool hasAudioFocus;
    private bool disposed;
    private PlayerStatus status = PlayerStatus.Stopped;

    public AndroidPlaybackEngine()
    {
        audioFocusListener = new AudioFocusListener(this);
    }

    public event EventHandler? StateChanged;

    public bool HasStream
    {
        get
        {
            lock (syncRoot)
                return mediaPlayer is not null && prepared;
        }
    }

    public PlayerStatus Status
    {
        get
        {
            lock (syncRoot)
                return status;
        }
    }

    public float Volume { get; private set; } = 0.3f;

    public bool Initialize()
    {
        lock (syncRoot)
        {
            if (disposed)
                return false;

            if (initialized)
                return true;

            audioManager = context.GetSystemService(Context.AudioService) as AudioManager;
            if (audioManager is null)
                return false;

            using (var attributesBuilder = new AudioAttributes.Builder())
            {
                attributesBuilder.SetUsage(AudioUsageKind.Media);
                attributesBuilder.SetContentType(AudioContentType.Music);
                audioAttributes = attributesBuilder.Build();
            }

            if (audioAttributes is null)
                return false;

            if (OperatingSystem.IsAndroidVersionAtLeast(26))
            {
                using var focusBuilder = new AudioFocusRequestClass.Builder(AudioFocus.Gain);
                focusBuilder.SetAudioAttributes(audioAttributes);
                focusBuilder.SetOnAudioFocusChangeListener(audioFocusListener);
                focusBuilder.SetWillPauseWhenDucked(true);
                audioFocusRequest = focusBuilder.Build();
            }

            initialized = true;
            return true;
        }
    }

    public bool Load(string source, long startPosition = 0)
    {
        if (!Initialize() || string.IsNullOrWhiteSpace(source))
            return false;

        AndroidUri? uri = AndroidUri.Parse(source);
        if (uri is null || !string.Equals(uri.Scheme, ContentResolver.SchemeContent, StringComparison.OrdinalIgnoreCase))
            return false;

        lock (syncRoot)
        {
            if (disposed || audioAttributes is null)
                return false;

            ReleasePlayerLocked();

            try
            {
                var player = new MediaPlayer();
                player.SetAudioAttributes(audioAttributes);
                player.Completion += MediaPlayer_Completion;
                player.Error += MediaPlayer_Error;
                player.SetDataSource(context, uri);
                player.Prepare();

                mediaPlayer = player;
                prepared = true;
                status = PlayerStatus.Stopped;
                ApplyVolumeLocked();
                SeekToMillisecondsLocked(startPosition);
                return true;
            }
            catch
            {
                ReleasePlayerLocked();
                AbandonAudioFocus();
                return false;
            }
        }
    }

    public bool Play(bool restart = false)
    {
        if (!RequestAudioFocus())
            return false;

        lock (syncRoot)
        {
            if (disposed || mediaPlayer is null || !prepared)
            {
                AbandonAudioFocus();
                return false;
            }

            try
            {
                if (restart)
                    SeekToMillisecondsLocked(0);

                mediaPlayer.Start();
                status = PlayerStatus.Playing;
                return true;
            }
            catch
            {
                status = PlayerStatus.Stopped;
                AbandonAudioFocus();
                return false;
            }
        }
    }

    public bool Pause()
    {
        bool paused;
        lock (syncRoot)
        {
            if (mediaPlayer is null || !prepared)
                return false;

            try
            {
                if (status == PlayerStatus.Playing)
                    mediaPlayer.Pause();

                status = PlayerStatus.Paused;
                paused = true;
            }
            catch
            {
                paused = false;
            }
        }

        AbandonAudioFocus();
        return paused;
    }

    public bool Stop()
    {
        bool stopped = true;
        lock (syncRoot)
        {
            if (mediaPlayer is not null && prepared)
            {
                try
                {
                    mediaPlayer.Stop();
                }
                catch
                {
                    stopped = false;
                }

                prepared = false;
            }

            status = PlayerStatus.Stopped;
        }

        AbandonAudioFocus();
        return stopped;
    }

    public void Free()
    {
        lock (syncRoot)
            ReleasePlayerLocked();

        AbandonAudioFocus();
    }

    public bool SetPositionBytes(long positionBytes) => false;

    public bool SetPositionSeconds(double seconds)
    {
        if (double.IsNaN(seconds) || double.IsInfinity(seconds))
            return false;

        double milliseconds = Math.Max(0, seconds) * 1000;
        long position = milliseconds >= long.MaxValue ? long.MaxValue : (long)milliseconds;
        return SetPersistedPosition(position);
    }

    public bool SetPersistedPosition(long position)
    {
        lock (syncRoot)
            return SeekToMillisecondsLocked(position);
    }

    public long GetPositionBytes() => 0;

    public long GetLengthBytes() => 0;

    public long GetPersistedPosition()
    {
        lock (syncRoot)
        {
            if (mediaPlayer is null || !prepared)
                return 0;

            try
            {
                return Math.Max(0, mediaPlayer.CurrentPosition);
            }
            catch
            {
                return 0;
            }
        }
    }

    public double GetPositionSeconds() => GetPersistedPosition() / 1000.0;

    public double GetLengthSeconds()
    {
        lock (syncRoot)
        {
            if (mediaPlayer is null || !prepared)
                return 0;

            try
            {
                return Math.Max(0, mediaPlayer.Duration) / 1000.0;
            }
            catch
            {
                return 0;
            }
        }
    }

    public void SetVolume(float volume)
    {
        lock (syncRoot)
        {
            Volume = Math.Clamp(volume, 0f, 1f);
            ApplyVolumeLocked();
        }
    }

    public void Dispose()
    {
        lock (syncRoot)
        {
            if (disposed)
                return;

            disposed = true;
            ReleasePlayerLocked();
        }

        AbandonAudioFocus();
        audioFocusRequest?.Dispose();
        audioAttributes?.Dispose();
        audioFocusListener.Dispose();
    }

    private bool RequestAudioFocus()
    {
        if (!Initialize())
            return false;

        AudioFocusRequest result;
        lock (syncRoot)
        {
            if (audioManager is null)
                return false;

            if (hasAudioFocus)
                return true;

            if (OperatingSystem.IsAndroidVersionAtLeast(26) && audioFocusRequest is not null)
            {
                result = audioManager.RequestAudioFocus(audioFocusRequest);
            }
            else
            {
#pragma warning disable CA1422
                result = audioManager.RequestAudioFocus(audioFocusListener, global::Android.Media.Stream.Music, AudioFocus.Gain);
#pragma warning restore CA1422
            }

            hasAudioFocus = result == AudioFocusRequest.Granted;
            return hasAudioFocus;
        }
    }

    private void AbandonAudioFocus()
    {
        lock (syncRoot)
        {
            if (!hasAudioFocus || audioManager is null)
                return;

            if (OperatingSystem.IsAndroidVersionAtLeast(26) && audioFocusRequest is not null)
            {
                audioManager.AbandonAudioFocusRequest(audioFocusRequest);
            }
            else
            {
#pragma warning disable CA1422
                audioManager.AbandonAudioFocus(audioFocusListener);
#pragma warning restore CA1422
            }

            hasAudioFocus = false;
        }
    }

    private bool SeekToMillisecondsLocked(long position)
    {
        if (mediaPlayer is null || !prepared)
            return false;

        try
        {
            long duration = Math.Max(0, mediaPlayer.Duration);
            long clamped = duration > 0
                ? Math.Clamp(position, 0, duration)
                : Math.Max(0, position);

            if (OperatingSystem.IsAndroidVersionAtLeast(26))
            {
                mediaPlayer.SeekTo(clamped, MediaPlayerSeekMode.Closest);
            }
            else
            {
#pragma warning disable CA1422
                mediaPlayer.SeekTo((int)Math.Min(clamped, int.MaxValue));
#pragma warning restore CA1422
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private void ApplyVolumeLocked()
    {
        if (mediaPlayer is null)
            return;

        try
        {
            mediaPlayer.SetVolume(Volume, Volume);
        }
        catch
        {
            // The next successfully loaded player receives the saved volume.
        }
    }

    private void ReleasePlayerLocked()
    {
        if (mediaPlayer is not null)
        {
            mediaPlayer.Completion -= MediaPlayer_Completion;
            mediaPlayer.Error -= MediaPlayer_Error;

            try
            {
                mediaPlayer.Release();
            }
            catch
            {
                // Release is best effort while replacing or disposing a player.
            }

            mediaPlayer.Dispose();
            mediaPlayer = null;
        }

        prepared = false;
        status = PlayerStatus.Stopped;
    }

    private void MediaPlayer_Completion(object? sender, EventArgs e)
    {
        lock (syncRoot)
        {
            if (!ReferenceEquals(sender, mediaPlayer))
                return;

            status = PlayerStatus.Stopped;
        }

        AbandonAudioFocus();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void MediaPlayer_Error(object? sender, MediaPlayer.ErrorEventArgs e)
    {
        lock (syncRoot)
        {
            if (!ReferenceEquals(sender, mediaPlayer))
                return;

            status = PlayerStatus.Stopped;
            e.Handled = true;
        }

        AbandonAudioFocus();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnAudioFocusChanged(AudioFocus focusChange)
    {
        if (focusChange == AudioFocus.Gain)
        {
            lock (syncRoot)
                hasAudioFocus = true;
            return;
        }

        bool stateChanged = false;
        lock (syncRoot)
        {
            hasAudioFocus = false;
            if (status == PlayerStatus.Playing && mediaPlayer is not null && prepared)
            {
                try
                {
                    mediaPlayer.Pause();
                    status = PlayerStatus.Paused;
                    stateChanged = true;
                }
                catch
                {
                    status = PlayerStatus.Stopped;
                    stateChanged = true;
                }
            }
        }

        if (stateChanged)
            StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private sealed class AudioFocusListener : Java.Lang.Object, AudioManager.IOnAudioFocusChangeListener
    {
        private readonly WeakReference<AndroidPlaybackEngine> owner;

        public AudioFocusListener(AndroidPlaybackEngine owner)
        {
            this.owner = new WeakReference<AndroidPlaybackEngine>(owner);
        }

        public void OnAudioFocusChange(AudioFocus focusChange)
        {
            if (owner.TryGetTarget(out AndroidPlaybackEngine? engine))
                engine.OnAudioFocusChanged(focusChange);
        }
    }
}

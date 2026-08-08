using wildflower.Models;
using wildflower.Services.Playback;

namespace wildflowerFrontend.Services;

/// <summary>
/// Keeps unsupported MAUI targets launchable without pretending that audio is available.
/// </summary>
public sealed class UnavailablePlaybackEngine : IPlaybackEngine
{
    public bool HasStream => false;
    public PlayerStatus Status => PlayerStatus.Stopped;
    public float Volume { get; private set; } = 0.3f;

    public bool Initialize() => false;
    public bool Load(string filePath, long startPositionBytes = 0) => false;
    public bool Play(bool restart = false) => false;
    public bool Pause() => false;
    public bool Stop() => true;
    public void Free() { }
    public bool SetPositionBytes(long positionBytes) => false;
    public bool SetPositionSeconds(double seconds) => false;
    public long GetPositionBytes() => 0;
    public long GetLengthBytes() => 0;
    public double GetPositionSeconds() => 0;
    public double GetLengthSeconds() => 0;
    public void SetVolume(float volume) => Volume = Math.Clamp(volume, 0f, 1f);
    public void Dispose() { }
}

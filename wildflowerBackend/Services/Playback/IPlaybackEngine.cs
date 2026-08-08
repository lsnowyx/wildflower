using wildflower.Models;

namespace wildflower.Services.Playback
{
    public interface IPlaybackEngine : IDisposable
    {
        event EventHandler? StateChanged
        {
            add { }
            remove { }
        }

        bool Initialize();
        bool HasStream { get; }
        PlayerStatus Status { get; }
        float Volume { get; }

        bool Load(string source, long startPosition = 0);
        bool Play(bool restart = false);
        bool Pause();
        bool Stop();
        void Free();
        bool SetPositionBytes(long positionBytes);
        bool SetPositionSeconds(double seconds);
        bool SetPersistedPosition(long position) => SetPositionBytes(position);
        long GetPositionBytes();
        long GetLengthBytes();
        long GetPersistedPosition() => GetPositionBytes();
        double GetPositionSeconds();
        double GetLengthSeconds();
        void SetVolume(float volume);
    }
}

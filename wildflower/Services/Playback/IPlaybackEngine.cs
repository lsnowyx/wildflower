using wildflower.Models;

namespace wildflower.Services.Playback
{
    public interface IPlaybackEngine : IDisposable
    {
        bool Initialize();
        bool HasStream { get; }
        PlayerStatus Status { get; }
        float Volume { get; }

        bool Load(string filePath, long startPositionBytes = 0);
        bool Play(bool restart = false);
        bool Pause();
        bool Stop();
        void Free();
        bool SetPositionBytes(long positionBytes);
        bool SetPositionSeconds(double seconds);
        long GetPositionBytes();
        long GetLengthBytes();
        double GetPositionSeconds();
        double GetLengthSeconds();
        void SetVolume(float volume);
    }
}

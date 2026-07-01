namespace wildflower.Services.Audio
{
    public interface IAudioDeviceWatcher : IDisposable
    {
        event EventHandler? DefaultDeviceChanged;
        void Start();
        void Stop();
    }
}

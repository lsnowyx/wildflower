using wildflower.Services.Audio;

namespace wildflowerFrontend.Services;

/// <summary>
/// A truthful no-op for targets that do not have the Windows/NAudio watcher.
/// </summary>
public sealed class UnavailableAudioDeviceWatcher : IAudioDeviceWatcher
{
    public event EventHandler? DefaultDeviceChanged
    {
        add { }
        remove { }
    }

    public void Start() { }
    public void Stop() { }
    public void Dispose() { }
}

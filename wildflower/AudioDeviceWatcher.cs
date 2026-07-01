using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using wildflower.Services.Audio;

namespace wildflower
{
    public sealed class AudioDeviceWatcher : IAudioDeviceWatcher
    {
        private readonly MMDeviceEnumerator deviceEnumerator = new();
        private readonly DefaultDeviceNotificationClient notificationClient;
        private bool started;
        private bool disposed;

        public AudioDeviceWatcher()
        {
            notificationClient = new DefaultDeviceNotificationClient(OnDefaultDeviceChanged);
        }

        public event EventHandler? DefaultDeviceChanged;

        public void Start()
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (started) return;

            deviceEnumerator.RegisterEndpointNotificationCallback(notificationClient);
            started = true;
        }

        public void Stop()
        {
            if (!started) return;

            try
            {
                deviceEnumerator.UnregisterEndpointNotificationCallback(notificationClient);
            }
            finally
            {
                started = false;
            }
        }

        public void Dispose()
        {
            if (disposed) return;

            Stop();
            deviceEnumerator.Dispose();
            disposed = true;
        }

        private void OnDefaultDeviceChanged()
        {
            DefaultDeviceChanged?.Invoke(this, EventArgs.Empty);
        }

        private sealed class DefaultDeviceNotificationClient : IMMNotificationClient
        {
            private readonly Action defaultDeviceChanged;

            public DefaultDeviceNotificationClient(Action defaultDeviceChanged)
            {
                this.defaultDeviceChanged = defaultDeviceChanged;
            }

            public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
            {
                if (flow == DataFlow.Render && role == Role.Console)
                    defaultDeviceChanged();
            }

            public void OnDeviceAdded(string pwstrDeviceId) { }
            public void OnDeviceRemoved(string deviceId) { }
            public void OnDeviceStateChanged(string deviceId, DeviceState newState) { }
            public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) { }
        }
    }
}

using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace wildflower
{
    public class AudioDeviceWatcher : IMMNotificationClient
    {
        public event Action DefaultDeviceChanged;
        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
            if (flow == DataFlow.Render && role == Role.Console)
            {
                DefaultDeviceChanged?.Invoke();
            }
        }
        public void OnDeviceAdded(string pwstrDeviceId) { }
        public void OnDeviceRemoved(string deviceId) { }
        public void OnDeviceStateChanged(string deviceId, DeviceState newState) { }
        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) { }
    }
}

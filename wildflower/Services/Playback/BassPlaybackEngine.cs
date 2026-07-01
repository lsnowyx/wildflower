using Un4seen.Bass;
using wildflower.Models;

namespace wildflower.Services.Playback
{
    public sealed class BassPlaybackEngine : IPlaybackEngine
    {
        private int streamHandle;
        private bool initialized;

        public bool HasStream => streamHandle != 0;

        public PlayerStatus Status
        {
            get
            {
                if (streamHandle == 0)
                    return PlayerStatus.Stopped;

                return Bass.BASS_ChannelIsActive(streamHandle) switch
                {
                    BASSActive.BASS_ACTIVE_PLAYING => PlayerStatus.Playing,
                    BASSActive.BASS_ACTIVE_PAUSED => PlayerStatus.Paused,
                    _ => PlayerStatus.Stopped
                };
            }
        }

        public float Volume { get; private set; } = 0.3f;

        public bool Initialize()
        {
            if (initialized)
                return true;

            initialized = Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            return initialized;
        }

        public bool Load(string filePath, long startPositionBytes = 0)
        {
            if (!initialized && !Initialize())
                return false;

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return false;

            Stop();
            Free();

            streamHandle = Bass.BASS_StreamCreateFile(filePath, 0L, 0L, BASSFlag.BASS_DEFAULT);
            if (streamHandle == 0)
                return false;

            SetVolume(Volume);

            if (startPositionBytes > 0)
                SetPositionBytes(startPositionBytes);

            return true;
        }

        public bool Play(bool restart = false)
        {
            return streamHandle != 0 && Bass.BASS_ChannelPlay(streamHandle, restart);
        }

        public bool Pause()
        {
            return streamHandle != 0 && Bass.BASS_ChannelPause(streamHandle);
        }

        public bool Stop()
        {
            return streamHandle == 0 || Bass.BASS_ChannelStop(streamHandle);
        }

        public void Free()
        {
            if (streamHandle == 0)
                return;

            Bass.BASS_StreamFree(streamHandle);
            streamHandle = 0;
        }

        public bool SetPositionBytes(long positionBytes)
        {
            if (streamHandle == 0)
                return false;

            var lengthBytes = GetLengthBytes();
            if (lengthBytes > 0)
                positionBytes = Math.Min(positionBytes, lengthBytes);

            return Bass.BASS_ChannelSetPosition(streamHandle, Math.Max(0, positionBytes));
        }

        public bool SetPositionSeconds(double seconds)
        {
            if (streamHandle == 0)
                return false;

            long bytePosition = Bass.BASS_ChannelSeconds2Bytes(streamHandle, Math.Max(0, seconds));
            return bytePosition >= 0 && SetPositionBytes(bytePosition);
        }

        public long GetPositionBytes()
        {
            return streamHandle == 0 ? 0 : Math.Max(0, Bass.BASS_ChannelGetPosition(streamHandle));
        }

        public long GetLengthBytes()
        {
            return streamHandle == 0 ? 0 : Math.Max(0, Bass.BASS_ChannelGetLength(streamHandle));
        }

        public double GetPositionSeconds()
        {
            if (streamHandle == 0)
                return 0;

            return Math.Max(0, Bass.BASS_ChannelBytes2Seconds(streamHandle, GetPositionBytes()));
        }

        public double GetLengthSeconds()
        {
            if (streamHandle == 0)
                return 0;

            return Math.Max(0, Bass.BASS_ChannelBytes2Seconds(streamHandle, GetLengthBytes()));
        }

        public void SetVolume(float volume)
        {
            Volume = Math.Clamp(volume, 0f, 1f);
            if (streamHandle != 0)
                Bass.BASS_ChannelSetAttribute(streamHandle, BASSAttribute.BASS_ATTRIB_VOL, Volume);
        }

        public void Dispose()
        {
            Stop();
            Free();

            if (initialized)
            {
                Bass.BASS_Free();
                initialized = false;
            }
        }
    }
}

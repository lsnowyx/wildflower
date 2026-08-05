namespace wildflower.Models
{
    public sealed record PlayerSessionSnapshot(
        PlaylistInfo? CurrentPlaylist,
        IReadOnlyList<TrackInfo> Tracks,
        int CurrentIndex,
        TrackInfo? CurrentTrack,
        PlayerStatus Status,
        LoopMode LoopMode,
        bool IsPlaying,
        bool IsTemporaryPlayback,
        long PositionBytes,
        long LengthBytes,
        double PositionSeconds,
        double LengthSeconds,
        double ProgressPercent,
        float Volume,
        bool CanPlay,
        bool CanPause,
        bool CanGoNext,
        bool CanGoPrevious,
        bool CanSeek);
}

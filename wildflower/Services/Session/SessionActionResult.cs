namespace wildflower.Services.Session
{
    public sealed record SessionActionResult(
        bool Succeeded = true,
        bool TrackListChanged = false,
        bool PlaybackChanged = false,
        bool TemporaryPlaybackChanged = false,
        bool MissingMusicFolder = false,
        bool ClearedPlaylist = false,
        string? Message = null,
        string? TemporaryTrackDisplayName = null)
    {
        public static SessionActionResult NoChange { get; } = new();
    }
}

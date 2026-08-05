using wildflower.Models;
using wildflower.Services.Playlist;

namespace wildflower.Services.Session
{
    public interface IPlayerSessionService : IDisposable
    {
        event EventHandler<PlayerSessionSnapshot>? SnapshotChanged;
        event EventHandler<PlaybackProgress>? ProgressChanged;

        IReadOnlyList<string> Tracks { get; }
        PlaylistInfo? CurrentPlaylist { get; }
        int CurrentIndex { get; }
        long SavedPositionBytes { get; }
        bool IsPlaying { get; }
        bool IsTemporaryPlayback { get; }
        bool IsLooped { get; }
        bool HasTracks { get; }
        float Volume { get; }

        bool InitializePlaybackEngine();
        Task<PlayerSessionInitializationResult> InitializeAsync();
        Task<IReadOnlyList<PlaylistInfo>> GetPlaylistsAsync();
        Task<SessionActionResult> AddPlaylistAsync(string musicFolderPath);
        Task<SessionActionResult> SelectPlaylistAsync(string playlistId);
        Task<DeletePlaylistResult> DeletePlaylistAsync(string playlistId);
        Task<SessionActionResult> RefreshPlaylistAsync();
        Task<SessionActionResult> RefreshPlaylistAndRestoreAsync();
        Task<SessionActionResult> ShuffleTracksAsync();
        Task<SessionActionResult> AdvanceIfStoppedAsync();
        Task<SessionActionResult> PlayTemporaryTrackAsync(string filePath);
        Task<SessionActionResult> ReturnFromTemporaryPlaybackAsync();
        bool PlayTrack(int index, long startPositionBytes = 0);
        bool PlayCurrentTrack(long startPositionBytes = 0);
        bool TogglePlayPause();
        bool NextTrack();
        bool PreviousTrack();
        void SetLooped(bool looped);
        void SetVolume(float volume);
        void SeekToMilliseconds(int milliseconds);
        PlayerSessionSnapshot GetSnapshot();
        PlaybackProgress GetProgress();
        Task SavePlaybackStateAsync();
        Task<IReadOnlyList<string>> GetTrackDisplayNamesAsync();
        Task<string> GetTrackDisplayNameAsync(string filePath);
    }
}

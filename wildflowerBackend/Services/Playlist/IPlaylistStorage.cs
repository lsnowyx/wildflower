using wildflower.Models;

namespace wildflower.Services.Playlist
{
    public interface IPlaylistStorage
    {
        string PlaylistsDirectory { get; }

        Task EnsureInitializedAsync();
        Task<string?> ReadLastUsedPlaylistIdAsync();
        Task WriteLastUsedPlaylistIdAsync(string playlistId);
        Task<IReadOnlyList<PlaylistInfo>> GetPlaylistsAsync();
        Task<PlaylistInfo?> GetPlaylistAsync(string playlistId);
        Task<PlaylistInfo> CreatePlaylistAsync(string playlistId, string musicFolderPath);
        Task DeletePlaylistAsync(string playlistId);
        Task<bool> PlaylistFileExistsAsync(PlaylistInfo playlist);
        Task<IReadOnlyList<string>> LoadTrackPathsAsync(PlaylistInfo playlist);
        Task SaveTrackPathsAsync(PlaylistInfo playlist, IEnumerable<string> trackPaths);
        Task<PlaybackState?> LoadPlaybackStateAsync(PlaylistInfo playlist);
        Task SavePlaybackStateAsync(PlaylistInfo playlist, PlaybackState playbackState);
    }
}

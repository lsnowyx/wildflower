using wildflower.Models;

namespace wildflower.Services.Playlist
{
    public interface IPlaylistService
    {
        string PlaylistsDirectory { get; }

        Task<PlaylistInfo?> GetLastOrFirstPlaylistAsync();
        Task<IReadOnlyList<PlaylistInfo>> GetPlaylistsAsync();
        Task<PlaylistInfo?> GetPlaylistAsync(string playlistId);
        Task<AddPlaylistResult> AddPlaylistAsync(string musicFolderPath);
        Task DeletePlaylistAsync(string playlistId);
        Task<PlaylistInfo?> FindAvailablePlaylistAsync();
        Task RemoveInvalidPlaylistsAsync();
        Task<bool> PlaylistFileExistsAsync(PlaylistInfo playlist);
        Task<IReadOnlyList<string>> LoadTrackPathsAsync(PlaylistInfo playlist);
        Task SaveTrackPathsAsync(PlaylistInfo playlist, IEnumerable<string> trackPaths);
        Task<PlaybackState?> LoadPlaybackStateAsync(PlaylistInfo playlist);
        Task SavePlaybackStateAsync(PlaylistInfo playlist, PlaybackState playbackState);
        Task SaveLastUsedPlaylistAsync(PlaylistInfo playlist);
    }
}

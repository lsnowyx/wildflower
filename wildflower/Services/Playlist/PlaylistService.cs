using wildflower.Models;

namespace wildflower.Services.Playlist
{
    public sealed class PlaylistService : IPlaylistService
    {
        private readonly IPlaylistStorage storage;

        public PlaylistService(IPlaylistStorage storage)
        {
            this.storage = storage;
        }

        public string PlaylistsDirectory => storage.PlaylistsDirectory;

        public async Task<PlaylistInfo?> GetLastOrFirstPlaylistAsync()
        {
            await storage.EnsureInitializedAsync();

            string? lastUsedId = await storage.ReadLastUsedPlaylistIdAsync();
            if (lastUsedId != null)
            {
                PlaylistInfo? lastUsedPlaylist = await storage.GetPlaylistAsync(lastUsedId);
                if (lastUsedPlaylist != null)
                    return lastUsedPlaylist;
            }

            PlaylistInfo? firstPlaylist = (await storage.GetPlaylistsAsync()).FirstOrDefault();
            if (firstPlaylist != null)
                await storage.WriteLastUsedPlaylistIdAsync(firstPlaylist.Id);

            return firstPlaylist;
        }

        public Task<IReadOnlyList<PlaylistInfo>> GetPlaylistsAsync()
        {
            return storage.GetPlaylistsAsync();
        }

        public Task<PlaylistInfo?> GetPlaylistAsync(string playlistId)
        {
            return storage.GetPlaylistAsync(playlistId);
        }

        public async Task<AddPlaylistResult> AddPlaylistAsync(string musicFolderPath)
        {
            if (string.IsNullOrWhiteSpace(musicFolderPath))
                return new AddPlaylistResult(false, null, Message: "No folder selected.");

            await storage.EnsureInitializedAsync();

            foreach (PlaylistInfo playlist in await storage.GetPlaylistsAsync())
            {
                if (string.Equals(playlist.MusicFolderPath, musicFolderPath, StringComparison.OrdinalIgnoreCase))
                {
                    return new AddPlaylistResult(
                        false,
                        playlist,
                        Duplicate: true,
                        Message: "This folder is already part of a playlist.");
                }
            }

            int nextIndex = 0;
            while (Directory.Exists(Path.Combine(storage.PlaylistsDirectory, nextIndex.ToString())))
                nextIndex++;

            PlaylistInfo newPlaylist = await storage.CreatePlaylistAsync(nextIndex.ToString(), musicFolderPath);
            return new AddPlaylistResult(true, newPlaylist);
        }

        public Task DeletePlaylistAsync(string playlistId)
        {
            return storage.DeletePlaylistAsync(playlistId);
        }

        public async Task<PlaylistInfo?> FindAvailablePlaylistAsync()
        {
            PlaylistInfo? playlist = (await storage.GetPlaylistsAsync()).FirstOrDefault();
            if (playlist != null)
                await storage.WriteLastUsedPlaylistIdAsync(playlist.Id);

            return playlist;
        }

        public async Task RemoveInvalidPlaylistsAsync()
        {
            foreach (PlaylistInfo playlist in await storage.GetPlaylistsAsync())
            {
                if (!Directory.Exists(playlist.MusicFolderPath))
                    await storage.DeletePlaylistAsync(playlist.Id);
            }
        }

        public Task<bool> PlaylistFileExistsAsync(PlaylistInfo playlist)
        {
            return storage.PlaylistFileExistsAsync(playlist);
        }

        public Task<IReadOnlyList<string>> LoadTrackPathsAsync(PlaylistInfo playlist)
        {
            return storage.LoadTrackPathsAsync(playlist);
        }

        public Task SaveTrackPathsAsync(PlaylistInfo playlist, IEnumerable<string> trackPaths)
        {
            return storage.SaveTrackPathsAsync(playlist, trackPaths);
        }

        public Task<PlaybackState?> LoadPlaybackStateAsync(PlaylistInfo playlist)
        {
            return storage.LoadPlaybackStateAsync(playlist);
        }

        public Task SavePlaybackStateAsync(PlaylistInfo playlist, PlaybackState playbackState)
        {
            return storage.SavePlaybackStateAsync(playlist, playbackState);
        }

        public Task SaveLastUsedPlaylistAsync(PlaylistInfo playlist)
        {
            return storage.WriteLastUsedPlaylistIdAsync(playlist.Id);
        }
    }
}

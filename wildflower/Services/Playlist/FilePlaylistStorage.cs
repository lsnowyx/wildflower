using wildflower.Models;

namespace wildflower.Services.Playlist
{
    public sealed class FilePlaylistStorage : IPlaylistStorage
    {
        public string PlaylistsDirectory { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ".wildflower",
            "playlists");

        private string LastUsedFile => Path.Combine(PlaylistsDirectory, "lastUsed.txt");

        public Task EnsureInitializedAsync()
        {
            Directory.CreateDirectory(PlaylistsDirectory);
            return Task.CompletedTask;
        }

        public async Task<string?> ReadLastUsedPlaylistIdAsync()
        {
            if (!File.Exists(LastUsedFile))
                return null;

            string playlistId = (await File.ReadAllTextAsync(LastUsedFile)).Trim();
            return string.IsNullOrWhiteSpace(playlistId) ? null : playlistId;
        }

        public async Task WriteLastUsedPlaylistIdAsync(string playlistId)
        {
            await EnsureInitializedAsync();
            await File.WriteAllTextAsync(LastUsedFile, playlistId);
        }

        public async Task<IReadOnlyList<PlaylistInfo>> GetPlaylistsAsync()
        {
            await EnsureInitializedAsync();

            var playlists = new List<PlaylistInfo>();
            foreach (string directory in Directory.GetDirectories(PlaylistsDirectory))
            {
                string musicFolderPathFile = Path.Combine(directory, "musicFolderPath.txt");
                if (!File.Exists(musicFolderPathFile))
                    continue;

                string musicFolderPath = await File.ReadAllTextAsync(musicFolderPathFile);
                playlists.Add(new PlaylistInfo(Path.GetFileName(directory), directory, musicFolderPath));
            }

            return playlists;
        }

        public async Task<PlaylistInfo?> GetPlaylistAsync(string playlistId)
        {
            if (string.IsNullOrWhiteSpace(playlistId))
                return null;

            await EnsureInitializedAsync();
            string directory = Path.Combine(PlaylistsDirectory, playlistId);
            string musicFolderPathFile = Path.Combine(directory, "musicFolderPath.txt");
            if (!Directory.Exists(directory) || !File.Exists(musicFolderPathFile))
                return null;

            string musicFolderPath = await File.ReadAllTextAsync(musicFolderPathFile);
            return new PlaylistInfo(playlistId, directory, musicFolderPath);
        }

        public async Task<PlaylistInfo> CreatePlaylistAsync(string playlistId, string musicFolderPath)
        {
            await EnsureInitializedAsync();
            string directory = Path.Combine(PlaylistsDirectory, playlistId);
            Directory.CreateDirectory(directory);
            await File.WriteAllTextAsync(Path.Combine(directory, "musicFolderPath.txt"), musicFolderPath);
            await WriteLastUsedPlaylistIdAsync(playlistId);
            return new PlaylistInfo(playlistId, directory, musicFolderPath);
        }

        public async Task DeletePlaylistAsync(string playlistId)
        {
            await EnsureInitializedAsync();
            string directory = Path.Combine(PlaylistsDirectory, playlistId);
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }

        public Task<bool> PlaylistFileExistsAsync(PlaylistInfo playlist)
        {
            return Task.FromResult(File.Exists(playlist.PlaylistFile));
        }

        public async Task<IReadOnlyList<string>> LoadTrackPathsAsync(PlaylistInfo playlist)
        {
            if (!File.Exists(playlist.PlaylistFile))
                return Array.Empty<string>();

            var lines = await File.ReadAllLinesAsync(playlist.PlaylistFile);
            return lines
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => Path.Combine(playlist.MusicFolderPath, line))
                .ToArray();
        }

        public async Task SaveTrackPathsAsync(PlaylistInfo playlist, IEnumerable<string> trackPaths)
        {
            Directory.CreateDirectory(playlist.DirectoryPath);
            var fileNames = trackPaths.Select(Path.GetFileName).Where(fileName => fileName != null);
            await File.WriteAllLinesAsync(playlist.PlaylistFile, fileNames!);
        }

        public async Task<PlaybackState?> LoadPlaybackStateAsync(PlaylistInfo playlist)
        {
            if (!File.Exists(playlist.StateFile))
                return null;

            string stateText = await File.ReadAllTextAsync(playlist.StateFile);
            string[] parts = stateText.Split('|');
            if (parts.Length != 2 ||
                !int.TryParse(parts[0], out int index) ||
                !long.TryParse(parts[1], out long positionBytes))
            {
                return null;
            }

            return new PlaybackState(index, positionBytes);
        }

        public async Task SavePlaybackStateAsync(PlaylistInfo playlist, PlaybackState playbackState)
        {
            Directory.CreateDirectory(playlist.DirectoryPath);
            await File.WriteAllTextAsync(playlist.StateFile, $"{playbackState.CurrentIndex}|{playbackState.PositionBytes}");
        }
    }
}

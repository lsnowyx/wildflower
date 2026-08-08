using Microsoft.Maui.Storage;
using wildflower.Models;
using wildflower.Services.Playlist;

namespace wildflowerFrontend.Platforms.Android;

public sealed class AndroidPlaylistStorage : IPlaylistStorage
{
    public string PlaylistsDirectory { get; } = Path.Combine(
        FileSystem.AppDataDirectory,
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
            string sourceFile = Path.Combine(directory, "musicFolderPath.txt");
            if (!File.Exists(sourceFile))
                continue;

            string sourceUri = (await File.ReadAllTextAsync(sourceFile)).Trim();
            if (!string.IsNullOrWhiteSpace(sourceUri))
                playlists.Add(new PlaylistInfo(Path.GetFileName(directory), directory, sourceUri));
        }

        return playlists;
    }

    public async Task<PlaylistInfo?> GetPlaylistAsync(string playlistId)
    {
        if (string.IsNullOrWhiteSpace(playlistId))
            return null;

        await EnsureInitializedAsync();
        string directory = Path.Combine(PlaylistsDirectory, playlistId);
        string sourceFile = Path.Combine(directory, "musicFolderPath.txt");
        if (!Directory.Exists(directory) || !File.Exists(sourceFile))
            return null;

        string sourceUri = (await File.ReadAllTextAsync(sourceFile)).Trim();
        return string.IsNullOrWhiteSpace(sourceUri)
            ? null
            : new PlaylistInfo(playlistId, directory, sourceUri);
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

        return (await File.ReadAllLinesAsync(playlist.PlaylistFile))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();
    }

    public async Task SaveTrackPathsAsync(PlaylistInfo playlist, IEnumerable<string> trackPaths)
    {
        Directory.CreateDirectory(playlist.DirectoryPath);
        await File.WriteAllLinesAsync(playlist.PlaylistFile, trackPaths);
    }

    public async Task<PlaybackState?> LoadPlaybackStateAsync(PlaylistInfo playlist)
    {
        if (!File.Exists(playlist.StateFile))
            return null;

        string[] parts = (await File.ReadAllTextAsync(playlist.StateFile)).Split('|');
        if (parts.Length != 2 ||
            !int.TryParse(parts[0], out int index) ||
            !long.TryParse(parts[1], out long position))
        {
            return null;
        }

        return new PlaybackState(index, position);
    }

    public async Task SavePlaybackStateAsync(PlaylistInfo playlist, PlaybackState playbackState)
    {
        Directory.CreateDirectory(playlist.DirectoryPath);
        await File.WriteAllTextAsync(
            playlist.StateFile,
            $"{playbackState.CurrentIndex}|{playbackState.Position}").ConfigureAwait(false);
    }
}

using System.Text.Json;
using Microsoft.Maui.Storage;
using wildflower.Models;

namespace wildflowerFrontend.Services;

public sealed class JsonTrackMetadataCache : ITrackMetadataCache
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly string cacheFile = Path.Combine(FileSystem.AppDataDirectory, ".wildflower", "metadata-cache.json");
    private Dictionary<string, TrackInfo>? entries;

    public async Task<IReadOnlyDictionary<string, TrackInfo>> LoadAsync()
    {
        await gate.WaitAsync().ConfigureAwait(false);
        try
        {
            await EnsureLoadedAsync().ConfigureAwait(false);
            return new Dictionary<string, TrackInfo>(entries!, StringComparer.OrdinalIgnoreCase);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task StoreAsync(IEnumerable<TrackInfo> tracks)
    {
        await gate.WaitAsync().ConfigureAwait(false);
        try
        {
            await EnsureLoadedAsync().ConfigureAwait(false);
            foreach (TrackInfo track in tracks)
                entries![track.FilePath] = track;

            string? directory = Path.GetDirectoryName(cacheFile);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            string temporaryFile = cacheFile + ".tmp";
            string json = JsonSerializer.Serialize(entries);
            await File.WriteAllTextAsync(temporaryFile, json).ConfigureAwait(false);
            File.Move(temporaryFile, cacheFile, overwrite: true);
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task EnsureLoadedAsync()
    {
        if (entries is not null)
            return;

        entries = new Dictionary<string, TrackInfo>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(cacheFile))
            return;

        try
        {
            string json = await File.ReadAllTextAsync(cacheFile).ConfigureAwait(false);
            Dictionary<string, TrackInfo>? stored = JsonSerializer.Deserialize<Dictionary<string, TrackInfo>>(json);
            if (stored is not null)
                entries = new Dictionary<string, TrackInfo>(stored, StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            // A damaged cache is disposable; metadata will be rebuilt in the background.
        }
    }
}

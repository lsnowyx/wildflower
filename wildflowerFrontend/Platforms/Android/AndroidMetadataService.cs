using Android.Content;
using Android.Media;
using wildflower.Models;
using wildflower.Services.Library;
using AndroidApplication = global::Android.App.Application;
using AndroidUri = global::Android.Net.Uri;

namespace wildflowerFrontend.Platforms.Android;

public sealed class AndroidMetadataService : IMetadataService
{
    private readonly Context context = AndroidApplication.Context;

    public TrackInfo GetTrackInfo(string filePath)
    {
        string fallbackTitle = GetDisplayTitle(filePath);
        try
        {
            AndroidUri uri = AndroidUri.Parse(filePath)
                ?? throw new InvalidOperationException("The Android track URI is invalid.");
            using var retriever = new MediaMetadataRetriever();
            retriever.SetDataSource(context, uri);
            string title = retriever.ExtractMetadata(MetadataKey.Title) ?? fallbackTitle;
            string artist = retriever.ExtractMetadata(MetadataKey.Artist) ?? string.Empty;
            return new TrackInfo(filePath, title, artist);
        }
        catch
        {
            return new TrackInfo(filePath, fallbackTitle, string.Empty);
        }
    }

    public Task<IReadOnlyList<TrackInfo>> GetTrackInfoAsync(IEnumerable<string> filePaths)
    {
        string[] paths = filePaths.ToArray();
        return Task.Run<IReadOnlyList<TrackInfo>>(() =>
        {
            var tracks = new TrackInfo[paths.Length];
            Parallel.For(
                0,
                paths.Length,
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                index => tracks[index] = GetTrackInfo(paths[index]));
            return tracks;
        });
    }

    private static string GetDisplayTitle(string filePath)
    {
        string fallback = AndroidUri.Decode(AndroidUri.Parse(filePath)?.LastPathSegment ?? filePath) ?? filePath;
        int separator = fallback.LastIndexOf(':');
        if (separator >= 0 && separator < fallback.Length - 1)
            fallback = fallback[(separator + 1)..];
        return Path.GetFileNameWithoutExtension(fallback);
    }
}

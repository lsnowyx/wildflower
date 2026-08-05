using wildflower.Models;

namespace wildflower.Services.Library
{
    public sealed class TagLibMetadataService : IMetadataService
    {
        public TrackInfo GetTrackInfo(string filePath)
        {
            try
            {
                using var file = TagLib.File.Create(filePath);
                string title = file.Tag.Title ?? Path.GetFileNameWithoutExtension(filePath);
                string artist = file.Tag.FirstPerformer ?? string.Empty;
                return new TrackInfo(filePath, title, artist);
            }
            catch
            {
                return new TrackInfo(filePath, Path.GetFileNameWithoutExtension(filePath), string.Empty);
            }
        }

        public async Task<IReadOnlyList<TrackInfo>> GetTrackInfoAsync(IEnumerable<string> filePaths)
        {
            var paths = filePaths.ToArray();
            if (paths.Length == 0)
                return Array.Empty<TrackInfo>();

            var chunks = paths
                .Select((item, index) => new { item, index })
                .GroupBy(x => x.index / 25)
                .Select((group, order) => new { order, paths = group.Select(x => x.item).ToArray() })
                .ToArray();

            var tasks = chunks.Select(chunk => Task.Run(() =>
            {
                var tracks = chunk.paths.Select(GetTrackInfo).ToArray();
                return (chunk.order, tracks);
            }));

            var results = await Task.WhenAll(tasks);
            return results
                .OrderBy(result => result.order)
                .SelectMany(result => result.tracks)
                .ToArray();
        }
    }
}

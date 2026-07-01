using wildflower.Services.Library;

namespace wildflower.Services.Search
{
    public sealed class SearchService : ISearchService
    {
        private readonly IMetadataService metadataService;

        public SearchService(IMetadataService metadataService)
        {
            this.metadataService = metadataService;
        }

        public Task<IReadOnlyList<string>> FindMatchingTracksAsync(IEnumerable<string> filePaths, string query, bool ignoreCase = true)
        {
            return Task.Run<IReadOnlyList<string>>(() =>
            {
                if (string.IsNullOrWhiteSpace(query))
                    return Array.Empty<string>();

                var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                return filePaths
                    .Where(filePath =>
                    {
                        string fileName = Path.GetFileNameWithoutExtension(filePath);
                        var metadata = metadataService.GetTrackInfo(filePath);

                        return fileName.IndexOf(query, comparison) >= 0
                            || metadata.Title.IndexOf(query, comparison) >= 0
                            || metadata.Artist.IndexOf(query, comparison) >= 0;
                    })
                    .ToArray();
            });
        }
    }
}

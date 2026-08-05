namespace wildflower.Services.Search
{
    public interface ISearchService
    {
        Task<IReadOnlyList<string>> FindMatchingTracksAsync(IEnumerable<string> filePaths, string query, bool ignoreCase = true);
    }
}

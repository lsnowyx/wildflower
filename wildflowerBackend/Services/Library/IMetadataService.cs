using wildflower.Models;

namespace wildflower.Services.Library
{
    public interface IMetadataService
    {
        TrackInfo GetTrackInfo(string filePath);
        Task<IReadOnlyList<TrackInfo>> GetTrackInfoAsync(IEnumerable<string> filePaths);
    }
}

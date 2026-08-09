using wildflower.Models;

namespace wildflowerFrontend.Services;

public interface ITrackMetadataCache
{
    Task<IReadOnlyDictionary<string, TrackInfo>> LoadAsync();
    Task StoreAsync(IEnumerable<TrackInfo> tracks);
}

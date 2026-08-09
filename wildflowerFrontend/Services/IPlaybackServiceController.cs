using wildflower.Models;

namespace wildflowerFrontend.Services;

public interface IPlaybackServiceController
{
    void Update(PlayerSessionSnapshot snapshot);
}

public sealed class UnavailablePlaybackServiceController : IPlaybackServiceController
{
    public void Update(PlayerSessionSnapshot snapshot)
    {
    }
}

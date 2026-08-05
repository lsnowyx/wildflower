namespace wildflower.Services.Session
{
    public sealed record DeletePlaylistResult(
        bool Succeeded,
        bool DeletedCurrentPlaylist,
        bool HasAnyPlaylist,
        SessionActionResult SessionResult,
        string? Message = null);
}

using wildflower.Models;

namespace wildflower.Services.Playlist
{
    public sealed record AddPlaylistResult(
        bool Succeeded,
        PlaylistInfo? Playlist,
        bool Duplicate = false,
        string? Message = null);
}

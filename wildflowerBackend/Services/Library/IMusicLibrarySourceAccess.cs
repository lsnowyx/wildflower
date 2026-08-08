namespace wildflower.Services.Library
{
    /// <summary>
    /// Validates opaque music-library and track identifiers without assuming
    /// that they are filesystem paths.
    /// </summary>
    public interface IMusicLibrarySourceAccess
    {
        bool IsLibraryAvailable(string sourceIdentifier);
        bool IsTrackAvailable(string trackIdentifier);
    }
}

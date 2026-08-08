namespace wildflower.Services.Library
{
    public sealed class FileSystemMusicLibrarySourceAccess : IMusicLibrarySourceAccess
    {
        public bool IsLibraryAvailable(string sourceIdentifier)
        {
            return !string.IsNullOrWhiteSpace(sourceIdentifier) && Directory.Exists(sourceIdentifier);
        }

        public bool IsTrackAvailable(string trackIdentifier)
        {
            return !string.IsNullOrWhiteSpace(trackIdentifier) && File.Exists(trackIdentifier);
        }
    }
}

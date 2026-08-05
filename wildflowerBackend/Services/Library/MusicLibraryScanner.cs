namespace wildflower.Services.Library
{
    public sealed class MusicLibraryScanner : IMusicLibraryScanner
    {
        private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp3",
            ".wav",
            ".flac",
            ".ogg"
        };

        public Task<IReadOnlyList<string>> ScanAsync(string folderPath)
        {
            return Task.Run<IReadOnlyList<string>>(() =>
            {
                if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                    return Array.Empty<string>();

                return Directory
                    .GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(IsSupportedAudioFile)
                    .ToArray();
            });
        }

        public bool IsSupportedAudioFile(string filePath)
        {
            return SupportedExtensions.Contains(Path.GetExtension(filePath));
        }
    }
}

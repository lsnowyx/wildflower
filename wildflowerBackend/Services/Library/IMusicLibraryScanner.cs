namespace wildflower.Services.Library
{
    public interface IMusicLibraryScanner
    {
        Task<IReadOnlyList<string>> ScanAsync(string folderPath);
        bool IsSupportedAudioFile(string filePath);
    }
}

namespace wildflowerFrontend.Services;

public sealed class UnavailableFolderPickerService : IFolderPickerService
{
    public Task<string?> PickFolderAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromException<string?>(new PlatformNotSupportedException(
            "Folder selection is not implemented for this platform yet."));
    }
}

namespace wildflower.Services.Session
{
    public enum PlayerSessionInitializationStatus
    {
        Loaded,
        NeedsMusicFolder,
        Failed
    }

    public sealed record PlayerSessionInitializationResult(
        PlayerSessionInitializationStatus Status,
        SessionActionResult SessionResult,
        string? Message = null)
    {
        public bool Loaded => Status == PlayerSessionInitializationStatus.Loaded;
        public bool NeedsMusicFolder => Status == PlayerSessionInitializationStatus.NeedsMusicFolder;
        public bool Failed => Status == PlayerSessionInitializationStatus.Failed;
    }
}

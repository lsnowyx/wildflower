namespace wildflower.Models
{
    public sealed record TrackInfo(string FilePath, string Title, string Artist)
    {
        public string DisplayName => string.IsNullOrWhiteSpace(Artist)
            ? Title
            : $"{Title} - {Artist}";
    }
}

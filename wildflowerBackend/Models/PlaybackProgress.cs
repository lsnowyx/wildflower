namespace wildflower.Models
{
    public sealed record PlaybackProgress(
        int PositionMilliseconds,
        int LengthMilliseconds,
        long PositionBytes,
        long LengthBytes)
    {
        public static PlaybackProgress Empty { get; } = new(0, 0, 0, 0);
    }
}

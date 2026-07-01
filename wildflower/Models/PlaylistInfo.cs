namespace wildflower.Models
{
    public sealed record PlaylistInfo(string Id, string DirectoryPath, string MusicFolderPath)
    {
        public string MusicFolderPathFile => Path.Combine(DirectoryPath, "musicFolderPath.txt");
        public string PlaylistFile => Path.Combine(DirectoryPath, "playlist.txt");
        public string StateFile => Path.Combine(DirectoryPath, "state.txt");
    }
}

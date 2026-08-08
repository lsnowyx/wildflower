using wildflower.Models;

namespace wildflowerFrontend.ViewModels;

public sealed class PlaylistRowViewModel
{
    public PlaylistRowViewModel(PlaylistInfo playlist, string name)
    {
        Playlist = playlist;
        Name = name;
    }

    public PlaylistInfo Playlist { get; }
    public string Name { get; }
    public string MusicFolderPath => Playlist.MusicFolderPath;
}

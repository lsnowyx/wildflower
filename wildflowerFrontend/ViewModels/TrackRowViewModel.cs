using wildflower.Models;

namespace wildflowerFrontend.ViewModels;

public sealed class TrackRowViewModel : ObservableObject
{
    private bool isCurrent;
    private string title;
    private string artist;

    public TrackRowViewModel(int index, string filePath, string title, string artist)
    {
        Index = index;
        FilePath = filePath;
        this.title = title;
        this.artist = artist;
    }

    public int Index { get; }
    public string FilePath { get; }
    public string Number => (Index + 1).ToString("00");
    public string Title
    {
        get => title;
        private set => SetProperty(ref title, value);
    }

    public string Artist
    {
        get => artist;
        private set
        {
            if (SetProperty(ref artist, value))
                OnPropertyChanged(nameof(HasArtist));
        }
    }
    public bool HasArtist => !string.IsNullOrWhiteSpace(Artist);

    public bool IsCurrent
    {
        get => isCurrent;
        set
        {
            if (SetProperty(ref isCurrent, value))
                OnPropertyChanged(nameof(CurrentIndicator));
        }
    }

    public string CurrentIndicator => IsCurrent ? "PLAYING" : string.Empty;

    public void ApplyMetadata(TrackInfo track)
    {
        Title = track.Title;
        Artist = track.Artist;
    }
}

namespace wildflowerFrontend.ViewModels;

public sealed class TrackRowViewModel : ObservableObject
{
    private bool isCurrent;

    public TrackRowViewModel(int index, string filePath, string title, string artist)
    {
        Index = index;
        FilePath = filePath;
        Title = title;
        Artist = artist;
    }

    public int Index { get; }
    public string FilePath { get; }
    public string Number => (Index + 1).ToString("00");
    public string Title { get; }
    public string Artist { get; }
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
}

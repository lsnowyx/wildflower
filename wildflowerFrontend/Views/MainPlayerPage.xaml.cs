using System.ComponentModel;
using wildflowerFrontend.ViewModels;

namespace wildflowerFrontend.Views;

public partial class MainPlayerPage : ContentPage
{
    private bool initialized;
    private bool scrollDispatchPending;
    private int trackRowsRevision;
    private int lastScrolledIndex = -1;
    private int lastScrolledTrackRowsRevision = -1;

    public MainPlayerPage(MainPlayerViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = viewModel;

        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        ViewModel.TrackRowsReady += ViewModel_TrackRowsReady;
    }

    public MainPlayerViewModel ViewModel { get; }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (initialized)
            return;

        initialized = true;
        await ViewModel.InitializeAsync();
    }

    private void ProgressSlider_DragStarted(object? sender, EventArgs e)
    {
        ViewModel.BeginSeek(ProgressSlider.Value);
    }

    private void ProgressSlider_ValueChanged(object? sender, ValueChangedEventArgs e)
    {
        ViewModel.PreviewSeek(e.NewValue);
    }

    private void ProgressSlider_DragCompleted(object? sender, EventArgs e)
    {
        ViewModel.CommitSeek(ProgressSlider.Value);
    }

    private async void PlayButton_Tapped(object? sender, TappedEventArgs e)
    {
        await PlayButton.ScaleToAsync(0.93, 80, Easing.CubicOut);
        await PlayButton.ScaleToAsync(1, 140, Easing.CubicOut);
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainPlayerViewModel.CurrentIndex))
            DispatchScrollToCurrentTrack();
    }

    private void ViewModel_TrackRowsReady(object? sender, EventArgs e)
    {
        trackRowsRevision++;
        DispatchScrollToCurrentTrack();
    }

    private void DispatchScrollToCurrentTrack()
    {
        if (scrollDispatchPending)
            return;

        scrollDispatchPending = true;
        Dispatcher.Dispatch(() =>
        {
            scrollDispatchPending = false;
            if (ViewModel.IsPlaylistMode)
                return;

            int currentIndex = ViewModel.CurrentIndex;
            if (currentIndex == lastScrolledIndex &&
                trackRowsRevision == lastScrolledTrackRowsRevision)
            {
                return;
            }

            TrackRowViewModel? currentTrack = ViewModel.VisibleTracks.FirstOrDefault(track => track.Index == currentIndex);
            if (currentTrack is null)
                return;

            TrackCollection.ScrollTo(currentTrack, position: ScrollToPosition.Start, animate: true);
            lastScrolledIndex = currentIndex;
            lastScrolledTrackRowsRevision = trackRowsRevision;
        });
    }
}

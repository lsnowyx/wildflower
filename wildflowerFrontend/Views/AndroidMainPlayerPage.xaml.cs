using System.ComponentModel;
using wildflowerFrontend.ViewModels;

namespace wildflowerFrontend.Views;

public partial class AndroidMainPlayerPage : ContentPage
{
    private const uint OverlayAnimationMilliseconds = 220;
    private bool initialized;
    private bool isPlaylistOpen;
    private bool isTransitioning;
    private bool scrollDispatchPending;
    private bool forceScrollPending;
    private int trackRowsRevision;
    private int lastScrolledIndex = -1;
    private int lastScrolledTrackRowsRevision = -1;

    public AndroidMainPlayerPage(MainPlayerViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = viewModel;

        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        ViewModel.TrackInvoked += ViewModel_TrackInvoked;
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

    protected override bool OnBackButtonPressed()
    {
        if (ViewModel.IsSearchMode)
        {
            ViewModel.CloseSearchCommand.Execute(null);
            DispatchScrollToCurrentTrack(force: true);
            return true;
        }

        if (ViewModel.IsPlaylistMode)
        {
            ViewModel.ClosePlaylistsCommand.Execute(null);
            DispatchScrollToCurrentTrack(force: true);
            return true;
        }

        if (isPlaylistOpen)
        {
            _ = ClosePlaylistAsync();
            return true;
        }

        return base.OnBackButtonPressed();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (!isPlaylistOpen && !isTransitioning)
            PlaylistOverlay.TranslationX = -Math.Max(1, width);
    }

    private async void PlayerHamburger_Tapped(object? sender, TappedEventArgs e)
    {
        await OpenPlaylistAsync();
    }

    private async void PlaylistHamburger_Tapped(object? sender, TappedEventArgs e)
    {
        await ClosePlaylistAsync();
    }

    private async Task OpenPlaylistAsync()
    {
        if (isPlaylistOpen || isTransitioning)
            return;

        isTransitioning = true;
        try
        {
            await ViewModel.SynchronizeFromBackendAsync();
            double overlayWidth = Math.Max(1, PageRoot.Width);
            PlaylistOverlay.TranslationX = -overlayWidth;
            PlaylistOverlay.IsVisible = true;
            isPlaylistOpen = true;
            await PlaylistOverlay.TranslateToAsync(0, 0, OverlayAnimationMilliseconds, Easing.CubicOut);
            DispatchScrollToCurrentTrack(force: true);
        }
        finally
        {
            isTransitioning = false;
        }
    }

    private async Task ClosePlaylistAsync()
    {
        if (!isPlaylistOpen || isTransitioning)
            return;

        isTransitioning = true;
        try
        {
            double overlayWidth = Math.Max(1, PageRoot.Width);
            await PlaylistOverlay.TranslateToAsync(-overlayWidth, 0, OverlayAnimationMilliseconds, Easing.CubicIn);
            PlaylistOverlay.IsVisible = false;
            isPlaylistOpen = false;
        }
        finally
        {
            isTransitioning = false;
        }
    }

    private void SearchAction_Tapped(object? sender, TappedEventArgs e)
    {
        ViewModel.OpenSearchCommand.Execute(null);
        Dispatcher.Dispatch(() => SearchEntry.Focus());
    }

    private void ProgressSlider_DragStarted(object? sender, EventArgs e)
    {
        ViewModel.BeginSeek(AndroidProgressSlider.Value);
    }

    private void ProgressSlider_ValueChanged(object? sender, ValueChangedEventArgs e)
    {
        ViewModel.PreviewSeek(e.NewValue);
    }

    private void ProgressSlider_DragCompleted(object? sender, EventArgs e)
    {
        ViewModel.CommitSeek(AndroidProgressSlider.Value);
    }

    private async void PlayButton_Tapped(object? sender, TappedEventArgs e)
    {
        await AndroidPlayButton.ScaleToAsync(0.93, 80, Easing.CubicOut);
        await AndroidPlayButton.ScaleToAsync(1, 140, Easing.CubicOut);
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainPlayerViewModel.CurrentIndex) && isPlaylistOpen)
            DispatchScrollToCurrentTrack();
    }

    private void ViewModel_TrackRowsReady(object? sender, EventArgs e)
    {
        trackRowsRevision++;
        if (isPlaylistOpen)
            DispatchScrollToCurrentTrack();
    }

    private void ViewModel_TrackInvoked(object? sender, EventArgs e)
    {
        if (isPlaylistOpen)
            Dispatcher.Dispatch(() => _ = ClosePlaylistAsync());
    }

    private void DispatchScrollToCurrentTrack(bool force = false)
    {
        forceScrollPending |= force;
        if (scrollDispatchPending)
            return;

        scrollDispatchPending = true;
        Dispatcher.Dispatch(() =>
        {
            scrollDispatchPending = false;
            bool forceScroll = forceScrollPending;
            forceScrollPending = false;

            if (!isPlaylistOpen || !PlaylistOverlay.IsVisible || ViewModel.IsPlaylistMode)
                return;

            int currentIndex = ViewModel.CurrentIndex;
            if (!forceScroll &&
                currentIndex == lastScrolledIndex &&
                trackRowsRevision == lastScrolledTrackRowsRevision)
            {
                return;
            }

            TrackRowViewModel? currentTrack = ViewModel.VisibleTracks.FirstOrDefault(track => track.Index == currentIndex);
            if (currentTrack is null)
                return;

            TracksCollectionView.ScrollTo(currentTrack, position: ScrollToPosition.Start, animate: true);
            lastScrolledIndex = currentIndex;
            lastScrolledTrackRowsRevision = trackRowsRevision;
        });
    }
}

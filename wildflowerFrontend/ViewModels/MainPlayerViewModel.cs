using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Dispatching;
using wildflower.Models;
using wildflower.Services.Audio;
using wildflower.Services.Library;
using wildflower.Services.Search;
using wildflower.Services.Session;
using wildflowerFrontend.Services;

namespace wildflowerFrontend.ViewModels;

public sealed class MainPlayerViewModel : ObservableObject, IAsyncDisposable
{
    private readonly IPlayerSessionService playerSession;
    private readonly ISearchService searchService;
    private readonly IMetadataService metadataService;
    private readonly IAudioDeviceWatcher audioDeviceWatcher;
    private readonly IFolderPickerService folderPickerService;
    private readonly Dictionary<string, TrackInfo> metadataCache = new(StringComparer.OrdinalIgnoreCase);

    private IDispatcherTimer? progressTimer;
    private IDispatcherTimer? saveTimer;
    private bool initialized;
    private bool subscribed;
    private bool disposed;
    private bool progressTickActive;
    private bool saveTickActive;
    private bool playbackAvailable;
    private bool isSeeking;
    private bool applyingVolume;
    private string trackSetKey = string.Empty;

    private bool isBusy;
    private bool isSearchMode;
    private bool isPlaylistMode;
    private bool isTemporaryPlayback;
    private bool isPlaying;
    private bool canGoPrevious;
    private bool canGoNext;
    private bool canPlayPause;
    private bool canSeek;
    private bool isLooped;
    private string playlistName = "NO PLAYLIST";
    private string trackCountText = "0 TRACKS";
    private string currentPositionText = "00 / 00";
    private string currentTitle = "SELECT A TRACK";
    private string currentArtist = "WILDFLOWER SESSION IDLE";
    private string searchQuery = string.Empty;
    private string statusMessage = "INITIALIZING AUDIO SESSION";
    private double progressValue;
    private double progressMaximum = 1;
    private string elapsedText = "00:00";
    private string durationText = "00:00";
    private string progressPercentText = "0%";
    private double volumePercent = 30;
    private int currentIndex = -1;

    public event EventHandler? TrackInvoked;
    public event EventHandler? TrackRowsReady;

    public MainPlayerViewModel(
        IPlayerSessionService playerSession,
        ISearchService searchService,
        IMetadataService metadataService,
        IAudioDeviceWatcher audioDeviceWatcher,
        IFolderPickerService folderPickerService)
    {
        this.playerSession = playerSession;
        this.searchService = searchService;
        this.metadataService = metadataService;
        this.audioDeviceWatcher = audioDeviceWatcher;
        this.folderPickerService = folderPickerService;

        SelectTrackCommand = new Command<TrackRowViewModel>(async track => await SelectTrackAsync(track));
        TogglePlayPauseCommand = new Command(TogglePlayPause);
        PreviousCommand = new Command(PreviousTrack);
        NextCommand = new Command(NextTrack);
        ToggleLoopCommand = new Command(ToggleLoop);
        OpenSearchCommand = new Command(OpenSearch);
        CloseSearchCommand = new Command(CloseSearch);
        ExecuteSearchCommand = new Command(async () => await ExecuteSearchAsync());
        OpenPlaylistsCommand = new Command(async () => await OpenPlaylistsAsync());
        ClosePlaylistsCommand = new Command(ClosePlaylists);
        SelectPlaylistCommand = new Command<PlaylistRowViewModel>(async playlist => await SelectPlaylistAsync(playlist));
        AddFolderCommand = new Command(async () => await AddFolderAsync());
        ReturnFromTemporaryCommand = new Command(async () => await ReturnFromTemporaryAsync());
        RefreshCommand = new Command(async () => await RefreshPlaylistAsync());
    }

    public ObservableCollection<TrackRowViewModel> VisibleTracks { get; } = new();
    public ObservableCollection<PlaylistRowViewModel> Playlists { get; } = new();

    public ICommand SelectTrackCommand { get; }
    public ICommand TogglePlayPauseCommand { get; }
    public ICommand PreviousCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand ToggleLoopCommand { get; }
    public ICommand OpenSearchCommand { get; }
    public ICommand CloseSearchCommand { get; }
    public ICommand ExecuteSearchCommand { get; }
    public ICommand OpenPlaylistsCommand { get; }
    public ICommand ClosePlaylistsCommand { get; }
    public ICommand SelectPlaylistCommand { get; }
    public ICommand AddFolderCommand { get; }
    public ICommand ReturnFromTemporaryCommand { get; }
    public ICommand RefreshCommand { get; }

    public bool IsBusy
    {
        get => isBusy;
        private set => SetProperty(ref isBusy, value);
    }

    public bool IsSearchMode
    {
        get => isSearchMode;
        private set
        {
            if (SetProperty(ref isSearchMode, value))
            {
                OnPropertyChanged(nameof(IsTrackMode));
                OnPropertyChanged(nameof(IsNotSearchMode));
                OnPropertyChanged(nameof(ContentModeLabel));
            }
        }
    }

    public bool IsPlaylistMode
    {
        get => isPlaylistMode;
        private set
        {
            if (SetProperty(ref isPlaylistMode, value))
            {
                OnPropertyChanged(nameof(IsTrackMode));
                OnPropertyChanged(nameof(ContentModeLabel));
            }
        }
    }

    public bool IsTrackMode => !IsPlaylistMode;
    public bool IsNotSearchMode => !IsSearchMode;
    public string ContentModeLabel => IsPlaylistMode ? "PLAYLIST SELECTOR" : IsSearchMode ? "SEARCH RESULTS" : "ACTIVE PLAYLIST";

    public bool IsTemporaryPlayback
    {
        get => isTemporaryPlayback;
        private set => SetProperty(ref isTemporaryPlayback, value);
    }

    public bool IsPlaying
    {
        get => isPlaying;
        private set
        {
            if (SetProperty(ref isPlaying, value))
                OnPropertyChanged(nameof(IsPaused));
        }
    }

    public bool IsPaused => !IsPlaying;

    public bool CanGoPrevious
    {
        get => canGoPrevious;
        private set => SetProperty(ref canGoPrevious, value);
    }

    public bool CanGoNext
    {
        get => canGoNext;
        private set => SetProperty(ref canGoNext, value);
    }

    public bool CanPlayPause
    {
        get => canPlayPause;
        private set => SetProperty(ref canPlayPause, value);
    }

    public bool CanSeek
    {
        get => canSeek;
        private set => SetProperty(ref canSeek, value);
    }

    public bool IsLooped
    {
        get => isLooped;
        private set
        {
            if (SetProperty(ref isLooped, value))
                OnPropertyChanged(nameof(LoopStateText));
        }
    }

    public string LoopStateText => IsLooped ? "ON" : "OFF";

    public string PlaylistName
    {
        get => playlistName;
        private set => SetProperty(ref playlistName, value);
    }

    public string TrackCountText
    {
        get => trackCountText;
        private set => SetProperty(ref trackCountText, value);
    }

    public string CurrentPositionText
    {
        get => currentPositionText;
        private set => SetProperty(ref currentPositionText, value);
    }

    public int CurrentIndex
    {
        get => currentIndex;
        private set => SetProperty(ref currentIndex, value);
    }

    public string CurrentTitle
    {
        get => currentTitle;
        private set => SetProperty(ref currentTitle, value);
    }

    public string CurrentArtist
    {
        get => currentArtist;
        private set => SetProperty(ref currentArtist, value);
    }

    public string SearchQuery
    {
        get => searchQuery;
        set => SetProperty(ref searchQuery, value);
    }

    public string StatusMessage
    {
        get => statusMessage;
        private set => SetProperty(ref statusMessage, value);
    }

    public double ProgressValue
    {
        get => progressValue;
        set => SetProperty(ref progressValue, value);
    }

    public double ProgressMaximum
    {
        get => progressMaximum;
        private set => SetProperty(ref progressMaximum, Math.Max(1, value));
    }

    public string ElapsedText
    {
        get => elapsedText;
        private set => SetProperty(ref elapsedText, value);
    }

    public string DurationText
    {
        get => durationText;
        private set => SetProperty(ref durationText, value);
    }

    public string ProgressPercentText
    {
        get => progressPercentText;
        private set => SetProperty(ref progressPercentText, value);
    }

    public double VolumePercent
    {
        get => volumePercent;
        set
        {
            double clamped = Math.Clamp(value, 0, 100);
            if (!SetProperty(ref volumePercent, clamped))
                return;

            OnPropertyChanged(nameof(VolumeText));
            if (!applyingVolume)
                playerSession.SetVolume((float)(clamped / 100));
        }
    }

    public string VolumeText => $"{Math.Round(VolumePercent):0}%";

    public async Task InitializeAsync()
    {
        if (initialized || disposed)
            return;

        initialized = true;
        IsBusy = true;
        Subscribe();

        try
        {
            playbackAvailable = playerSession.InitializePlaybackEngine();
            if (!playbackAvailable)
                StatusMessage = GetPlaybackUnavailableMessage();

            TryStartAudioWatcher();

            PlayerSessionInitializationResult result = await playerSession.InitializeAsync();
            if (result.Loaded)
            {
                StatusMessage = playbackAvailable ? "SESSION RESTORED" : StatusMessage;
            }
            else if (result.NeedsMusicFolder)
            {
                StatusMessage = "SELECT A MUSIC FOLDER TO BEGIN";
                await AddFolderAsync();
            }
            else
            {
                StatusMessage = result.Message ?? "SESSION INITIALIZATION FAILED";
            }

            await RefreshFromBackendAsync(forceTracks: true);
            StartTimers();
        }
        catch (Exception ex)
        {
            StatusMessage = $"STARTUP ERROR: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void BeginSeek(double milliseconds)
    {
        isSeeking = true;
        PreviewSeek(milliseconds);
    }

    public void PreviewSeek(double milliseconds)
    {
        if (!isSeeking)
            return;

        double previewPosition = Math.Clamp(milliseconds, 0, ProgressMaximum);
        ProgressValue = previewPosition;
        ElapsedText = FormatTime(previewPosition);
        double percent = ProgressMaximum <= 0 ? 0 : previewPosition / ProgressMaximum * 100;
        ProgressPercentText = $"{Math.Clamp(percent, 0, 100):0}%";
    }

    public void CommitSeek(double milliseconds)
    {
        if (!playbackAvailable)
        {
            isSeeking = false;
            StatusMessage = GetPlaybackUnavailableMessage();
            return;
        }

        try
        {
            playerSession.SeekToMilliseconds((int)Math.Clamp(milliseconds, 0, int.MaxValue));
        }
        finally
        {
            isSeeking = false;
        }

        ApplyProgress(playerSession.GetProgress());
    }

    public Task SynchronizeFromBackendAsync()
    {
        return RefreshFromBackendAsync(forceTracks: false);
    }

    public async Task PauseAsync()
    {
        if (!initialized || disposed)
            return;

        StopTimers();
        Unsubscribe();

        try
        {
#if ANDROID
            if (playerSession.GetSnapshot().IsPlaying)
                playerSession.TogglePlayPause();
#endif
            audioDeviceWatcher.Stop();
            await playerSession.SavePlaybackStateAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"STATE SAVE WARNING: {ex.Message}";
        }
    }

    public void Resume()
    {
        if (!initialized || disposed)
            return;

        Subscribe();
        TryStartAudioWatcher();
        StartTimers();
        _ = RefreshFromBackendAsync(forceTracks: false);
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed)
            return;

        disposed = true;
        StopTimers();
        Unsubscribe();

        if (progressTimer is not null)
            progressTimer.Tick -= ProgressTimer_Tick;
        if (saveTimer is not null)
            saveTimer.Tick -= SaveTimer_Tick;

        try
        {
            await playerSession.SavePlaybackStateAsync();
        }
        catch
        {
            // Shutdown is best effort and must never block window destruction.
        }

        audioDeviceWatcher.Stop();
        audioDeviceWatcher.Dispose();
        playerSession.Dispose();
    }

    private void Subscribe()
    {
        if (subscribed)
            return;

        playerSession.SnapshotChanged += PlayerSession_SnapshotChanged;
        playerSession.ProgressChanged += PlayerSession_ProgressChanged;
        audioDeviceWatcher.DefaultDeviceChanged += AudioDeviceWatcher_DefaultDeviceChanged;
        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed)
            return;

        playerSession.SnapshotChanged -= PlayerSession_SnapshotChanged;
        playerSession.ProgressChanged -= PlayerSession_ProgressChanged;
        audioDeviceWatcher.DefaultDeviceChanged -= AudioDeviceWatcher_DefaultDeviceChanged;
        subscribed = false;
    }

    private void TryStartAudioWatcher()
    {
        try
        {
            audioDeviceWatcher.Start();
        }
        catch (Exception ex)
        {
            StatusMessage = $"DEVICE WATCHER WARNING: {ex.Message}";
        }
    }

    private void StartTimers()
    {
        IDispatcher? dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null)
            return;

        if (progressTimer is null)
        {
            progressTimer = dispatcher.CreateTimer();
            progressTimer.Interval = TimeSpan.FromMilliseconds(250);
            progressTimer.Tick += ProgressTimer_Tick;
        }

        if (saveTimer is null)
        {
            saveTimer = dispatcher.CreateTimer();
            saveTimer.Interval = TimeSpan.FromSeconds(30);
            saveTimer.Tick += SaveTimer_Tick;
        }

        if (!progressTimer.IsRunning)
            progressTimer.Start();
        if (!saveTimer.IsRunning)
            saveTimer.Start();
    }

    private void StopTimers()
    {
        progressTimer?.Stop();
        saveTimer?.Stop();
    }

    private async void ProgressTimer_Tick(object? sender, EventArgs e)
    {
        if (progressTickActive || disposed)
            return;

        progressTickActive = true;
        try
        {
            ApplyProgress(playerSession.GetProgress());
            SessionActionResult result = await playerSession.AdvanceIfStoppedAsync();
            if (result.PlaybackChanged || result.TrackListChanged || result.TemporaryPlaybackChanged)
                await RefreshFromBackendAsync(result.TrackListChanged);
        }
        catch (Exception ex)
        {
            StatusMessage = $"PLAYBACK WARNING: {ex.Message}";
        }
        finally
        {
            progressTickActive = false;
        }
    }

    private async void SaveTimer_Tick(object? sender, EventArgs e)
    {
        if (saveTickActive || disposed)
            return;

        saveTickActive = true;
        try
        {
            await playerSession.SavePlaybackStateAsync();
            StatusMessage = "PLAYBACK STATE SAVED";
        }
        catch (Exception ex)
        {
            StatusMessage = $"STATE SAVE WARNING: {ex.Message}";
        }
        finally
        {
            saveTickActive = false;
        }
    }

    private void PlayerSession_SnapshotChanged(object? sender, PlayerSessionSnapshot snapshot)
    {
        _ = MainThread.InvokeOnMainThreadAsync(() => ApplySnapshotAsync(snapshot, forceTracks: false));
    }

    private void PlayerSession_ProgressChanged(object? sender, PlaybackProgress progress)
    {
        MainThread.BeginInvokeOnMainThread(() => ApplyProgress(progress));
    }

    private void AudioDeviceWatcher_DefaultDeviceChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (playerSession.IsPlaying)
                playerSession.TogglePlayPause();

            StatusMessage = "OUTPUT DEVICE CHANGED — PLAYBACK PAUSED";
            _ = ApplySnapshotAsync(playerSession.GetSnapshot(), forceTracks: false);
        });
    }

    private async Task RefreshFromBackendAsync(bool forceTracks)
    {
        await ApplySnapshotAsync(playerSession.GetSnapshot(), forceTracks);
        ApplyProgress(playerSession.GetProgress());
    }

    private async Task ApplySnapshotAsync(PlayerSessionSnapshot snapshot, bool forceTracks)
    {
        string newTrackSetKey = string.Join('\u001f', snapshot.Tracks.Select(track => track.FilePath));
        bool tracksChanged = forceTracks || !string.Equals(trackSetKey, newTrackSetKey, StringComparison.Ordinal);

        PlaylistName = GetPlaylistName(snapshot.CurrentPlaylist?.MusicFolderPath);
        TrackCountText = $"{snapshot.Tracks.Count} {(snapshot.Tracks.Count == 1 ? "TRACK" : "TRACKS")}";
        CurrentPositionText = snapshot.CurrentIndex >= 0
            ? $"{snapshot.CurrentIndex + 1:00} / {snapshot.Tracks.Count:00}"
            : $"00 / {snapshot.Tracks.Count:00}";
        CurrentIndex = snapshot.CurrentIndex;
        IsTemporaryPlayback = snapshot.IsTemporaryPlayback;
        IsPlaying = snapshot.IsPlaying;
        IsLooped = snapshot.LoopMode == LoopMode.Track;
        CanGoPrevious = playbackAvailable && snapshot.CanGoPrevious;
        CanGoNext = playbackAvailable && snapshot.CanGoNext;
        CanPlayPause = playbackAvailable && (snapshot.CanPlay || snapshot.CanPause);
        CanSeek = playbackAvailable && snapshot.CanSeek;

        applyingVolume = true;
        VolumePercent = snapshot.Volume * 100;
        applyingVolume = false;

        if (tracksChanged)
        {
            trackSetKey = newTrackSetKey;
            await LoadTrackMetadataAsync(snapshot.Tracks);
            if (!IsSearchMode)
                PopulateTrackRows(snapshot.Tracks.Select(track => track.FilePath), snapshot.CurrentIndex);
        }
        else
        {
            UpdateCurrentRows(snapshot.CurrentIndex);
        }

        TrackInfo? displayTrack = snapshot.CurrentTrack;
        if (displayTrack is not null && metadataCache.TryGetValue(displayTrack.FilePath, out TrackInfo? metadata))
            displayTrack = metadata;

        CurrentTitle = displayTrack?.Title ?? "SELECT A TRACK";
        CurrentArtist = string.IsNullOrWhiteSpace(displayTrack?.Artist)
            ? displayTrack is null ? "WILDFLOWER SESSION IDLE" : "FILENAME SOURCE"
            : displayTrack.Artist;

        ApplySnapshotProgress(snapshot);
    }

    private async Task LoadTrackMetadataAsync(IEnumerable<TrackInfo> snapshotTracks)
    {
        string[] paths = snapshotTracks.Select(track => track.FilePath).ToArray();
        IReadOnlyList<TrackInfo> metadata = await metadataService.GetTrackInfoAsync(paths);

        metadataCache.Clear();
        foreach (TrackInfo track in metadata)
            metadataCache[track.FilePath] = track;
    }

    private void PopulateTrackRows(IEnumerable<string> paths, int currentIndex)
    {
        VisibleTracks.Clear();
        IReadOnlyList<string> sessionTracks = playerSession.Tracks;

        foreach (string path in paths)
        {
            int index = FindTrackIndex(sessionTracks, path);
            if (index < 0)
                continue;

            TrackInfo info = metadataCache.TryGetValue(path, out TrackInfo? cached)
                ? cached
                : new TrackInfo(path, Path.GetFileNameWithoutExtension(path), string.Empty);

            VisibleTracks.Add(new TrackRowViewModel(index, path, info.Title, info.Artist)
            {
                IsCurrent = index == currentIndex
            });
        }

        TrackRowsReady?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateCurrentRows(int currentIndex)
    {
        foreach (TrackRowViewModel row in VisibleTracks)
            row.IsCurrent = row.Index == currentIndex;
    }

    private async Task SelectTrackAsync(TrackRowViewModel? track)
    {
        if (track is null || IsBusy)
            return;

        if (!playbackAvailable)
        {
            StatusMessage = GetPlaybackUnavailableMessage();
            TrackInvoked?.Invoke(this, EventArgs.Empty);
            return;
        }

        try
        {
            if (IsSearchMode)
            {
                SessionActionResult result = await playerSession.PlayTemporaryTrackAsync(track.FilePath);
                StatusMessage = result.Succeeded ? "TEMPORARY PLAYBACK ACTIVE" : result.Message ?? "TRACK COULD NOT PLAY";
            }
            else if (!playerSession.PlayTrack(track.Index))
            {
                StatusMessage = "TRACK COULD NOT PLAY";
            }

            await RefreshFromBackendAsync(forceTracks: false);
            TrackInvoked?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            StatusMessage = $"PLAYBACK ERROR: {ex.Message}";
        }
    }

    private void TogglePlayPause()
    {
        if (!playbackAvailable)
        {
            StatusMessage = GetPlaybackUnavailableMessage();
            return;
        }

        if (!playerSession.TogglePlayPause())
            StatusMessage = "PLAY/PAUSE COMMAND WAS NOT AVAILABLE";
        _ = RefreshFromBackendAsync(forceTracks: false);
    }

    private void PreviousTrack()
    {
        if (!playbackAvailable)
        {
            StatusMessage = GetPlaybackUnavailableMessage();
            return;
        }

        if (!playerSession.PreviousTrack())
            StatusMessage = "ALREADY AT THE FIRST TRACK";
        _ = RefreshFromBackendAsync(forceTracks: false);
    }

    private void NextTrack()
    {
        if (!playbackAvailable)
        {
            StatusMessage = GetPlaybackUnavailableMessage();
            return;
        }

        if (!playerSession.NextTrack())
            StatusMessage = "ALREADY AT THE LAST TRACK";
        _ = RefreshFromBackendAsync(forceTracks: false);
    }

    private void ToggleLoop()
    {
        playerSession.SetLooped(!playerSession.IsLooped);
        StatusMessage = playerSession.IsLooped ? "TRACK LOOP ENABLED" : "TRACK LOOP DISABLED";
        _ = RefreshFromBackendAsync(forceTracks: false);
    }

    private void OpenSearch()
    {
        IsPlaylistMode = false;
        IsSearchMode = true;
        SearchQuery = string.Empty;
        VisibleTracks.Clear();
        StatusMessage = "ENTER A TITLE, ARTIST, OR FILENAME";
    }

    private void CloseSearch()
    {
        IsSearchMode = false;
        SearchQuery = string.Empty;
        PopulateTrackRows(playerSession.Tracks, playerSession.GetSnapshot().CurrentIndex);
        StatusMessage = "ACTIVE PLAYLIST";
    }

    private async Task ExecuteSearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            VisibleTracks.Clear();
            StatusMessage = "ENTER A SEARCH TERM";
            return;
        }

        IsBusy = true;
        try
        {
            IReadOnlyList<string> results = await searchService.FindMatchingTracksAsync(playerSession.Tracks, SearchQuery);
            PopulateTrackRows(results, playerSession.GetSnapshot().CurrentIndex);
            StatusMessage = $"{results.Count} SEARCH {(results.Count == 1 ? "MATCH" : "MATCHES")}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"SEARCH ERROR: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OpenPlaylistsAsync()
    {
        IsSearchMode = false;
        IsPlaylistMode = true;
        IsBusy = true;
        Playlists.Clear();

        try
        {
            IReadOnlyList<PlaylistInfo> playlists = await playerSession.GetPlaylistsAsync();
            foreach (PlaylistInfo playlist in playlists)
                Playlists.Add(new PlaylistRowViewModel(playlist, GetPlaylistName(playlist.MusicFolderPath)));

            StatusMessage = $"{playlists.Count} {(playlists.Count == 1 ? "PLAYLIST" : "PLAYLISTS")} AVAILABLE";
        }
        catch (Exception ex)
        {
            StatusMessage = $"PLAYLIST ERROR: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClosePlaylists()
    {
        IsPlaylistMode = false;
        PopulateTrackRows(playerSession.Tracks, playerSession.GetSnapshot().CurrentIndex);
        StatusMessage = "ACTIVE PLAYLIST";
    }

    private async Task SelectPlaylistAsync(PlaylistRowViewModel? playlist)
    {
        if (playlist is null)
            return;

        IsBusy = true;
        try
        {
            SessionActionResult result = await playerSession.SelectPlaylistAsync(playlist.Playlist.Id);
            StatusMessage = result.Succeeded ? "PLAYLIST LOADED" : result.Message ?? "PLAYLIST COULD NOT LOAD";
            IsPlaylistMode = false;
            await RefreshFromBackendAsync(forceTracks: true);
        }
        catch (Exception ex)
        {
            StatusMessage = $"PLAYLIST ERROR: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AddFolderAsync()
    {
        IsBusy = true;
        try
        {
            string? folder = await folderPickerService.PickFolderAsync();
            if (string.IsNullOrWhiteSpace(folder))
            {
                StatusMessage = "FOLDER SELECTION CANCELLED";
                return;
            }

            SessionActionResult result = await playerSession.AddPlaylistAsync(folder);
            StatusMessage = result.Succeeded
                ? playerSession.Tracks.Count == 0
                    ? "NO SUPPORTED AUDIO FILES IN THE SELECTED FOLDER"
                    : "MUSIC FOLDER ADDED"
                : result.Message ?? "FOLDER COULD NOT BE ADDED";
            if (result.Succeeded)
            {
                IsPlaylistMode = false;
                IsSearchMode = false;
                await RefreshFromBackendAsync(forceTracks: true);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"FOLDER ERROR: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ReturnFromTemporaryAsync()
    {
        try
        {
            SessionActionResult result = await playerSession.ReturnFromTemporaryPlaybackAsync();
            StatusMessage = result.Succeeded ? "PERSISTED PLAYLIST STATE RESTORED" : result.Message ?? "RETURN FAILED";
            await RefreshFromBackendAsync(forceTracks: false);
        }
        catch (Exception ex)
        {
            StatusMessage = $"RETURN ERROR: {ex.Message}";
        }
    }

    private async Task RefreshPlaylistAsync()
    {
        IsBusy = true;
        try
        {
            SessionActionResult result = await playerSession.RefreshPlaylistAndRestoreAsync();
            StatusMessage = result.Succeeded ? "PLAYLIST REFRESHED" : result.Message ?? "REFRESH FAILED";
            await RefreshFromBackendAsync(forceTracks: true);
        }
        catch (Exception ex)
        {
            StatusMessage = $"REFRESH ERROR: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplySnapshotProgress(PlayerSessionSnapshot snapshot)
    {
        if (isSeeking)
            return;

        double positionMilliseconds = snapshot.PositionSeconds * 1000;
        double lengthMilliseconds = snapshot.LengthSeconds * 1000;
        ProgressMaximum = lengthMilliseconds;
        ProgressValue = Math.Clamp(positionMilliseconds, 0, ProgressMaximum);
        ElapsedText = FormatTime(positionMilliseconds);
        DurationText = FormatTime(lengthMilliseconds);
        ProgressPercentText = $"{snapshot.ProgressPercent:0}%";
    }

    private void ApplyProgress(PlaybackProgress progress)
    {
        if (isSeeking)
            return;

        ProgressMaximum = progress.LengthMilliseconds;
        ProgressValue = Math.Clamp(progress.PositionMilliseconds, 0, ProgressMaximum);
        ElapsedText = FormatTime(progress.PositionMilliseconds);
        DurationText = FormatTime(progress.LengthMilliseconds);
        double percent = progress.LengthMilliseconds <= 0
            ? 0
            : (double)progress.PositionMilliseconds / progress.LengthMilliseconds * 100;
        ProgressPercentText = $"{Math.Clamp(percent, 0, 100):0}%";
    }

    private static int FindTrackIndex(IReadOnlyList<string> tracks, string filePath)
    {
        for (int i = 0; i < tracks.Count; i++)
        {
            if (string.Equals(tracks[i], filePath, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    private static string GetPlaylistName(string? musicFolderPath)
    {
        if (string.IsNullOrWhiteSpace(musicFolderPath))
            return "NO PLAYLIST";

        if (Uri.TryCreate(musicFolderPath, UriKind.Absolute, out Uri? sourceUri) &&
            string.Equals(sourceUri.Scheme, "content", StringComparison.OrdinalIgnoreCase))
        {
            string documentId = Uri.UnescapeDataString(sourceUri.Segments.LastOrDefault()?.Trim('/') ?? string.Empty);
            int separator = documentId.LastIndexOf(':');
            if (separator >= 0 && separator < documentId.Length - 1)
                documentId = documentId[(separator + 1)..];
            if (!string.IsNullOrWhiteSpace(documentId))
                return documentId.ToUpperInvariant();
        }

        string trimmed = Path.TrimEndingDirectorySeparator(musicFolderPath);
        string name = Path.GetFileName(trimmed);
        return string.IsNullOrWhiteSpace(name) ? trimmed : name.ToUpperInvariant();
    }

    private static string FormatTime(double milliseconds)
    {
        if (double.IsNaN(milliseconds) || double.IsInfinity(milliseconds) || milliseconds <= 0)
            return "00:00";

        TimeSpan time = TimeSpan.FromMilliseconds(milliseconds);
        return time.TotalHours >= 1 ? time.ToString(@"h\:mm\:ss") : time.ToString(@"mm\:ss");
    }

    private static string GetPlaybackUnavailableMessage()
    {
#if ANDROID
        return "ANDROID PLAYBACK BACKEND NOT IMPLEMENTED YET";
#else
        return "AUDIO ENGINE COULD NOT INITIALIZE";
#endif
    }
}

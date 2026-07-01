using wildflower.Models;
using wildflower.Services.Library;
using wildflower.Services.Playback;
using wildflower.Services.Playlist;

namespace wildflower.Services.Session
{
    public sealed class PlayerSessionService : IPlayerSessionService
    {
        private readonly IPlaybackEngine playbackEngine;
        private readonly IPlaylistService playlistService;
        private readonly IMusicLibraryScanner musicLibraryScanner;
        private readonly IMetadataService metadataService;
        private readonly Random random = new();
        private readonly List<string> tracks = new();
        private IReadOnlyList<TrackInfo> trackSnapshot = Array.AsReadOnly(Array.Empty<TrackInfo>());
        private bool isTransitioning;
        private int temporaryTrackIndex = -1;

        public PlayerSessionService(
            IPlaybackEngine playbackEngine,
            IPlaylistService playlistService,
            IMusicLibraryScanner musicLibraryScanner,
            IMetadataService metadataService)
        {
            this.playbackEngine = playbackEngine;
            this.playlistService = playlistService;
            this.musicLibraryScanner = musicLibraryScanner;
            this.metadataService = metadataService;
        }

        public IReadOnlyList<string> Tracks => tracks;
        public PlaylistInfo? CurrentPlaylist { get; private set; }
        public int CurrentIndex { get; private set; }
        public long SavedPositionBytes { get; private set; }
        public bool IsPlaying { get; private set; }
        public bool IsTemporaryPlayback { get; private set; }
        public bool HasTracks => tracks.Count > 0;
        public float Volume => playbackEngine.Volume;
        public LoopMode LoopMode { get; private set; } = LoopMode.Off;
        public bool IsLooped => LoopMode == LoopMode.Track;

        public event EventHandler<PlayerSessionSnapshot>? SnapshotChanged;
        public event EventHandler<PlaybackProgress>? ProgressChanged;

        public bool InitializePlaybackEngine()
        {
            playbackEngine.SetVolume(0.3f);
            return playbackEngine.Initialize();
        }

        public async Task<PlayerSessionInitializationResult> InitializeAsync()
        {
            CurrentPlaylist = await playlistService.GetLastOrFirstPlaylistAsync();
            if (CurrentPlaylist == null)
            {
                return new PlayerSessionInitializationResult(
                    PlayerSessionInitializationStatus.NeedsMusicFolder,
                    SessionActionResult.NoChange,
                    "No playlist selected.");
            }

            await playlistService.SaveLastUsedPlaylistAsync(CurrentPlaylist);
            SessionActionResult loadResult = NotifyFromResult(await LoadCurrentPlaylistAsync());
            PlayerSessionInitializationStatus status = loadResult switch
            {
                { MissingMusicFolder: true } => PlayerSessionInitializationStatus.NeedsMusicFolder,
                { Succeeded: true } => PlayerSessionInitializationStatus.Loaded,
                _ => PlayerSessionInitializationStatus.Failed
            };

            return new PlayerSessionInitializationResult(
                status,
                loadResult,
                loadResult.Message);
        }

        public Task<IReadOnlyList<PlaylistInfo>> GetPlaylistsAsync()
        {
            return playlistService.GetPlaylistsAsync();
        }

        public async Task<SessionActionResult> AddPlaylistAsync(string musicFolderPath)
        {
            AddPlaylistResult addResult = await playlistService.AddPlaylistAsync(musicFolderPath);
            if (!addResult.Succeeded || addResult.Playlist == null)
            {
                return new SessionActionResult(
                    false,
                    Message: addResult.Message,
                    TrackListChanged: false);
            }

            CurrentPlaylist = addResult.Playlist;
            return NotifyFromResult(await LoadCurrentPlaylistAsync());
        }

        public async Task<SessionActionResult> SelectPlaylistAsync(string playlistId)
        {
            if (CurrentPlaylist?.Id == playlistId)
                return SessionActionResult.NoChange;

            PlaylistInfo? playlist = await playlistService.GetPlaylistAsync(playlistId);
            if (playlist == null)
            {
                PlaylistInfo? fallbackPlaylist = await playlistService.FindAvailablePlaylistAsync();
                if (fallbackPlaylist == null)
                {
                    ClearPlaylist();
                    return NotifyFromResult(new SessionActionResult(
                        false,
                        TrackListChanged: true,
                        ClearedPlaylist: true,
                        Message: "All playlists have been deleted."));
                }

                CurrentPlaylist = fallbackPlaylist;
                return NotifyFromResult(await LoadCurrentPlaylistAsync());
            }

            CurrentPlaylist = playlist;
            await playlistService.SaveLastUsedPlaylistAsync(playlist);
            return NotifyFromResult(await LoadCurrentPlaylistAsync());
        }

        public async Task<DeletePlaylistResult> DeletePlaylistAsync(string playlistId)
        {
            bool deletedCurrent = CurrentPlaylist?.Id == playlistId;
            await playlistService.DeletePlaylistAsync(playlistId);

            if (!deletedCurrent)
            {
                bool hasAny = (await playlistService.GetPlaylistsAsync()).Count > 0;
                return new DeletePlaylistResult(true, false, hasAny, SessionActionResult.NoChange);
            }

            PlaylistInfo? fallbackPlaylist = await playlistService.FindAvailablePlaylistAsync();
            if (fallbackPlaylist == null)
            {
                ClearPlaylist();
                return NotifyFromResult(new DeletePlaylistResult(
                    true,
                    true,
                    false,
                    new SessionActionResult(
                        TrackListChanged: true,
                        PlaybackChanged: true,
                        ClearedPlaylist: true,
                        Message: "All playlists have been deleted.")));
            }

            CurrentPlaylist = fallbackPlaylist;
            SessionActionResult loadResult = await LoadCurrentPlaylistAsync();
            return NotifyFromResult(new DeletePlaylistResult(true, true, true, loadResult));
        }

        public async Task<SessionActionResult> RefreshPlaylistAsync()
        {
            if (CurrentPlaylist == null)
                return new SessionActionResult(false, Message: "No playlist selected.");

            if (string.IsNullOrWhiteSpace(CurrentPlaylist.MusicFolderPath) ||
                !Directory.Exists(CurrentPlaylist.MusicFolderPath))
            {
                await playlistService.RemoveInvalidPlaylistsAsync();
                ClearPlaylist();
                return NotifyFromResult(new SessionActionResult(
                    false,
                    TrackListChanged: true,
                    PlaybackChanged: true,
                    MissingMusicFolder: true,
                    ClearedPlaylist: true,
                    Message: "Update your music folder path"));
            }

            bool changed = await CleanMissingTracksAsync();
            changed |= await UpdatePlaylistWithNewSongsAsync();

            if (tracks.Count == 0)
            {
                CurrentIndex = 0;
                SavedPositionBytes = 0;
                playbackEngine.Stop();
                playbackEngine.Free();
                IsPlaying = false;
                RefreshTrackSnapshot();
            }
            else
            {
                CurrentIndex = ClampTrackIndex(CurrentIndex);
            }

            if (changed)
                RefreshTrackSnapshot();

            return NotifyFromResult(new SessionActionResult(TrackListChanged: changed, PlaybackChanged: changed));
        }

        public async Task<SessionActionResult> RefreshPlaylistAndRestoreAsync()
        {
            await SavePlaybackStateAsync();
            SessionActionResult refreshResult = await RefreshPlaylistAsync();
            if (!refreshResult.Succeeded || refreshResult.MissingMusicFolder || !HasTracks)
                return refreshResult;

            PlayCurrentTrack(SavedPositionBytes);
            return refreshResult with { PlaybackChanged = true };
        }

        public async Task<SessionActionResult> ShuffleTracksAsync()
        {
            if (tracks.Count == 0)
                return SessionActionResult.NoChange;

            SessionActionResult refreshResult = await RefreshPlaylistAsync();
            if (!refreshResult.Succeeded || refreshResult.MissingMusicFolder || tracks.Count == 0)
                return refreshResult;

            for (int i = tracks.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (tracks[i], tracks[j]) = (tracks[j], tracks[i]);
            }

            CurrentIndex = 0;
            SavedPositionBytes = 0;
            RefreshTrackSnapshot();
            await SavePlaylistAsync();
            bool played = PlayTrack(0);
            await SavePlaybackStateAsync();

            var result = new SessionActionResult(TrackListChanged: true, PlaybackChanged: true);
            return played ? result : NotifyFromResult(result);
        }

        public async Task<SessionActionResult> AdvanceIfStoppedAsync()
        {
            if (isTransitioning ||
                tracks.Count == 0 ||
                !playbackEngine.HasStream ||
                playbackEngine.Status != PlayerStatus.Stopped)
            {
                return SessionActionResult.NoChange;
            }

            isTransitioning = true;
            try
            {
                if (IsTemporaryPlayback)
                {
                    if (IsLooped)
                    {
                        PlayTemporaryIndex();
                        return NotifyFromResult(new SessionActionResult(PlaybackChanged: true));
                    }

                    return await ReturnFromTemporaryPlaybackAsync();
                }

                int nextIndex = CurrentIndex;
                if (!IsLooped)
                    nextIndex++;

                if (nextIndex < tracks.Count)
                {
                    PlayTrack(nextIndex);
                    return new SessionActionResult(PlaybackChanged: true);
                }

                return await ShuffleTracksAsync();
            }
            finally
            {
                isTransitioning = false;
            }
        }

        public async Task<SessionActionResult> PlayTemporaryTrackAsync(string filePath)
        {
            if (tracks.Count == 0 || string.IsNullOrWhiteSpace(filePath))
                return new SessionActionResult(false, Message: "No song selected.");

            int index = tracks.FindIndex(path => string.Equals(path, filePath, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
                return new SessionActionResult(false, Message: "Song is no longer available.");

            bool wasTemporaryPlayback = IsTemporaryPlayback;
            int previousTemporaryTrackIndex = temporaryTrackIndex;

            temporaryTrackIndex = index;
            IsTemporaryPlayback = true;
            if (!PlayTemporaryIndex())
            {
                temporaryTrackIndex = previousTemporaryTrackIndex;
                IsTemporaryPlayback = wasTemporaryPlayback;
                NotifySessionChanged();
                return new SessionActionResult(false, Message: "Could not play selected song.");
            }

            string displayName = await GetTrackDisplayNameAsync(tracks[index]);
            return NotifyFromResult(new SessionActionResult(
                PlaybackChanged: true,
                TemporaryPlaybackChanged: true,
                TemporaryTrackDisplayName: displayName));
        }

        public async Task<SessionActionResult> ReturnFromTemporaryPlaybackAsync()
        {
            if (!IsTemporaryPlayback)
                return SessionActionResult.NoChange;

            IsTemporaryPlayback = false;
            temporaryTrackIndex = -1;
            await RestorePersistedPlaybackStateAsync();
            bool restoredPlayback = PlayCurrentTrack(SavedPositionBytes);

            var result = new SessionActionResult(
                PlaybackChanged: true,
                TemporaryPlaybackChanged: true);
            return restoredPlayback ? result : NotifyFromResult(result);
        }

        public bool PlayTrack(int index, long startPositionBytes = 0)
        {
            bool wasPlaying = IsPlaying;
            bool played = PlayTrackCore(index, startPositionBytes, updateCurrentIndex: !IsTemporaryPlayback);
            if (played || wasPlaying != IsPlaying)
                NotifySessionChanged();

            return played;
        }

        public bool PlayCurrentTrack(long startPositionBytes = 0)
        {
            if (tracks.Count == 0)
                return false;

            CurrentIndex = ClampTrackIndex(CurrentIndex);
            bool wasPlaying = IsPlaying;
            bool played = PlayTrackCore(CurrentIndex, startPositionBytes, updateCurrentIndex: true);
            if (played || wasPlaying != IsPlaying)
                NotifySessionChanged();

            return played;
        }

        public bool TogglePlayPause()
        {
            if (tracks.Count == 0)
                return false;

            if (IsPlaying)
            {
                playbackEngine.Pause();
                IsPlaying = false;
                NotifySessionChanged();
                return true;
            }

            if (!playbackEngine.HasStream)
            {
                CurrentIndex = ClampTrackIndex(CurrentIndex);
                bool wasPlaying = IsPlaying;
                if (!PlayTrackCore(CurrentIndex, SavedPositionBytes, updateCurrentIndex: true))
                {
                    if (wasPlaying != IsPlaying)
                        NotifySessionChanged();

                    return false;
                }

                NotifySessionChanged();
                return true;
            }
            else if (!playbackEngine.Play(false))
            {
                return false;
            }

            IsPlaying = true;
            NotifySessionChanged();
            return true;
        }

        public bool NextTrack()
        {
            if (tracks.Count == 0 || CurrentIndex >= tracks.Count - 1)
                return false;

            return PlayTrack(CurrentIndex + 1);
        }

        public bool PreviousTrack()
        {
            if (tracks.Count == 0 || CurrentIndex <= 0)
                return false;

            return PlayTrack(CurrentIndex - 1);
        }

        public void SetLooped(bool looped)
        {
            LoopMode newLoopMode = looped ? LoopMode.Track : LoopMode.Off;
            if (LoopMode == newLoopMode)
                return;

            LoopMode = newLoopMode;
            NotifySessionChanged(includeProgress: false);
        }

        public void SetVolume(float volume)
        {
            float previousVolume = playbackEngine.Volume;
            playbackEngine.SetVolume(volume);
            if (!previousVolume.Equals(playbackEngine.Volume))
                NotifySessionChanged(includeProgress: false);
        }

        public void SeekToMilliseconds(int milliseconds)
        {
            bool positionChanged = playbackEngine.SetPositionSeconds(milliseconds / 1000.0);
            SavedPositionBytes = playbackEngine.GetPositionBytes();
            if (positionChanged || playbackEngine.HasStream)
                NotifySessionChanged();
        }

        public PlayerSessionSnapshot GetSnapshot()
        {
            PlayerStatus status = playbackEngine.Status;
            bool hasStream = playbackEngine.HasStream;
            long positionBytes = hasStream ? playbackEngine.GetPositionBytes() : SavedPositionBytes;
            long lengthBytes = hasStream ? playbackEngine.GetLengthBytes() : 0;
            double positionSeconds = hasStream ? playbackEngine.GetPositionSeconds() : 0;
            double lengthSeconds = hasStream ? playbackEngine.GetLengthSeconds() : 0;
            int activeIndex = IsTemporaryPlayback && temporaryTrackIndex >= 0
                ? temporaryTrackIndex
                : CurrentIndex;
            int snapshotCurrentIndex = tracks.Count == 0 ? -1 : ClampTrackIndex(activeIndex);
            TrackInfo? currentTrack = snapshotCurrentIndex >= 0 && snapshotCurrentIndex < trackSnapshot.Count
                ? trackSnapshot[snapshotCurrentIndex]
                : null;

            return new PlayerSessionSnapshot(
                CurrentPlaylist,
                trackSnapshot,
                snapshotCurrentIndex,
                currentTrack,
                status,
                LoopMode,
                status == PlayerStatus.Playing,
                IsTemporaryPlayback,
                positionBytes,
                lengthBytes,
                positionSeconds,
                lengthSeconds,
                CalculateProgressPercent(positionSeconds, lengthSeconds),
                playbackEngine.Volume,
                tracks.Count > 0 && status != PlayerStatus.Playing,
                tracks.Count > 0 && status == PlayerStatus.Playing,
                !IsTemporaryPlayback && snapshotCurrentIndex >= 0 && snapshotCurrentIndex < tracks.Count - 1,
                !IsTemporaryPlayback && snapshotCurrentIndex > 0,
                hasStream && lengthBytes > 0);
        }

        public PlaybackProgress GetProgress()
        {
            if (!playbackEngine.HasStream)
                return PlaybackProgress.Empty;

            int positionMilliseconds = SecondsToMilliseconds(playbackEngine.GetPositionSeconds());
            int lengthMilliseconds = SecondsToMilliseconds(playbackEngine.GetLengthSeconds());

            return new PlaybackProgress(
                Math.Min(positionMilliseconds, lengthMilliseconds),
                lengthMilliseconds,
                playbackEngine.GetPositionBytes(),
                playbackEngine.GetLengthBytes());
        }

        public async Task SavePlaybackStateAsync()
        {
            if (CurrentPlaylist == null || tracks.Count == 0 || IsTemporaryPlayback)
                return;

            CurrentIndex = ClampTrackIndex(CurrentIndex);
            SavedPositionBytes = playbackEngine.GetPositionBytes();
            await playlistService.SavePlaybackStateAsync(
                CurrentPlaylist,
                new PlaybackState(CurrentIndex, SavedPositionBytes))
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<string>> GetTrackDisplayNamesAsync()
        {
            var trackInfo = await metadataService.GetTrackInfoAsync(tracks);
            return trackInfo.Select(track => track.DisplayName).ToArray();
        }

        public Task<string> GetTrackDisplayNameAsync(string filePath)
        {
            return Task.Run(() => metadataService.GetTrackInfo(filePath).DisplayName);
        }

        public void Dispose()
        {
            playbackEngine.Dispose();
        }

        private async Task<SessionActionResult> LoadCurrentPlaylistAsync()
        {
            if (CurrentPlaylist == null)
                return new SessionActionResult(false, Message: "No playlist selected.");

            if (string.IsNullOrWhiteSpace(CurrentPlaylist.MusicFolderPath) ||
                !Directory.Exists(CurrentPlaylist.MusicFolderPath))
            {
                await playlistService.RemoveInvalidPlaylistsAsync();
                ClearPlaylist();
                return new SessionActionResult(
                    false,
                    TrackListChanged: true,
                    MissingMusicFolder: true,
                    ClearedPlaylist: true,
                    Message: "Update your music folder path");
            }

            bool playlistFileExists = await playlistService.PlaylistFileExistsAsync(CurrentPlaylist);
            if (!playlistFileExists)
            {
                tracks.Clear();
                tracks.AddRange(await musicLibraryScanner.ScanAsync(CurrentPlaylist.MusicFolderPath));
                RefreshTrackSnapshot();
                SavedPositionBytes = 0;
                CurrentIndex = 0;

                if (tracks.Count == 0)
                    return new SessionActionResult(TrackListChanged: true);

                return await ShuffleTracksAsync();
            }

            tracks.Clear();
            tracks.AddRange(await playlistService.LoadTrackPathsAsync(CurrentPlaylist));
            RefreshTrackSnapshot();

            PlaybackState? savedState = await playlistService.LoadPlaybackStateAsync(CurrentPlaylist);
            CurrentIndex = savedState?.CurrentIndex ?? 0;
            SavedPositionBytes = Math.Max(0, savedState?.PositionBytes ?? 0);

            SessionActionResult refreshResult = await RefreshPlaylistAsync();
            if (!refreshResult.Succeeded || refreshResult.MissingMusicFolder || tracks.Count == 0)
                return refreshResult;

            CurrentIndex = ClampTrackIndex(CurrentIndex);
            PlayTrackCore(CurrentIndex, SavedPositionBytes, updateCurrentIndex: true);
            await SavePlaybackStateAsync();

            return refreshResult with { TrackListChanged = true, PlaybackChanged = true };
        }

        private bool PlayTrackCore(int index, long startPositionBytes, bool updateCurrentIndex)
        {
            if (index < 0 || index >= tracks.Count)
                return false;

            bool loaded = playbackEngine.Load(tracks[index], startPositionBytes);
            if (!loaded)
            {
                IsPlaying = false;
                return false;
            }

            bool played = playbackEngine.Play(false);
            IsPlaying = played;
            if (played && updateCurrentIndex)
                CurrentIndex = index;
            return played;
        }

        private bool PlayTemporaryIndex()
        {
            if (temporaryTrackIndex >= 0 && temporaryTrackIndex < tracks.Count)
                return PlayTrackCore(temporaryTrackIndex, 0, updateCurrentIndex: false);

            return false;
        }

        private async Task RestorePersistedPlaybackStateAsync()
        {
            PlaybackState? persistedState = CurrentPlaylist == null
                ? null
                : await playlistService.LoadPlaybackStateAsync(CurrentPlaylist);

            CurrentIndex = persistedState?.CurrentIndex ?? 0;
            SavedPositionBytes = Math.Max(0, persistedState?.PositionBytes ?? 0);
            CurrentIndex = ClampTrackIndex(CurrentIndex);
        }

        private async Task<bool> CleanMissingTracksAsync()
        {
            int removedBeforeCurrent = 0;
            var validTracks = new List<string>();

            await Task.Run(() =>
            {
                for (int i = 0; i < tracks.Count; i++)
                {
                    string file = tracks[i];
                    if (File.Exists(file))
                    {
                        validTracks.Add(file);
                    }
                    else if (i < CurrentIndex)
                    {
                        removedBeforeCurrent++;
                    }
                }
            });

            bool changed = validTracks.Count != tracks.Count;
            if (!changed)
                return false;

            tracks.Clear();
            tracks.AddRange(validTracks);
            CurrentIndex = Math.Max(0, CurrentIndex - removedBeforeCurrent);
            RefreshTrackSnapshot();
            await SavePlaylistAsync();
            return true;
        }

        private async Task<bool> UpdatePlaylistWithNewSongsAsync()
        {
            if (CurrentPlaylist == null)
                return false;

            var currentPaths = new HashSet<string>(tracks, StringComparer.OrdinalIgnoreCase);
            var allSongs = await musicLibraryScanner.ScanAsync(CurrentPlaylist.MusicFolderPath);
            var newSongs = allSongs.Where(song => !currentPaths.Contains(song)).ToArray();
            if (newSongs.Length == 0)
                return false;

            tracks.AddRange(newSongs);
            RefreshTrackSnapshot();
            await SavePlaylistAsync();
            return true;
        }

        private Task SavePlaylistAsync()
        {
            return CurrentPlaylist == null
                ? Task.CompletedTask
                : playlistService.SaveTrackPathsAsync(CurrentPlaylist, tracks);
        }

        private void ClearPlaylist()
        {
            playbackEngine.Stop();
            playbackEngine.Free();
            tracks.Clear();
            RefreshTrackSnapshot();
            CurrentPlaylist = null;
            CurrentIndex = 0;
            SavedPositionBytes = 0;
            IsPlaying = false;
            IsTemporaryPlayback = false;
            temporaryTrackIndex = -1;
        }

        private int ClampTrackIndex(int index)
        {
            if (tracks.Count == 0)
                return 0;

            return Math.Clamp(index, 0, tracks.Count - 1);
        }

        private static int SecondsToMilliseconds(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds <= 0)
                return 0;

            double milliseconds = seconds * 1000;
            return milliseconds >= int.MaxValue ? int.MaxValue : (int)milliseconds;
        }

        private void RefreshTrackSnapshot()
        {
            TrackInfo[] trackInfos = tracks
                .Select(path => new TrackInfo(path, GetFallbackTrackTitle(path), string.Empty))
                .ToArray();

            trackSnapshot = Array.AsReadOnly(trackInfos);
        }

        private static string GetFallbackTrackTitle(string filePath)
        {
            string? title = Path.GetFileNameWithoutExtension(filePath);
            return string.IsNullOrWhiteSpace(title) ? filePath : title;
        }

        private static double CalculateProgressPercent(double positionSeconds, double lengthSeconds)
        {
            if (double.IsNaN(positionSeconds) ||
                double.IsInfinity(positionSeconds) ||
                double.IsNaN(lengthSeconds) ||
                double.IsInfinity(lengthSeconds) ||
                lengthSeconds <= 0)
            {
                return 0;
            }

            return Math.Clamp(positionSeconds / lengthSeconds * 100, 0, 100);
        }

        private SessionActionResult NotifyFromResult(SessionActionResult result)
        {
            if (ShouldNotify(result))
                NotifySessionChanged();

            return result;
        }

        private DeletePlaylistResult NotifyFromResult(DeletePlaylistResult result)
        {
            if (ShouldNotify(result.SessionResult))
                NotifySessionChanged();

            return result;
        }

        private static bool ShouldNotify(SessionActionResult result)
        {
            return result.TrackListChanged ||
                   result.PlaybackChanged ||
                   result.TemporaryPlaybackChanged ||
                   result.ClearedPlaylist;
        }

        private void NotifySessionChanged(bool includeProgress = true)
        {
            SnapshotChanged?.Invoke(this, GetSnapshot());
            if (includeProgress)
                ProgressChanged?.Invoke(this, GetProgress());
        }
    }
}

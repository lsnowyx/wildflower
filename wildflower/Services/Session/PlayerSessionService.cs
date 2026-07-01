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

        public bool InitializePlaybackEngine()
        {
            playbackEngine.SetVolume(0.3f);
            return playbackEngine.Initialize();
        }

        public async Task<SessionActionResult> InitializeAsync(Func<Task<string?>> requestMusicFolderAsync)
        {
            CurrentPlaylist = await playlistService.GetLastOrFirstPlaylistAsync();
            if (CurrentPlaylist == null)
            {
                string? selectedFolder = await requestMusicFolderAsync();
                if (selectedFolder == null)
                    return new SessionActionResult(false, Message: "No playlist selected.");

                return await AddPlaylistAsync(selectedFolder);
            }

            await playlistService.SaveLastUsedPlaylistAsync(CurrentPlaylist);
            return await LoadCurrentPlaylistAsync();
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
            return await LoadCurrentPlaylistAsync();
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
                    return new SessionActionResult(
                        false,
                        TrackListChanged: true,
                        ClearedPlaylist: true,
                        Message: "All playlists have been deleted.");
                }

                CurrentPlaylist = fallbackPlaylist;
                return await LoadCurrentPlaylistAsync();
            }

            CurrentPlaylist = playlist;
            await playlistService.SaveLastUsedPlaylistAsync(playlist);
            return await LoadCurrentPlaylistAsync();
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
                return new DeletePlaylistResult(
                    true,
                    true,
                    false,
                    new SessionActionResult(
                        TrackListChanged: true,
                        PlaybackChanged: true,
                        ClearedPlaylist: true,
                        Message: "All playlists have been deleted."));
            }

            CurrentPlaylist = fallbackPlaylist;
            SessionActionResult loadResult = await LoadCurrentPlaylistAsync();
            return new DeletePlaylistResult(true, true, true, loadResult);
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
                return new SessionActionResult(
                    false,
                    TrackListChanged: true,
                    PlaybackChanged: true,
                    MissingMusicFolder: true,
                    ClearedPlaylist: true,
                    Message: "Update your music folder path");
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
            }
            else
            {
                CurrentIndex = ClampTrackIndex(CurrentIndex);
            }

            return new SessionActionResult(TrackListChanged: changed, PlaybackChanged: changed);
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
            await SavePlaylistAsync();
            PlayTrack(0);
            await SavePlaybackStateAsync();

            return new SessionActionResult(TrackListChanged: true, PlaybackChanged: true);
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
                        return new SessionActionResult(PlaybackChanged: true);
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
                return new SessionActionResult(false, Message: "Could not play selected song.");
            }

            string displayName = await GetTrackDisplayNameAsync(tracks[index]);
            return new SessionActionResult(
                PlaybackChanged: true,
                TemporaryPlaybackChanged: true,
                TemporaryTrackDisplayName: displayName);
        }

        public async Task<SessionActionResult> ReturnFromTemporaryPlaybackAsync()
        {
            if (!IsTemporaryPlayback)
                return SessionActionResult.NoChange;

            IsTemporaryPlayback = false;
            temporaryTrackIndex = -1;
            await RestorePersistedPlaybackStateAsync();
            PlayCurrentTrack(SavedPositionBytes);

            return new SessionActionResult(
                PlaybackChanged: true,
                TemporaryPlaybackChanged: true);
        }

        public bool PlayTrack(int index, long startPositionBytes = 0)
        {
            return PlayTrackCore(index, startPositionBytes, updateCurrentIndex: !IsTemporaryPlayback);
        }

        public bool PlayCurrentTrack(long startPositionBytes = 0)
        {
            if (tracks.Count == 0)
                return false;

            CurrentIndex = ClampTrackIndex(CurrentIndex);
            return PlayTrackCore(CurrentIndex, startPositionBytes, updateCurrentIndex: true);
        }

        public bool TogglePlayPause()
        {
            if (tracks.Count == 0)
                return false;

            if (IsPlaying)
            {
                playbackEngine.Pause();
                IsPlaying = false;
                return true;
            }

            if (!playbackEngine.HasStream)
            {
                if (!PlayCurrentTrack(SavedPositionBytes))
                    return false;
            }
            else if (!playbackEngine.Play(false))
            {
                return false;
            }

            IsPlaying = true;
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
            LoopMode = looped ? LoopMode.Track : LoopMode.Off;
        }

        public void SetVolume(float volume)
        {
            playbackEngine.SetVolume(volume);
        }

        public void SeekToMilliseconds(int milliseconds)
        {
            playbackEngine.SetPositionSeconds(milliseconds / 1000.0);
            SavedPositionBytes = playbackEngine.GetPositionBytes();
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
                SavedPositionBytes = 0;
                CurrentIndex = 0;

                if (tracks.Count == 0)
                    return new SessionActionResult(TrackListChanged: true);

                return await ShuffleTracksAsync();
            }

            tracks.Clear();
            tracks.AddRange(await playlistService.LoadTrackPathsAsync(CurrentPlaylist));

            PlaybackState? savedState = await playlistService.LoadPlaybackStateAsync(CurrentPlaylist);
            CurrentIndex = savedState?.CurrentIndex ?? 0;
            SavedPositionBytes = Math.Max(0, savedState?.PositionBytes ?? 0);

            SessionActionResult refreshResult = await RefreshPlaylistAsync();
            if (!refreshResult.Succeeded || refreshResult.MissingMusicFolder || tracks.Count == 0)
                return refreshResult;

            CurrentIndex = ClampTrackIndex(CurrentIndex);
            PlayTrack(CurrentIndex, SavedPositionBytes);
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
    }
}

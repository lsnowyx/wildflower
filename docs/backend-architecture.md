# wildflower Backend Architecture

## 1. Overview

`wildflower` currently uses WinForms as its frontend. The WinForms forms are responsible for the visual controls, icons, panels, timers, folder picker, and user interaction.

The reusable music-player behavior now lives in backend models and services under `wildflower/Models` and `wildflower/Services`. These services are intended to be reusable by a future MAUI frontend while preserving the current app behavior.

MAUI migration has not started yet. This document describes the current backend/frontend boundary so a later MAUI UI can replace the WinForms UI without redesigning playlist storage, playback state, or playback workflow.

## 2. Backend Building Blocks

### Models

Models are small data records and enums used by the services and UI:

- `TrackInfo`: file path, title, artist, and display name.
- `PlaylistInfo`: playlist id, numbered playlist directory path, selected music folder path, and derived paths for `musicFolderPath.txt`, `playlist.txt`, and `state.txt`.
- `PlaybackState`: persisted playback state as `CurrentIndex` and `PositionBytes`.
- `PlaybackProgress`: current position/length in milliseconds and BASS byte positions.
- `PlayerSessionSnapshot`: UI-friendly read model for the current session, including playlist, tracks, current track, player status, loop mode, byte/second progress, volume, and capability flags.
- `PlayerSessionInitializationResult` and `PlayerSessionInitializationStatus`: startup result types that tell a frontend whether the session loaded, needs a music folder, or failed.
- `PlayerStatus`: stopped, playing, or paused.
- `LoopMode`: currently `Off` or `Track`.

### `IPlayerSessionService` / `PlayerSessionService`

`IPlayerSessionService` is the main application-facing backend interface. A frontend should usually call this service instead of calling lower-level services directly.

`PlayerSessionService` owns the active playlist workflow, track order, current track index, loop state, temporary playback state, playback progress, playlist refresh, state saving/restoring, and auto-advance behavior. It coordinates the playback engine, playlist service, music scanner, and metadata service.

`InitializeAsync()` is result-based. It does not call a UI folder picker or any UI callback. It returns `PlayerSessionInitializationResult` so the frontend can decide whether to render the loaded session, open its own folder picker, or show an error.

`GetSnapshot()` returns a `PlayerSessionSnapshot` that future frontends can poll or bind to without knowing about BASS, playlist files, or WinForms controls. It is synchronous and intended to be safe for frequent UI reads. Command failures still come back through `SessionActionResult`; the snapshot represents current state, not the last command error.

`SnapshotChanged` is raised after meaningful session state changes, such as playlist load/switch/delete, track refresh or shuffle, track changes, play/pause changes, loop or volume changes, seek changes, temporary playback entry/exit, and playback failures that affect session state.

`ProgressChanged` is raised when a command changes playback progress or stream state. The backend still does not own a progress timer; WinForms and a future MAUI UI can keep their own timer and call `GetSnapshot()` or `GetProgress()` for continuous progress updates.

These events are UI-neutral. They do not marshal to any UI thread, so each frontend must marshal event handling to its own UI dispatcher when needed.

### `IPlaybackEngine` / `BassPlaybackEngine`

`IPlaybackEngine` hides playback implementation details from the rest of the app.

`BassPlaybackEngine` is the current BASS-backed implementation. It initializes BASS, owns the current BASS stream handle, creates/frees streams, plays, pauses, stops, seeks, reads byte positions, converts seconds/bytes, sets volume, and calls `BASS_Free` during disposal.

The UI should not know about BASS stream handles or BASS APIs.

### `IPlaylistStorage` / `FilePlaylistStorage`

`IPlaylistStorage` defines physical playlist persistence operations.

`FilePlaylistStorage` implements the current `%APPDATA%\.wildflower\playlists` layout. It reads and writes `lastUsed.txt`, creates numbered playlist folders, stores selected music folder paths, reads/writes `playlist.txt`, and reads/writes `state.txt`.

This class is filesystem-specific and should remain behind the storage interface.

### `IPlaylistService` / `PlaylistService`

`IPlaylistService` defines higher-level playlist operations.

`PlaylistService` chooses the last or first valid playlist, checks for duplicate music folders, creates the next numbered playlist id, deletes playlists, finds fallback playlists, removes invalid playlists, and delegates persistence to `IPlaylistStorage`.

### `IMusicLibraryScanner` / `MusicLibraryScanner`

`IMusicLibraryScanner` scans a selected music folder for supported files.

`MusicLibraryScanner` currently scans only the top-level folder and supports `.mp3`, `.wav`, `.flac`, and `.ogg` with case-insensitive extension matching.

### `IMetadataService` / `TagLibMetadataService`

`IMetadataService` reads metadata and returns `TrackInfo`.

`TagLibMetadataService` uses TagLibSharp to read title and artist. If metadata cannot be read, it falls back to the file name without extension.

### `ISearchService` / `SearchService`

`ISearchService` searches tracks from a collection of file paths.

`SearchService` matches against file name, title, and artist. It depends on `IMetadataService` so the search logic does not depend directly on TagLibSharp.

## 3. UI/Backend Boundary

The UI is allowed to call:

- `IPlayerSessionService` for normal app workflow.
- `PlayerSessionSnapshot` from `IPlayerSessionService.GetSnapshot()` as the preferred UI read/bind model.
- `IPlayerSessionService.SnapshotChanged` and `IPlayerSessionService.ProgressChanged` for backend-owned notifications.
- `ISearchService` for search result lists.
- `IMetadataService` when the UI needs display metadata.
- `IPlaylistService` for playlist management screens when session-level methods are not enough.
- Model types such as `PlaylistInfo`, `TrackInfo`, `PlayerSessionSnapshot`, `PlayerSessionInitializationResult`, `PlaybackProgress`, and `PlaybackState`.

The UI must not touch directly:

- BASS APIs or BASS stream handles.
- `BassPlaybackEngine` internals.
- Playlist files such as `lastUsed.txt`, `musicFolderPath.txt`, `playlist.txt`, or `state.txt`.
- TagLibSharp APIs.
- Physical storage paths except for display/debug information.

BASS, file storage, TagLib, and playlist files must stay hidden behind services because they are implementation details. This keeps the future MAUI UI focused on rendering and user interaction, while the backend continues to own playback, persistence, scanning, and metadata behavior.

The backend does not call WinForms or MAUI dispatchers. Event subscribers must handle UI-thread marshaling in the frontend.

## 4. Saved Data Contract

The current saved-data layout is:

```text
%APPDATA%\.wildflower\playlists\
    lastUsed.txt
    1\
        musicFolderPath.txt
        playlist.txt
        state.txt
    2\
        musicFolderPath.txt
        playlist.txt
        state.txt
```

`lastUsed.txt` stays directly inside the playlist root and stores the active numbered playlist id.

`musicFolderPath.txt` stays inside a numbered playlist folder and stores the real selected music folder path, for example:

```text
D:\snowyx_\Documents\!using\playlist to listen to after a car crash
```

`playlist.txt` stays inside a numbered playlist folder and stores discovered track file names, not absolute paths.

`state.txt` stays inside a numbered playlist folder and its format is exactly:

```text
index|BASS byte position
```

The saved position is a BASS byte position. It must not be treated as milliseconds.

Temporary playback restore uses the persisted `state.txt` for the active playlist. It must not use a runtime UI snapshot or an in-memory copy of the previous UI-selected track/position as the restore source.

## 5. Future MAUI Integration Guide

### Startup

A MAUI frontend should manually compose the current services or use a lightweight composition root:

1. Create `TagLibMetadataService`.
2. Create `MusicLibraryScanner`.
3. Create `FilePlaylistStorage`.
4. Create `PlaylistService`.
5. Create `BassPlaybackEngine`.
6. Create `SearchService`.
7. Create `PlayerSessionService`.
8. Subscribe to `SnapshotChanged` and `ProgressChanged` if the UI wants backend notifications.
9. Call `InitializePlaybackEngine()`.
10. Call `InitializeAsync()`.

Event handlers should copy snapshot/progress values into MAUI bindable state on the MAUI UI thread. The backend does not marshal events to a dispatcher.

`InitializeAsync()` returns:

- `Loaded`: the backend loaded the last/first available playlist and restored track/index/byte position.
- `NeedsMusicFolder`: no usable playlist is available, or the saved playlist points at a missing music folder. The frontend should open its own folder picker.
- `Failed`: startup failed for another reason. The frontend can display the result message.

### Loading Last Playlist

`InitializeAsync()` loads the last used playlist through `PlaylistService`, loads track paths through storage, reads `state.txt`, refreshes missing/new songs, and restores the saved track and BASS byte position.

### Choosing A Folder

The UI owns the folder picker. The backend does not request folders through callbacks. If initialization returns `NeedsMusicFolder`, the frontend should show its own folder picker and return a selected music folder path string to the backend through playlist creation.

### Creating A Playlist

Pass the selected music folder path to:

```csharp
await playerSession.AddPlaylistAsync(musicFolderPath);
```

The backend should create or select the numbered playlist folder and save the playlist files according to the saved-data contract.

After the command completes, the backend raises `SnapshotChanged` if the active playlist/session state changed.

### Listing Tracks

Use `playerSession.Tracks` for exact file paths and `await playerSession.GetTrackDisplayNamesAsync()` for display names.

For UI binding or repeated reads, prefer:

```csharp
PlayerSessionSnapshot snapshot = playerSession.GetSnapshot();
var tracks = snapshot.Tracks;
var currentTrack = snapshot.CurrentTrack;
```

The UI owns the visible list and selected visual row.

### Playing Tracks

Call:

```csharp
playerSession.PlayTrack(index);
```

Then update UI state from `CurrentIndex`, `IsPlaying`, and `GetProgress()`.

Future UIs should prefer updating from `PlayerSessionSnapshot` directly or by handling `SnapshotChanged`.

### Pause/Resume

Call:

```csharp
playerSession.TogglePlayPause();
```

### Next/Previous

Call:

```csharp
playerSession.NextTrack();
playerSession.PreviousTrack();
```

### Seeking

The UI slider can work in milliseconds. Call:

```csharp
playerSession.SeekToMilliseconds(milliseconds);
```

The backend converts to BASS byte position.

Seeking raises `SnapshotChanged` and `ProgressChanged` when the current stream position changes.

### Volume

Call:

```csharp
playerSession.SetVolume(volume);
```

`volume` is a float from `0.0f` to `1.0f`.

### Loop Mode

Call:

```csharp
playerSession.SetLooped(looped);
```

### Search

Call:

```csharp
await searchService.FindMatchingTracksAsync(playerSession.Tracks, query);
```

The returned values are exact track file paths.

### Temporary Playback

To start temporary playback, pass an exact track path to:

```csharp
await playerSession.PlayTemporaryTrackAsync(filePath);
```

Temporary playback must not save its temporary track/index into normal playlist state.

### Returning From Temporary Playback

Call:

```csharp
await playerSession.ReturnFromTemporaryPlaybackAsync();
```

The backend reloads the persisted normal playlist state from `state.txt` and restores that track and BASS byte position.

### Saving State Every 30 Seconds

The frontend currently owns the timer. A future MAUI frontend should run its own timer and call:

```csharp
await playerSession.SavePlaybackStateAsync();
```

The backend skips normal state saving while temporary playback is active.

### App Close/Disposal

On app close or suspend, save state and dispose the session:

```csharp
await playerSession.SavePlaybackStateAsync();
playerSession.Dispose();
```

Disposing the session disposes the playback engine, which frees BASS resources.

Frontends should unsubscribe from session events during disposal if the session may outlive the UI object.

## 6. State Ownership

Backend-owned state:

- Current playlist.
- Track list and ordering.
- Current track index.
- Current playback stream.
- Current playback byte position.
- Loop state.
- Temporary playback state.
- Persisted state interpretation.

UI-owned state:

- Selected visual list item.
- Buttons and icons.
- Labels.
- Sliders.
- Panels and navigation.
- Folder picker UI.
- Visual temporary-playback layout.

The UI should reflect backend state. It should not become the source of truth for playlist position, playback stream, playlist files, or temporary playback restore behavior.

For future UI work, `PlayerSessionSnapshot` is the recommended backend-owned read model. A UI can copy values from it into controls or bindable view models, but it should still issue commands through `IPlayerSessionService`.

Backends own the event notifications. Frontends own the UI-thread marshaling and visual updates that happen in response to those events.

## 7. Remaining Migration Blockers

- Timers are still UI-owned. WinForms currently owns progress polling, auto-advance polling, and 30-second state saving.
- Backend session events now exist, but WinForms still mostly calls service methods and manually refreshes controls instead of subscribing to them.
- The audio device watcher is not behind an interface yet.
- BASS native DLL deployment must be handled carefully in any future MAUI package.
- Storage uses Windows AppData and Windows path assumptions.
- `Helper` is WinForms-specific and includes icon/image and animation helpers.

## 8. Recommended Future Refactors

1. Start using `PlayerSessionSnapshot` as the main UI read model in WinForms and future UI prototypes.
2. Subscribe WinForms and future UI prototypes to `SnapshotChanged` and `ProgressChanged`.
3. Add `IAudioDeviceWatcher`.
4. Harden playlist path validation.
5. Add unit tests with fake playback/storage.
6. Keep documenting saved-data behavior.

# Wildflower Architecture

## Current solution

Wildflower is split into three projects:

- `wildflowerBackend` contains the reusable models and services used by the current app.
- `wildflowerFrontend` is the active .NET MAUI application. It targets Windows and Android, with iOS and Mac Catalyst project targets still present but not functionally completed.
- `wildflower` is the legacy WinForms application. It remains in the solution as reference code and is not the frontend used by the MAUI application.

The backend does not reference WinForms or MAUI. `wildflowerFrontend/MauiProgram.cs` is the composition root and selects platform implementations through dependency injection.

## Session boundary

`IPlayerSessionService` is the application-facing playback/session interface. `PlayerSessionService` owns the active playlist and track order, current and temporary playback, commands, persistence interpretation, library reconciliation, and automatic track advancement.

`InitializeAsync()` is result-based. It returns `PlayerSessionInitializationResult` with `Loaded`, `NeedsMusicFolder`, or `Failed`; the backend never opens a folder picker.

`PlayerSessionSnapshot` is the UI read model. It includes the active playlist, immutable track snapshot, current track/index, playback status, loop and temporary-playback state, progress, volume, and command capability flags.

The session raises UI-neutral events:

- `SnapshotChanged` after meaningful playlist or playback changes.
- `ProgressChanged` when a command changes stream progress/state.

The backend does not marshal these events to a UI thread. The MAUI view model dispatches them through `MainThread`.

## Startup and caching

For an existing playlist, startup takes the fast path:

1. Load the last-used playlist descriptor.
2. Load its persisted `playlist.txt` and `state.txt`.
3. Restore the saved track and playback position immediately.
4. Render filename fallbacks or values from the persistent metadata cache.
5. Reconcile the selected folder and warm missing metadata in the background.

Startup no longer blocks on per-track existence checks, a complete metadata pass, or a folder scan. Reconciliation performs one folder scan, removes missing identifiers, appends new identifiers, and writes `playlist.txt` only when the list changed.

The MAUI frontend stores display metadata in `<platform app-data>/.wildflower/metadata-cache.json`. Cached metadata is used immediately. Uncached tracks are read through the platform `IMetadataService`, persisted, and applied to existing rows in the background. Track-row replacement uses a single collection reset instead of one UI notification per track.

The first launch for a new folder must still scan that folder and read its metadata. Later launches use the persisted playlist, state, and metadata cache.

## Playback engines

`IPlaybackEngine` hides platform playback details.

- Windows uses `BassPlaybackEngine` and the bundled x64 `bass.dll`. Persisted positions are BASS byte positions.
- Android uses `AndroidPlaybackEngine`, backed by `Android.Media.MediaPlayer`, and plays persisted Storage Access Framework `content://` document URIs directly. Persisted positions are milliseconds on Android.
- Other targets currently receive `UnavailablePlaybackEngine`.

The session treats the persisted position as an opaque platform value through the playback-engine abstraction. A Windows `state.txt` position must not be interpreted as an Android position, or vice versa.

## Android background playback

Android playback is hosted by `AndroidPlaybackService`, a foreground service declared with the `mediaPlayback` type. It owns the background runtime while the MAUI window is stopped:

- playback continues when the user presses Home, locks the phone, or switches apps;
- a media notification and native `MediaSession` expose play/pause, previous, next, seek, and stop controls, using the same cached title/artist metadata as the in-app UI;
- the service polls for completed tracks so automatic advancement does not depend on the UI timer;
- it saves normal playlist state every 30 seconds while running;
- it uses `StartCommandResult.NotSticky`, so Android does not recreate it after it has been intentionally stopped.

The MAUI window detaches its UI timers and event subscriptions when backgrounded but does not pause or dispose the Android session.

The service is declared with `android:stopWithTask="false"` so Android calls `OnTaskRemoved`. Wildflower then saves `state.txt`, stops/frees the player, removes the notification, stops the service, and terminates its process. Therefore swiping the app from Recents stops playback and removes the app runtime. A system force-stop also terminates playback.

## Storage and playlist contract

Persistence is behind `IPlaylistStorage`.

Windows uses `%APPDATA%/.wildflower/playlists/`. Each numbered playlist directory contains `musicFolderPath.txt`, `playlist.txt`, and `state.txt`; `lastUsed.txt` sits at the playlist root. `FilePlaylistStorage` stores the Windows folder path, track file names, and `index|BASS byte position`.

Android uses the same logical file names under the app-private `FileSystem.AppDataDirectory/.wildflower/playlists` directory. `musicFolderPath.txt` stores the persisted SAF tree URI, `playlist.txt` stores SAF document URIs, and `state.txt` stores `index|position in milliseconds`.

Android does create text files; they are in private app storage and are not visible beside the music files in a normal file browser.

The current Windows and Android state directories are not directly interchangeable. Besides their different private locations, they identify folders/tracks differently and use different position units. Future cloud synchronization requires a platform-neutral track identity and time-based shared state contract rather than copying the current files unchanged.

## Temporary playback

Temporary playback never overwrites normal playlist state. `ReturnFromTemporaryPlaybackAsync()` reloads the active playlist's persisted `state.txt` and restores that track/position. It does not restore from a transient UI snapshot.

## Library, metadata, and device implementations

- Windows: `MusicLibraryScanner`, `FileSystemMusicLibrarySourceAccess`, `TagLibMetadataService`, and the NAudio-backed `AudioDeviceWatcher` behind `IAudioDeviceWatcher`.
- Android: `AndroidMusicLibraryScanner`, `AndroidMusicLibrarySourceAccess`, `AndroidMetadataService`, and `UnavailableAudioDeviceWatcher`. Android audio focus is handled inside `AndroidPlaybackEngine`.

Both scanners currently scan only the selected folder's top level and support `.mp3`, `.wav`, `.flac`, and `.ogg`.

## Current frontend ownership

`MainPlayerViewModel` owns bindable display state, search/playlist overlays, folder-picker commands, UI progress and save timers while the window is visible, metadata-cache hydration, and UI-thread event dispatching.

`MainPlayerPage` contains the Windows-specific MAUI layout/behavior. `AndroidMainPlayerPage` contains the Android-specific MAUI layout/behavior. Platform code also includes the Windows folder picker and Android SAF picker, scanner, storage, metadata reader, playback engine, service controller, and foreground service.

The legacy `FormsFolder/Form1.cs` still contains WinForms-specific control updates, timers, dialogs, and interaction logic. It is not used by the MAUI frontend and should not be moved back into the backend.

## Remaining migration and hardening work

- iOS and Mac Catalyst do not yet have functional playback, folder access, persistence, or background-media implementations.
- Android has no `IAudioDeviceWatcher`; it currently relies on audio-focus callbacks.
- The Android service uses platform `MediaPlayer`/`MediaSession` APIs rather than Media3, so richer queue/artwork integrations remain future work.
- Metadata cache entries are keyed by track identifier. New tracks are detected, but tag edits to an unchanged identifier are not automatically invalidated yet.
- Search can still read metadata for a large result set.
- Library scans are top-level only.
- Automated backend tests and Android lifecycle/device tests are still needed.
- A shared Windows/Android cloud state format needs neutral track identities and millisecond positions before Google Drive synchronization is safe.

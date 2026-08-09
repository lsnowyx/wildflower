using Microsoft.Extensions.Logging;
using wildflower.Services.Audio;
using wildflower.Services.Library;
using wildflower.Services.Playback;
using wildflower.Services.Playlist;
using wildflower.Services.Search;
using wildflower.Services.Session;
using wildflowerFrontend.Services;
using wildflowerFrontend.ViewModels;
using wildflowerFrontend.Views;

#if WINDOWS
using wildflower;
using wildflowerFrontend.Platforms.Windows;
#elif ANDROID
using wildflowerFrontend.Platforms.Android;
#endif

namespace wildflowerFrontend;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if ANDROID
        builder.Services.AddSingleton<IMetadataService, AndroidMetadataService>();
        builder.Services.AddSingleton<IMusicLibraryScanner, AndroidMusicLibraryScanner>();
        builder.Services.AddSingleton<IMusicLibrarySourceAccess, AndroidMusicLibrarySourceAccess>();
        builder.Services.AddSingleton<IPlaylistStorage, AndroidPlaylistStorage>();
#else
        builder.Services.AddSingleton<IMetadataService, TagLibMetadataService>();
        builder.Services.AddSingleton<IMusicLibraryScanner, MusicLibraryScanner>();
        builder.Services.AddSingleton<IMusicLibrarySourceAccess, FileSystemMusicLibrarySourceAccess>();
        builder.Services.AddSingleton<IPlaylistStorage, FilePlaylistStorage>();
#endif
        builder.Services.AddSingleton<IPlaylistService, PlaylistService>();
        builder.Services.AddSingleton<ISearchService, SearchService>();
        builder.Services.AddSingleton<IPlayerSessionService, PlayerSessionService>();
        builder.Services.AddSingleton<ITrackMetadataCache, JsonTrackMetadataCache>();

#if WINDOWS
        builder.Services.AddSingleton<IPlaybackEngine, BassPlaybackEngine>();
        builder.Services.AddSingleton<IAudioDeviceWatcher, AudioDeviceWatcher>();
        builder.Services.AddSingleton<IFolderPickerService, WindowsFolderPickerService>();
        builder.Services.AddSingleton<IPlaybackServiceController, UnavailablePlaybackServiceController>();
#elif ANDROID
        builder.Services.AddSingleton<IPlaybackEngine, AndroidPlaybackEngine>();
        builder.Services.AddSingleton<IAudioDeviceWatcher, UnavailableAudioDeviceWatcher>();
        builder.Services.AddSingleton<IFolderPickerService, AndroidFolderPickerService>();
        builder.Services.AddSingleton<IPlaybackServiceController, AndroidPlaybackServiceController>();
#else
        builder.Services.AddSingleton<IPlaybackEngine, UnavailablePlaybackEngine>();
        builder.Services.AddSingleton<IAudioDeviceWatcher, UnavailableAudioDeviceWatcher>();
        builder.Services.AddSingleton<IFolderPickerService, UnavailableFolderPickerService>();
        builder.Services.AddSingleton<IPlaybackServiceController, UnavailablePlaybackServiceController>();
#endif

        builder.Services.AddSingleton<MainPlayerViewModel>();
#if ANDROID
        builder.Services.AddSingleton<AndroidMainPlayerPage>();
#else
        builder.Services.AddSingleton<MainPlayerPage>();
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

using Microsoft.Extensions.DependencyInjection;
using wildflowerFrontend.ViewModels;
using wildflowerFrontend.Views;

namespace wildflowerFrontend;

public partial class App : Application
{
    private readonly IServiceProvider services;
    private MainPlayerViewModel? mainPlayerViewModel;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        this.services = services;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        mainPlayerViewModel = services.GetRequiredService<MainPlayerViewModel>();
#if ANDROID
        Page rootPage = services.GetRequiredService<AndroidMainPlayerPage>();
#else
        Page rootPage = services.GetRequiredService<MainPlayerPage>();
#endif
        var window = new Window(rootPage)
        {
            Title = "Wildflower"
#if WINDOWS
            ,
            Width = 1200,
            Height = 720,
            MinimumWidth = 1000,
            MinimumHeight = 620
#endif
        };

        window.Stopped += Window_Stopped;
        window.Resumed += Window_Resumed;
        window.Destroying += Window_Destroying;
        return window;
    }

    private async void Window_Stopped(object? sender, EventArgs e)
    {
        if (mainPlayerViewModel is not null)
            await mainPlayerViewModel.PauseAsync();
    }

    private void Window_Resumed(object? sender, EventArgs e)
    {
        mainPlayerViewModel?.Resume();
    }

    private async void Window_Destroying(object? sender, EventArgs e)
    {
        if (sender is Window window)
        {
            window.Stopped -= Window_Stopped;
            window.Resumed -= Window_Resumed;
            window.Destroying -= Window_Destroying;
        }

        if (mainPlayerViewModel is not null)
        {
#if ANDROID
            await mainPlayerViewModel.PauseAsync();
#else
            await mainPlayerViewModel.DisposeAsync();
#endif
        }
    }
}

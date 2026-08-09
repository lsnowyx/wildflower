using Android.App;
using Android.Runtime;

namespace wildflowerFrontend
{
    [Application]
    public class MainApplication : MauiApplication
    {
        internal static IServiceProvider? ServiceProvider { get; private set; }

        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp()
        {
            MauiApp app = MauiProgram.CreateMauiApp();
            ServiceProvider = app.Services;
            return app;
        }
    }
}

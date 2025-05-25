namespace wildflower
{
    internal static class Program
    {
        private static Mutex mutex;
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.

            const string appName = "wildflower";
            bool createdNew;
            mutex = new Mutex(true, appName, out createdNew);
            if (!createdNew) return;

            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());

            mutex.ReleaseMutex();
        }
    }
}
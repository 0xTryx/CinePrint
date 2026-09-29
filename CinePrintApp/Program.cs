namespace cineprint
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Facteur d'echelle DPI utilise par les controles dessines (1 = 100 %, 2 = 200 %)
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                UI.Theme.Echelle = g.DpiX / 96f;

            Application.Run(new Form1());
        }
    }
}

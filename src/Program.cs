
using System.Net;

namespace ctwebplayer
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Configure Flurl to use proxy example
            /*
             IHttpRequest request = new HttpRequest(url)
            {
                Proxy = new Uri("http://example.org:3128")
            };
             */

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}
using System;
using twitchDotIRC;

namespace twitchDotIRC_App
{
    class Program
    {
        static TwitchClient client;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            client = new TwitchClient("Snappey", "oauth:oqcfayxlg4wy8akt22m4t0e29fgcrq");

            client.Start();

            

            Console.ReadKey();
        }
    }
}

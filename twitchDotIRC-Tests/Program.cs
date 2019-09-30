using System;
using twitchDotIRC;

namespace twitchDotIRC_App
{
    class Program
    {
        static TwitchClient client;

        static void Main(string[] args)
        {
            client = new TwitchClient("Snappey", args[0]);
            client.Start();

            client.JoinChannel("xqcow");

            //client.OnRawMessage += message => { Console.WriteLine($"{message.Nick}: {message.RawParameters}"); };
            client.OnChatMessage += message => { Console.WriteLine($"[{message.Time.ToLongTimeString()} ({String.Format("{0, 3:D3}", message.Time.Millisecond)}ms)] {message.User} in #{message.Channel}: {message.Message}"); };
            
            //JoinChannel("forsen");]
            //JoinChannel("valkia");
            //JoinChannel("Sodapoppin", "Testing123");
            //JoinChannel(new List<string>{"Lirik", "forsen", "Valkia"});
            //JoinChannel(new List<string>{"Cohh", "Soda", "Valkia"}, new List<string>{"123", "456", "789"});

            //LeaveChannel("CohhCarnage");
            //LeaveChannel(new List<string>{"CohhCarnage", "#Soda"});
            //LeaveChannel("#Soda");

            //SetTopic("Cohh", "Tersting 123123123");
            //SetTopic("#Soda");
            //SetTopic("#Cohh", ":Testint 123123");

            //SendPrivateMessage("Cohh", "Testing 123123123123123");
            //SendPrivateMessage("#Soda", ":12312313");
            //SendPrivateMessage("Testing123", "soda");

            Console.ReadLine();
        }
    }
}

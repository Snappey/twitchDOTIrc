using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using twitchDotIRC.Builders;

namespace twitchDotIRC
{
    public class TwitchClient
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private StreamWriter writer;

        private Thread readThread;

        #region ReadOnly Variables
        public readonly string IP;
        public readonly int Port;
        public readonly string Nick;
        public readonly string OAuth;
        #endregion

        public TwitchClient(string nick, string oauth, bool ssl=false)
        {
            if (ssl)
            {
                this.IP = TwitchConnectionOptions.SSLDomain;
                this.Port = TwitchConnectionOptions.SSLPort;
            }
            else
            {
                this.IP = TwitchConnectionOptions.Domain;
                this.Port = TwitchConnectionOptions.Port;
            }
            
            this.Nick = nick;
            this.OAuth = oauth;

            tcpClient = new TcpClient();
        }

        public void Start()
        {
            tcpClient.Connect(this.IP, this.Port);

            if (tcpClient.Connected)
            {
                Console.WriteLine("Connected");

                stream = tcpClient.GetStream();
                writer = new StreamWriter(stream);
            }
            else
            {
                Console.WriteLine("Failed to Connect");
            }

            readThread = new Thread(new ThreadStart(ReadLoop));
            readThread.IsBackground = true;
            readThread.Start();

            Regestration();

            //SendRawMessage("TESTING ERROR");
            //JoinChannel("CohhCarnage");
            //JoinChannel("Sodapoppin", "Testing123");
            //JoinChannel(new List<string>{"Cohh", "Soda", "Valkia"});
            //JoinChannel(new List<string>{"Cohh", "Soda", "Valkia"}, new List<string>{"123", "456", "789"});

            LeaveChannel("CohhCarnage");
            LeaveChannel(new List<string>{"CohhCarnage", "#Soda"});
            LeaveChannel("#Soda");

            SetTopic("Cohh", "Tersting 123123123");
            SetTopic("#Soda");
            SetTopic("#Cohh", ":Testint 123123");

            SendPrivateMessage("Cohh", "Testing 123123123123123");
            SendPrivateMessage("#Soda", ":12312313");
            SendPrivateMessage("Testing123", "soda");
        }

        private void ReadLoop()
        {
            while (true)
            {
                if (tcpClient.Available > 0)
                {
                    var bytes = new byte[tcpClient.ReceiveBufferSize];
                    stream.Read(bytes, 0, tcpClient.Available);

                    var messages = IRCMessage.Factory(bytes);

                    foreach(IRCMessage message in messages)
                    {
                        //Console.WriteLine("> " + message.RawTrimmed);
                        Console.WriteLine($"{message.User}: {message.RawParameters}");
                    }
                }
            }
        }

        #region Connection Messages

        private void SendPassword(string password)
        {
            SendRawMessage("PASS " + password);
        }

        private void SendNickname(string nick, int hopcount=0)
        {
            SendRawMessage("NICK " + nick.ToLower());
        }

        private void SendUser(string username, string hostname, string servername, string realname)
        {
            SendRawMessage($"USER {username} {hostname} {servername} :{realname}");
        }

        private void SendServer(string servername, int hopcount, string info)
        {
            SendRawMessage($"SERVER {servername} {hopcount.ToString()} :{info}");
        }

        private void SendOper(string username, string password)
        {
            SendRawMessage($"OPER {username} {password}");
        }

        private void SendQuit(string message = "")
        {
            SendRawMessage($"QUIT {message}");
        }

        private void SendServerQuit(string server, string comment)
        {
            SendRawMessage($"SQUIT {server} {comment}");
        }

        #endregion

        #region Channel Operations

        public void JoinChannel(string channel)
        {
            CommandBuilder builder = new CommandBuilder("JOIN", channel);

            SendRawMessage(builder.Build());
        }

        public void JoinChannel(List<string> channelList)
        {
            var channels = CommandBuilder.ListJoiner(channelList, ',');             
            CommandBuilder builder = new CommandBuilder("JOIN", channels);

            SendRawMessage(builder.Build());
        }

        public void JoinChannel(string channel, string key)
        {
            CommandBuilder builder = new CommandBuilder("JOIN", channel + " " + key);

            SendRawMessage(builder.Build());
        }

        public void JoinChannel(List<string> channelList, List<string> keyList)
        {
            var channels = CommandBuilder.ListJoiner(channelList, ',');
            var keys = CommandBuilder.ListJoiner(keyList, ',');
            CommandBuilder builder = new CommandBuilder("JOIN", channels + " " + keys);

            SendRawMessage(builder.Build());
        }

        public void LeaveChannel(string channel)
        {
            CommandBuilder builder = new CommandBuilder("PART", channel);

            SendRawMessage(builder.Build());
        }

        public void LeaveChannel(List<string> channelList)
        {
            var channels = CommandBuilder.ListJoiner(channelList, ',');
            CommandBuilder builder =new CommandBuilder("PART", channels);

            SendRawMessage(builder.Build());
        }

        // TODO: Implement MODE command

        public void SetTopic(string channel, string topic="")
        {
            CommandBuilder builder = new CommandBuilder("TOPIC", channel + " " + topic);

            SendRawMessage(builder.Build());
        }



        #endregion

        #region Message Operations

        private void SendPrivateMessage(string channel, string message)
        {
            CommandBuilder builder = new CommandBuilder("PRIVMSG", channel + " " + message);

            SendRawMessage(builder.Build());
        }

        #endregion

        // TODO: Unprivate, check so we dont reauthenticate if already connected
        private void Regestration()
        {
            SendRawMessage("PASS " + OAuth);
            SendRawMessage("NICK " + Nick.ToLower());
        }

        // TODO: Implement rate limiting for messages sent to server 
        private void SendRawMessage(string msg)
        {
            writer.WriteLine(msg);
            writer.Flush();

            Console.WriteLine("< " + msg);
        }
    }
}

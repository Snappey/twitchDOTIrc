using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

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

            Authenticate();

            //SendRawMessage("TESTING ERROR");
            JoinChannel("CohhCarnage");
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
                        Console.WriteLine($"{message.User}: {message.Params}");
                    }
                }
            }
        }

        public void JoinChannel(string channel)
        {
            SendRawMessage("JOIN #" + channel.ToLower());
        }

        public void LeaveChannel(string channel)
        {
            SendRawMessage("PART #" + channel.ToLower());
        }

        // TODO: Unprivate, check so we dont reauthenticate if already connected
        private void Authenticate()
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

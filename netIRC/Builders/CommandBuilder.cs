using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace twitchDotIRC.Builders
{
    internal class CommandBuilder
    {
        private string command { get; set; }
        private string[] parameters { get; set; }

        /// Builds messages to be sent through the IRC Network, collates parameters and formats them correctly for IRC, e.g. ':' for last mutliline parameter, ',' separation for certain parameters
        public CommandBuilder(string buildCommand, string paramCommands)
        {
            command = buildCommand;
            parameters = paramCommands.Split(' ');
        }

        public CommandBuilder(IRCReply buildCommand, string paramCommands)
        {
            command = buildCommand.ToString();
            parameters = paramCommands.Split(' ');
        }

        public CommandBuilder(string buildCommand, List<string> paramCommands)
        {
            command = buildCommand;
            parameters = paramCommands.ToArray(); 
        }

        public CommandBuilder(IRCReply buildCommand, List<string> paramCommands)
        {
            command = buildCommand.ToString();
            parameters = paramCommands.ToArray();
        }

        public string Build()
        {
            var output = string.Empty;

            if (command == "JOIN")
            {
                //  Possible Inputs:
                //          JOIN CohhCarnage
                //          JOIN #CohhCarnage (can start with a & / # )
                //          JOIN Cohh,Soda
                //          JOIN Cohh,Soda key1,key2
                //          JOIN #Cohh,Soda key1, key2
                {
                    if (parameters.Length < 1 || parameters.Length > 2)
                    {
                        throw new BuilderException("JOIN", "",
                            "Parameter count does not match '" + parameters.Length + "'");
                    }

                    string channels = parameters[0].ToLower();
                    string passwords = string.Empty;

                    if (parameters.Length == 2)
                    {
                        passwords = parameters[1];
                    }

                    if (channels == string.Empty)
                    {
                        throw new BuilderException("JOIN", "", "Parameter: 'channels' is empty");
                    }
                    //if (passwords == string.Empty) { throw new BuilderException("JOIN", "", "Parameter: 'passwords' is empty");}

                    string outputChannels = string.Empty;
                    outputChannels = BuildList(channels, ',', ',');

                    string outputParameters = string.Join(',', outputChannels) + " " + passwords;
                    output = $"{command} {outputParameters}";
                }
            }
            else if (command == "PART")
            {
                // Possible Inputs:
                //          PART CohhCarnage
                //          PART #CohhCarnage
                //          PART #Cohh,Soda
                //          PART Cohh,Soda
                if (parameters.Length != 1)
                {
                    throw new BuilderException("PART", "",
                        "Parameter count does not match '" + parameters.Length + "'");
                }

                string channels = parameters[0];

                if (channels == string.Empty)
                {
                    throw new BuilderException("JOIN", "", "Parameter: 'channels' is empty");
                }

                output = $"{command} {BuildList(channels, ',', ',')}";
            }
            else if (command == "TOPIC")
            {
                // Possible Inputs:
                //          TOPIC #Cohh :This is the topic
                //          TOPIC Cohh :This is the topic
                //          TOPIC #Soda                     <-- this returns the topic of the given channel 
                if (parameters.Length < 1)
                {
                    throw new BuilderException("TOPIC", "",
                        "Parameter count does not match '" + parameters.Length + "'");
                }

                string channels = parameters[0];
                string topic = string.Empty;

                if (channels == string.Empty)
                {
                    throw new BuilderException("TOPIC", "", "Parameter: 'channels' is empty");
                }

                if (parameters.Length > 1)
                {
                    topic = PrefixMessage(parameters.Skip(1));
                }

                if (topic == string.Empty)
                {
                    output = $"{command} {PrefixChannel(channels)}";
                }
                else
                {
                    output = $"{command} {PrefixChannel(channels)} {topic}";
                }
            }
            else if (command == "PASS")
            {
                if (parameters.Length != 1)
                {
                    throw new BuilderException("PASS", "", "Parameter count does not match '" + parameters.Length + "'");
                }

                string password = string.Empty;

                output = $"{command} {password}";
            }
            else if (command == "NICK")
            {
                if (parameters.Length != 1)
                {
                    throw new BuilderException("NICK", "", "Parameter count does not match '" + parameters.Length + "'");
                }

                string nick = string.Empty;

                output = $"{command} {nick}";
            }
            else if (command == "USER")
            {
                throw new NotImplementedException();
            }
            else if (command == "SERVER")
            {
                throw new NotImplementedException();
            }
            else if (command == "OPER")
            {
                throw new NotImplementedException();
            }
            else if (command == "QUIT")
            {
                if (parameters.Length > 1)
                {
                    throw new BuilderException("QUIT", "", "Parameter count does not match '" + parameters.Length + "'");
                }

                string quitMessage = string.Empty;

                if (parameters.Length == 1)
                {
                    quitMessage = parameters[0];
                }

                output = $"{command} {quitMessage}";
            }
            else if (command == "SQUIT")
            {
                throw new NotImplementedException();
            }
            else if (command == "NAMES")
            {
                if (parameters.Length > 1)
                {
                    throw new BuilderException("NAMES", "", "Parameter count does not match '" + parameters.Length + "'");
                }

                string channels = string.Empty;

                if (parameters.Length == 1)
                {
                    channels = BuildList(channels);
                }

                output = $"{command} {channels}";
            }
            else if (command == "LIST")
            {
                if (parameters.Length != 1)
                {
                    throw new BuilderException("LIST", "", "Parameter count does not match '" + parameters.Length + "'");
                }

                string channels = string.Empty;

                if (parameters.Length == 1)
                {
                    channels = BuildList(channels);
                }

                output = $"{command} {channels}";
            }
            else if (command == "INVITE")
            {
                if (parameters.Length != 2)
                {
                    throw new BuilderException("INVITE", "", "Parameter count does not match '" + parameters.Length + "'");
                }

                string nick = parameters[0];
                string channel = parameters[1];

                output = $"{command} {nick} {channel}";
            }
            else if (command == "KICK")
            {
                if (parameters.Length < 2)
                {
                    throw  new BuilderException("KICK", "", "Parameter count does not match '" + parameters.Length + "'");
                }

                string channel = parameters[0];
                string user = parameters[1];
                string comments = string.Empty;

                if (parameters.Length == 3)
                {
                    comments = PrefixMessage(parameters.Skip(2));

                    if (!comments.StartsWith(':')) // Check it has the final arg denotion 
                    {
                        comments = comments.Insert(0, ":");
                    }
                }

                output = $"{command} {channel} {user} {comments}";
            }
            else if (command == "PRIVMSG")
            {
                if (parameters.Length < 2)
                {
                    throw new BuilderException("PRIVMSG", "", "Parameter count does not match '" + parameters.Length + "'");
                }

                string receivers = parameters[0];
                string msg = parameters[1];

                string outputReceivers = BuildList(receivers, shouldPrefix: false);
                string outputMsg = PrefixMessage(parameters.Skip(1));

                if (!outputMsg.StartsWith(':')) // Check it has the final arg denotion 
                {
                    outputMsg = outputMsg.Insert(0, ":");
                }

                output = $"{command} {outputReceivers} {outputMsg}";
            }
            else if (command == "NOTICE")
            {
                if (parameters.Length != 2)
                {
                    throw new BuilderException("NOTICE", "", "Parameter count does not match '" + parameters.Length + "'");
                }

                string nickname = parameters[0];
                string msg = parameters[1];

                output = $"{command} {nickname} {msg}";
            }
            else if (command == "WHO")
            {
                if (parameters.Length > 2)
                {
                    throw new BuilderException("WHO", "", "Parameter count does not match '" + parameters.Length + "'");
                }

                string name = string.Empty;
                bool operatorsOnly = false;

                if (parameters.Length == 0)
                {
                    output = $"{command}";
                }
                else
                {
                    if (parameters.Length == 1)
                    {
                        name = parameters[0];
                    }
                    else
                    {
                        name = parameters[0];
                        operatorsOnly = true;
                    }

                    output = $"{command} {name} {(operatorsOnly ? "o" : string.Empty)}";
                }         
            }
            else if (command == "WHOIS")
            {
                if (parameters.Length > 2)
                {
                    throw new BuilderException("WHOIS", "", "Parameter count does not match '" + parameters.Length + "'");
                }

                string server = string.Empty;
                string nickmask = string.Empty;

                if (parameters.Length == 1)
                {
                    nickmask = BuildList(parameters[0], shouldPrefix: false);

                    output = $"{command} {nickmask}";
                }
                else
                {
                    server = parameters[0];
                    nickmask = BuildList(parameters[1], shouldPrefix: false);

                    output = $"{command} {server} {nickmask}";
                }
            }
            else if (command == "WHOWAS")
            {
                if (parameters.Length > 3)
                {
                    throw new BuilderException("WHOWAS",  "", "Parameter count does not match '" + parameters.Length + "'");
                }

                string nickname = string.Empty;
                int count = 0;
                string server = string.Empty;

                if (parameters.Length == 1)
                {
                    nickname = parameters[0];

                    output = $"{command} {nickname}";
                }
                else
                {
                    if (parameters.Length >= 2)
                    {
                        nickname = parameters[0];
                        bool tryParse = int.TryParse(parameters[1], out count);

                        if (!tryParse)
                        {
                            throw new BuilderException("WHOWAS", "", "Failed to parse parameter 'count' to an integer");
                        }

                        if (parameters.Length == 3)
                        {
                            server = parameters[3];
                        }

                        output = $"{command} {nickname} {count} {server}";
                    }
                }
            }
            else if (command == "PONG")
            {
                if (parameters.Length > 2)
                {
                    throw new BuilderException("PONG", "", "Parameter count does not match '" + parameters.Length + "'");
                }

                string daemon1 = string.Empty;
                string daemon2 = string.Empty;

                if (parameters.Length == 1)
                {
                    daemon1 = parameters[0];
                }
                else
                {
                    daemon1 = parameters[0];
                    daemon2 = parameters[1];
                }

                output = $"{command} {daemon1} {daemon2}";
            }
            else if (command == "PING")
            {
                if (parameters.Length > 2)
                {
                    throw new BuilderException("PING", "", "Parameter count does not match '" + parameters.Length + "'");
                }

                string server1 = string.Empty;
                string server2 = string.Empty;

                if (parameters.Length == 1)
                {
                    server1 = parameters[0];
                }
                else
                {
                    server1 = parameters[0];
                    server2 = parameters[1];
                }

                output = $"{command} {server1} {server2}";
            }
            else if (command == "AWAY")
            {
                if (parameters.Length > 1)
                {
                    throw new BuilderException("AWAY", "", "Parameter count does not match '" + parameters.Length + "'");
                }

                string message = string.Empty;

                if (parameters.Length == 1)
                {
                    message = parameters[0];
                }

                output = $"{command} {message}";
            }
            else if (command == "REHASH")
            {
                output = $"{command}";
            }
            else if (command == "RESTART")
            {
                output = $"{command}";
            }
            else if (command == "SUMMON")
            {
                if (parameters.Length > 2)
                {
                    throw new  BuilderException("SUMMON", "", "Parameter count does not match '" + parameters.Length + "'");
                }

                string user = string.Empty;
                string server = string.Empty;

                if (parameters.Length == 1)
                {
                    user = parameters[0];
                }
                else
                {
                    user = parameters[0];
                    server = parameters[1];
                }

                output = $"{command} {user} {server}";
            }
            else if (command == "USERS")
            {
                if (parameters.Length > 1)
                {
                    throw new BuilderException("USERS", "", "Parameter count does not match '" + parameters.Length + "'");
                }

                string servers = string.Empty;

                if (parameters.Length == 1)
                {
                    servers = parameters[0];
                }

                output = $"{command} {servers}";
            }
            else if (command == "OPERWALL")
            {
                throw new NotImplementedException();
            }
            else if (command == "USERHOST")
            {
                if (parameters.Length != 1)
                {
                    throw new BuilderException("USERHOST", "", "Parameter count does not match '" + parameters.Length + "'");
                }

                string nickname = parameters[0];
                string outputNicknames = BuildList(nickname, ',', ' ', false); // TODO: NEEDS TESTING

                output = $"{command} {outputNicknames}";
            }
            else if (command == "ISON")
            {
                if (parameters.Length != 1)
                {
                    throw new BuilderException("ISON", "", "Parameter count does not match '" + parameters.Length + "'");
                }

                string nickname = parameters[0];
                string outputNicknames = BuildList(nickname, ',', ' ', false); // TODO: NEEDS TESTING

                output = $"{command} {outputNicknames}";
            }
            else
            {
                output = $"{command} {string.Join(' ', parameters)}";
            }

            return output.TrimEnd();
        }


        #region Utility Functions

        public static string PrefixMessage(string parameter, char prefix = ':')
        {
            if (!parameter.StartsWith(prefix)) // Check it has the final arg denotion 
            {
                parameter = parameter.Insert(0, prefix.ToString());
            }
            return parameter;
        }

        public static string PrefixMessage(IEnumerable<string> parameters, char prefix = ':')
        {
            return PrefixMessage(string.Join(' ', parameters));
        }

        public static string BuildList(string list, char splitter = ',', char joiner = ',', bool shouldPrefix = true)
        {
            var splitList = list.Split(splitter);
            string outputList = string.Empty;

            foreach (string channel in splitList) // Iterate through each of the channels and check that they have the '#'
            {
                if (channel.EndsWith(joiner) || channel == string.Empty) { continue; }

                if (shouldPrefix)
                {
                    outputList += PrefixChannel(channel) + joiner; // Read the ',' from the split
                }
                else
                {
                    outputList += channel + joiner; 
                }
                
            }
            return outputList.Substring(0, outputList.Length - 1);
        }

        public static string PrefixChannel(string channel) // Add a '#' if the user didnt supply it (& is also usable not sure how it works with twitch however)
        {
            if (!channel.StartsWith("#"))
            {
                return channel.Insert(0, "#");
            }
            return channel;
        }

        public static string ListJoiner<T>(IEnumerable<T> list, char joiner = ' ')
        {
            var channels = string.Empty;
            foreach (T s in list)
            {
                channels += $"{s.ToString()}{joiner}";
            }
            channels = channels.Substring(0, channels.Length - 1); // Strips the ,
            return channels;
        }

        #endregion
    }
}

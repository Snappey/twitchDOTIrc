using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace twitchDotIRC
{
    class IRCMessage
    {

        /*   
        <message>  ::= [':' <prefix> <SPACE> ] <command> <params> <crlf>
        <prefix>   ::= <servername> | <nick> [ '!' <user> ] [ '@' <host> ]
        <command>  ::= <letter> { <letter> } | <number> <number> <number>
        <SPACE>    ::= ' ' { ' ' }
        <params>   ::= <SPACE> [ ':' <trailing> | <middle> <params> ]

        <middle>   ::= <Any *non-empty* sequence of octets not including SPACE
                       or NUL or CR or LF, the first of which may not be ':'>
        <trailing> ::= <Any, possibly *empty*, sequence of octets not including
                         NUL or CR or LF>

        <crlf>     ::= CR LF 
         */
        
        public string Prefix { get; private set; }
        public string Server { get; private set; }
        public string Nick { get; private set; }
        public string User { get; private set; }
        public string Host { get; private set; }

        public string Command { get; private set; }
        public bool IsNumericReply { get; private set; }
        public IRCReply IRCReply { get; private set; }

        public string Params { get; private set; }
        public List<string> ParamsList { get; private set; }

        public string Raw { get; private set; }
        private string RawTrimmed { get; set; }

        public IRCMessage(byte[] msg)
        {
            Raw = Encoding.ASCII.GetString(msg);
            RawTrimmed = Encoding.ASCII.GetString(msg).Trim();

            #region Actual Parsing
            if (RawTrimmed.Length > 0)
            {
                var subIdx = 0; // If we find a prefix, we shift this value to the end of the prefix
                // Prefix Section
                #region Prefix Parsing (Optional in Message Format)
                if (RawTrimmed.StartsWith(':') || RawTrimmed.Contains('!') || RawTrimmed.Contains('@')) // Contains prefix if the 1st char is :
                {                  // (Sometimes twitch replies without the ':' in which case we search for other giveaways that it is a prefix e.g. '!' / '@')
                                   // [':' <prefix> <SPACE> ]
                                   // < prefix >   ::= < servername > | < nick > ['!' < user > ]['@' < host > ]

                    subIdx = RawTrimmed.IndexOf(' '); // searches for the <SPACE>

                    if (RawTrimmed.StartsWith(':'))
                    {
                        Prefix = RawTrimmed.Substring(1, subIdx - 1); // Omit ':' and ' '
                    }
                    else
                    {
                        Prefix = RawTrimmed.Substring(0, subIdx); // In the event we detect a prefix without the :
                    }

                    var excIdx = Prefix.IndexOf('!'); // Finds '!'
                    var atIdx = Prefix.IndexOf('@'); // Finds '@', use this info to determine if its a nickname or servername (?, unsure about this)

                    if (excIdx == -1 && atIdx == -1)
                    {
                        Server = Prefix;
                        Nick = string.Empty;
                    }
                    else
                    {
                        if (atIdx != -1)
                        {
                            Host = Prefix.Substring(atIdx);
                        }
                        else
                        {
                            Host = string.Empty;
                        }
                        if (excIdx != -1)
                        {
                            if (atIdx != -1) // We need to calculate the length of the User section based of the position of the '@'
                            {
                                User = Prefix.Substring(excIdx, (Prefix.Length - excIdx) - Host.Length);
                            }
                            else
                            {
                                User = Prefix.Substring(excIdx);
                            }
                        }

                        Server = string.Empty;
                        Nick = Prefix.Substring(0, Prefix.Length - User.Length - Host.Length);
                    }
                }
                #endregion
                // Commands Section
                #region Command Parsing 
                {
                    // < command >  ::= < letter > { < letter > } | < number > < number > < number >
                    Command = RawTrimmed.Substring(subIdx + 1);
                    Command = Command.Substring(0, Command.IndexOf(' ')); // <command> <params> (<params> starts with <SPACE>)
                    IsNumericReply = false;

                    char startChar = (char)Command.ToCharArray().GetValue(0);
                    if (startChar >= 0x30 && startChar <= 0x39) // Between 0 - 9
                    {
                        // Number Code                  
                        var num = 0;
                        if(int.TryParse(Command, out num))
                        {
                            IRCReply = (IRCReply)num;
                            IsNumericReply = true;
                        }
                    }
                }
                #endregion
                // Params Section
                #region Params Parsing
                {
                    // <params>   ::= <SPACE> [ ':' <trailing> | <middle> <params> ]
                    if (subIdx == 0)
                    {
                        // No prefix detected,
                        subIdx = RawTrimmed.IndexOf(' ');
                        Params = RawTrimmed.Substring(subIdx + 1);
                    }
                    else
                    {
                        // We detected a prefix, so there is already a SPACE, find the next one
                        subIdx = RawTrimmed.IndexOf(' ', subIdx + 1);
                        Params = RawTrimmed.Substring(subIdx + 1);
                    }

                    ParamsList = new List<string>();
                    var paramsChar = Params.ToCharArray();

                    for(int i=0; i < paramsChar.Length; i++)
                    {
                        var ch = paramsChar[i];

                        if (ch == ':')  //[':' < trailing > | ... ]
                        {
                            ParamsList.Add(new string(paramsChar.Skip(i).Take(paramsChar.Length - i).ToArray()));
                            break; // Last parameter
                        }
                        else // [ ... | < middle > <params> ]
                        {
                            var nextSpace = Params.IndexOf(' ', i);
                            if (nextSpace == -1)
                            {
                                ParamsList.Add(new string(paramsChar.ToArray()));
                                break; // There are no more parameters twitch lied to us (where was the ':' :( )
                            }
                            else
                            {
                                ParamsList.Add(new string(paramsChar.Skip(i).Take(nextSpace - i).ToArray()));
                                i = nextSpace;
                            }
                        }
                    }
                    

                }
                #endregion
            }
            else
            {
                // Empty message
                Prefix = string.Empty;
                Server = string.Empty;
                Nick = string.Empty;
                User = string.Empty;
                Host = string.Empty;
                Command = string.Empty;
                Params = string.Empty;
                ParamsList = new List<string>();
            }

            #endregion
        }

        public override string ToString()
        {
            return Raw;
        }

        public static List<IRCMessage> Factory(byte[] msg)
        {
            List<IRCMessage> messages = new List<IRCMessage>();

            int lastFind = 0;
            for (int i = 0; i < msg.Length - 1; i++) 
            {
                if (msg[i] == 0xD && msg[i+1] == 0xA) // Finds the <CR LF> sequence, defined in RFC1459 for the end of a message
                {
                    List<byte> tmp = new List<byte>();
                    
                    for (int k=i; k > lastFind; k--)
                    {
                        tmp.Add(msg[k]);                     
                    }

                    var arr = tmp.ToArray();
                    Array.Reverse(arr); // Reverse the array, because we iterate the array backwards when inserting
                    messages.Add(new IRCMessage(arr));

                    lastFind = i; // Designates the end point for the next message iteration
                }
            }
            return messages;
        }
    }
}

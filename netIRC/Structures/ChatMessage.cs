using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace twitchDotIRC.Structures
{
    public class ChatMessage
    {
        public string Channel;
        public string User;
        public string Message;
        public DateTime Time;

        public static ChatMessage Factory(IRCMessage message)
        {
            ChatMessage chatMessage = new ChatMessage();

            chatMessage.Channel = message.Parameters.First().Substring(1); // Strings the starting '#'
            chatMessage.User = message.Nick;
            chatMessage.Message = message.Parameters.Last().Substring(1); // Strips the starting ':'
            chatMessage.Time = message.Time;

            return chatMessage;
        }

        public static List<ChatMessage> Factory(List<IRCMessage> messages)
        {
            List<ChatMessage> chatMessages = new List<ChatMessage>();

            foreach (IRCMessage message in messages)
            {
                chatMessages.Add(Factory(message));
            }

            return chatMessages;
        }
    }
}

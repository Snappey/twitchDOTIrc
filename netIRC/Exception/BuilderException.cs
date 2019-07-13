using System;
using System.Collections.Generic;
using System.Text;

namespace twitchDotIRC
{
    [Serializable]
    internal class BuilderException : Exception
    {
        public BuilderException()
        {

        }

        public BuilderException(string command, string content, string message) : base(command + content + message)
        {

        }
    }
}

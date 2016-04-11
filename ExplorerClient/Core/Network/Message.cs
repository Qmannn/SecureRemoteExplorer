using System;

namespace ExplorerClient.Core.Network
{
    [Serializable]
    public class Message
    {
        public Commands Command { get;}
        public string StringMessage { get; }

        public Message(Commands command, string stringMessage)
        {
            Command = command;
            StringMessage = stringMessage;
        }
    }
}
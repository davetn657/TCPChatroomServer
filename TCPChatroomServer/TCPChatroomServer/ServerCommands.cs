using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPChatroomServer
{
    internal class ServerCommands
    {
        public const string serverCommand = "SERVER COMMAND";

        //Message Identification
        public const string disconnectMessage = "DISCONNECTED";
        public const string userConnectedMessage = "CONNECTED";
        public const string nameTakenMessage = "NAME TAKEN";
        public const string nameConfirmMessage = "NAME CONFIRMED";
        public const string serverCapacityMessage = "SERVER AT CAPACITY";
        public const string joinedServerMessage = "JOINED SERVER";
        public const string messageFailedMessage = "MESSAGE FAILED TO SEND";
        public const string sendingAllConnectedMessage = "SENDING CONNECTED USERS";
        public const string acceptAllConnectedMessage = "ACCEPTED CONNECTED USERS";
        public const string userMessage = "UserMessage";

        public static List<string> commandMessages = new List<string>
        {
            disconnectMessage,
            userConnectedMessage,
            nameTakenMessage,
            nameConfirmMessage,
            serverCapacityMessage,
            joinedServerMessage,
            messageFailedMessage,
            sendingAllConnectedMessage,
            acceptAllConnectedMessage
        };

    }
}

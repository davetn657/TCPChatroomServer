using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPChatroomServer
{
    internal class ServerCommands
    {
        //Message Identification
        public const string serverMessage = "SERVERMESSAGE";
        public const string userMessage = "USERMESSAGE";

        public const string disconnectMessage = "DISCONNECTED";
        public const string userConnectedMessage = "CONNECTED";
        public const string nameTakenMessage = "NAME TAKEN";
        public const string nameConfirmMessage = "NAME CONFIRMED";
        public const string serverCapacityMessage = "SERVER AT CAPACITY";
        public const string joinedServerMessage = "JOINED SERVER";
        public const string messageFailedMessage = "MESSAGE FAILED TO SEND";
    }
}

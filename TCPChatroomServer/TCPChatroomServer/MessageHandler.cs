using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPChatroomServer
{
    internal class MessageHandler
    {
        private ClientData serverData;
        private ClientData userData;
        private CancellationTokenSource cancelTokenSource;
        private CancellationToken cancelToken;

        //Message Identification
        private const string serverMessage = "SERVERMESSAGE";
        private const string userMessage = "USERMESSAGE";

        public readonly string disconnectMessage = "DISCONNECTED";
        public readonly string userConnectedMessage = "CONNECTED";
        public readonly string nameTakenMessage = "NAME TAKEN";
        public readonly string nameConfirmMessage = "NAME CONFIRMED";
        public readonly string serverCapacityMessage = "SERVER AT CAPACITY";
        public readonly string joinedServerMessage = "JOINED SERVER";
        public readonly string messageFailedMessage = "MESSAGE FAILED TO SEND";


        public MessageHandler(ClientData userData)
        {
            this.userData = userData;
            this.serverData = new ClientData("Server");
            cancelTokenSource = new CancellationTokenSource();
            cancelToken = cancelTokenSource.Token;
        }

        //MESSAGES
        public async Task WaitUserMessage(List<ClientData> connectedClients)
        {
            while (!cancelTokenSource.IsCancellationRequested)
            {
                try
                {
                    MessageData receivedMessage = await ReceiveMessage();

                    if (receivedMessage.messageType == userMessage)
                    {
                        await SendMessageToAll(receivedMessage, connectedClients);
                    }
                    else
                    {
                        await SendServerCommand(receivedMessage.message);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        public async Task<MessageData> ReceiveMessage()
        {
            try
            {
                byte[] data = new byte[1024];
                Int32 bytes = await userData.clientStream.ReadAsync(data, 0, data.Length, cancelToken);
                MessageData message = new MessageData();

                message = message.Deserialize(data, bytes);

                return message;
            }
            catch (IOException ex) 
            {
                Debug.WriteLine($"Error: {ex.Message} \n");
                userData.DisconnectClient();
                return null;
            }
        }

        public async Task SendMessageToSpecific(MessageData message)
        {
            MessageData data = message;
            byte[] messageToSend = data.Serialize();

            await userData.clientStream.WriteAsync(messageToSend, 0, messageToSend.Length);
        }

        public async Task SendMessageToAll(MessageData message, List<ClientData> connectedClients)
        {
            try
            {
                foreach (ClientData client in connectedClients)
                {
                    await SendMessageToSpecific(message);
                }
            }
            catch (InvalidOperationException ex) 
            {
                Debug.WriteLine($"Error {ex.Message} \nFailed to send to all users");
            }
        }

        public async Task SendServerCommand(string message)
        {
            MessageData serverCommand = new MessageData(serverMessage, serverData, message);
            await SendMessageToSpecific(serverCommand);

            if (message == disconnectMessage || message == serverCapacityMessage)
            {
                userData.DisconnectClient();
            }
        }

        public void StopWaitUserMessage()
        {
            cancelTokenSource.Cancel();
            cancelTokenSource.Dispose();
        }
    }
}

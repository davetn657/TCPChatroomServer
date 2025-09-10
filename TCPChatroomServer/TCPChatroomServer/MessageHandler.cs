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
        private ClientData ServerData;
        private ClientData UserData;
        private CancellationTokenSource CancelTokenSource;
        private CancellationToken CancelToken;

        //Message Identification
        private const string ServerMessage = "SERVERMESSAGE";
        private const string UserMessage = "USERMESSAGE";

        public readonly string DisconnectMessage = "DISCONNECTED";
        public readonly string NameTakenMessage = "NAME TAKEN";
        public readonly string UserConnectedMessage = "CONNECTED";
        public readonly string ServerCapacityMessage = "SERVER AT CAPACITY";
        public readonly string MessageFailedMessage = "MESSAGE FAILED TO SEND";


        public MessageHandler(ClientData userData)
        {
            this.UserData = userData;
            this.ServerData = new ClientData("Server");
            CancelTokenSource = new CancellationTokenSource();
            CancelToken = CancelTokenSource.Token;
        }

        //MESSAGES
        public async Task WaitUserMessage(List<ClientData> connectedClients)
        {
            while (!CancelTokenSource.IsCancellationRequested)
            {
                try
                {
                    MessageData receivedMessage = await ReceiveMessage();

                    if (receivedMessage.MessageType == UserMessage)
                    {
                        await SendMessageToAll(receivedMessage, connectedClients);
                    }
                    else
                    {
                        await SendServerCommand(receivedMessage.Message);
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
                Int32 bytes = await UserData.ClientStream.ReadAsync(data, 0, data.Length, CancelToken);
                MessageData message = new MessageData();

                message = message.Deserialize(data, bytes);

                return message;
            }
            catch (IOException ex) 
            {
                Debug.WriteLine($"Error: {ex.Message} \n");
                UserData.DisconnectClient();
                return null;
            }
        }

        public async Task SendMessageToSpecific(MessageData message)
        {
            MessageData data = message;
            byte[] messageToSend = data.Serialize();

            await UserData.ClientStream.WriteAsync(messageToSend, 0, messageToSend.Length);
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
            MessageData serverMessage = new MessageData(ServerMessage, ServerData, message);
            await SendMessageToSpecific(serverMessage);

            if (message == DisconnectMessage || message == ServerCapacityMessage)
            {
                UserData.DisconnectClient();
            }
        }

        public void StopWaitUserMessage()
        {
            CancelTokenSource.Cancel();
            CancelTokenSource.Dispose();
        }
    }
}

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
        private CancellationTokenSource CancelTokenSource;

        //Message Identification
        public readonly string ServerCommand = "SERVERCOMMAND";
        public readonly string ServerMessage = "SERVERMESSAGE";
        public readonly string UserMessage = "USERMESSAGE";

        public readonly string DisconnectMessage = "DISCONNECT";
        public readonly string NameTakenMessage = "NAME TAKEN";
        public readonly string UserConnectedMessage = "CONNECTED";
        public readonly string ServerCapacityMessage = "SERVER AT CAPACITY";
        public readonly string MessageFailedMessage = "MESSAGE FAILED TO SEND";


        public MessageHandler(ClientData serverData) 
        {
            this.ServerData = serverData;
        }

        //MESSAGES
        public async Task WaitUserMessage(ClientData user, List<ClientData> connectedClients)
        {
            CancelTokenSource = new CancellationTokenSource();

            
            while (!CancelTokenSource.IsCancellationRequested)
            {
                try
                {
                    MessageData receivedMessage = await ReceiveMessage(user);

                    if (receivedMessage == null)
                    {
                        MessageData message = new MessageData(ServerMessage, ServerData, MessageFailedMessage);
                        await SendMessageToSpecific(user, message);
                    }
                    else if (receivedMessage.MessageType == ServerCommand && receivedMessage.Message == DisconnectMessage)
                    {
                        await user.DisconnectClient();
                        break;
                    }
                    else if (receivedMessage.MessageType == UserMessage)
                    {
                        await SendMessageToAll(receivedMessage, connectedClients);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        public async Task<MessageData> ReceiveMessage(ClientData user)
        {
            try
            {
                byte[] data = new byte[1024];
                Int32 bytes = await user.ClientStream.ReadAsync(data, 0, data.Length);
                MessageData message = new MessageData();

                message = message.Deserialize(data, bytes);

                return message;
            }
            catch (IOException ex) 
            {
                Debug.WriteLine($"Error: {ex.Message} \n");
                await user.DisconnectClient();
                return null;
            }
        }

        public async Task SendMessageToSpecific(ClientData user, MessageData message)
        {
            MessageData data = message;
            byte[] messageToSend = data.Serialize();

            await user.ClientStream.WriteAsync(messageToSend, 0, messageToSend.Length);
        }

        public async Task SendMessageToAll(MessageData message, List<ClientData> connectedClients)
        {
            try
            {
                foreach (ClientData client in connectedClients)
                {
                    await SendMessageToSpecific(client, message);
                }
            }
            catch (InvalidOperationException ex) 
            {
                Debug.WriteLine($"Error {ex.Message} \nFailed to send to all users");
            }
        }

        public void StopWaitUserMessage()
        {
            CancelTokenSource.Cancel();
            CancelTokenSource.Dispose();
        }
    }
}

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
        private ClientData userData;
        private CancellationTokenSource cancelTokenSource;
        private CancellationToken cancelToken;

        public MessageHandler(ClientData userData)
        {
            this.userData = userData;
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

                    if (receivedMessage.messageType == ServerCommands.userMessage)
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

        public async Task SendMessageToSpecific(MessageData message, ClientData client)
        {
            MessageData data = message;
            byte[] messageToSend = data.Serialize();

            await client.clientStream.WriteAsync(messageToSend, 0, messageToSend.Length);

            Console.WriteLine($"SENT:{message.message}");
        }

        public async Task SendMessageToAll(MessageData message, List<ClientData> connectedClients)
        {
            try
            {
                foreach (ClientData client in connectedClients)
                {
                    await SendMessageToSpecific(message, client);
                }
            }
            catch (InvalidOperationException ex) 
            {
                Debug.WriteLine($"Error {ex.Message} \nFailed to send to all users");
            }
        }

        public async Task SendServerCommand(string message)
        {
            MessageData serverCommand = new MessageData(ServerCommands.serverMessage, "server", message);
            await SendMessageToSpecific(serverCommand, userData);

            if (message == ServerCommands.disconnectMessage || message == ServerCommands.serverCapacityMessage)
            {
                userData.DisconnectClient();
            }
        }

        public bool CheckIfUserCommand(MessageData messageData)
        {
            if (messageData.messageType == ServerCommands.userCommand)
            {
                return true;
            }
            return false;
        }

        public void StopWaitUserMessage()
        {
            cancelTokenSource.Cancel();
            cancelTokenSource.Dispose();
        }
    }
}

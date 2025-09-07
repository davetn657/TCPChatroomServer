using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPChatroomServer
{
    internal class MessageHandler
    {
        private Server ServerConnection;
        private CancellationTokenSource CancelTokenSource;


        public MessageHandler(Server server) 
        {
            this.ServerConnection = server;
        }

        //MESSAGES
        public async Task WaitUserMessage(ClientData user)
        {
            CancelTokenSource = new CancellationTokenSource();

            
            while (!CancelTokenSource.IsCancellationRequested)
            {
                try
                {
                    MessageData receivedMessage = await ReceiveMessage(user);

                    if (receivedMessage == null)
                    {
                        MessageData message = new MessageData(ServerConnection.ServerMessage, ServerConnection.serverData, ServerConnection.MessageFailedMessage);
                        await SendMessageToSpecific(user, message);
                    }
                    else if (receivedMessage.MessageType == ServerConnection.ServerCommand && receivedMessage.Message == ServerConnection.DisconnectMessage)
                    {
                        await ServerConnection.DisconnectClient(user.Name);
                        break;
                    }
                    else if (receivedMessage.MessageType == ServerConnection.UserMessage)
                    {
                        await SendMessageToAll(receivedMessage);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
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
                Console.WriteLine($"Error: {ex.Message} \n");
                await ServerConnection.DisconnectClient(user.Name);
                return null;
            }
        }

        public async Task SendMessageToSpecific(ClientData user, MessageData message)
        {
            MessageData data = message;
            byte[] messageToSend = data.Serialize();

            await user.ClientStream.WriteAsync(messageToSend, 0, messageToSend.Length);
        }

        public async Task SendMessageToAll(MessageData message)
        {
            try
            {
                foreach (ClientData client in ServerConnection.ConnectedClients)
                {
                    await SendMessageToSpecific(client, message);
                }
            }
            catch (InvalidOperationException ex) 
            {
                Console.WriteLine($"Error {ex.Message} \nFailed to send to all users");
            }
        }

        public void StopWaitUserMessage()
        {
            CancelTokenSource.Cancel();
            CancelTokenSource.Dispose();
        }
    }
}

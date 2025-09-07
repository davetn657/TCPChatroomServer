using System.Net;
using System.Net.Sockets;

namespace TCPChatroomServer
{
    internal class Server
    {
        private CancellationTokenSource CancelTokenSource;

        //SERVER IDENTIFICATION
        public readonly ClientData serverData = new ClientData("Server");

        //SERVER CREATION
        public IPAddress Host { get; }
        public int Port { get; }
        private TcpListener Listener;
        private const int MinPortVal = 40000;
        private const int MaxPortVal = 45000;

        //Message Identification
        public readonly string ServerCommand = "SERVERCOMMAND";
        public readonly string ServerMessage = "SERVERMESSAGE";
        public readonly string UserMessage = "USERMESSAGE";

        public readonly string DisconnectMessage = "DISCONNECT";
        public readonly string NameTakenMessage = "NAME TAKEN";
        public readonly string UserConnectedMessage = "CONNECTED";
        public readonly string ServerCapacityMessage = "SERVER AT CAPACITY";
        public readonly string MessageFailedMessage = "MESSAGE FAILED TO SEND";

        //ACCEPTED CLIENT VARIABLES
        public int MaxCapacity = 10;
        public List<ClientData> ConnectedClients;

        public Server(IPAddress host)
        {
            this.Host = host;
            this.Port = GeneratedPort();
            this.ConnectedClients = new List<ClientData>();
        }

        private int GeneratedPort()
        {
            Random random = new Random();
            int port = random.Next(MinPortVal, MaxPortVal);
            return port;
        }

        //SERVER CONNECTION

        public void StartServer()
        {
            CancelTokenSource = new CancellationTokenSource();
            Listener = new TcpListener(Host, Port);
            Listener.Start();
            Task.Run(() => StartClientConnectionLoop());
        }

        public async Task StopServer()
        {
            //go through all connected clients disconnecting them, then stop the server
            try
            {
                StopClientConnectionLoop();

                foreach (var client in ConnectedClients) 
                {
                    await DisconnectClient(client.Name);
                }

                Listener.Stop();
            }
            catch
            {
                Console.WriteLine("Error occured when stopping server");
            }
        }

        private async Task StartClientConnectionLoop()
        {
            while(!CancelTokenSource.IsCancellationRequested)
            {
                //wait for client connections than if a client connects add it to connected clients
                try
                {
                    MessageData incomingData;

                    TcpClient client = await Listener.AcceptTcpClientAsync();
                    NetworkStream stream = client.GetStream();

                    MessageHandler messageHandler = new MessageHandler(this);
                    ClientData clientData = new ClientData("Temp", client, stream, messageHandler);
                    MessageData outgoingMessage = new MessageData(ServerCommand, serverData, string.Empty);

                    if (ConnectedClients.Count >= MaxCapacity)
                    {
                        //if amount of clients connected exceeds capacity send a message back to client notifying that the chatroom is at max capacity then remove them from the stream
                        outgoingMessage.Message = ServerCapacityMessage;
                        await clientData.MessageHandler.SendMessageToSpecific(clientData, outgoingMessage);

                        stream.Close();
                        client.Close();
                    }
                    else
                    {
                        //await users first message (it will be a predefined)
                        //if the name of the user is already in the array connectedClients, send a message back to client notifying them. then await next message
                        var nameTaken = false;

                        while (true)
                        {
                            incomingData = await clientData.MessageHandler.ReceiveMessage(clientData);


                            for (int i = 0; i < ConnectedClients.Count; i++)
                            {
                                if (ConnectedClients[i].Name == incomingData.From.Name)
                                {
                                    outgoingMessage.Message = NameTakenMessage;
                                    nameTaken = true;

                                    break;
                                }
                            }

                            if (nameTaken)
                            {
                                //wait for the users to input a different username
                                await clientData.MessageHandler.SendMessageToSpecific(clientData, outgoingMessage);
                                continue;
                            }
                            else
                            {
                                outgoingMessage.Message = UserConnectedMessage + $":{incomingData}";
                                await clientData.MessageHandler.SendMessageToAll(outgoingMessage);
                                break;
                            }

                        }


                        //if name is not taken do the following:
                        //send a message back to client with the index number (id) which from now on it will use
                        //edit clientData using (ClientName, client, stream, numConnectedClients)
                        //add newclientdata to connectedClients
                        //increment numConnectedClients by one
                        Console.WriteLine($"{incomingData.From.Name} Connected");
                        clientData.Name = incomingData.From.Name;

                        outgoingMessage.Message = AllUsers();
                        await clientData.MessageHandler.SendMessageToSpecific(clientData, outgoingMessage);

                        ConnectedClients.Add(clientData);

                        await clientData.MessageHandler.WaitUserMessage(clientData);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error:{e.Message} \nCould not connect to user");
                }
            }
        }

        //CLIENT DISCONNECTION
        public async Task DisconnectClient(string username)
        {
            ClientData user = ConnectedClients.Find(e => e.Name == username);

            if (user != null)
            {
                try
                {
                    user.MessageHandler.StopWaitUserMessage();

                    ConnectedClients.Remove(user);

                    user.ClientStream?.Close();
                    user.Client?.Close();

                    Console.WriteLine($"{username} Disconnected");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error {ex.Message} \nUnexpected Disconnection");
                }
            }
            else
            {
                Console.WriteLine("User Not Found");
            }
        }

        private string AllUsers()
        {
            string connectedClientsString = string.Empty;

            for (int i = 0; i < ConnectedClients.Count; i++)
            {
                connectedClientsString += ConnectedClients[i].Name + ",";
            }

            return connectedClientsString.TrimEnd(',');
        }

        private void StopClientConnectionLoop()
        {
            CancelTokenSource.Cancel();
            CancelTokenSource.Dispose();
        }
    }
}

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace TCPChatroomServer
{
    internal class Server
    {
        private CancellationTokenSource CancelTokenSource;

        //SERVER CREATION
        public IPAddress host { get; }
        public int port { get; }
        private TcpListener listener;
        private const int minPortVal = 40000;
        private const int maxPortVal = 45000;

        //ACCEPTED CLIENT VARIABLES
        public int maxCapacity = 10;
        public List<ClientData> connectedClients;
        public string connectedClientsString;

        public Server(IPAddress host)
        {
            this.host = host;
            this.port = GeneratedPort();
            this.connectedClients = new List<ClientData>();
        }

        private int GeneratedPort()
        {
            Random random = new Random();
            int port = random.Next(minPortVal, maxPortVal);
            return port;
        }

        //SERVER CONNECTION

        public void StartServer()
        {
            CancelTokenSource = new CancellationTokenSource();
            listener = new TcpListener(host, port);
            listener.Start();
            Task.Run(() => StartClientConnectionLoop());
        }

        public async Task StopServer()
        {
            //go through all connected clients disconnecting them, then stop the server
            try
            {
                StopClientConnectionLoop();

                foreach (var client in connectedClients) 
                {
                    client.DisconnectClient();
                }

                listener.Stop();
            }
            catch
            {
                Debug.WriteLine("Error occured when stopping server");
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

                    TcpClient client = await listener.AcceptTcpClientAsync();
                    NetworkStream stream = client.GetStream();


                    ClientData clientData = new ClientData("Temp", client, stream);
                    MessageHandler messageHandler = clientData.messageHandler;
                    MessageData outgoingMessage = new MessageData();

                    incomingData = await messageHandler.ReceiveMessage();

                    if (incomingData != null && messageHandler.CheckIfUserCommand(incomingData) && incomingData.message != ServerCommands.userConnectedMessage) 
                    {
                        continue;
                    }

                    if (connectedClients.Count >= maxCapacity)
                    {
                        Console.WriteLine("USER TRIED TO CONNECT BUT SERVER IS FULL");
                        //if amount of clients connected exceeds capacity send a message back to client notifying that the chatroom is at max capacity then remove them from the stream
                        await messageHandler.SendServerCommand(ServerCommands.serverCapacityMessage);

                        clientData.DisconnectClient();
                    }
                    else
                    {
                        //await users first message (it will be a predefined)
                        //if the name of the user is already in the array connectedClients, send a message back to client notifying them. then await next message
                        Console.WriteLine("USER CONNECTED");
                        await messageHandler.SendServerCommand(ServerCommands.joinedServerMessage);
                        var nameTaken = false;

                        while (true)
                        {
                            incomingData = await messageHandler.ReceiveMessage();

                            foreach(ClientData c in connectedClients)
                            {
                                if (c.name == incomingData.message)
                                {
                                    nameTaken = true;
                                    break;
                                }
                            }

                            if (nameTaken)
                            {
                                //wait for the users to input a different username
                                await messageHandler.SendServerCommand(ServerCommands.nameTakenMessage);
                                continue;
                            }
                            else
                            {
                                await messageHandler.SendServerCommand(ServerCommands.nameConfirmMessage);
                                break;
                            }

                        }


                        //if name is not taken do the following:
                        //send a message back to client with the index number (id) which from now on it will use
                        //edit clientData using (ClientName, client, stream, numConnectedClients)
                        //add newclientdata to connectedClients
                        //increment numConnectedClients by one
                        outgoingMessage.message = ServerCommands.userConnectedMessage + $": {incomingData.message}";
                        Console.WriteLine(outgoingMessage.message);
                        clientData.name = incomingData.message;

                        connectedClients.Add(clientData);
                        AllUsers();

                        while (true)
                        {
                            await messageHandler.SendServerCommand(ServerCommands.sendingAllConnectedMessage);
                            incomingData = await messageHandler.ReceiveMessage();

                            if (incomingData.message == ServerCommands.acceptAllConnectedMessage && messageHandler.CheckIfUserCommand(incomingData))
                            {
                                outgoingMessage.message = connectedClientsString;
                                await messageHandler.SendMessageToSpecific(outgoingMessage, clientData);
                                break;
                            }
                            else
                            {
                                Console.WriteLine("Trying to send all users again");
                            }
                        }

                        await messageHandler.WaitUserMessage(connectedClients);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Error:{e.Message} \nCould not connect to user");
                    continue;
                }
            }
        }

        public void Disconnect(string userName)
        {
            ClientData user = connectedClients.Find(e => e.name == userName);

            if (user != null)
            {
                user.DisconnectClient();
                connectedClients.Remove(user);
            }
            else
            {
                Debug.WriteLine($"Could not find user {userName}");
            }
        }
        private void AllUsers()
        {
            for (int i = 0; i < connectedClients.Count; i++)
            {
                connectedClientsString += connectedClients[i].name + ",";
            }

            connectedClientsString.TrimEnd(',');
        }

        private void StopClientConnectionLoop()
        {
            CancelTokenSource.Cancel();
            CancelTokenSource.Dispose();
        }
    }
}

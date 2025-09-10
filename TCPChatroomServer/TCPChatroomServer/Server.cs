using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace TCPChatroomServer
{
    internal class Server
    {
        private CancellationTokenSource CancelTokenSource;

        //SERVER CREATION
        public IPAddress Host { get; }
        public int Port { get; }
        private TcpListener Listener;
        private const int MinPortVal = 40000;
        private const int MaxPortVal = 45000;

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
                    client.DisconnectClient();
                }

                Listener.Stop();
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

                    TcpClient client = await Listener.AcceptTcpClientAsync();
                    NetworkStream stream = client.GetStream();


                    ClientData clientData = new ClientData("Temp", client, stream);
                    MessageHandler messageHandler = clientData.MessageHandler;
                    MessageData outgoingMessage = new MessageData();

                    if (ConnectedClients.Count >= MaxCapacity)
                    {
                        //if amount of clients connected exceeds capacity send a message back to client notifying that the chatroom is at max capacity then remove them from the stream
                        await messageHandler.SendServerCommand(messageHandler.ServerCapacityMessage);

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
                            incomingData = await messageHandler.ReceiveMessage();


                            foreach(ClientData c in ConnectedClients)
                            {
                                if (c.Name == incomingData.From.Name)
                                {
                                    nameTaken = true;
                                    break;
                                }
                            }

                            if (nameTaken)
                            {
                                //wait for the users to input a different username
                                await messageHandler.SendServerCommand(messageHandler.NameTakenMessage);
                                continue;
                            }
                            else
                            {
                                outgoingMessage.Message = messageHandler.UserConnectedMessage + $":{incomingData}";
                                await messageHandler.SendMessageToAll(outgoingMessage, ConnectedClients);
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
                        await messageHandler.SendMessageToSpecific(outgoingMessage);

                        ConnectedClients.Add(clientData);

                        await messageHandler.WaitUserMessage(ConnectedClients);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Error:{e.Message} \nCould not connect to user");
                }
            }
        }

        public void Disconnect(string userName)
        {
            ClientData user = ConnectedClients.Find(e => e.Name == userName);

            if (user != null)
            {
                user.DisconnectClient();
                ConnectedClients.Remove(user);
            }
            else
            {
                Debug.WriteLine($"Could not find user {userName}");
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

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace TCPChatroomServer
{
    internal class Server
    {
        //SERVER IDENTIFICATION
        private ClientData serverData = new ClientData("Server");

        //SERVER CREATION
        public IPAddress Host { get; }
        public int Port { get; }
        private TcpListener Listener;
        private NetworkStream Stream;
        private int MinPortVal = 40000;
        private int MaxPortVal = 45000;
        

        //ACCEPTED CLIENT VARIABLES
        private int MaxCapacity = 10;
        private int NumConnectedClients = 0;
        private ClientData[] ConnectedClients;

        public Server(IPAddress host)
        {
            this.Host = host;
            this.Port = 41015;
            this.ConnectedClients = new ClientData[MaxCapacity];
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
            Listener = new TcpListener(Host, Port);
            Listener.Start();
            Task.Run(() => StartClientConnection());
        }

        public void StopServer()
        {
            //go through all connected clients disconnecting them, then stop the server
            try
            {
                Listener.Stop();
            }
            catch
            {
                Console.WriteLine("Error occured when stopping server");
            }
        }

        private async Task StartClientConnection()
        {
            while (true)
            {
                //wait for client connections than if a client connects add it to connected clients
                try
                {
                    string incomingData = string.Empty;
                    string outgoingData = string.Empty;

                    using TcpClient client = await Listener.AcceptTcpClientAsync();
                    Stream = client.GetStream();

                    ClientData clientData = new ClientData("Temp", client, Stream, 10);

                    if (NumConnectedClients >= MaxCapacity)
                    {
                        //if amount of clients connected exceeds capacity send a message back to client notifying that the chatroom is at max capacity
                        outgoingData = "Server at Capacity";
                        SendMessageToSpecific(serverData, clientData, outgoingData);

                        Stream.Close();

                    }
                    else
                    {
                        //await users first message (it will be a predefined)
                        //if the name of the user is already in the array connectedClients, send a message back to client notifying them. then await next message
                        var nameTaken = false;

                        while (true)
                        {
                            incomingData = RecieveMessage(clientData);
                            for (int i = 0; i < NumConnectedClients; i++)
                            {
                                if (ConnectedClients[i].Name == incomingData)
                                {
                                    outgoingData = "Name Taken";
                                    nameTaken = true;

                                    break;
                                }
                            }

                            if (nameTaken)
                            {
                                //wait for the users to input a different username
                                SendMessageToSpecific(serverData, clientData, outgoingData);
                                incomingData = RecieveMessage(clientData);
                                continue;
                            }
                            else
                            {
                                break;
                            }

                        }


                        //if name is not taken do the following:
                        //send a message back to client with the index number (id) which from now on it will use
                        //edit clientData using (ClientName, client, stream, numConnectedClients)
                        //add newclientdata to connectedClients
                        //increment numConnectedClients by one
                        Console.WriteLine($"{incomingData} Connected");
                        SendMessageToSpecific(serverData, clientData, NumConnectedClients.ToString());
                        clientData.Name = incomingData;
                        clientData.IsConnected = true;
                        ConnectedClients[NumConnectedClients] = clientData;
                        NumConnectedClients++;

                        await WaitUserMessage(clientData);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error:{e} \nCould not connect to user");
                }
            }
        }

        //CLIENT DISCONNECTION
        public async Task DisconnectClient(string username)
        {
            ClientData user = FindUser(username);
            if (user != null)
            {
                user.IsConnected = false;

                SendMessageToSpecific(serverData, user, "Disconnected");

                RemoveUserFromList(user);

                Console.WriteLine($"{username} Disconnected");
            }
        }

        private void RemoveUserFromList(ClientData user)
        {
            for (int i = 0; i < NumConnectedClients; i++)
            {
                if (ConnectedClients[i] == user)
                {
                    for (int k = i + 1; k < NumConnectedClients; k++)
                    {
                        ConnectedClients[i] = ConnectedClients[k];
                    }

                    NumConnectedClients--;
                    break;
                }
            }
        }

        private ClientData FindUser(string username)
        {
            for(int i = 0; i < NumConnectedClients; i++)
            {
                if (ConnectedClients[i].Name == username)
                {
                    return ConnectedClients[i];
                }
            }

            return null;
        }

        //MESSAGES
        private async Task WaitUserMessage(ClientData user)
        {
            string disconnectMessage = $"{user.Name}:USERCOMMAND:DISCONNECT";

            while (true)
            {
                if (user.IsConnected == false)
                {
                    break;
                }

                string recievedMessage = RecieveMessage(user);

                Console.WriteLine(recievedMessage + "HELLO");


                if (recievedMessage == disconnectMessage)
                {
                    DisconnectClient(user.Name);
                    break;
                }

                await SendMessageToAll(user, recievedMessage);
            }
        }

        private string RecieveMessage(ClientData user)
        {
            byte[] data = new byte[1024];
            Int32 bytes = user.ClientStream.Read(data, 0, data.Length);
            MessageData message = new MessageData();

            message = message.Deserialize(data, bytes);

            return message.Message;
        }

        private async Task SendMessageToSpecific(ClientData fromuser, ClientData user, string message)
        {
            MessageData data = new MessageData(fromuser.Name, message);
            byte[] messageToSend = data.Serialize();

            user.ClientStream.Write(messageToSend, 0, messageToSend.Length);
        }

        private async Task SendMessageToAll(ClientData fromuser, string message)
        {
            for (int i = 0; i < NumConnectedClients; i++)
            {
                SendMessageToSpecific(fromuser, ConnectedClients[i], message);
            }
        }
    }
}

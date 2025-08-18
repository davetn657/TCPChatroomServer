using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TCPChatroomServer;

//Create a Listener
//Await connection to clients
//Once client connects send them a message "Connected!"
//Wait for input from client
//If client sends message to the chatroom, send that message to all other users

//Server Identification Variables
const string ServerName = "Server";
const string ServerId = "01210";

//Server Creation Variables
var _port = 0;
int _minPortVal = 40000;
int _maxPortVal = 45000;
IPAddress _host = IPAddress.Loopback;
TcpListener _server;
NetworkStream _stream;

//Accepted Client Variables
var _maxCapacity = 10;
var _numConnectedClients = 0;
ClientData[] _connectedClients = new ClientData[_maxCapacity];

/*
 * USER INPUT
 */

while (true)
{
    Console.WriteLine("Enter your IP address (ex. 123.123.1.1): ");

    bool validateIP = IPAddress.TryParse(Console.ReadLine(), out _host);

    if (validateIP)
    {
        break;
    }
}

while (true)
{
    Console.WriteLine($"Enter a port number (between {_minPortVal} - {_maxPortVal}):");

    bool validatePort = Int32.TryParse(Console.ReadLine(), out _port);

    if (validatePort && (_port >= _minPortVal && _port <= _maxPortVal) )
    {
        break;
    }
}

/*
 *  CREATING SERVER
 */
StartServer();
Console.WriteLine("AWAITING CONNECTION....");
Task.Run(() => StartClientConnection());


Console.WriteLine("BEEP BOOP");
Console.Read();

/*
 * SERVER FUNCTIONS
 */

void StartServer()
{
    _server = new TcpListener(_host, _port);
    _server.Start();
}

void StopServer()
{
    //go through all connected clients disconnecting them, then stop the server
    try
    {
        _server.Stop();
    }
    catch
    {
        Console.WriteLine("Error occured when stopping server");
    }
}
async Task StartClientConnection()
{
    while (true)
    {
        //wait for client connections than if a client connects add it to connected clients
        try
        {
            string incomingData = string.Empty;
            string outgoingData = string.Empty;

            using TcpClient client = await _server.AcceptTcpClientAsync();
            _stream = client.GetStream();

            ClientData clientData = new ClientData("Temp", client, _stream, 10);

            if (_numConnectedClients >= _maxCapacity)
            {
                //if amount of clients connected exceeds capacity send a message back to client notifying that the chatroom is at max capacity
                outgoingData = "Server at Capacity";
                SendMessageToSpecific(clientData, outgoingData);

                _stream.Close();

            }
            else
            {
                //await users first message (it will be a predefined)
                //if the name of the user is already in the array connectedClients, send a message back to client notifying them. then await next message
                var nameTaken = false;
                incomingData = RecieveMessage(clientData);

                while (true)
                {
                    for (int i = 0; i < _numConnectedClients; i++)
                    {
                        if (_connectedClients[i].Name == incomingData)
                        {
                            outgoingData = "Name Taken";
                            nameTaken = true;

                            break;
                        }
                    }

                    if (nameTaken)
                    {
                        //wait for the users to input a different username
                        SendMessageToSpecific(clientData, outgoingData);
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

                SendMessageToSpecific(clientData, _numConnectedClients.ToString());
                clientData.Name = incomingData;
                clientData.Index = _numConnectedClients;
                clientData.IsConnected = true;
                _connectedClients[_numConnectedClients] = clientData;
                _numConnectedClients++;
            }

            Task.Run(() => WaitUserMessage(clientData));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error:{e} \nCould not connect to user");
        }
    }
}
void DisconnectClient(ClientData user)
{
    user.IsConnected = false;
    
    //close stream and remove client from client list
    //refactor client list
}

async Task WaitUserMessage(ClientData user) 
{
    string disconnectMessage = user.Name + user.Index + "Disconnect";

    while (true)
    {
        if(user.IsConnected == false)
        {
            break;
        }

       string recievedMessage = RecieveMessage(user);

        if(recievedMessage == disconnectMessage)
        {
            DisconnectClient(user);
            break;
        }
        
        await SendMessageToAll(recievedMessage);
    }
}

string RecieveMessage(ClientData user)
{
    byte[] data = new byte[1024];
    Int32 bytes = user.ClientStream.Read(data, 0, data.Length);
    MessageData message = new MessageData();
    
    message = message.Deserialize(data, bytes);

    return message.Message;
}

void SendMessageToSpecific(ClientData user, string message)
{
    MessageData data = new MessageData(ServerName, ServerId, message);
    byte[] messageToSend = data.Serialize();

    user.ClientStream.Write(messageToSend, 0, messageToSend.Length);
}

async Task SendMessageToAll(string message)
{
    for(int i = 0; i < _numConnectedClients; i++)
    {
        SendMessageToSpecific(_connectedClients[i], message);
    }
}
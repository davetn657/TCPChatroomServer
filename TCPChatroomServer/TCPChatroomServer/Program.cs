using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using TCPChatroomServer;

//Create a Listener
//Await connection to clients
//Once client connects send them a message "Connected!"
//Wait for input from client
//If client sends message to the chatroom, send that message to all other users

var port = 0;
int minPortVal = 40000;
int maxPortVal = 45000;
IPAddress host = IPAddress.Loopback;
TcpListener server;
NetworkStream stream;

ClientData[] connectedClients;
var maxCapacity = 10;
var numConnectedClients = 0;

// USER INPUTS

while (true)
{
    Console.WriteLine("Enter your IP address (ex. 123.123.1.1): ");

    bool validateIP = IPAddress.TryParse(Console.ReadLine(), out host);

    if (validateIP)
    {
        break;
    }
}

while (true)
{
    Console.WriteLine($"Enter a port number (between {minPortVal} - {maxPortVal}");

    bool validatePort = Int32.TryParse(Console.ReadLine(), out port);

    if (validatePort && (port >= minPortVal && port <= maxPortVal) )
    {
        break;
    }
}

// SERVER 



void StartServer()
{
    server = new TcpListener(host, port);
    server.Start();
}

void StopServer()
{
    //go through all connected clients disconnecting them, then stop the server
    try
    {
        server.Stop();
    }
    catch
    {
        Console.WriteLine();
    }
}

async Task ConnectToClient()
{
    //wait for client connections than if a client connects add it to connected clients
    try
    {
        using TcpClient client = await server.AcceptTcpClientAsync();
        stream = client.GetStream();

        MessageData message;

        if(numConnectedClients > maxCapacity)
        {
            //if amount of clients connected exceeds capacity send a message back to client notifying that the chatroom is at max capacity
            message = new MessageData("Server", "01210", "Server at Capacity");
        }
        else
        {
            //await users first message (it will be a predefined)
            //if the name of the user is already in the array connectedClients, send a message back to client notifying them. then await next message

            //if name is not taken do the following:
            //create a new clientData using (ClientName, client, stream, numConnectedClients)
            //increment numConnectedClients by one
            //add newclientdata to connectedClients
            //send a message back to client with the index number (id) which from now on it will use
        }
    }
    catch
    {

    }
}

void SendMessage(string message)
{

}
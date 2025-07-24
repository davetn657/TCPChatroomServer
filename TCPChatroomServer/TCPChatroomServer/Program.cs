using System.Net;
using System.Net.Sockets;

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

//Validate user inputs
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
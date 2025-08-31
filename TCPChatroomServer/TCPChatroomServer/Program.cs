using System.Net;
using System.Net.Sockets;
using TCPChatroomServer;

IPAddress host;

/*
 * USER INPUT
 */

while (true)
{
    Console.WriteLine("Enter your IP address (ex. 123.123.1.1): ");
    bool validateIP = IPAddress.TryParse(Console.ReadLine(), out host);

    if (validateIP)
    {
        break;
    }
}

/*
 *  CREATING SERVER
 */

Server server = new Server(host);
server.StartServer();

string command;

while (true)
{
    Console.WriteLine("COMMAND: ");
    command = Console.ReadLine().ToLower();

    /*
     * COMMANDS:
     *  HELP - DISPLAYS LIST OF AVAILABLE COMMANDS
     *  EXIT - CLSOES THE SERVER AND EXITS THE PROGRAM
     *  KICK *USERNAME* - REMOVES THE USER WITH NAME USERNAME FROM THE SERVER
     *  
     */

    switch (command)
    {
        case "exit":
            break;
        default:
            continue;
    }
}

Console.WriteLine("BEEP BOOP");
Console.Read();


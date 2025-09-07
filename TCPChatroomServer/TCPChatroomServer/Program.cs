using System.Net;
using TCPChatroomServer;

IPAddress _host;
string _helpMessage = 
    """
    Note - All commands are not case sensitive!

    KICK - Prompts a username which will be kicked from the server
    EXIT - Stops the server and exits the application
    """;


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

/*
 *  CREATING SERVER
 */

Server server = new Server(_host);
server.StartServer();

Console.WriteLine($"Connecting to: HOST: {server.Host} - PORT: {server.Port}");

Console.WriteLine("AWAITING CONNECTION....");

Console.WriteLine("Type 'help' to see all server commands");

string command;

while (true)
{
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
        case "kick":
            Console.WriteLine("Username to kick: ");
            command = Console.ReadLine();
            server.DisconnectClient(command);
            continue;
        case "help":
            Console.WriteLine(_helpMessage);
            continue;
        case "exit":
            break;
        default:
            Console.WriteLine("Type 'help' to see all server commands");
            continue;
    }
}
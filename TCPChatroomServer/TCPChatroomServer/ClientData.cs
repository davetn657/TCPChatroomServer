using System.Diagnostics;
using System.Net.Sockets;

namespace TCPChatroomServer
{
    internal class ClientData
    {
        public string Name { get; set; }
        public TcpClient Client { get; set; }
        public NetworkStream ClientStream {  get; set; }
        public MessageHandler MessageHandler { get; set; }

        public ClientData()
        {
            this.Name = string.Empty;
        }

        public ClientData(string name, TcpClient client, NetworkStream stream, MessageHandler messageHandler) 
        {
            this.Name = name;
            this.Client = client;
            this.ClientStream = stream;
            this.MessageHandler = messageHandler;
        }

        public ClientData(string name) { this.Name = name; }

        //CLIENT DISCONNECTION
        public async Task DisconnectClient()
        {
            try
            {
                this.MessageHandler.StopWaitUserMessage();

                this.ClientStream?.Close();
                this.Client?.Close();

                Console.WriteLine($"{Name} Disconnected");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error {ex.Message} \nUnexpected Disconnection");
            }
        }
    }
}

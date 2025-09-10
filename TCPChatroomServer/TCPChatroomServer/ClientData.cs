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
            this.Client = new TcpClient();
            this.ClientStream = this.Client.GetStream();
            this.MessageHandler = new MessageHandler(this);
        }

        public ClientData(string name, TcpClient client, NetworkStream stream) 
        {
            this.Name = name;
            this.Client = client;
            this.ClientStream = stream;
            this.MessageHandler = new MessageHandler(this);
        }

        public ClientData(string name) 
        { 
            this.Name = name;
            this.Client = new TcpClient();
            this.ClientStream = this.Client.GetStream();
            this.MessageHandler = new MessageHandler(this);
        }

        //CLIENT DISCONNECTION
        public void DisconnectClient()
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

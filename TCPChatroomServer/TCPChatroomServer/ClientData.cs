using System.Diagnostics;
using System.Net.Sockets;

namespace TCPChatroomServer
{
    internal class ClientData
    {
        public string name { get; set; }
        public TcpClient client { get; set; }
        public NetworkStream clientStream {  get; set; }
        public MessageHandler messageHandler { get; set; }

        public ClientData()
        {
            this.name = string.Empty;
            this.client = new TcpClient();
            this.clientStream = this.client.GetStream();
            this.messageHandler = new MessageHandler(this);
        }

        public ClientData(string name, TcpClient client, NetworkStream stream) 
        {
            this.name = name;
            this.client = client;
            this.clientStream = stream;
            this.messageHandler = new MessageHandler(this);
        }

        public ClientData(string name) 
        { 
            this.name = name;
            this.client = new TcpClient();
            this.clientStream = this.client.GetStream();
            this.messageHandler = new MessageHandler(this);
        }

        //CLIENT DISCONNECTION
        public void DisconnectClient()
        {
            try
            {
                this.messageHandler.StopWaitUserMessage();

                this.clientStream?.Close();
                this.client?.Close();

                Console.WriteLine($"{name} Disconnected");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error {ex.Message} \nUnexpected Disconnection");
            }
        }
    }
}

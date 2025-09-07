using System.Net.Sockets;

namespace TCPChatroomServer
{
    internal class ClientData
    {
        public string Name { get; set; }
        public TcpClient Client { get; set; }
        public NetworkStream ClientStream {  get; set; }

        public ClientData()
        {
            this.Name = string.Empty;
        }

        public ClientData(string name, TcpClient client, NetworkStream stream, int index) 
        {
            this.Name = name;
            this.Client = client;
            this.ClientStream = stream;
        }

        public ClientData(string name) { this.Name = name; }
    }
}

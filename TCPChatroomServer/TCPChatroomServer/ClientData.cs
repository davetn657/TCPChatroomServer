using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TCPChatroomServer
{
    internal class ClientData
    {
        public string Name { get; set; }
        public TcpClient Client { get; set; }
        public NetworkStream ClientStream {  get; set; }
        public bool IsConnected { get; set; }

        public ClientData(string name, TcpClient client, NetworkStream stream, int index) 
        {
            this.Name = name;
            this.Client = client;
            this.ClientStream = stream;
            this.IsConnected = false;
        }

        public ClientData(string name) { this.Name = name; }
    }
}

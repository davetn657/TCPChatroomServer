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
        public int Index { get; set; }
        public bool IsConnected { get; set; }

        public ClientData(string name, TcpClient client, NetworkStream stream, int index) 
        {
            this.Name = name;
            this.Client = client;
            this.ClientStream = stream;
            this.Index = index;
            this.IsConnected = false;
        }

        public void DisconnectClient()
        {
            //given the id (index number) disconnect that clients stream, remove from the servers clients, and remove from the connectedClients array
            //for every other element in the connectedClient, move them back one index
            //remember to increment numConnectedClients down one
        }
    }
}

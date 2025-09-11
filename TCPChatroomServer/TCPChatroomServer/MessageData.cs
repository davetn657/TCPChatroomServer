using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace TCPChatroomServer
{
    internal class MessageData
    {
        //messagedata needs:
        //  FROM: who the message is from
        //  ID: The ID of who the message is from (this will be null on the first sent message)
        //  MESSAGE: the contents of what the user sent
        public ClientData from { get; set; }
        public string message { get; set; }
        public string messageType { get; set; }

        public MessageData()
        {
            this.from = new ClientData();
            this.message = string.Empty;
            this.messageType = string.Empty;
        }

        public MessageData(string messageType, ClientData from, string message)
        {
            this.from = from;
            this.message = message;
            this.messageType = messageType;
        }

        public byte[] Serialize()
        {
            string jsonSerializer = JsonSerializer.Serialize(this);
            byte[] messageSerialized = Encoding.UTF8.GetBytes(jsonSerializer);
            return messageSerialized;
        }

        public MessageData Deserialize(byte[] data, Int32 bytes)
        {
            try
            {
                string byteDeserializer = Encoding.UTF8.GetString(data, 0, bytes);
                MessageData jsonDeserializer = JsonSerializer.Deserialize<MessageData>(byteDeserializer);
                return jsonDeserializer;
            }
            catch (NullReferenceException e)
            {
                //return a could not "deserialize message" message
                Debug.WriteLine($"Error: Message was null\n{e}");
                return new MessageData();
            }
        }
    }
}

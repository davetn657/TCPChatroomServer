using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TCPChatroomServer
{
    internal class MessageData
    {
        //messagedata needs:
        //  FROM: who the message is from
        //  ID: The ID of who the message is from (this will be null on the first sent message)
        //  MESSAGE: the contents of what the user sent
        public string From {  get; set; }
        public string ID { get; set; }
        public string Message { get; set; }

        public MessageData()
        {
            this.From = string.Empty;
            this.ID = string.Empty;
            this.Message = string.Empty;
        }

        public MessageData(string from, string message)
        {
            this.From = from;
            this.ID = ID;
            this.Message = message;
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

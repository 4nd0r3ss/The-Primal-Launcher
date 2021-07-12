using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimalLauncher
{   
    public class MessagePacket : GamePacket
    {       
        public string Message { get; set; }
        public uint TargetId { get; set; }
        public MessageType MessageType { get; set; }
        public string Sender { get; set; } = ""; //always empty so far.

        public MessagePacket()
        {
            //Opcode = 0x0003;
        }

        public void ProcessData()
        {
            byte[] data = new byte[0x228];
            int strSenderSize = Encoding.ASCII.GetByteCount(Sender);
            int strMessageSize = Encoding.ASCII.GetByteCount(Message);

            Buffer.BlockCopy(Encoding.ASCII.GetBytes(Sender), 0, data, 0, strSenderSize >= 0x20 ? 0x20 : strSenderSize);
            Buffer.BlockCopy(BitConverter.GetBytes((uint)MessageType), 0, data, 0x20, 0x04);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(Message), 0, data, 0x24, strMessageSize >= 0x200 ? 0x200 : strMessageSize);       

            Data = data; //put processed mesage data in base class Data property.
        }
    }
}

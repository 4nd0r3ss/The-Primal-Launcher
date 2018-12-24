using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.packets
{
    public class GamePacket
    {
        byte[] _data;
        public ushort Opcode { get; set; }
        public byte[] TimeStamp { get; set; }
        public byte[] Data
        {
            get => _data;
            set
            {
                _data = value;
                Size = (ushort)(0x10 + _data.Length); //0x10 = header size
            }
        }
        public ushort Size { get; set; }

        #region Constructors
        public GamePacket(byte[] packet) => GamePacketSetup(packet);

        public GamePacket() { }
        #endregion

        private void GamePacketSetup(byte[] packet)
        {
            Opcode = (ushort)(packet[2] << 8 | packet[3]);
            TimeStamp = new byte[] { packet[0x8], packet[0x9], packet[0xa], packet[0xb] };

            byte[] data = new byte[packet.Length - 16];
            Buffer.BlockCopy(packet, packet.Length + 16, data, 0, data.Length);

            Data = data;
        }
                       
        public byte[] ToBytes(Blowfish blowfish)
        {
            byte[] toBytes = new byte[Data.Length + 0x10];
            byte[] header = new byte[0x10];

            header[0x00] = 0x14;
            Buffer.BlockCopy(BitConverter.GetBytes(Opcode), 0, header, 0x02, 0x02);            
            Buffer.BlockCopy(Server.GetTimeStampHex(), 0, header, 0x08, 0x04);

            Buffer.BlockCopy(header, 0, toBytes, 0, header.Length);
            Buffer.BlockCopy(Data, 0, toBytes, 0x10, Data.Length);

            if(blowfish != null)
                blowfish.Encipher(toBytes, 0, toBytes.Length);

            return toBytes;
        }        
    }
}

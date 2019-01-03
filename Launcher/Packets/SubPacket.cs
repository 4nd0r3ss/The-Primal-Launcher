using System;
using System.Collections.Generic;

namespace Launcher.Packets
{
    public class SubPacket
    {
        private static readonly Log _log = Log.Instance;

        #region Properties
        public ushort Size { get; set; }
        public ushort Type { get; set; } = 0x03;
        public uint SourceId { get; set; }
        public uint TargetId { get; set; }
        public uint Unknown { get; set; }
        public byte[] Data { get; set; }
        public List<GamePacket> GamePacketList { get; set; } = new List<GamePacket>();
        #endregion

        #region Constructors
        public SubPacket() { }
        public SubPacket(GamePacket gamePacket)
        {
            Size += (ushort)(0x10 + gamePacket.Size); //0x10 = subpacketheader
            GamePacketList.Add(gamePacket);
        }
        public SubPacket(List<GamePacket> gamePacketList)
        {
            GamePacketList = gamePacketList;
            ushort subPacketSize = 0;

            foreach (GamePacket gp in GamePacketList)
                subPacketSize += gp.Size;

            Size = subPacketSize;
        }
        public SubPacket(MessagePacket messagePacket)
        {
            messagePacket.ProcessData();
            TargetId = messagePacket.TargetId;
            Size += (ushort)(0x10 + messagePacket.Size);
            GamePacketList.Add(messagePacket);            
        }        
        #endregion       

        public byte[] ToBytes(Blowfish blowfish)
        {
            //if (Size == 0 && GamePacketList.Count == 0) Size = (ushort)(0x10 + Data.Length);

            byte[] toBytes = new byte[Size];
            byte[] header = new byte[0x10];
            int index = 0x10;

            if (SourceId == 0 && Type == 0x03)
                SourceId = 0xe0006868;

            if (TargetId == 0 && Type == 0x03)
                TargetId = 0xe0006868;                    

            Buffer.BlockCopy(BitConverter.GetBytes(Size), 0, header, 0, 0x02);
            Buffer.BlockCopy(BitConverter.GetBytes(Type), 0, header, 0x02, 0x02);
            Buffer.BlockCopy(BitConverter.GetBytes(SourceId), 0, header, 0x04, 0x04);
            Buffer.BlockCopy(BitConverter.GetBytes(TargetId), 0, header, 0x08, 0x04);           

            Buffer.BlockCopy(header, 0, toBytes, 0, header.Length);

            if (GamePacketList.Count > 0)
            {              
                foreach (GamePacket gp in GamePacketList)
                {
                    Buffer.BlockCopy(gp.ToBytes(blowfish), 0, toBytes, index, gp.Size);
                    index += gp.Size;
                }
            }
            else //if there are no GamePackets, then there SHOULD be something inside Data. If both are empty, return subpacket header only.
            {
                Buffer.BlockCopy(Data, 0, toBytes, index, Data.Length);
            }           

            return toBytes;
        }
               
        public int Opcode() => (Data[0x03]) >> 8 | (Data[0x02]);

        #region Encoding
        public void Encrypt(Blowfish bf)
        {
            try { bf.Encipher(Data, 0, Data.Length); }
            catch (Exception) { _log.Error("Error encrypting subpacket!"); }
        }

        public void Decrypt(Blowfish bf)
        {
            try { bf.Decipher(Data, 0, Data.Length); }
            catch (Exception) { _log.Error("Error decrypting subpacket!"); }
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.packets
{
    public class SubPacket
    {
        private static readonly Log _log = Log.Instance;

        #region Properties
        public ushort Size { get; set; } = 0;
        public ushort Type { get; set; }
        public uint SourceId { get; set; }
        public uint TargetId { get; set; }
        public uint Unknown { get; set; } = 0x00;
        public byte[] Data { get; set; }
        public List<GamePacket> GamePacketList { get; set; } = new List<GamePacket>();
        #endregion

        #region Constructors
        public SubPacket(GamePacket gamePacket) => AddGamePacket(gamePacket);

        public SubPacket(List<GamePacket> gamePacketList)
        {
            GamePacketList = gamePacketList;
            ushort subPacketSize = 0;

            foreach (GamePacket gp in GamePacketList)
                subPacketSize += gp.Size;

            Size = (ushort)subPacketSize;
        }

        public SubPacket() { }
        #endregion

        public void AddGamePacket(GamePacket gamePacket)
        {
            Size += (ushort)(0x10 + gamePacket.Size); //0x10 = subpacketheader
            GamePacketList.Add(gamePacket);
        }

        public byte[] ToBytes(Blowfish blowfish)
        {
            byte[] toBytes = new byte[Size];
            byte[] header = new byte[0x10];
            int index = 0x10;

            if (Type == 0)
                Type = 0x03;

            if (SourceId == 0)
                SourceId = 0xe0006868;

            if (TargetId == 0)
                TargetId = 0xe0006868;

            Buffer.BlockCopy(BitConverter.GetBytes(Size), 0, header, 0, 0x02);
            Buffer.BlockCopy(BitConverter.GetBytes(Type), 0, header, 0x02, 0x02);
            Buffer.BlockCopy(BitConverter.GetBytes(SourceId), 0, header, 0x04, 0x04);
            Buffer.BlockCopy(BitConverter.GetBytes(TargetId), 0, header, 0x08, 0x04);           

            Buffer.BlockCopy(header, 0, toBytes, 0, header.Length);

            foreach (GamePacket gp in GamePacketList)
            {
                Buffer.BlockCopy(gp.ToBytes(blowfish), 0, toBytes, index, gp.Size);
                index += gp.Size;
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

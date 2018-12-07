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

        public byte[] ToBytes(ref Blowfish blowfish)
        {
            byte[] toBytes = new byte[Size];

            byte[] header = new byte[0x10];
            Buffer.BlockCopy(BitConverter.GetBytes(Size), 0, header, 0, 0x02);
            header[0x02] = 0x03;
            header[0x04] = 0x68;
            header[0x05] = 0x68;
            header[0x07] = 0xe0;
            header[0x08] = 0x68;
            header[0x09] = 0x68;
            header[0x0b] = 0xe0;

            int index = 0x10;
            Buffer.BlockCopy(header, 0, toBytes, 0, header.Length);

            foreach (GamePacket gp in GamePacketList)
            {
                Buffer.BlockCopy(gp.ToBytes(ref blowfish), 0, toBytes, index, gp.Size);
                index += gp.Size;
            }

            return toBytes;
        }

        public byte[] ToBytes()
        {
            byte[] toBytes = new byte[Size];

            byte[] header = new byte[0x10];
            Buffer.BlockCopy(BitConverter.GetBytes(Size), 0, header, 0, 0x02);
            header[0x02] = 0x03;
            header[0x04] = 0x68;
            header[0x05] = 0x68;
            header[0x07] = 0xe0;
            header[0x08] = 0x68;
            header[0x09] = 0x68;
            header[0x0b] = 0xe0;

            int index = 0x10;
            Buffer.BlockCopy(header, 0, toBytes, 0, header.Length);

            foreach (GamePacket gp in GamePacketList)
            {
                Buffer.BlockCopy(gp.ToBytes(), 0, toBytes, index, gp.Size);
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

        public void Dencrypt(Blowfish bf)
        {
            try { bf.Decipher(Data, 0, Data.Length); }
            catch (Exception) { _log.Error("Error decrypting subpacket!"); }
        }
        #endregion
    }
}

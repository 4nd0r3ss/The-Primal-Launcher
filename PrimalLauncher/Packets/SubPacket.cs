/* 
Copyright (C) 2022 Andreus Faria

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;

namespace PrimalLauncher
{
    public class SubPacket
    {           
        private byte[] _data;

        #region Properties
        private uint PlayerCharacterId { get; set; } = User.Instance.Character == null ? 0xe0006868 : User.Instance.Character.Id;
        public ushort Size { get; set; }
        public ushort Type { get; set; } = 0x03;
        public uint SourceId { get; set; }
        public uint TargetId { get; set; }
        public uint Unknown { get; set; }
        
        public byte[] Data
        {
            get => _data;
            set {
                Size = (ushort)(0x10 + value.Length);
                _data = value;
            }

        }
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
                SourceId = PlayerCharacterId;

            if (TargetId == 0 && Type == 0x03)
                TargetId = PlayerCharacterId;                    

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

        public string Stringify(bool isFromClient)
        {
            string result = "";

            foreach (GamePacket gp in GamePacketList)
                result += gp.Stringify(isFromClient, SourceId, TargetId);

            return result;
        }
               
        public int Opcode() => (Data[0x03]) >> 8 | (Data[0x02]);

        #region Encoding
        public void Encrypt(Blowfish bf)
        {
            try { bf.Encipher(Data, 0, Data.Length); }
            catch (Exception) { Log.Instance.Error("Error encrypting subpacket!"); }
        }

        public void Decrypt(Blowfish bf)
        {
            try { bf.Decipher(Data, 0, Data.Length); }
            catch (Exception) { Log.Instance.Error("Error decrypting subpacket!"); }
        }
        #endregion
    }
}

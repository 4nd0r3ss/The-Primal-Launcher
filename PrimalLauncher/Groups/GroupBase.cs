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
using System.IO;
using System.Text;

namespace PrimalLauncher
{
    /// <summary>
    /// Sends a sequence of subpackets to the game client containing information on the different types of groups a player might have.
    /// </summary>
    /// <remarks>Verify if this is mandatory at a later time.</remarks>
    [Serializable]
    public class GroupBase
    {
        protected ulong _idMask = 0x8000000000000000;
        protected byte IsOnline { get; set; } = 1;
        protected byte Flag { get; set; } //try changing values to see what happens. 
        protected GroupType Type { get; set; }
        public List<uint> MemberList { get; set; }  
        
        public ulong Id
        {
            get { return _idMask + Sequence; }
        }
        protected ulong TimeStamp { get; set; }
        protected byte Sequence { get; set; }
        //private uint[] PacketInitialBytes { get; set; } // need a better name for this...
        

        public GroupBase(byte id, GroupType type)
        {
            Sequence = id;
            Type = type;            
            MemberList = new List<uint>
            {
                User.Instance.Character.Id
            };            
        }
        
        protected byte[] GetPrepByteArray(int size)
        {
            byte[] data = new byte[size];

            using (MemoryStream stream = new MemoryStream(data))
            using(BinaryWriter bw = new BinaryWriter(stream))
            {
                bw.Seek(0, SeekOrigin.Begin);
                bw.Write((ulong)User.Instance.Character.Position.ZoneId);                
                bw.Write(TimeStamp);                
            }

            return data;
        }

        private void Header()
        {
            byte[] data = GetPrepByteArray(0x78);

            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryWriter bw = new BinaryWriter(stream))
            {
                bw.Seek(0x10, SeekOrigin.Begin);
                bw.Write((ulong)0x03);
                bw.Write(Id);
                bw.Write((ulong)0);
                bw.Write(Id);
                bw.Write((ulong)Type);
                bw.Write((ulong)0);
                bw.Write(-1);
                bw.Seek(0x60, SeekOrigin.Begin);
                bw.Write(0);

                for (int i = 0; i < 4; i++)
                    bw.Write((uint)0x6d);

                bw.Write((byte)(MemberList.Count));
            }
            
            Packet.Send(ServerOpcode.GroupHeader, data);
        }

        private void Begin()
        {
            byte[] data = GetPrepByteArray(0x20);
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0x10, sizeof(ulong));
            data[0x18] = (byte)(MemberList.Count);
            Packet.Send(ServerOpcode.GroupBegin, data);
        }

        protected virtual void Members()
        {
            byte[] data = GetPrepByteArray(0x198); //only groups of 8 allowed here. Many more are allowed in linkshell  groups for example, but I chose to limit to 8.
            Buffer.BlockCopy(BitConverter.GetBytes(User.Instance.Character.Id), 0, data, 0x10, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(-1), 0, data, 0x14, sizeof(int)); //localized name
            data[0x1c] = Flag;
            data[0x1d] = IsOnline;
            Buffer.BlockCopy(User.Instance.Character.Name, 0, data, 0x1e, User.Instance.Character.Name.Length);
            data[0x190] = (byte)(MemberList.Count); //only one member, the player.

            Packet.Send(ServerOpcode.GroupMembers, data);
        }

        private void End()
        {
            byte[] data = GetPrepByteArray(0x18);
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0x10, sizeof(ulong));
            Packet.Send(ServerOpcode.GroupEnd, data);
        }

        public virtual void InitWork()
        {
            byte[] data = new byte[0x90];

            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(Id);
                    bw.Write((ushort)0x8807);
                    bw.Write(Encoding.ASCII.GetBytes("/_init"));
                }
            }

            Packet.Send(ServerOpcode.GroupInitWork, data);
        }

        public void SendPackets(GroupType type = GroupType.None)
        {
            TimeStamp = Server.GetTimeStampHexMiliseconds();
            Header();
            Begin();
            Members();
            End();
        }              

        public virtual void AddMembers(List<Actor> membersToAdd)
        {
            foreach (Actor a in membersToAdd)
            {
                MemberList.Add(a.Id);

            }
                
        }
    }        
}

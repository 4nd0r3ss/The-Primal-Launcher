using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Launcher
{
    /// <summary>
    /// Sends a sequence of subpackets to the game client containing information on the different types of groups a player might have.
    /// </summary>
    /// <remarks>Verify if this is mandatory at a later time.</remarks>
    [Serializable]
    public class Group
    {
        private readonly ulong _idMask = 0x8000000000000000;
        private byte IsOnline { get; set; } = 1;
        private byte Flag { get; set; } //try changing values to see what happens. 
        private GroupType Type { get; set; }
        private byte NumMembers
        {
            get { return (byte)(MemeberList.Count + 1); }
        }
        protected ulong GroupId
        {
            get { return _idMask + Sequence; }
        }

        private ulong Code { get; set; }

        public byte Sequence { get; set; }                       
        public List<Actor> MemeberList { get; set; }

        public Group(ulong code, byte id, GroupType type)
        {
            Sequence = id;
            Type = type;
            Code = code;
            MemeberList = new List<Actor>();
        }
        
        private byte[] GetPrepByteArray(int size)
        {
            byte[] data = new byte[size];
            Buffer.BlockCopy(BitConverter.GetBytes(User.Instance.Character.Position.ZoneId), 0, data, 0, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(Code), 0, data, 0x08, sizeof(ulong));
            return data;
        }

        private void Header(Socket sender)
        {
            byte[] data = GetPrepByteArray(0x78);
            data[0x10] = 0x03; //unknown
            Buffer.BlockCopy(BitConverter.GetBytes(GroupId), 0, data, 0x18, sizeof(ulong));
            Buffer.BlockCopy(BitConverter.GetBytes(GroupId), 0, data, 0x28, sizeof(ulong));
            Buffer.BlockCopy(BitConverter.GetBytes((uint)Type), 0, data, 0x30, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(-1), 0, data, 0x40, sizeof(int));
            byte index = 0x60;
            for (int i = 0; i < 4; i++)
                Buffer.BlockCopy(BitConverter.GetBytes(0x6d), 0, data, (index += 0x04), sizeof(uint));
            data[0x74] = NumMembers;

            SendPacket(sender, ServerOpcode.GroupHeader, data);
        }

        private void Begin(Socket sender)
        {
            byte[] data = GetPrepByteArray(0x20);
            Buffer.BlockCopy(BitConverter.GetBytes(GroupId), 0, data, 0x10, sizeof(ulong));
            data[0x18] = NumMembers;
            SendPacket(sender, ServerOpcode.GroupBegin, data);
        }

        private void Members(Socket sender)
        {
            byte[] data = GetPrepByteArray(0x198); //only groups of 8 allowed here. Many more are allowed in linkshell  groups for example, but I chose to limit to 8.
            Buffer.BlockCopy(BitConverter.GetBytes(User.Instance.Character.Id), 0, data, 0x10, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(-1), 0, data, 0x14, sizeof(int)); //localized name
            data[0x1c] = Flag;
            data[0x1d] = IsOnline;
            Buffer.BlockCopy(User.Instance.Character.Name, 0, data, 0x1e, User.Instance.Character.Name.Length);
            data[0x190] = NumMembers; //only one member, the player.

            SendPacket(sender, ServerOpcode.GroupMembers, data);
        }

        private void End(Socket sender)
        {
            byte[] data = GetPrepByteArray(0x18);
            Buffer.BlockCopy(BitConverter.GetBytes(GroupId), 0, data, 0x10, sizeof(ulong));
            SendPacket(sender, ServerOpcode.GroupEnd, data);
        }

        public void SendPackets(Socket sender, GroupType type = GroupType.None)
        {
            Header(sender);
            Begin(sender);
            Members(sender);
            End(sender);
        }               

        protected void SendPacket(Socket handler, ServerOpcode opcode, byte[] data)
        {
            uint characterId = User.Instance.Character.Id;
            GamePacket gamePacket = new GamePacket
            {
                Opcode = (ushort)opcode,
                Data = data
            };

            Packet packet = new Packet(new SubPacket(gamePacket));
            handler.Send(packet.ToBytes());
        }
    }   

    [Serializable]
    public class PartyGroup : Group
    {
        public PartyGroup() : base(0x000001780d9bee40, 0x01, GroupType.Party) { }

        public void InitWork(Socket sender)
        {
            byte[] data = new byte[0x90];

            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(GroupId);
                    bw.Write((uint)0x039d0814);
                    bw.Write((uint)0x6f929dce);
                    bw.Write((ushort)0x00b3);
                    bw.Write((uint)User.Instance.Character.Id);
                    bw.Write((byte)0x88);
                    bw.Write("/_init");
                }
            }
           
            SendPacket(sender, ServerOpcode.GroupInitWork, data);
        }

    }

    [Serializable]
    public class RetainerGroup : Group
    {
        public RetainerGroup() : base(0x000001780d9bee73, 0x02, GroupType.Retainer) { }

        public void InitWork(Socket sender)
        {
            byte[] data = new byte[0x90];

            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(GroupId);
                    bw.Write((ushort)0x8807);
                    bw.Write("/_init");
                }
            }
            
            SendPacket(sender, ServerOpcode.GroupInitWork, data);
        }

    }
}

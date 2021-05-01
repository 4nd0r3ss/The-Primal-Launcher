using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Launcher
{
    /// <summary>
    /// Sends a sequence of subpackets to the game client containing information on the different types of groups a player might have.
    /// </summary>
    /// <remarks>Verify if this is mandatory at a later time.</remarks>
    [Serializable]
    public class Group
    {
        protected ulong _idMask = 0x8000000000000000;
        protected byte IsOnline { get; set; } = 1;
        protected byte Flag { get; set; } //try changing values to see what happens. 
        protected GroupType Type { get; set; }
        protected byte NumMembers
        {
            get { return (byte)(TempMemberCount + 1); }
        }
        public ulong Id
        {
            get { return _idMask + Sequence; }
        }

        protected uint Code { get; set; }

        protected byte Sequence { get; set; }                       
        public List<Actor> MemeberList { get; set; }
        public int TempMemberCount { get; set; } = 0; //temporary while I dont implement members properly.

        private uint[] PacketInitialBytes { get; set; } // need a better name for this...

        public Group(uint code, byte id, GroupType type)
        {
            Sequence = id;
            Type = type;
            Code = code;
            MemeberList = new List<Actor>();

            PacketInitialBytes = new uint[]
            {
                User.Instance.Character.Position.ZoneId,
                0,
                Code,
                0x0178
            };
        }
        
        protected byte[] GetPrepByteArray(int size)
        {
            byte[] data = new byte[size];

            using (MemoryStream stream = new MemoryStream(data))
            using(BinaryWriter bw = new BinaryWriter(stream))
            {
                bw.Seek(0, SeekOrigin.Begin);
                bw.Write(User.Instance.Character.Position.ZoneId);
                bw.Write((uint)0);
                bw.Write((uint)Code);
                bw.Write((uint)0x0178);
            }

            return data;
        }

        private void Header(Socket sender)
        {
            byte[] data = GetPrepByteArray(0x78);

            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryWriter bw = new BinaryWriter(stream))
            {
                bw.Seek(0, SeekOrigin.Begin);

                foreach (uint i in PacketInitialBytes)
                    bw.Write(i);

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

                bw.Write(NumMembers);
            }
            
            SendPacket(sender, ServerOpcode.GroupHeader, data);
        }

        private void Begin(Socket sender)
        {
            byte[] data = GetPrepByteArray(0x20);
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0x10, sizeof(ulong));
            data[0x18] = NumMembers;
            SendPacket(sender, ServerOpcode.GroupBegin, data);
        }

        protected virtual void Members(Socket sender)
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
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0x10, sizeof(ulong));
            SendPacket(sender, ServerOpcode.GroupEnd, data);
        }

        public virtual void InitWork(Socket sender)
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

            SendPacket(sender, ServerOpcode.GroupInitWork, data);
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
        public PartyGroup() : base(0x0d_9b_ee_40, 0x01, GroupType.Party) { }

        public override void InitWork(Socket sender)
        {
            byte[] data = new byte[0x90];

            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(Id);
                    bw.Write((uint)0x039d0814);
                    bw.Write((uint)0x6f929dce);
                    bw.Write((ushort)0x00b3);
                    bw.Write((uint)User.Instance.Character.Id);
                    bw.Write((byte)0x88);
                    bw.Write(Encoding.ASCII.GetBytes("/_init"));
                }
            }
           
            SendPacket(sender, ServerOpcode.GroupInitWork, data);
        }

    }

    [Serializable]
    public class RetainerGroup : Group
    {
        public RetainerGroup() : base(0x0d9bee73, 0x02, GroupType.Retainer) { }       

    }

    [Serializable]
    public class DutyGroup : Group
    {
        public DutyGroup() : base(0x6FBECA9B, 0x01, GroupType.Duty)
        {
            _idMask = 0x3000000000000000;
            Sequence = 1;
            TempMemberCount = 6;
        }
        
        protected override void Members(Socket sender)
        {
            byte[] data = GetPrepByteArray(0x198); //only groups of 8 allowed here. Many more are allowed in linkshell  groups for example, but I chose to limit to 8.

            uint[] memberIds =
            {
                User.Instance.Character.Id,
                0x66080001,
                0x46080001,
                0x46080002,
                0x46080003,
                0x46080004,
                0x46080005
            };

            

            using (MemoryStream ms = new MemoryStream(data))           
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Seek(0x10, SeekOrigin.Begin);

                foreach(uint member in memberIds)
                {
                    bw.Write(member);
                    bw.Write(0x03E9);  //unknown
                    bw.Write(1);       //unknown
                }

                //empty member slot
                bw.Write(0);
                bw.Write(0);
                bw.Write(0);

                bw.Write(memberIds.Length);                
            }

            SendPacket(sender, ServerOpcode.GroupDutyMembers, data);
        }

        /// <summary>
        /// So far this was only used during the intro when changing to instanced area. It will possible be used whenever an instance starts.
        /// </summary>
        /// <param name="sender"></param>
        public void InitializeGroup(Socket sender)
        {
            byte[] work =
               {
                    0x27, 0x04, 0x89, 0xE1, 0xF9, 0x0D, 0x36, 0x75, 0x00, 0x00, 0x9F, 0x63, 0x68, 0x61, 0x72, 0x61,
                    0x57, 0x6F, 0x72, 0x6B, 0x2F, 0x63, 0x75, 0x72, 0x72, 0x65, 0x6E, 0x74, 0x43, 0x6F, 0x6E, 0x74,
                    0x65, 0x6E, 0x74, 0x47, 0x72, 0x6F, 0x75, 0x70, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                };
            SendPacket(sender, ServerOpcode.ActorInit, work);
        }

        public override void InitWork(Socket sender)
        {
            byte[] init = new byte[0x28];
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, init, 0, sizeof(long));
            Buffer.BlockCopy(Encoding.ASCII.GetBytes("/_init"), 0, init, 0x08, 6);
            SendPacket(sender, ServerOpcode.GeneralData, init);

            byte[] data = new byte[0x90];

            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(Id);
                    bw.Write((byte)0x1A); //# of bytes from here on

                    // unknow property -->
                    bw.Write((byte)0x08); //data type (0x08 == array of ints? or long?)
                    bw.Write(0x33D2BD7B); //hashed string - property name
                    bw.Write(0);          //first value
                    bw.Write(0x66080001); //director id
                    bw.Write((byte)0x01); //data type - bool
                    bw.Write(0x9627ABD8); //hashed string - property name
                    bw.Write((byte)0x01); // true
                    //<--
                    bw.Write((byte)0x88); //wrapper
                    bw.Write(Encoding.ASCII.GetBytes("/_init"));
                }
            }

            SendPacket(sender, ServerOpcode.GroupInitWork, data);
        }
    }
}

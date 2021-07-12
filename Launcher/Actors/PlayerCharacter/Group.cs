using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace PrimalLauncher
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
        public List<uint> MemberList { get; set; }        
        public ulong Id
        {
            get { return _idMask + Sequence; }
        }
        protected ulong TimeStamp { get; set; }
        protected byte Sequence { get; set; }
        //private uint[] PacketInitialBytes { get; set; } // need a better name for this...
        

        public Group(byte id, GroupType type)
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

        private void Header(Socket sender)
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
            
            Packet.Send(sender, ServerOpcode.GroupHeader, data);
        }

        private void Begin(Socket sender)
        {
            byte[] data = GetPrepByteArray(0x20);
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0x10, sizeof(ulong));
            data[0x18] = (byte)(MemberList.Count);
            Packet.Send(sender, ServerOpcode.GroupBegin, data);
        }

        protected virtual void Members(Socket sender)
        {
            byte[] data = GetPrepByteArray(0x198); //only groups of 8 allowed here. Many more are allowed in linkshell  groups for example, but I chose to limit to 8.
            Buffer.BlockCopy(BitConverter.GetBytes(User.Instance.Character.Id), 0, data, 0x10, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(-1), 0, data, 0x14, sizeof(int)); //localized name
            data[0x1c] = Flag;
            data[0x1d] = IsOnline;
            Buffer.BlockCopy(User.Instance.Character.Name, 0, data, 0x1e, User.Instance.Character.Name.Length);
            data[0x190] = (byte)(MemberList.Count); //only one member, the player.

            Packet.Send(sender, ServerOpcode.GroupMembers, data);
        }

        private void End(Socket sender)
        {
            byte[] data = GetPrepByteArray(0x18);
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0x10, sizeof(ulong));
            Packet.Send(sender, ServerOpcode.GroupEnd, data);
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

            Packet.Send(sender, ServerOpcode.GroupInitWork, data);
        }

        public void SendPackets(Socket sender, GroupType type = GroupType.None)
        {
            TimeStamp = Server.GetTimeStampHexMiliseconds();
            Header(sender);
            Begin(sender);
            Members(sender);
            End(sender);
        }              

        public void AddMembers(List<Actor> membersToAdd)
        {
            foreach (Actor a in membersToAdd)
                MemberList.Add(a.Id);
        }
    }   

    [Serializable]
    public class PartyGroup : Group
    {
        public PartyGroup() : base(0x01, GroupType.Party) { }       

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
           
            Packet.Send(sender, ServerOpcode.GroupInitWork, data);
        }

    }

    [Serializable]
    public class RetainerGroup : Group
    {
        public RetainerGroup() : base(0x02, GroupType.Retainer) { }
    }

    [Serializable]
    public class DutyGroup : Group
    {
        public DutyGroup() : base(0x01, GroupType.Duty)
        {
            _idMask = 0x3000000000000000;          
        }
        
        protected override void Members(Socket sender)
        {
            byte[] data = GetPrepByteArray(0x198); //only groups of 8 allowed here. Many more are allowed in linkshell  groups for example, but I chose to limit to 8.
                       
            using (MemoryStream ms = new MemoryStream(data))           
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Seek(0x10, SeekOrigin.Begin);

                foreach(uint member in MemberList)
                {
                    bw.Write(member);
                    bw.Write(0x03E9);  //unknown
                    bw.Write(1);       //unknown
                }

                //empty member slot
                bw.Write(0);
                bw.Write(0);
                bw.Write(0);

                bw.Write((byte)(MemberList.Count));                
            }

            Packet.Send(sender, ServerOpcode.GroupDutyMembers, data);
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
            Packet.Send(sender, ServerOpcode.ActorInit, work);
        }

        public override void InitWork(Socket sender)
        {
            byte[] init = new byte[0x28];
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, init, 0, sizeof(long));
            Buffer.BlockCopy(Encoding.ASCII.GetBytes("/_init"), 0, init, 0x08, 6);
            Packet.Send(sender, ServerOpcode.GeneralData, init);

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
                    bw.Write(World.Instance.GetDirector("Quest").Id); //director id
                    bw.Write((byte)0x01); //data type - bool
                    bw.Write(0x9627ABD8); //hashed string - property name
                    bw.Write((byte)0x01); // true
                    //<--
                    bw.Write((byte)0x88); //wrapper
                    bw.Write(Encoding.ASCII.GetBytes("/_init"));
                }
            }

            Packet.Send(sender, ServerOpcode.GroupInitWork, data);
        }
    }
}

using System;
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
        public byte NumMembers { get; set; } = 0x01;
        public ulong GroupId { get; set; } = 0x8000000000000001;        
        public byte Flag { get; set; } //try changing values to see what happens.
        public byte IsOnline { get; set; } = 1;        

        public PlayerCharacter Character { get; set; }
        public Socket Sender { get; set; }

        /// <summary>
        /// Keeps all the opcodes used for group packet generation. Each member indicates a different type of packet.
        /// </summary>
        [Serializable]
        enum Opcode
        {
            Header = 0x017c,
            Begin = 0x017d,
            End = 0x017e,
            Members = 0x017f,
            Occupancy = 0x0187,
            Sync = 0x1020, //check this
            Linkshell = 0x18a
        }

        /// <summary>
        /// Keeps the hex IDs of the differente types of groups.
        /// </summary>
        [Serializable]
        enum Type
        {
            None = 0,
            Retainer = 0x013881,
            Party = 0x002711,
            Linkshell = 0x004e22,
        }        

        /// <summary>
        /// Writes group packet data according to the parameters.
        /// </summary> 
        /// <param name="opcode">Which type of packet must be returned.</param>       
        /// <param name="type" >The type of group the packet refer to.</param>       
        private GamePacket WriteGamePacket(Opcode opcode, Type type = 0)
        {
            byte[] data = null;
            byte[] tsMilliseconds = BitConverter.GetBytes((ulong)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);
            GamePacket gamePacket = new GamePacket{ Opcode = (ushort)opcode };

            switch (opcode)
            {
                case Opcode.Header:
                    data = new byte[0x78];
                    data[0x10] = 0x03; //unknown
                    Buffer.BlockCopy(BitConverter.GetBytes(GroupId), 0, data, 0x18, sizeof(ulong));
                    Buffer.BlockCopy(BitConverter.GetBytes(GroupId), 0, data, 0x28, sizeof(ulong));
                    Buffer.BlockCopy(BitConverter.GetBytes((uint)type), 0, data, 0x30, sizeof(uint));
                    Buffer.BlockCopy(BitConverter.GetBytes(-1), 0, data, 0x40, sizeof(int));
                    byte index = 0x60;
                    for(int i=0;i<4;i++)
                        Buffer.BlockCopy(BitConverter.GetBytes(0x6d), 0, data, (index+=0x04), sizeof(uint));                    
                    data[0x74] = NumMembers;                   
                    break;
                case Opcode.Begin:
                    data = new byte[0x20];
                    Buffer.BlockCopy(BitConverter.GetBytes(GroupId), 0, data, 0x10, sizeof(ulong));
                    data[0x18] = NumMembers;                    
                    break;
                case Opcode.Members:
                    data = new byte[0x198]; //only groups of 8 allowed here. Many more are allowed in linkshell  groups for example, but I chose to limit to 8.
                    Buffer.BlockCopy(BitConverter.GetBytes(Character.Id), 0, data, 0x10, sizeof(uint));
                    Buffer.BlockCopy(BitConverter.GetBytes(-1), 0, data, 0x14, sizeof(int)); //localized name
                    data[0x1c] = Flag;
                    data[0x1d] = IsOnline;
                    Buffer.BlockCopy(Character.CharacterName, 0, data, 0x1e, Character.CharacterName.Length);
                    data[0x190] = NumMembers; //only one member, the player.
                    break;
                case Opcode.End:
                    data = new byte[0x18];
                    Buffer.BlockCopy(BitConverter.GetBytes(GroupId), 0, data, 0x10, sizeof(ulong));
                    break;                            
            }

            //same for all            
            Buffer.BlockCopy(BitConverter.GetBytes(Character.Position.ZoneId), 0, data, 0, sizeof(uint));            
            Buffer.BlockCopy(BitConverter.GetBytes(0x000001682436b1aa), 0, data, 0x08, sizeof(ulong));

            gamePacket.Data = data;

            GroupId++;

            return gamePacket;
        }

        /// <summary>
        /// Processes the group information adding avery subpacket necessary for the group sequence. 
        /// </summary>
        /// <param name="type">The type of  the group being processed.</param>       
        /// <returns>returns a packet ready for sending.</returns>
        private void SendGroupPackets(Type type = Type.None)
        {
            SendPacket(WriteGamePacket(Opcode.Header, type));  
            SendPacket(WriteGamePacket(Opcode.Begin));  
            SendPacket(WriteGamePacket(Opcode.Members));  
            SendPacket(WriteGamePacket(Opcode.End));  
        }

        public void SendPacket(GamePacket gamePacket)
        {
            SubPacket subPacket = new SubPacket(gamePacket)
            {
                SourceId = Character.Id,
                TargetId = Character.Id
            };

            Packet packet = new Packet(subPacket);
            Sender.Send(packet.ToBytes());
        }
               
        public void SendPartyPackets() => SendGroupPackets(Type.Party);
        public void SendRetainerPackets() => SendGroupPackets(Type.Retainer);

        public void SendLinkshellPackets()
        {
            byte[] data = new byte[0x78];

            Buffer.BlockCopy(BitConverter.GetBytes(0x4e22), 0, data, 0x40, sizeof(ushort));            

            GamePacket linkshell = new GamePacket
            {
                Opcode = (ushort)Opcode.Linkshell,
                Data = data
            };

            SendPacket(linkshell);            
        }


    }   
}

using Launcher.Packets;
using System;

namespace Launcher.Characters
{
    /// <summary>
    /// Keeps the hex IDs of the differente types of groups.
    /// </summary>
    [Serializable]
    enum GroupType
    {
        None = 0,
        Retainer = 0x013881,
        Party = 0x002711,
        Linkshell = 0x004e22,        
    }

    /// <summary>
    /// Keeps all the opcodes used for group packet generation. Each member indicates a different type of packet.
    /// </summary>
    [Serializable]
    enum GroupOpcode
    {
        Header = 0x017c,
        Begin = 0x017d,
        End = 0x017e,
        Members = 0x017f,
        Occupancy = 0x0187
    }

    /// <summary>
    /// Sends a sequence of subpackets to the game client containing information on the different types of groups a player might have.
    /// </summary>
    /// <remarks>Verify if this is mandatory at a later time.</remarks>
    [Serializable]
    public class Group
    {
        /// <summary>
        /// 
        /// </summary>
        public byte NumMembers { get; set; } = 0x01;
        public ulong GroupId { get; set; } = 0x800000000064500d;        
        public byte Flag { get; set; } //try changing values to see what happens.
        public byte IsOnline { get; set; } = 1;
        private ulong Unknown { get; set; } = 0x00001534a046b219; //tried timestamp in milliseconds here, but game client cracshes. it seems like the first 2 bytes should be zeros. try different numbers.

        public PlayerCharacter Character { get; set; }
        public uint ActorId { get; set; }

        /// <summary>
        /// Returns a subpacket that sinalizes that a set of group-related subpackets will be sent right after.
        /// </summary>
        /// <param name="groupType">The type of packet to be written.</param> 
        ///<param name="zoneId">The zone to which the packet will be sent to.</param>
        private GamePacket Header(uint zoneId, GroupType groupType) => WritePacket(GroupOpcode.Header, zoneId, groupType: groupType);

        /// <summary>
        /// Returns a subpacket that sinalizes the beginning of a player group list.
        /// </summary> 
        /// <param name="zoneId">The zone to which the packet will be sent to.</param>
        private GamePacket Begin(uint zoneId) => WritePacket(GroupOpcode.Begin, zoneId);

        /// <summary>
        /// Returns a subpacket that contains a list containing the group's members.
        /// </summary> 
        /// <param name="zoneId">The zone to which the packet will be sent to.</param>
        private GamePacket Members(uint zoneId, uint actorId, byte[] characterName) => WritePacket(GroupOpcode.Members, zoneId, actorId: actorId, characterName: characterName);

        /// <summary>
        /// Returns a subpacket that sinalizes the end of a player group list.
        /// </summary> 
        /// <param name="zoneId">The zone to which the packet will be sent to.</param>
        private GamePacket End(uint zoneId) => WritePacket(GroupOpcode.End, zoneId);

        /// <summary>
        /// Writes group packet data according to the parameters.
        /// </summary> 
        /// <param name="opcode">Which type of packet must be returned.</param>
        /// <param name="zoneId">The zone to which the packet will be sent to.</param>
        /// <param name="groupType" >The type of group the packet refer to.</param>
        /// <param name="actorId">The actor ID added to every packet.</param>
        /// <param name="characterName">A byte array contianing the urrent characters' name.</param>
        private GamePacket WritePacket(GroupOpcode opcode, uint zoneId, GroupType groupType = 0, uint actorId = 0, byte[] characterName = null)
        {
            byte[] data = null;
            byte[] tsMilliseconds = BitConverter.GetBytes((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);
            GamePacket gamePacket = new GamePacket{ Opcode = (ushort)opcode };

            switch (opcode)
            {
                case GroupOpcode.Header:
                    data = new byte[0x78];
                    Buffer.BlockCopy(BitConverter.GetBytes(GroupId), 0, data, 0x28, sizeof(ulong));
                    Buffer.BlockCopy(BitConverter.GetBytes((uint)groupType), 0, data, 0x30, sizeof(uint));
                    Buffer.BlockCopy(BitConverter.GetBytes(-1), 0, data, 0x40, sizeof(int));
                    data[0x74] = NumMembers;                   
                    break;
                case GroupOpcode.Begin:
                    data = new byte[0x20];
                    Buffer.BlockCopy(BitConverter.GetBytes(GroupId), 0, data, 0x10, sizeof(ulong));
                    data[0x18] = NumMembers;                    
                    break;
                case GroupOpcode.Members:
                    data = new byte[0x198]; //only groups of 8 allowed here. Many more are allowed in linkshell  groups for example, but I chose to limit to 8.
                    Buffer.BlockCopy(BitConverter.GetBytes(actorId), 0, data, 0x10, sizeof(uint));
                    Buffer.BlockCopy(BitConverter.GetBytes((long)-1), 0, data, 0x14, sizeof(long)); //localized name
                    data[0x1c] = Flag;
                    data[0x1d] = IsOnline;
                    Buffer.BlockCopy(characterName, 0, data, 0x1e, characterName.Length);
                    data[0x190] = NumMembers; //only one member, the player.
                    break;
                case GroupOpcode.End:
                    data = new byte[0x18];
                    Buffer.BlockCopy(BitConverter.GetBytes(GroupId), 0, data, 0x10, sizeof(ulong));
                    break;                            
            }

            //same for all            
            Buffer.BlockCopy(BitConverter.GetBytes(zoneId), 0, data, 0, sizeof(uint));            
            Buffer.BlockCopy(BitConverter.GetBytes(Unknown), 0, data, 0x08, sizeof(ulong));

            gamePacket.Data = data;

            return gamePacket;
        }

        /// <summary>
        /// Processes the group information adding avery subpacket necessary for the group sequence. 
        /// </summary>
        /// <param name="groupType">The type of  the group being processed.</param>
        /// <param name="character">An instance of the PlayerCharacter object containing the current character used by the player.</param>
        /// <param name="actorId">The number of the instantiated player achracter actor.</param>
        /// <returns>returns a packet ready for sending.</returns>
        private Packet GetGroupPacket(PlayerCharacter character, uint actorId, GroupType groupType = GroupType.None)
        {
            Packet packet = new Packet{ ConnType = 0x01 };

            packet.AddSubPacket(new SubPacket(Header(character.Position.ZoneId, groupType)) { SourceId = actorId, TargetId = actorId });
            packet.AddSubPacket(new SubPacket(Begin(character.Position.ZoneId)) { SourceId = actorId, TargetId = actorId });
            packet.AddSubPacket(new SubPacket(Members(character.Position.ZoneId, actorId, character.Name)) { SourceId = actorId, TargetId = actorId });
            packet.AddSubPacket(new SubPacket(End(character.Position.ZoneId)) { SourceId = actorId, TargetId = actorId });

            return packet;
        }

        public Packet GetPartyPacket() => GetGroupPacket(Character, ActorId, GroupType.Party);
        public Packet GetRetainerPacket() => GetGroupPacket(Character, ActorId, GroupType.Retainer);
        public Packet GetLinkshellPacket() => GetGroupPacket(Character, ActorId, GroupType.Linkshell);

        public Packet GetOccupancyPacket()
        {
            byte[] data = new byte[0x40];

            Buffer.BlockCopy(BitConverter.GetBytes(GroupId), 0, data, 0, sizeof(ulong));

        }

    }
}

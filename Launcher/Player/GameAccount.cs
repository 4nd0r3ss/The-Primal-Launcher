
using System;
using System.Collections.Generic;

namespace Launcher
{
    [Serializable]
    public class GameAccount
    {
        #region Packet build        
        public static readonly ushort OPCODE = 0x0C;
        public static readonly byte MAX_SLOTS = 0x08;
        public static readonly ushort SLOT_SIZE = 0x40;
        #endregion

        #region Properties
        public int Id { get; set; }
        public string Name { get; set; }
        public List<PlayerCharacter> CharacterList { get; set; } = new List<PlayerCharacter>();
        #endregion
    }
}

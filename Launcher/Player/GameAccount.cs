using System;
using System.Collections.Generic;

namespace Launcher.Characters
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

        public byte[] GetCharacterListInfo()
        {
            byte[] characterData = new byte[(PlayerCharacter.SLOT_SIZE * PlayerCharacter.MAX_SLOTS) + 0x10];
            characterData[0x08] = 0x02;
            characterData[0x09] = 0x02;
            for(int i=0;i< PlayerCharacter.MAX_SLOTS; i++)
            {
                characterData[(PlayerCharacter.SLOT_SIZE * i) + 0x10 + 0x08] = (byte)i;
            }

            return characterData;
        }
    }
}

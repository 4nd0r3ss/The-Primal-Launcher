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
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace PrimalLauncher
{
    [Serializable]
    public class GameAccount
    {        
        public const ushort SlotSize = 0x40;        

        #region Game account data
        public int Id { get; set; }
        public string Name { get; set; }
        public Dictionary<byte, object> Characters { get; set; }
        #endregion

        #region Temp data
        public byte SelectedCharacterSlot { get; set; }
        #endregion

        public PlayerCharacter SelectedCharacter 
        {
            get { return (PlayerCharacter)Characters[SelectedCharacterSlot]; }
            set { Characters[SelectedCharacterSlot] = value; }     
        }
        
        public GameAccount()
        {            
            Characters = new Dictionary<byte, object> 
            {
                {0, null},
                {1, null},
                {2, null},
                {3, null},
                {4, null},
                {5, null},
                {6, null},
                {7, null},
            };
        }

        private byte[] GetCharacterListSlot(byte slotNum)
        {
            var character = Characters[slotNum];
            byte[] characterData = new byte[0x1D0];

            if (character != null)
                characterData = ((PlayerCharacter)Characters[slotNum]).ToLobbyData();

            characterData.Write(new Dictionary<int, object>
                {
                    {0x08, slotNum},
                    {0x09, Preferences.Instance.Options.LobbyOption}
                });

            return characterData;
        }

        public byte[] ReserveName(byte[] data, byte worldId)
        {            
            PlayerCharacter newChar = new PlayerCharacter
            {                
                Id = NewCharacterId(),
                Name = data.GetSubset(0x24, 0x20),
                WorldId = worldId
            };

            Characters[SelectedCharacterSlot] = newChar;

            Log.Instance.Success("Character name reserved.");

            return newChar.Name;
        }

        public void RenameCharacter(byte[] data)
        {
            byte[] newName = data.GetSubset(0x24, 0x20);
            ((PlayerCharacter)Characters[data[0x20]]).Name = newName;
            User.Instance.Save();
            Log.Instance.Success("Character renamed.");
        }

        public PlayerCharacter GetCharacterById(uint id)
        {
            return (PlayerCharacter)Characters.Where(x => x.Value != null && ((PlayerCharacter)x.Value).Id == id).FirstOrDefault().Value;
        }

        public List<byte[]> GetCharacters()
        {
            List<byte[]> result = new List<byte[]>();
            int numChars = 8;
            decimal charMod = numChars % 2;
            int numPackets = (int)(numChars == 0 ? 1 : numChars / 2 + charMod);
            //byte charsInPacket = (byte)(charMod > 0 ? 1 : 2); //leaving this here as a reminder for future improvements if we want to make the number of char slots dynamic.
            int slotIndex = -1;

            for (int i = 0; i < numPackets; i++)
            {
                byte[] packetData = new byte[0x3B0];
                int packetIndex = i * 4;

                if (i == numPackets - 1) packetIndex++;

                packetData.Write(new Dictionary<int, object>
                {
                    {0x08, (byte)packetIndex},
                    {0x09, 2}, //as we are opening all slots from the beginning, we will always have 2 slots per packet.
                    {0x10, GetCharacterListSlot((byte)++slotIndex) },
                    {0x01E0, GetCharacterListSlot((byte)++slotIndex) }
                });      

                result.Add(packetData);
            }

            return result;
        }

        public uint NewCharacterId()
        {
            Random rnd = new Random();
            return (uint)rnd.Next(0xff, 0xffff);
        }
    }
}

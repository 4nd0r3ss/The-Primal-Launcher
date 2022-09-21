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
using System.Linq;
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
            //so far only 2 char slots allowed.
            Characters = new Dictionary<byte, object>
            {
                {0, null},
                {1, null}
            };
        }

        public void ReserveName(byte[] data, byte worldId)
        {
            SelectedCharacterSlot = data[0x20];

            Characters[SelectedCharacterSlot] = new PlayerCharacter
            {
                Name = data.GetSubset(0x24, 0x20),
                WorldId = worldId
            };

            Log.Instance.Success("Character name reserved.");
        }

        public void CreateCharacter(byte[] characterData)
        {
            SelectedCharacter.Setup(characterData);            
            User.Instance.Save();
            Log.Instance.Success("Character ID# 0x" + SelectedCharacter.Id.ToString("X") + " created!");
        }

        public void DeleteCharacter(byte slot)
        {            
            Characters[slot] = null;
            User.Instance.Save();
            Log.Instance.Success("Character deleted.");
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
            List<byte[]> packetList = new List<byte[]>();
            byte[] packetBytes = new byte[0x3B0];
            byte packetSequence = 0;

            foreach(var slot in Characters)
            {
                byte[] characterData = new byte[0x1D0];

                if (slot.Value != null)
                    characterData = ((PlayerCharacter)slot.Value).ToLobbyData();
             
                characterData.Write(new Dictionary<int, object>
                {
                    {0x08, slot.Key},
                    {0x09, Preferences.Instance.Options.LobbyOption}  
                });

                if (slot.Key > 0 && slot.Key % 2 != 0)
                {
                    packetBytes.Write(0x01E0, characterData);
                    packetSequence++;
                    packetBytes.Write(new Dictionary<int, object>
                    {
                        {0x08, packetSequence},
                        {0x09, Characters.Count}
                    });
                    packetList.Add(packetBytes);
                    packetBytes = new byte[0x3B0];
                }
                else
                {
                    packetBytes.Write(0x10, characterData);
                }
            }      

            return packetList;
        }
    }
}

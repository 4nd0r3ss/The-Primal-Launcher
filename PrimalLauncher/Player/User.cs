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
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace PrimalLauncher
{
    [Serializable]
    public class User
    {
        private const string UserFileName = @"user_data.dat";

        private static User _instance = null;

        #region User account data
        public int Id { get; set; }
        public string Uname { get; set; }
        public string Pwd { get; set; } = "";

        public Dictionary<byte, GameAccount> GameAccounts = new Dictionary<byte, GameAccount>();
        #endregion

        #region Temp data
        public uint SessionId { get; set; }
        public byte SelectedAccount { get; set; } = 0;
        #endregion
        public static User Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Load("FFXIVUser", "FFXIVUser");
                }
                return _instance;
            }
        }
        public GameAccount GameAccount
        {
            get { return GameAccounts[SelectedAccount]; }
        }
        public PlayerCharacter Character 
        { 
            get { return GameAccount.SelectedCharacter; } 
            set { GameAccount.SelectedCharacter = value; }
        }

        private User() { }      

        public byte[] GetAccountListData()
        {
            byte[] accountListData = new byte[(GameAccount.SlotSize * GameAccounts.Count) + 0x10]; //0x10 = slot list header size (fixed)
            
            //slot list header
            accountListData.Write(new Dictionary<int, object>
            {
                {0x00, 0x01},
                {0x09, (byte)GameAccounts.Count},
                {0x0a, 0x02},
                {0x0b, 0x99},
            });

            //slots data
            for (byte i = 0; i < GameAccounts.Count; i++)
            {
                GameAccount account = (GameAccount)GameAccounts[i];

                if (account.Id > 0)
                {
                    byte[] accountData = new byte[GameAccount.SlotSize];

                    accountData.Write(new Dictionary<int, object>
                    {
                        {0,    account.Id},
                        {0x04, (byte)(i + 1)},
                        {0x08, account.Name}
                    });

                    accountListData.Write(((i * GameAccount.SlotSize) + 0x10), accountData);                    
                }
            }

            return accountListData;
        }

        public static User Load(string uname, string pwd)
        {
            if (File.Exists(Preferences.Instance.Options.UserFilesPath + Preferences.AppFolder + UserFileName))
            {
                try
                {
                    using (var fileStream = new FileStream(Preferences.Instance.Options.UserFilesPath + Preferences.AppFolder + UserFileName, FileMode.Open))
                    {
                        var bFormatter = new BinaryFormatter();
                        User user = (User)bFormatter.Deserialize(fileStream);
                        Log.Instance.Success("Loaded user data.");
                        return user;
                    }
                }
                catch (Exception e) { Log.Instance.Error("There is a problem with the user file. Please try again."); throw e; }
            }
            else
            {
                User user = CreateNewUser(1, "FFXIVUser", "FFXIVUser");
                WriteFile(user);
                return Load(uname, pwd);
            }
        }

        public static User CreateNewUser(int id, string name, string pwd)
        {
            User newUser = new User()
            {
                Id = id,
                Uname = name,
                Pwd = pwd
            };

            newUser.GameAccounts.Add(0, new GameAccount()
            {
                Id = 1,
                Name = "The Primal Launcher"
            });

            return newUser;
        }

        private static void WriteFile(User user)
        {
            try //Create repository file with user list.
            {
                using (var fileStream = new FileStream(Preferences.Instance.Options.UserFilesPath + Preferences.AppFolder + UserFileName, FileMode.Create))
                {
                    var bFormatter = new BinaryFormatter();
                    try
                    {
                        bFormatter.Serialize(fileStream, user);
                    }
                    catch (Exception e) { throw e; }
                }
            }
            catch (Exception e) { Log.Instance.Error("Could not write user file. Please check app permissions."); throw e; }
        }

        public void SendUserCharacterList(Blowfish blowfish)
        {
            List<byte[]> packetList = GameAccount.GetCharacters();

            foreach (var packet in packetList)
            {
                GamePacket characterList = new GamePacket
                {
                    Opcode = 0x0d,
                    Data = packet //only one account supported so far.          
                };

                Packet characterListPacket = new Packet(characterList);
                LobbyServer.Instance.Sender.Send(characterListPacket.ToBytes(blowfish));
            }

            Log.Instance.Info("Character list sent.");
        }

        public void Save()
        {
            WriteFile(Instance);
        }

        public void SavePlayerCharacter(PlayerCharacter playerCharacter = null)
        {
            if (playerCharacter == null)
                playerCharacter = Character;

            GameAccount.SelectedCharacter = playerCharacter;
            Save();
        }

        public void SendAccountList()
        {
            Packet packet = new Packet(new GamePacket(0x0C, GetAccountListData()));
            LobbyServer.Instance.Sender.Send(packet.ToBytes(LobbyServer.Instance.Blowfish));
            Log.Instance.Info("Account list sent.");
        }
    }
}

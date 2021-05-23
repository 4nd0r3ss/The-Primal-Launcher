using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    [Serializable]
    public class User
    {
        private const string USER_FILE = @"user_data.dat";

        private static User _instance = null;
        public int Id { get; set; }
        public string Uname { get; set; }
        public string Pwd { get; set; } = "";
        public List<GameAccount> AccountList { get; set; } = new List<GameAccount>();
        private byte NumAccounts => (byte)AccountList.Count;
        public uint SessionId { get; set; }
        public PlayerCharacter Character { get; set; }

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

        public byte[] GetAccountListData()
        {
            byte[] accountListData = new byte[(GameAccount.SLOT_SIZE * NumAccounts) + 0x10]; //0x10 = slot list header size (fixed)
            //slot list header
            accountListData[0x00] = 0x01; //unknown       
            accountListData[0x09] = (byte)NumAccounts;
            accountListData[0x0a] = 0x02; //unknown
            accountListData[0x0b] = 0x99; //unknown

            //slots data
            for (int i = 0; i < NumAccounts; i++)
            {
                if (AccountList[i].Id > 0) //if the account is valid
                {
                    byte[] account = new byte[GameAccount.SLOT_SIZE];

                    byte[] name = Encoding.ASCII.GetBytes(AccountList[i].Name);
                    Buffer.BlockCopy(BitConverter.GetBytes((uint)AccountList[i].Id), 0, account, 0x00, 0x04);
                    account[0x04] = (byte)(i + 1);
                    Buffer.BlockCopy(name, 0, account, 0x08, name.Length);

                    Buffer.BlockCopy(account, 0, accountListData, ((i * GameAccount.SLOT_SIZE) + 0x10), account.Length);
                }
            }
            return accountListData;
        }

        public byte[] GetCharacters(int accountId)
        {
            int numSlots = 2;
            ushort characterSlotSize = 0x1d0;
            List<PlayerCharacter> characterList = AccountList[accountId].CharacterList;
            byte[] result = new byte[(characterSlotSize * numSlots) + 0x10];

            result[0x08] = 0x01; //list sequence (?)
            result[0x09] = (byte)numSlots; //no logic for 8 character slots yet, 2 only.

            for (int i = 0; i < numSlots/*characterList.Count*/; i++)
            {
                byte[] characterSlot = new byte[characterSlotSize];
                characterSlot[0x08] = (byte)i;
                characterSlot[0x09] = 0x00; //0X01 = inactive, 0x02 = rename char, 0x08 = legacy                

                if (characterList.Count >= (i + 1))
                {
                    PlayerCharacter character = characterList.Find(x => x.Slot == i);

                    if(character != null)
                    {
                        Zone currentZone = character.GetCurrentZone();

                        Buffer.BlockCopy(BitConverter.GetBytes(currentZone is PrivateArea ? ((PrivateArea)currentZone).ParentZoneId : currentZone.Id), 0, characterSlot, 0xc, sizeof(uint));

                        byte[] name = Encoding.ASCII.GetBytes(Encoding.ASCII.GetString(character.Name).Trim(new[] { '\0' }));
                        byte[] gearSet = character.Appearance.ToBytes();
                        byte[] worldName = GameServer.GetNameBytes(character.WorldId); // WorldFactory.GetWorld(character.WorldId).Name);
                        Job currentClass = character.Jobs[character.CurrentClassId];

                        Buffer.BlockCopy(BitConverter.GetBytes(character.Id), 0, characterSlot, 0x04, 0x04); //sequence?                    
                        Buffer.BlockCopy(name, 0, characterSlot, 0x10, name.Length);
                        Buffer.BlockCopy(worldName, 0, characterSlot, 0x30, worldName.Length);

                        //Base64 info
                        byte[] base64Info = new byte[0x100];

                        using (MemoryStream ms = new MemoryStream(base64Info))
                        {
                            using (BinaryWriter bw = new BinaryWriter(ms))
                            {
                                bw.Write((uint)0x000004c0); //??
                                bw.Write((uint)0x232327ea); //??
                                bw.Write((uint)name.Length + 0x01);
                                bw.Write(name);
                                bw.Write((byte)0); //name end byte
                                bw.Write((ulong)0x040000001c); //??                           
                                bw.Write(character.Appearance.BaseModel);
                                bw.Write(character.Appearance.Size);
                                bw.Write(character.SkinColor | (uint)(character.HairColor << 10) | (uint)(character.EyeColor << 20));
                                bw.Write(BitField.PrimitiveConversion.ToUInt32(character.Face));
                                bw.Write(character.HairHighlightColor | (uint)(character.HairStyle << 10) | character.Face.CharacteristicsColor << 20);
                                bw.Write(character.Appearance.Voice);
                                bw.Write(gearSet);
                                bw.Write((ulong)0);
                                bw.Write((uint)0x01);
                                bw.Write((uint)0x01);
                                bw.Write(currentClass.Id);
                                bw.Write(currentClass.Level);
                                bw.Write(character.CurrentJobId);
                                bw.Write((ushort)0x01); //Job level?
                                bw.Write(character.Tribe);
                                bw.Write(0xe22222aa); //??
                                bw.Write(0x0000000a); //size of the string below
                                bw.Write(Encoding.ASCII.GetBytes("prv0Inn01\0")); //figure out if this can change
                                bw.Write(0x00000011); //size of the string below
                                bw.Write(Encoding.ASCII.GetBytes("defaultTerritory\0")); //figure out if this can change
                                bw.Write(character.Guardian);
                                bw.Write(character.BirthMonth);
                                bw.Write(character.BirthDay);
                                bw.Write((ushort)0x17); //??
                                bw.Write((uint)0x04); //??
                                bw.Write((uint)0x04); //??
                                bw.Seek(0x10, SeekOrigin.Current);
                                bw.Write(character.InitialTown);
                                bw.Write(character.InitialTown);
                            }

                            base64Info = Encoding.ASCII.GetBytes(Convert.ToBase64String(base64Info).Replace('+', '-').Replace('/', '_'));
                        }
                        //Write encoded character info into the character slot byte array.
                        Buffer.BlockCopy(base64Info, 0, characterSlot, 0x40, base64Info.Length);
                    }                   
                }
                //Write character slot into slot list.
                Buffer.BlockCopy(characterSlot, 0, result, ((characterSlotSize * i) + 0x10), characterSlot.Length);
            }
            //File.WriteAllBytes("getChracaters.txt", result);
            return result;
        }

        public static User Load(string uname, string pwd)
        {           
            if (File.Exists(Preferences.Instance.Options.UserFilesPath + Preferences.AppFolder + USER_FILE))
            {
                try
                {         
                    using (var fileStream = new FileStream(Preferences.Instance.Options.UserFilesPath + Preferences.AppFolder + USER_FILE, FileMode.Open))
                    {
                        var bFormatter = new BinaryFormatter();                       
                        User user = (User)bFormatter.Deserialize(fileStream);
                        Log.Instance.Success("Loaded saved user.");
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

            newUser.AccountList.Add(new GameAccount()
            {
                Id = 1,
                Name = "FFXIV1.0 Primal Launcher"
            });

            return newUser;
        }

        private static void WriteFile(User user)
        {
            try //Create repository file with user list.
            {
                using (var fileStream = new FileStream(Preferences.Instance.Options.UserFilesPath + Preferences.AppFolder + USER_FILE, FileMode.Create))
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

        public void SendUserCharacterList(Socket handler, Blowfish blowfish)
        {
            GamePacket characterList = new GamePacket
            {
                Opcode = 0x0d,
                Data = GetCharacters(0) //only one account supported so far.          
            };

            Packet characterListPacket = new Packet(characterList);
            handler.Send(characterListPacket.ToBytes(blowfish));
            Log.Instance.Info("Character list sent.");
        }

        public void Save()
        {
            WriteFile(Instance);
        }       
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Linq;

namespace Launcher
{
    [Serializable]
    public class PlayerCharacter : Actor
    {
        #region Info        
        public byte[] CharacterName { get; set; } = new byte[0x20];
        public byte WorldId { get; set; }
        public byte Slot { get; set; }
        #endregion       

        #region Background
        public byte Tribe { get; set; }
        public byte Guardian { get; set; }
        public byte BirthMonth { get; set; }
        public byte BirthDay { get; set; }
        public uint InitialTown { get; set; }       
        #endregion        

        #region Class/Job
        public byte CurrentJobId { get; set; }
        public byte CurrentJob { get; set; } //specialized job?  
        #endregion
        
        public Inventory Inventory { get; set; }
        public Dictionary<byte, Job> Jobs { get; set; } = Job.LoadAll();
        public OrderedDictionary GeneralParameters { get; set; } = new OrderedDictionary();

        public Queue<byte[]> PacketQueue { get; set; }

        public void Setup(byte[] data)
        {
            //Character ID
            Id = NewId();

            //prepare packet info for decoding
            byte[] info = new byte[0x90];
            Buffer.BlockCopy(data, 0x30, info, 0, info.Length);
            string tmp = Encoding.ASCII.GetString(info).Trim(new[] { '\0' }).Replace('-', '+').Replace('_', '/');

            //decoded packet info
            data = Convert.FromBase64String(tmp);            

            //General
            Size = data[0x09];
            Voice = data[0x26];
            SkinColor = (ushort)(data[0x23] >> 8 | data[0x22]);

            //Head
            HairStyle = (ushort)(data[0x0b] >> 8 | data[0x0a]);
            HairColor = (ushort)(data[0x1d] >> 8 | data[0x1c]);
            HairHighlightColor = data[0x0c];
            HairVariation = data[0x0d];
            EyeColor = (ushort)(data[0x25] >> 8 | data[0x24]);

            //Face
            Face = new Face
            {
                Characteristics = data[0x0f],
                CharacteristicsColor = data[0x10],
                Type = data[0x0e],
                Ears = data[0x1b],
                Mouth = data[0x1a],
                Features = data[0x19],
                Nose = data[0x18],
                EyeShape = data[0x17],
                IrisSize = data[0x16],
                EyeBrows = data[0x15],
                Unknown = 0
            };

            //Background
            Guardian = data[0x27];
            BirthMonth = data[0x28];
            BirthDay = data[0x29];
            InitialTown = data[0x48];
            Tribe = data[0x08];
            GetBaseModel(Tribe);
            GeneralParameters = GeneralParameter.Get(Tribe);
            
            //Starting class
            CurrentJobId = data[0x2a];
            Jobs[CurrentJobId].Level = 50; //having a class level > 0 makes it active.
            Jobs[CurrentJobId].IsCurrent = true; //current class the player will start with.                        
            LoadInitialEquipment();  
            
            Position = Position.GetInitialPosition(InitialTown);

            //Lua
            LuaParameters = new LuaParameters
            {
                ActorName = string.Format("_pc{0:00000000}", Id),
                ClassName = "Player"
            };

            //Actor Lua parameters
            LuaParameters.Add("/Chara/Player/Player_work");
            LuaParameters.Add(true);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(true);
            LuaParameters.Add(0);
            LuaParameters.Add(false);
            LuaParameters.Add(true);
        }

        private void LoadInitialEquipment()
        {
            int equipmentSetNumber = (Tribe * 100) + CurrentJobId;            
            uint underShirtId = (uint)8040000 + Tribe;
            uint underGarmentId = (uint)8060000 + Tribe;

            DataTable defaultSet = GameData.Instance.GetGameData("boot_skillequip");
            uint[] itemGraphicIds = defaultSet.Select("id = '" + equipmentSetNumber + "'")[0].ItemArray.Select(Convert.ToUInt32).ToArray();

            GearGraphics = new GearGraphics();
            GearGraphics.SetToSlots(itemGraphicIds, underShirtId, underGarmentId);

            Inventory = new Inventory();           
            Inventory.AddDefaultItems(itemGraphicIds, underShirtId, underGarmentId);
        }

        private List<KeyValuePair<uint, string>> Commands { get; } = new List<KeyValuePair<uint, string>>
        {
            new KeyValuePair<uint, string>(0x0b, "commandForced"),
            new KeyValuePair<uint, string>(0x0a, "commandDefault"),
            new KeyValuePair<uint, string>(0x06, "commandWeak"),
            new KeyValuePair<uint, string>(0x04, "commandContent"),
            new KeyValuePair<uint, string>(0x06, "commandJudgeMode"),
            new KeyValuePair<uint, string>(0x100, "commandRequest"),
            new KeyValuePair<uint, string>(0x100, "widgetCreate"),
            new KeyValuePair<uint, string>(0x100, "macroRequest"),
        };

        private void CommandSequence(Socket handler)
        {
            foreach (var command in Commands)
            {
                byte[] data = new byte[0x28];
                Buffer.BlockCopy(BitConverter.GetBytes(command.Key), 0, data, 0, sizeof(ushort));
                Buffer.BlockCopy(Encoding.ASCII.GetBytes(command.Value), 0, data, 0x02, command.Value.Length);
                SendPacket(handler, ServerOpcode.PlayerCommand, data);
            }
        }

        public void LockTargetActor(Socket handler, uint targetId)
        {
            CurrentTarget = targetId; //store target id
            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes(CurrentTarget), 0, data, 0, sizeof(uint));           
            SendPacket(handler, ServerOpcode.SetTarget, data);         
        }

        public override void Spawn(Socket sender, ushort spawnType = 0x01, ushort isZoning = 0, ushort actorIndex = 0)
        {
            PacketQueue = null;

            //For packet creation            
            TargetId = Id;

            SendGroupPackets(sender);
            CreateActor(sender, 0x08);
            CommandSequence(sender);
            SetSpeeds(sender);
            SetPosition(sender, Position, spawnType, isZoning);
            SetAppearance(sender);
            SetName(sender, -1, CharacterName); //-1 = it's a custom name.
            SendUnknown(sender);
            SetMainState(sender, MainState.Passive, 0xbf);
            SetSubState(sender);
            SetAllStatus(sender);
            SetIcon(sender);
            SetIsZoning(sender);

            /* Grand Company packets here */
            SendPacket(sender, ServerOpcode.SetGrandCompany, new byte[] { 0x02, 0x7F, 0x0B, 0x7F, 0x00, 0x00, 0x00, 0x00 });

            //Set Player title
            SendPacket(sender, ServerOpcode.SetTitle, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

            //set current job
            SendPacket(sender, ServerOpcode.SetCurrentJob, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

            SetSpecialEventWork(sender);

            /* Chocobo mounts packet here */
            SendPacket(sender, ServerOpcode.SetChocoboName, new byte[] { 0x42, 0x6f, 0x6b, 0x6f, 0x00, 0x00, 0x00, 0x00 });
            SendPacket(sender, ServerOpcode.SetHasChocobo, new byte[] { 0x1f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });            

            GetAchievementPoints(sender);
            GetAchievementsLatest(sender);
            GetAchievementsCompleted(sender);

            ///* Set mounts packets */            

            LoadActorScript(sender, LuaParameters);

            //Send inventory
            Inventory.SendInventories(sender);

            //Send properties
            WriteProperties(sender);
        }

        public void SendGroupPackets(Socket sender)
        {
            Group group = new Group
            {
                Character = this,
                Sender = sender
            };

            group.SendPartyPackets();
            group.SendRetainerPackets();
            group.SendLinkshellPackets();
        }

        private void SetSpecialEventWork(Socket handler)
        {
            byte[] data = new byte[0x18];
            data[0x02] = 0x12;
            SendPacket(handler, ServerOpcode.SetSpecialEventWork, data);
        }

        private void GetAchievementPoints(Socket handler)
        {
            byte[] data = new byte[0x08];
            SendPacket(handler, ServerOpcode.AchievementPoints, data);
        }

        private void GetAchievementsLatest(Socket handler)
        {
            byte[] data = new byte[0x20];
            SendPacket(handler, ServerOpcode.AchievementsLatest, data);
        }

        private void GetAchievementsCompleted(Socket handler)
        {
            byte[] data = new byte[0x80];
            SendPacket(handler, ServerOpcode.AchievementsCompeted, data);
        }

        public void WriteProperties(Socket sender)
        {
            Property property = new Property(sender, Id, @"/_init");

            property.Add("charaWork.eventSave.bazaarTax", (byte)5);
            property.Add("charaWork.battleSave.potencial", 6.6f);

            for(int i = 0; i < 32; i++) 
                if(i<5 && i != 3) property.Add(string.Format("charaWork.property[{0}]", i), (byte)1);

            //Current class info
            Job currentClass = Jobs[CurrentJobId];
            property.Add("charaWork.parameterSave.hp[0]", currentClass.Hp); //always start with HP filled up
            property.Add("charaWork.parameterSave.hpMax[0]", currentClass.MaxHp);
            property.Add("charaWork.parameterSave.mp", currentClass.Mp); //always start with MP filled up
            property.Add("charaWork.parameterSave.mpMax", currentClass.MaxMp);
            property.Add("charaWork.parameterTemp.tp", currentClass.Tp);

            property.Add("charaWork.parameterSave.state_mainSkill[0]", currentClass.Id);
            property.Add("charaWork.parameterSave.state_mainSkillLevel", (short)currentClass.Level);

            //status buff/ailment timer? database.cs ln 892
            //property.Add(sender, Id, string.Format("charaWork.statusShownTime[{0}]", i), ); 

            //Write character's parameters
            for (int i = 0; i < GeneralParameters.Count; i++)
                property.Add(string.Format("charaWork.battleTemp.generalParameter[{0}]", i), GeneralParameters[i]);

            //unknown
            property.Add("charaWork.battleTemp.castGauge_speed[0]", 1.0f);
            property.Add("charaWork.battleTemp.castGauge_speed[1]", 0.25f);
            property.Add("charaWork.commandBorder", (byte)0x20);
            property.Add("charaWork.battleSave.negotiationFlag[0]", true);

            //Add character commands
            //DataTable commandTable = GameData.Instance.GetGameData("command");
            //DataRow[] commandRows = commandTable.Select("(id > 0 AND id < 23000) OR (id > 24000 AND id < 26000)"); //(id > 0 AND id < 23000) OR (id > 24000 AND id < 26000)

            //for (int i = 0; i < commandRows.Length; i++)
            //{
            //    uint commandId = (uint)commandRows[i].ItemArray[0];
            //    property.Add(string.Format("charaWork.command[{0}]", i), (0xA0F00000 | (ushort)commandId));
            //}


            //for (int i = 0; i < commandRows.Length; i++)
            //    property.Add(string.Format("charaWork.commandCategory[{0}]", i), (byte)1);


            uint[] command = new uint[64];
            
            command[0] = 0xA0F00000 | 21001;
            command[1] = 0xA0F00000 | 21001;
            command[2] = 0xA0F00000 | 21002;
            command[3] = 0xA0F00000 | 12004;
            command[4] = 0xA0F00000 | 21005;
            command[5] = 0xA0F00000 | 21006;
            command[6] = 0xA0F00000 | 21007;
            command[7] = 0xA0F00000 | 12009;
            command[8] = 0xA0F00000 | 12010;
            command[9] = 0xA0F00000 | 12005;
            command[10] = 0xA0F00000 | 12007;
            command[11] = 0xA0F00000 | 12011;
            command[12] = 0xA0F00000 | 22012;
            command[13] = 0xA0F00000 | 22013;
            command[14] = 0xA0F00000 | 29497;
            command[15] = 0xA0F00000 | 22015;
            //command[15] = 0xA0F00000 | 22001; //synthesize
            //command[16] = 0xA0F00000 | 24312; //sit
            //command[17] = 0xA0F00000 | 24220; //teleport
            //command[18] = 0xA0F00000 | 24101; //talk
            //command[19] = 0xA0F00000 | 24238; //check
                       
            //hotbar
            command[32] = 0xA0F00000 | 27150; //index 32 ~ 61 - hotbar
            

            //????
            //command[62] = 0xA0F00000 | 27150;
            //command[63] = 0xA0F00000 | 27150;        

            for (int i = 0; i < command.Length; i++)
                if (command[i] != 0) property.Add(string.Format("charaWork.command[{0}]", i), command[i]);

            for (int i = 0; i < 64; i++)
                property.Add(string.Format("charaWork.commandCategory[{0}]", i), (byte)1);
                       

            //for(int i = 0; i < 4096; i++)
            property.Add(string.Format("charaWork.commandAcquired[{0}]", 1150), true);

            for (int i = 0; i < 36; i++)
                property.Add(string.Format("charaWork.additionalCommandAcquired[{0}]", i), false);

            for (int i = 0; i < 40; i++)
                property.Add(string.Format("charaWork.parameterSave.commandSlot_compatibility[{0}]", i), true);

            //unknown
            property.Add("charaWork.parameterTemp.forceControl_float_forClientSelf[0]", 1.0f);
            property.Add("charaWork.parameterTemp.forceControl_float_forClientSelf[1]", 1.0f);
            property.Add("charaWork.parameterTemp.forceControl_int16_forClientSelf[0]", (short)-1);
            property.Add("charaWork.parameterTemp.forceControl_int16_forClientSelf[1]", (short)-1);
            property.Add("charaWork.parameterTemp.otherClassAbilityCount[0]", (byte)4);
            property.Add("charaWork.parameterTemp.otherClassAbilityCount[1]", (byte)5);
            property.Add("charaWork.parameterTemp.giftCount[1]", (byte)5);
            property.Add("charaWork.depictionJudge", 0xa0f50911);

            //for (int i = 0; i < 40; i++)
                //property.Add(string.Format("playerWork.questScenario[{0}]", (uint)1), true);

            //GuildLeve - local
            //for (int i = 0; i < 40; i++)
                //property.Add(string.Format("playerWork.questGuildleve[{0}]", (uint)49), true);

            //GuildLeve - regional
            //for(int i = 0; i < 16; i++)
            //{
            //    //if(guildLeveId[i]!=0)
            //    //    property.Add(string.Format("work.guildleveId[{0}]", i), guildLeveId[i]);

            //    //if(guildLeveDone[i]!=0)
            //    //    property.Add(string.Format("work.guildleveDone[{0}]", i), guildLeveDone[i]);

            //    //if(guildLeveChecked[i]!=0)
            //    //    property.Add(string.Format("work.guildleveChecked[{0}]", i), guildLeveChecked[i]);
            //}
               
            //NPC linkshell
            //for(int i = 0; i < 64; i++)
            //{
            //    //if(npcLinkshellCalling[i])
            //    //    property.Add(string.Format("playerWork.npcLinkshellChatCalling[{0}]", i), true);

            //    //if (npcLinkshellExtra[i])
            //    //    property.Add(string.Format("playerWork.npcLinkshellChatExtra[{0}]", i), true);
            //}
                

            //ok
            property.Add("playerWork.restBonusExpRate", 0f);

            //From PlayerCharacter obj
            property.Add("playerWork.tribe", Tribe);
            property.Add("playerWork.guardian", Guardian);
            property.Add("playerWork.birthdayMonth", BirthMonth);
            property.Add("playerWork.birthdayDay", BirthDay);
            property.Add("playerWork.initialTown", (byte)InitialTown);

            property.FinishWriting();
        }
        
        public void BattleActionResult(Socket sender, ushort command)
        {
            byte[] data = new byte[0x38];

            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0, sizeof(uint));            
            Buffer.BlockCopy(BitConverter.GetBytes(command), 0, data, 0x24, sizeof(ushort));
            Buffer.BlockCopy(BitConverter.GetBytes(0x0810), 0, data, 0x26, sizeof(ushort));
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0x28, sizeof(uint));
            data[0x04] = 0x62; //figure out
            data[0x07] = 0x7c; //figure out
            data[0x20] = 0x01; //figure out
            data[0x30] = 0x01; //figure out
            data[0x35] = 0x01; //figure out

            SendPacket(sender, ServerOpcode.BattleActionResult01, data);
        }

        public void EndClientOrderEvent(Socket sender, string eventType)
        {
            byte[] data = new byte[0x30];

            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0, sizeof(uint));
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(eventType), 0, data, 0x08, eventType.Length);

            Buffer.BlockCopy(BitConverter.GetBytes(0x09d4f200), 0, data, 0x28, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(0x0a09a930), 0, data, 0x2c, sizeof(uint));

            SendPacket(sender, ServerOpcode.EndClientOrderEvent, data);
        }

        public void ToggleMount(Socket sender, Command command)
        {
            //change to chocobo bgm
            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)(command == Command.MountChocobo ? 0x53 : ActorRepository.Instance.Zones.Find(x=>x.Id==Position.ZoneId).GetCurrentBGM())), 0, data, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)MusicMode.FadeStart), 0, data, 0x02, 2);
            SendPacket(sender, ServerOpcode.SetMusic, data);

            //set player state
            data = new byte[0x08];

            if (command == Command.MountChocobo)
            {
                data[0x05] = 0x1f; //mounted     
                SendPacket(sender, ServerOpcode.SetChocoboMounted, data);
            }
            else
                SetMainState(sender, MainState.Passive, 0xbf);         

            //send command answer
            data = new byte[0x28];
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((uint)Animation.MountChocobo), 0, data, 0x4, 4);
            data[0x27] = 0x08; //8 animation slots to be played in sequence?
            SendPacket(sender, ServerOpcode.CommandResult, data);

            //set chocobo speeds (this packet is originally sent 3 times for unknown reasons)
            if (command == Command.MountChocobo)
            {
                data = new byte[] {
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x66, 0x66, 0x66, 0x40, 0x01, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x10, 0x41, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x41, 0x03, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                };

                SendPacket(sender, ServerOpcode.SetSpeed, data);
                SendPacket(sender, ServerOpcode.SetSpeed, data);
                SendPacket(sender, ServerOpcode.SetSpeed, data);
            }
            else
            {
                SetSpeeds(sender);
                SetSpeeds(sender);
                SetSpeeds(sender);
            }

            //text sheet message ('you call [chocobo name]' chat message?) (should be sent by world?)
            if(command == Command.MountChocobo)
                data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0xF8, 0x5F, 0x91, 0x65, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00 };
            else
                data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0xF8, 0x5F, 0x93, 0x65, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00 };

            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0, 4);
            SendPacket(sender, ServerOpcode.TextSheetMessage30b, data, sourceId: 0x5ff80001);

            //command result 
            data = new byte[0x38];            
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((uint)Animation.MountChocobo), 0, data, 0x4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((uint)1), 0, data, 0x20, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)command), 0, data, 0x24, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)0x810), 0, data, 0x26, 2); //unknown
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0x28, 4);
            data[0x30] = 1;
            data[0x36] = 1;
            SendPacket(sender, ServerOpcode.CommandResultX1, data);

            //end event order
            if(command == Command.MountChocobo)
            {
                data = new byte[0x30];               
                Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0, 4);
                Buffer.BlockCopy(Encoding.ASCII.GetBytes("commandForced"), 0, data, 0x9, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(0x09d4f200), 0, data, 0x28, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(0x0a09a930), 0, data, 0x2c, 4);
                SendPacket(sender, ServerOpcode.EndClientOrderEvent, data);
            }            
        }

        public void EquipSoulStone(Socket sender)
        {
            SendPacket(sender, ServerOpcode.ChangeJob, new byte[] { 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

            SendPacket(sender, ServerOpcode.ParticleAnimation, new byte[] { 0x29, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00 });

            //send text sheet should be a method in the world class.
            byte[] text = new byte[]
            {
                0x41, 0x29, 0x9B, 0x02, 0x01, 0x00, 0xF8, 0x5F, 0x97, 0x75, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x02, 0x9B, 0x29, 0x41, 0x00, 0x00, 0x00, 0x00, 0x10,
                0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, text, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Id).Reverse().ToArray(), 0, text, 0x17, 4);            
            SendPacket(sender, ServerOpcode.TextSheetMessage50b, text, sourceId: 0x5ff80001);

            SetSubState(sender, 0x0c);
            SetSpeeds(sender);
            SetSpeeds(sender);
            SetSpeeds(sender);

            byte[] commandResult = new byte[]
            {
                0x41, 0x29, 0x9B, 0x02, 0x62, 0x00, 0x00, 0x7C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x01, 0x00, 0x00, 0x00, 0xF1, 0x2E, 0x10, 0x08, 0x41, 0x29, 0x9B, 0x02, 0x00, 0x00, 0x00, 0x00,
                0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00
            };
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, text, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, text, 0x28, 4);
        }

        public byte[] CharaWorkExp(Socket sender)
        {
            if(PacketQueue == null || PacketQueue.Count == 0)
            {
                Inventory.Update(sender);

                Queue<short> jobLevel = new Queue<short>();
                Queue<short> jobLevelCap = new Queue<short>();
                int count = 0;

                foreach (var item in Jobs)
                {
                    count++;
                    if (count > 52)
                        break;
                    Job job = item.Value;
                    jobLevel.Enqueue(job.Level);
                    jobLevelCap.Enqueue(job.MaxLevel);
                }

                Property property = new Property(sender, Id, @"charaWork/exp");
                property.Add("charaWork.battleSave.skillLevel", jobLevel);
                property.Add("charaWork.battleSave.skillLevelCap", jobLevelCap, true);            
                PacketQueue = property.PacketQueue;
            }

            return PacketQueue.Dequeue();
        }
    }

    [Serializable]
    public class GeneralParameter
    {
        //static initial attribute values. 
        //TODO: try to find these values in the game data files.
        private static List<ushort[]> Initial = new List<ushort[]>
        {
            //--, --, --, str, vit, dex, int, mnd, pie, fir, ice, wnd, ear, lit, wat, acc, eva, att, def, --, --, --, --, attMagP, heaMagP, enhMagP, enfMagP, magAcc, magEva
            new ushort[]{0,0,0,16,15,14,16,13,16,16,13,15,15,15,16,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //midlander
            new ushort[]{0,0,0,18,17,15,13,15,12,15,16,14,14,18,13,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //highlander
            new ushort[]{0,0,0,14,13,18,17,12,16,12,14,18,17,14,15,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //wildwood
            new ushort[]{0,0,0,15,14,15,18,15,13,14,16,12,17,15,16,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //duskwight
            new ushort[]{0,0,0,13,13,17,16,15,16,14,13,15,16,17,15,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //plainsfolk
            new ushort[]{0,0,0,12,12,15,16,17,18,17,12,16,18,15,12,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //dunesfolk
            new ushort[]{0,0,0,16,15,17,13,14,15,18,15,13,15,12,17,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //seeker of the sun
            new ushort[]{0,0,0,13,12,16,14,18,17,13,18,15,14,16,14,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //keeper of the moon
            new ushort[]{0,0,0,17,18,13,12,16,14,13,17,17,12,13,18,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //sea wolf
            new ushort[]{0,0,0,15,16,12,15,17,15,18,14,16,13,14,18,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //hellsguard
        };

        /// <summary>
        /// This method creates a dictionary with all the attricute points alloted to a slot identified by name. It was modeled like that so the values can be edited.
        /// </summary>
        /// <param name="tribeId"></param>
        /// <returns></returns>
        public static OrderedDictionary Get(byte tribeId)
        {
            int index = 0; //midlander male, female

            switch (tribeId)
            {
                case 3: //highlander male
                    index = 1;
                    break;
                case 4: //wildwood male
                case 5: //wildwood female
                    index = 2;
                    break;
                case 6: //duskwight male 
                case 7: //duskwight female
                    index = 3;
                    break;
                case 8: //plainsfolk male
                case 9: //plainsfolk female
                    index = 4;
                    break;
                case 10: //dunesfolk male
                case 11: //dunesfolk female
                    index = 5;
                    break;
                case 12: //seeker of the sun
                    index = 6;
                    break;
                case 13: //keeper of the moon
                    index = 7;
                    break;
                case 14: //sea wolf
                    index = 8;
                    break;
                case 15: //hellsguard     
                    index = 9;
                    break;               
            }

            return new OrderedDictionary
            {
                {"unknown1", Initial[index][0]},
                {"unknown2", Initial[index][1]},
                {"unknown3", Initial[index][2]},

                {"strength", Initial[index][3]},
                {"vitality", Initial[index][4]},
                {"dexterity", Initial[index][5]},
                {"intelligence", Initial[index][6]},
                {"mind", Initial[index][7]},
                {"piety", Initial[index][8]},

                {"fire", Initial[index][9]},
                {"ice", Initial[index][10]},
                {"wind", Initial[index][11]},
                {"earth", Initial[index][12]},
                {"lightning", Initial[index][13]},
                {"water", Initial[index][14]},

                {"accuracy", Initial[index][15]},
                {"evasion", Initial[index][16]},
                {"attack", Initial[index][17]},
                {"defense", Initial[index][18]},

                {"unknown4", Initial[index][19]},
                {"unknown5", Initial[index][20]},
                {"unknown6", Initial[index][21]},
                {"unknown7", Initial[index][22]},

                {"attack magic potency", Initial[index][23]},
                {"healing magic potency", Initial[index][24]},
                {"enhance magic potency", Initial[index][25]},
                {"enfeeble magic potency", Initial[index][26]},

                {"magic accuracy", Initial[index][27]},
                {"magic avasion", Initial[index][28]},
            };
        }
    }
}

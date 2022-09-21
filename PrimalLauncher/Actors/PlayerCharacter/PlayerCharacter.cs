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
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    [Serializable]
    public class PlayerCharacter : ActorBattle
    {
        public byte WorldId { get; set; }       

        #region Background
        public byte Tribe { get; set; }
        public byte Guardian { get; set; }
        public byte BirthMonth { get; set; }
        public byte BirthDay { get; set; }
        public uint InitialTown { get; set; }
        #endregion

        #region Travel
        public uint Anima { get; set; }
        public uint HomePoint { get; set; }
        public List<uint> AttunedAetherytes { get; set; }
        #endregion

        #region Mounts
        public string ChocoboName { get; set; }
        public ChocoboAppearance ChocoboAppearance { get; set; }
        public bool HasGoobbue { get; set; }
        public sbyte CurrentMount { get; set; } //0 = not mounted, 1 = chocobo, 2 = goobbue
        #endregion

        #region Config
        public long TotalPlaytime { get; set; }
        public bool IsTutorialComplete { get; set; }
        public int ControlScheme { get; set; } //player selected keyboard or joystick
        private uint SpawnDistance { get; set; } = 30; //this is the maximum distance in wich NPCs will be spawned at.
        #endregion

        #region Grand Company
        public byte CompanyId { get; set; }
        public byte CompanyRank { get; set; }
        #endregion

        public Inventory Inventory { get; set; }
        public Journal Journal { get; set; }
        public Linkshell Linkshell { get; set; }
        public List<GroupBase> Groups { get; set; } = new List<GroupBase>();
        public GroupRetainer RetainerGroup { get; set; }
        public GroupParty PartyGroup { get; set; }
        public Queue<byte[]> PacketQueue { get; set; }

        public void Setup(byte[] data)
        {
            //Character ID
            Id = NewId();
            ClassPath = "/Chara/Player/Player_work";
            State.Type = MainStateType.Player;

            //speeds 
            Speeds.Walking = ActorSpeed.Walking;  //Walking speed
            Speeds.Running = ActorSpeed.Running; // 0x40a00000;  //Running speed
            Speeds.Active = ActorSpeed.Active;  //Acive

            //prepare packet data for decoding
            byte[] info = new byte[0x90];
            Buffer.BlockCopy(data, 0x30, info, 0, info.Length);
            string tmp = Encoding.ASCII.GetString(info).Trim(new[] { '\0' }).Replace('-', '+').Replace('_', '/');

            //decoded packet data
            data = Convert.FromBase64String(tmp);

            //General
            Appearance.Size = data[0x09];
            Appearance.Voice = data[0x26];
            Appearance.SkinColor = (ushort)(data[0x23] >> 8 | data[0x22]);

            //Head
            Appearance.HairStyle = (ushort)(data[0x0b] >> 8 | data[0x0a]);
            Appearance.HairColor = (ushort)(data[0x1d] >> 8 | data[0x1c]);
            Appearance.HairHighlightColor = data[0x0c];
            Appearance.HairVariation = data[0x0d];
            Appearance.EyeColor = (ushort)(data[0x25] >> 8 | data[0x24]);

            //Face
            Appearance.Face = new Face(data);

            //Background
            Guardian = data[0x27];
            BirthMonth = data[0x28];
            BirthDay = data[0x29];
            InitialTown = data[0x48];
            Tribe = data[0x08];
            GetBaseModel(Tribe);
            CharaWork.Jobs = Job.LoadAll();
            CharaWork.GeneralParameters = GeneralParameters.Get(Tribe);

            //Starting class
            CharaWork.CurrentClassId = data[0x2a];
            CharaWork.CurrentJob.Level = 1; //having a class level > 0 makes it active.
            CharaWork.CurrentJob.IsCurrent = true; //current class the player will start with.                        
            LoadInitialEquipment();

            //work
            Journal = new Journal(InitialTown);
            Linkshell = new Linkshell();            
            
            //travel
            Anima = 100;
            Position = EntryPoints.GetStartPosition(InitialTown);
            uint homePoint = Aetheryte.GetStartHomePoint(InitialTown);
            AttunedAetherytes = new List<uint>{ homePoint };
            HomePoint = homePoint;

            //battle
            AutoAttackDelay = 4 * 1000; //4 seconds delay
        }

        public void UpdatePlayTime()
        {            
            if (TotalPlaytime > 0)
                TotalPlaytime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - TotalPlaytime;
        }

        private void StartPlayTime()
        {
            if (TotalPlaytime == 0)
                TotalPlaytime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public override void Prepare(){}

        private void LoadInitialEquipment()
        {
            int equipmentSetNumber = (Tribe * 100) + CharaWork.CurrentClassId;
            uint underShirtId = (uint)8040000 + Tribe;
            uint underGarmentId = (uint)8060000 + Tribe;
            DataTable defaultSet = GameData.Instance.GetGameData("boot_skillequip");
            uint[] itemGraphicIds = defaultSet.Select("id = '" + equipmentSetNumber + "'")[0].ItemArray.Select(Convert.ToUInt32).ToArray();   
            
            Appearance.SetToSlots(itemGraphicIds, underShirtId, underGarmentId);
            Inventory = new Inventory();
            Inventory.AddDefaultItems(itemGraphicIds, underShirtId, underGarmentId);
        }

        private void CommandSequence()
        {
            List<KeyValuePair<uint, string>> commands = new List<KeyValuePair<uint, string>>
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

            foreach (var command in commands)
            {
                byte[] data = new byte[0x28];
                data.Write(0, command.Key);
                data.Write(0x02, command.Value);
                Packet.Send(ServerOpcode.PlayerCommand, data);
            }
        }

        public void SetUnendingJourney()
        {
            byte[] data = new byte[0x130];

            using(MemoryStream ms = new MemoryStream(data))
            using(BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write((byte)0); //beast tribe intro
                bw.Write((ushort)2); //nightmares/castrum
                bw.Write((byte)0); //unknown
                bw.Write((ushort)11);
                bw.Write((byte)1); 
                bw.Write((byte)1); 
                //bw.Write((byte)126);

                for (int i = 0; i < 256; i++)
                    bw.Write((byte)0xFF);

                bw.Seek(0x109, SeekOrigin.Begin);
                bw.WriteNullTerminatedString("Companion Name");
            }

            Packet.Send(ServerOpcode.UnendingJourney, data);
        }

        public void SetEntrustedItems()
        {
            byte[] data = new byte[0x30];

            for (int i = 0; i < 15; i++)
                data[i] = 0xFF;

            data[0xf] = 0x3F;

            Packet.Send(ServerOpcode.EntrustedItems, data);

        }       

        public void Spawn(ushort spawnType = 0x01, ushort isZoning = 0)
        {
            PacketQueue = null;
            State.Main = MainState.Passive;
            SpawnDistance = 50;
            //Icon = 0x00_02_00_00;
            CurrentTargetId = 0;
            CharaWork.CurrentJob.Tp = 0;
            CharaWork.CurrentJob.Hp = 300;

            //remove later
            if (CharaWork == null)
                CharaWork = new CharaWork();

            //in case the player quit the game while monted.
            Speeds.SetUnmounted();
            Journal.InitializeQuests();
            CreateActor(0x08);
            CommandSequence();
            SetSpeeds();
            GetPosition(spawnType, isZoning);
            SetAppearance();
            SetName(-1, Name); //-1 = it's a custom name.
            SendUnknown();
            SetMainState();
            SetSubState();
            SetAllStatus();
            SetIcon();
            SetIsZoning();

            SetGrandCompany();
            SetTitle();
            SendCurrentJob();
            SpecialEventWork();
            SetMounts();
            AchievementPoints();
            AchievementsLatest();
            AchievementsCompleted();
            LoadLuaParameters();
            SetLuaScript();           
            Inventory.Send();
            Work();
            StartPlayTime();

            if (GetCurrentZone().Id == 0xF4) //if it's inn
            {
                SetUnendingJourney();
                SetEntrustedItems();
            }            
        }

        public override Zone GetCurrentZone() 
        {
            Zone zone = World.Instance.GetZone(Position.ZoneId);

            //if zone is null, player is in an instance
            if (zone == null)           
                zone = World.Instance.CreateInstance(Position.ZoneId);

            return zone;
        }

        public void SetGrandCompany()
        {           
            Packet.Send(ServerOpcode.SetGrandCompany, new byte[] { 0x03, 0x7f, 0x7f, 0x0b, 0x00, 0x00, 0x00, 0x00 });
        }

        public void SetTitle()
        {
            Packet.Send(ServerOpcode.SetTitle, new byte[] { 0x8f, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
        }

        public void SetMounts()
        {
            ChocoboName = "Boko";
            HasGoobbue = true;

            if (!string.IsNullOrEmpty(ChocoboName))
            {
                Packet.Send(ServerOpcode.SetChocoboName, Encoding.ASCII.GetBytes(ChocoboName));
                Packet.Send(ServerOpcode.SetHasChocobo, new byte[] { 0x1f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            }
            
            if(HasGoobbue)
                Packet.Send(ServerOpcode.SetHasGobbue, new byte[] { 0x1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
        }

        public static void Logout() => Packet.Send(ServerOpcode.Logout, new byte[0x08]);

        public static void ExitGame() => Packet.Send(ServerOpcode.ExitGame, new byte[0x08]);

        public void InitializeOpening()
        {
            if (TotalPlaytime == 0)
            {                
                GetCurrentZone().Directors.Add(new OpeningDirector());                
                GetCurrentZone().NpcFile = "opening.npc.xml";
            }            
        }   

        public void LoadLuaParameters(uint director = 0)
        {
            if(LuaParameters == null)
            {
                LuaParameters = new LuaParameters
                {
                    ActorName = string.Format("_pc{0:00000000}", Id),
                    ClassName = "Player",
                    ClassCode = 0x30400000
                };
                LuaParameters.Add(ClassPath);

                if (director > 0) //opening 
                {
                    LuaParameters.Add(false);
                    LuaParameters.Add(false);
                    LuaParameters.Add(true);
                    LuaParameters.Add(director);
                }
                else //no opening
                {
                    LuaParameters.Add(true);
                    LuaParameters.Add(false);
                    LuaParameters.Add(false);
                }

                LuaParameters.Add(true);
                LuaParameters.Add(0);
                LuaParameters.Add(false);

                //add timer placeholders
                for (int i = 0; i < 20; i++)
                    LuaParameters.Add(0);

                LuaParameters.Add(true);
            }
        }

        #region Group Methods
        public void GetGroups()
        {
            if(PartyGroup == null)
                PartyGroup = new GroupParty();

            if(RetainerGroup == null)
                RetainerGroup = new GroupRetainer();            

            PartyGroup.SendPackets();
            RetainerGroup.SendPackets();
            ActiveLinkshell();
        }

        

        
        #endregion

        public void ActiveLinkshell()
        {
            byte[] data = new byte[0x78];

            Buffer.BlockCopy(BitConverter.GetBytes(0x4e22), 0, data, 0x40, sizeof(ushort));           

            Packet.Send(ServerOpcode.ActiveLinkshell, data);
        }

        private void SpecialEventWork()
        {
            byte[] data = new byte[0x18];
            data[0x02] = 0x12;
            Packet.Send(ServerOpcode.SetSpecialEventWork, data);
        }

        #region Achievement Methods
        private void AchievementPoints()
        {
            byte[] data = new byte[0x08];
            Packet.Send(ServerOpcode.AchievementPoints, data);
        }

        private void AchievementsLatest()
        {
            byte[] data = new byte[0x20];
            Packet.Send(ServerOpcode.AchievementsLatest, data);
        }

        private void AchievementsCompleted()
        {
            byte[] data = new byte[0x80];
            Packet.Send(ServerOpcode.AchievementsCompeted, data);
        }
        #endregion

        #region Work Methods
        public void Work()
        {
            WorkProperties property = new WorkProperties(Id, @"/_init");           

            property.Add("charaWork.eventSave.bazaarTax", (byte)5);
            property.Add("charaWork.evsureentSave.bazaar", true);
            property.Add("charaWork.battleSave.potencial", 6.6f);

            for (int i = 0; i < 32; i++)
                if (i < 5 && i != 3) property.Add(string.Format("charaWork.property[{0}]", i), (byte)1);
                       
            CharaWork.AddWorkClassParameters(ref property);
            CharaWork.AddStatusShownTime(ref property);
            CharaWork.AddGeneralParameters(ref property);

            property.Add("charaWork.battleTemp.castGauge_speed[0]", 1.0f);
            property.Add("charaWork.battleTemp.castGauge_speed[1]", 0.25f);
            property.Add("charaWork.commandBorder", CharaWork.CommandBorder);
            property.Add("charaWork.battleSave.negotiationFlag[0]", true);

            CharaWork.AddWorkCommands(ref property);               

            for (int i = 0; i < 64; i++)
                property.Add(string.Format("charaWork.commandCategory[{0}]", i), (byte)1);

            //for (int i = 0; i < 4096; i++)
                property.Add(string.Format("charaWork.commandAcquired[{0}]", 1150), true);

            //job abilities
            for (int i = 0; i < 36; i++)
                property.Add(string.Format("charaWork.additionalCommandAcquired[{0}]", i), true);

            for (int i = 0; i < 40; i++)
                property.Add(string.Format("charaWork.parameterSave.commandSlot_compatibility[{0}]", i), true);

            CharaWork.AddWorkSystem(ref property);

            Journal.AddToWork(ref property);            
            Linkshell.AddToWork(ref property);

            property.Add("playerWork.restBonusExpRate", 0f);
            AddWorkCharacterBackground(ref property);            

            property.FinishWritingAndSend();
        }

        private void AddWorkCharacterBackground(ref WorkProperties property)
        {           
            property.Add("playerWork.tribe", Tribe);
            property.Add("playerWork.guardian", Guardian);
            property.Add("playerWork.birthdayMonth", BirthMonth);
            property.Add("playerWork.birthdayDay", BirthDay);
            property.Add("playerWork.initialTown", (byte)InitialTown);
        }
        #endregion Work Methods

        #region Exp & Level Methods
        public void UpdateExp()
        {
            WorkProperties prop = new WorkProperties(Id, "charaWork/battleStateForSelf");
            prop.Add("charaWork.battleSave.skillPoint[" + (CharaWork.CurrentClassId - 1) + "]", (int)CharaWork.CurrentJob.TotalExp);
            prop.FinishWritingAndSend();
        }

        public byte[] ClassExp()
        {
            if (PacketQueue == null || PacketQueue.Count == 0)
            {
                Inventory.Update();

                Queue<short> jobLevel = new Queue<short>();
                Queue<short> jobLevelCap = new Queue<short>();
                int count = 0;

                foreach (var item in CharaWork.Jobs)
                {
                    count++;
                    if (count > 52)
                        break;
                    Job job = item.Value;
                    jobLevel.Enqueue(job.Level);
                    jobLevelCap.Enqueue(job.LevelCap);
                }

                WorkProperties property = new WorkProperties(Id, @"charaWork/exp");
                property.Add("charaWork.battleSave.skillLevel", jobLevel);
                property.Add("charaWork.battleSave.skillLevelCap", jobLevelCap, true);
                PacketQueue = property.PacketQueue;
            }

            return PacketQueue.Dequeue();
        }
        
        public void AddExp(int exp)
        {
            //we want to add exp only if level is below cap.
            if (CharaWork.CurrentJob.Level < CharaWork.CurrentJob.LevelCap)
            {
                //add exp bonus multiplier TODO:put multiplier definition somewhere else (add as an option in UI?)
                float expBonus = 1.2f;
                CharaWork.CurrentJob.TotalExp += Convert.ToInt64(exp * expBonus);

                //send add exp command result
                SendCommandResult(0, new List<CommandResult> {
                    new CommandResult
                    {
                        TargetId = Id,
                        Amount = (short)(exp * expBonus),
                        TextId = 33934,
                        Direction = (byte)(expBonus > 1 ? ((expBonus -1) * 100) : 0)
                    }
                });

                //calculate leveling
                long totalExp = CharaWork.CurrentJob.TotalExp;
                short currentLevel = CharaWork.CurrentJob.Level;
                short levelsToUp = 0;

                while (totalExp >= Job.ExpTable[currentLevel])
                {
                    totalExp -= Job.ExpTable[currentLevel];
                    levelsToUp++;
                }

                if (levelsToUp > 0)
                {
                    CharaWork.CurrentJob.TotalExp = (currentLevel + levelsToUp) >= CharaWork.CurrentJob.LevelCap ? 0 : totalExp;
                    LevelUp(levelsToUp);
                }

                //refresh exp values in game client UI.
                UpdateExp();
            }
        }

        private void LevelUp(short numLevels)
        {
            CharaWork.CurrentJob.Level += numLevels;

            SendCommandResult(0, new List<CommandResult> {
                new CommandResult
                {
                    TargetId = Id,
                    Amount = CharaWork.CurrentJob.Level,
                    TextId = 33909
                }
            });

            UpdateLevel();
            World.Instance.SetMusic(0x52, MusicMode.Layer);
            Journal.InitializeQuests();
        }

        public void LevelDown(short toLevel)
        {
            if (toLevel > 0)
            {
                CharaWork.CurrentJob.Level = toLevel;
                CharaWork.CurrentJob.TotalExp = 0;
                UpdateLevel();
                UpdateExp();
            }
        }

        private void UpdateLevel()
        {
            WorkProperties property = new WorkProperties(Id, @"charaWork/stateForAll");
            property.Add("charaWork.battleSave.skillLevel[" + (CharaWork.CurrentClassId - 1) + "]", CharaWork.CurrentJob.Level);
            property.Add("charaWork.parameterSave.state_mainSkillLevel", CharaWork.CurrentJob.Level);
            property.FinishWritingAndSend();
        }

        public int GetCurrentLevel()
        {
            return CharaWork.CurrentJob.Level;
        }
        #endregion

        private void UpdateClass()
        {
            WorkProperties property = new WorkProperties(Id, @"charaWork/stateForAll");
            property.Add("charaWork.parameterSave.state_mainSkill[0]", CharaWork.CurrentClassId);
            property.Add("charaWork.parameterSave.state_mainSkillLevel", CharaWork.CurrentJob);
            property.FinishWritingAndSend();
        }

        public void ToggleZoneActors()
        {
            Zone zone = GetCurrentZone();

            if (zone.Actors != null)
            {
                foreach (Actor actor in zone.Actors)
                {
                    if(!(actor is PlayerCharacter))
                    {
                        //if actor position is inside player's aquare...
                        if (
                           actor.Position.X >= (Position.X - SpawnDistance) &&
                           actor.Position.X <= (Position.X + SpawnDistance) &&

                           actor.Position.Y >= (Position.Y - SpawnDistance) &&
                           actor.Position.Y <= (Position.Y + SpawnDistance) &&

                           actor.Position.Z >= (Position.Z - SpawnDistance) &&
                           actor.Position.Z <= (Position.Z + SpawnDistance)
                        )
                        {
                            //if actor is not spawned, spawn and add to remove exemption list.
                            if (!actor.Spawned)
                                actor.Spawn();
                        }
                        else
                        {
                            //if actor is NOT inside the player's square, set it do despawn.
                            if (actor.Spawned)
                                actor.Despawn();
                        }
                    }
                }
            }            
        }

        public void ToggleMount(Command command, bool isChocobo)
        {         
            byte[] data = new byte[0x08];
            int mountTextSheet = 0x6591;
            uint bgm = 0x53;

            if (command == Command.Mount)
            {          
                if (isChocobo)
                {
                    CurrentMount = 1;
                    data[0x05] = (byte)ChocoboAppearance.Maelstrom4;     
                    Packet.Send(ServerOpcode.SetChocoboMounted, data);
                }
                else
                {
                    CurrentMount = 2;
                    mountTextSheet = 0x65A3;
                    data = new byte[0x28];
                    data[0] = 0x01;
                    Packet.Send(ServerOpcode.SetGobbueMounted, data);
                    bgm = 0x62;
                }

                Speeds.SetMounted();
            }
            else
            {
                if(CurrentMount == 1)
                    mountTextSheet = 0x6593;
                else
                    mountTextSheet = 0x65A5; //dismount goobbue

                CurrentMount = 0;
                State.Main = MainState.Passive;
                SetMainState();
                Speeds.SetUnmounted();
                bgm = World.Instance.GetZone(Position.ZoneId).GetCurrentBGM();
            }
            
            World.Instance.SetMusic(bgm, MusicMode.FadeStart);

            //TODO: send command answer - this should probably be in event manager.
            data = new byte[0x28];
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(0x7c000062), 0, data, 0x4, 4);
            data[0x27] = 0x08; //8 animation slots to be played in sequence?
            Packet.Send(ServerOpcode.CommandResult, data);      

            SetSpeeds();
            SetSpeeds();
            SetSpeeds();

            World.SendTextSheet(mountTextSheet, source: Id);

            //TODO: command result - this should probably be in event manager.
            data = new byte[0x38];
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(0x7c000062), 0, data, 0x4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((uint)1), 0, data, 0x20, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)command), 0, data, 0x24, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)0x810), 0, data, 0x26, 2); //unknown
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0x28, 4);
            data[0x30] = 1;
            data[0x36] = 1;
            Packet.Send(ServerOpcode.CommandResultX1, data);
        }
             
        private byte GetClassJob()
        {
            byte jobIndex = 13;

            if (CharaWork.CurrentClassId == 7 || CharaWork.CurrentClassId == 8)
                jobIndex = 11;
            else if (CharaWork.CurrentClassId == 22 || CharaWork.CurrentClassId == 23)
                jobIndex = 4;

            return (byte)(CharaWork.CurrentClassId + jobIndex);
        }

        public void EquipSoulStone(byte[] data)
        {
            int classJob;

            if (CharaWork.CurrentJob.Id == 0)
            {
                CharaWork.CurrentJob.Id = GetClassJob();
                SendCurrentJob();
                PlayAnimationEffect(Job.AnimationEffectId(CharaWork.CurrentJob.Id));

                //SetSubState(0x0c);
                SetSpeeds();
                SetSpeeds();
                SetSpeeds();

                SendCommandResult(Command.EquipSoulStone, new List<CommandResult> {
                    new CommandResult
                    {
                        TargetId = Id,
                        EffectId = 1,
                        HitSequence = 1
                    }
                }, 0x7c000062);

                classJob = CharaWork.CurrentJob.Id;
            }
            else
            {
                CharaWork.CurrentJob.Id = 0;
                SendCurrentJob();
                PlayAnimationEffect(AnimationEffect.ChangeClass);
                classJob = CharaWork.CurrentClassId;
            }

            World.SendTextSheet(0x7597, new object[] { 0, 0, User.Instance.Character.Id, classJob }, User.Instance.Character.Id);
            UpdateLevel();
            UpdateClass();
            UpdateExp();
        }

        private void SendCurrentJob()
        {
            byte[] data = new byte[0x08];
            data[0] = CharaWork.CurrentJob.Id;
            Packet.Send(ServerOpcode.SetCurrentJob, data);
        }
               
        public void ChangeEquipment(byte[] data)
        {
            //we read the bytes in the index below to be able to differentiate equip/unequip packets. It's the fastest way I can think of.
            uint pattern = (uint)(data[0x53] << 24 | data[0x52] << 16 | data[0x51] << 8 | data[0x50]);
            bool isEquipping = pattern == 0x05050505 ? true : false;
            byte gearSlot = 0;
            uint itemUniqueId = (uint)(data[0x5e] << 24 | data[0x5f] << 16 | data[0x60] << 8 | data[0x61]);       

            if (isEquipping)
            {
                gearSlot = (byte)(data[0x58] - 1);
                Item itemToEquip = Inventory.GetBagItemByUniqueId(itemUniqueId);
                World.SendTextSheet(0x7789, new object[] {/*quality?*/ 1, (int)itemToEquip.Id, 1, 0, 0, 1, 0 }, User.Instance.Character.Id);

                //if a weapon is being equipped
                if (gearSlot == 0)
                {                    
                    Item equippedWeapon = (Item)Inventory.Bag[Inventory.GearSlots[0]];
                    ushort equippedCategory = Convert.ToUInt16(equippedWeapon.Id.ToString().Substring(0, 3));
                    ushort toEquipCategory = Convert.ToUInt16(itemToEquip.Id.ToString().Substring(0, 3));

                    if (itemToEquip == null)
                    {
                        Log.Instance.Error("Something went wrong... The requested item wasn't found in the inventory.");
                        return;
                    }

                    //if the weapon being equipped is from a different class/job
                    if (equippedCategory != toEquipCategory)
                    {
                        byte jobToChangeTo = Job.Category[toEquipCategory];
                        CharaWork.CurrentClassId = jobToChangeTo;

                        //for now, if the job is not activated, activate it.
                        short level = CharaWork.CurrentJob.Level;
                        CharaWork.CurrentJob.Level = level == 0 ? (short)1 : level;

                        //if a soul stone is equipped, remove it.
                        if (CharaWork.CurrentJob.Id != 0)
                        {
                            CharaWork.CurrentJob.Id = 0;
                            SendCurrentJob();
                        }                                                   

                        SendCommandResult(Command.ChangeEquipment, new List<CommandResult> {
                            new CommandResult
                            {
                                TargetId = Id,
                                EffectId = 1,
                                HitSequence = 1
                            }
                        }, 0x7c000062, 0x40000000);

                        UpdateLevel();
                        UpdateClass();
                        UpdateExp();

                        Inventory.ChangeGear(gearSlot, itemUniqueId);

                        //check if character has soul of the current class                       
                        DataTable jobsTable = GameData.Instance.GetGameData("xtx/text_jobName");
                        DataRow[] selected = jobsTable.Select("ID = '" + GetClassJob() + "'");
                        string jobName = (string)selected[0][1];
                        jobName = jobName.Substring(0, jobName.Length - 1);

                        if (Inventory.HasKeyItem("soul of the " + jobName))
                        {
                            EquipSoulStone(null);
                        }
                        else
                        {
                            PlayAnimationEffect(AnimationEffect.ChangeClass);
                            World.SendTextSheet(0x7597, new object[] { 0, 0, User.Instance.Character.Id, (int)CharaWork.CurrentClassId }, User.Instance.Character.Id);
                        }
                    }
                }
                else
                {
                    Inventory.ChangeGear(gearSlot, itemUniqueId);
                }
            }
            else //unequip
            {
                gearSlot = (byte)(data[0x51] - 1);
                Item itemToUnequip = Inventory.GetBagItemByGearSlot(gearSlot);
                World.SendTextSheet(0x778A, new object[] {/*quality?*/ 1, (int)itemToUnequip.Id, 1, 0, 0, 1, 0 }, User.Instance.Character.Id);
                Inventory.ChangeGear(gearSlot, itemUniqueId);
            }
        }
        
        public void ToggleUIControl(UIControl control, uint unknown = 0x02)
        {
            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes((uint)control), 0, data, 0, sizeof(uint)); // (0x02 & 0xff);           
            Buffer.BlockCopy(BitConverter.GetBytes(unknown), 0, data, 0x04, sizeof(uint)); // (0x02 & 0xff);           
            Packet.Send(ServerOpcode.SetUIControl, data);
        }

        public void GetBlackList()
        {
            byte[] data = new byte[0x666];
            Buffer.BlockCopy(BitConverter.GetBytes(1), 0, data, 0x04, sizeof(uint));         
            Buffer.BlockCopy(Encoding.ASCII.GetBytes("Test2"), 0, data, 0x08, sizeof(uint));   
            Packet.Send(ServerOpcode.SendBlackList, data);
        }

        public void GetFriendlist()
        {
            byte[] data = new byte[0x666];
            Buffer.BlockCopy(BitConverter.GetBytes(1), 0, data, 0x04, sizeof(uint));
            Buffer.BlockCopy(Encoding.ASCII.GetBytes("Test"), 0, data, 0x08, sizeof(uint));           
            Packet.Send(ServerOpcode.SendFriendList, data);
        }

        #region Position Methods
        public void GoForward(float distance)
        {
            Position.X += (float)(distance * Math.Sin(Position.R));
            Position.Z += (float)(distance * Math.Cos(Position.R));
            SetPosition(0x11);
        }

        public void TurnBack(float distance)
        {
            Position.R = (3.2f - Math.Abs(Position.R)) * (Position.R <= 0 ? 1 : -1);
            GoForward(distance);
        }

        public void GetPosition(ushort spawnType = 0, ushort isZonning = 0)
        {
            base.SetPosition(spawnType, isZonning, true);
        }

        public void SetPosition(uint zoneId, float x, float y, float z, float r, ushort spawnType)
        {
            Position position = new Position(zoneId, x, y, z, r, spawnType);
            Position = position;
            base.SetPosition(spawnType, 0, true);
        }

        public void SetPosition(string offsetStr)
        {
            float[] offset = Array.ConvertAll(offsetStr.Split(new char[] { ',' }), float.Parse);
            World.Instance.MapUIChange(0x10);
            SetPosition((uint)offset[0], offset[1], offset[2], offset[3], offset[4], (ushort)offset[5]);
        }

        public void UpdatePosition(byte[] data)
        {        
            //position from packet
            float x = BitConverter.ToSingle(new byte[] { data[0x18], data[0x19], data[0x1a], data[0x1b] }, 0);
            float y = BitConverter.ToSingle(new byte[] { data[0x1c], data[0x1d], data[0x1e], data[0x1f] }, 0);
            float z = BitConverter.ToSingle(new byte[] { data[0x20], data[0x21], data[0x22], data[0x23] }, 0);
            float r = BitConverter.ToSingle(new byte[] { data[0x24], data[0x25], data[0x26], data[0x27] }, 0);

            bool positionChanged = x != Position.X || y != Position.Y || z != Position.Z;
            
            Position.X = x;
            Position.Y = y;
            Position.Z = z;
            Position.R = r;
            
            if (positionChanged)            
                ToggleZoneActors();

            //this might be useful someday...
            //byte[] moveState = new byte[] { data[0x28], data[0x29] }; //unused so far. maybe part of mouse coords?
            //byte[] mousePosition = byte[] { data[0x2a], data[0x2b] }; //2d mouse cursor hud position?
            //byte[] cameraRotation = new byte[] { data[0x2c], data[0x2d], data[0x2e], data[0x2f] }; //indicates the camera rotation (maybe it's 2 shorts?).

            //save player position 
            User.Instance.SavePlayerCharacter(this);
        }     
        #endregion

        public uint NewId()
        {
            Random rnd = new Random();   
            return (uint)rnd.Next(0xff, 0xffff);
        }

        public void Unknown0x02()
        {
            byte[] data = new byte[0x10];
            Buffer.BlockCopy(BitConverter.GetBytes(User.Instance.Character.Id), 0, data, 0x08, 0x04);
            Packet.Send(ServerOpcode.Unknown0x02, data);           
        } 
        
        public bool CanAcceptQuest(string questName)
        {
            string[] townInitials = { "", "l", "g", "u" };
            string questTypeChar = questName.Substring(questName.Length - 2, 1);
            
            if(
                int.TryParse(questTypeChar, out _) || //is number
                Array.IndexOf(townInitials, questTypeChar) < 0 || //is not in the array above
                questTypeChar == townInitials[InitialTown] //is equ
              )
                return true;
            else
                return false;
        }

        public bool HasInn()
        {
            List<uint> InnQuestNumbers = new List<uint>{110828, 110838, 110848};
            return Journal.QuestsFinished.Intersect(InnQuestNumbers).Any();
        }

        public void RemoveFromZone()
        {
            Zone zone = GetCurrentZone();

            if (zone != null && zone.GetActorById(Id) != null)
                zone.Actors.Remove(this);
        }

        #region Targeting
        public void SelectTarget(byte[] srcData)
        {
            uint targetId = (uint)(srcData[0x13] << 24 | srcData[0x12] << 16 | srcData[0x11] << 8 | srcData[0x10]);
            //uint unknown = (uint)(srcData[0x17] << 24 | srcData[0x16] << 16 | srcData[0x15] << 8 | srcData[0x014]); //unused so far

            ////previous target
            //if (CurrentTargetId > 0 && CurrentTargetId != Id && !(GetCurrentZone().GetActorById(CurrentTargetId) is Monster))
            //    GetCurrentZone().GetActorById(CurrentTargetId).ToggleHeadDirection();

            CurrentTargetId = targetId != 0xC0000000 ? targetId : 0; //store target id

            ////current target
            //if (CurrentTargetId > 0 && CurrentTargetId != Id && !(GetCurrentZone().GetActorById(CurrentTargetId) is Monster))
            //    GetCurrentZone().GetActorById(CurrentTargetId).ToggleHeadDirection(true);

            if (CurrentTargetId == 0) ToggleHeadDirection();

            //byte[] data = new byte[0x08];
            //Buffer.BlockCopy(BitConverter.GetBytes(CurrentTargetId), 0, data, 0, sizeof(uint));
            //Packet.Send(ServerOpcode.SetTarget, data);
        }

        public void LockTarget(byte[] srcData)
        {
            uint targetId = (uint)(srcData[0x13] << 24 | srcData[0x12] << 16 | srcData[0x11] << 8 | srcData[0x10]);
            //uint unknown = (uint)(srcData[0x17] << 24 | srcData[0x16] << 16 | srcData[0x15] << 8 | srcData[0x014]); //unused so far

            if (targetId != 0xC0000000)
            {
                Actor actor = GetCurrentZone().GetActorById(CurrentTargetId);
                if (actor != null && (actor is Monster monster) && ((Monster)actor).Family != "fighter")
                {
                    //Engage(CurrentTargetId);
                    //monster.Engage(Id);
                    BattleManager.Instance.Engage(this);
                }
            }
            else
            {
                IsEngaged = false;
            }
        }

        public void GetTargetData()
        {
            if (CurrentTargetId > 0)
            {
                Actor actor = User.Instance.Character.GetCurrentZone().Actors.Find(x => x.Id == CurrentTargetId);

                if (actor != null)
                {
                    ChatProcessor.SendMessage(MessageType.System, "Id: " + actor.Id);
                    ChatProcessor.SendMessage(MessageType.System, "ClassId: " + actor.ClassId);
                    ChatProcessor.SendMessage(MessageType.System, "ClassName: " + actor.ClassName);
                    ChatProcessor.SendMessage(MessageType.System, "Animation: " + actor.SubState.MotionPack);
                }
            }
            else
            {
                ChatProcessor.SendMessage(MessageType.System, "No target selected.");
            }
        }
        #endregion

        #region Battle methods
        //public override void AutoAttack()
        //{
        //    if(CurrentTargetId > 0 && GetTargetDistance() <= CharaWork.CurrentJob.AutoAttackMaxDistance)
        //    {
        //        ActorBattle target = ((ActorBattle)GetCurrentZone().GetActorById(CurrentTargetId));

        //        if (target != null && !target.IsDead() && !IsDead())
        //        {
        //            short damageDealt = 80; //to be calculated                

        //            CommandResult cr = new CommandResult
        //            {
        //                TargetId = target.Id,
        //                Amount = damageDealt,
        //                TextId = 0x765D,
        //                EffectId = 0x08000604, //target hit animation
        //                Direction = 1,
        //                HitSequence = 1
        //            };

        //            SendCommandResult(Command.PlayerAutoAttack, new List<CommandResult> { cr }, 0x19001000);
        //            Thread.Sleep(500);
        //            AddTp(100);
        //            target.TakeDamage(this, damageDealt);

        //            //this should be in BM?
        //            //if (target.CharaWork.CurrentJob.Hp <= 0)
        //            //{
        //            //    SendCommandResult(0, new List<CommandResult> { new CommandResult(Id, 1, 0x847F, 0, 50, 1) }, 0);
        //            //    AddExp(target.Exp);
        //            //    Inventory.AddGil(target.Gil);
        //            //}

                    
        //        }

        //        if (!IsTutorialComplete) BattleTutorial.Instance.NextTutorial("tp");
        //    }            
        //}

        public override void Engage(uint attacker)
        {
            IsEngaged = true;
            AutoAttack(); //first hit happens instantly.
            
        }

        public override void Disengage()
        {
            IsEngaged = false;
            ToggleStance(Command.NormalStance);
        }

        #endregion

        public override void AddTp(short amount)
        {
            base.AddTp(amount);
            if (!IsTutorialComplete && CharaWork.CurrentJob.Tp >= 1000) BattleTutorial.Instance.NextTutorial("weaponskill");
        }

        public void SetCommandRecast(int recastTime)
        {            
            WorkProperties prop = new WorkProperties(Id, "charaWork/commandDetailForSelf");
            prop.Add("charaWork.parameterTemp.maxCommandRecastTime[0]", (short)recastTime); 
            prop.Add("charaWork.parameterSave.commandSlot_recastTime[0]", Server.GetTimeStampHex(recastTime)); //timestamp
            prop.FinishWritingAndSend();
        }

        public void SetComboAction(short nextCommandId)
        {
            WorkProperties prop = new WorkProperties(Id, "playerWork/combo");
            prop.Add("playerWork.comboNextCommandId[0]", nextCommandId);
            prop.Add("playerWork.comboCostBonusRate", 0x3F800000); //float?
            prop.FinishWritingAndSend();
        }

        public void ExecuteBattleCommand(int commandId)
        {
            AddTp(-1000);
            SetCommandRecast(0x0A); //recast time should come from skill definition?
            SetComboAction(0x6A37);
            Thread.Sleep(2000);
            if (!IsTutorialComplete) BattleTutorial.Instance.NextTutorial("weaponskillsuccess");
        }

        public override void Die()
        {
            //Thread.Sleep(500);
            //SetSubState();
            //State.Main = MainState.Dead;
            //SetMainState();
            //SetEnmity(-1);

            //the command result is crashing the game. maybe the animation is wrong?
            ////this command result makes monster die instantly after killing blow.
            //List<CommandResult> commandresults = new List<CommandResult>
            //{
            //    new CommandResult(Id, 0, 0, 0x08080604, 1, 1), //die animation
            //    new CommandResult(Id, 0, 0xC755, 0, 0, 1), //enemy defeated message                
            //};

            //SendCommandResult(0, commandresults, 0x7C000062/*, senderId: attacker*/);

            //Disengage();
        }

        public override void AddEnmity(ActorBattle attacker){}

        public byte[] ToLobbyData()
        {
            byte[] characterData = new byte[0x1D0];

            Zone currentZone = GetCurrentZone();
            uint zoneId = currentZone is ZoneInstance instance ? instance.ZoneId : currentZone.Id;   
            byte[] name = Encoding.ASCII.GetBytes(Encoding.ASCII.GetString(Name).Trim(new[] { '\0' }));
            byte[] gearSet = Appearance.ToBytes();
            byte[] worldName = GameServer.GetNameBytes(WorldId); // WorldFactory.GetWorld(character.WorldId).Name);           

            Buffer.BlockCopy(BitConverter.GetBytes(zoneId), 0, characterData, 0xC, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, characterData, 0x04, 0x04);                 
            Buffer.BlockCopy(name, 0, characterData, 0x10, name.Length);
            Buffer.BlockCopy(worldName, 0, characterData, 0x30, worldName.Length);
                        
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
                    bw.Write(Appearance.BaseModel);
                    bw.Write(Appearance.Size);
                    bw.Write(Appearance.SkinColor | (uint)(Appearance.HairColor << 10) | (uint)(Appearance.EyeColor << 20));
                    bw.Write(BitField.PrimitiveConversion.ToUInt32(Appearance.Face));
                    bw.Write(Appearance.HairHighlightColor | (uint)(Appearance.HairStyle << 10) | Appearance.Face.CharacteristicsColor << 20);
                    bw.Write(Appearance.Voice);
                    bw.Write(gearSet);
                    bw.Write((ulong)0);
                    bw.Write((uint)0x01);
                    bw.Write((uint)0x01);
                    bw.Write(CharaWork.CurrentJob.Id);
                    bw.Write(CharaWork.CurrentJob.Level);
                    bw.Write(CharaWork.CurrentJob.Id);
                    bw.Write((ushort)0x01); //Job level?
                    bw.Write(Tribe);
                    bw.Write(0xe22222aa); //??
                    bw.Write(0x0000000a); //size of the string below
                    bw.Write(Encoding.ASCII.GetBytes("prv0Inn01\0")); //figure out if this can change
                    bw.Write(0x00000011); //size of the string below
                    bw.Write(Encoding.ASCII.GetBytes("defaultTerritory\0")); //figure out if this can change
                    bw.Write(Guardian);
                    bw.Write(BirthMonth);
                    bw.Write(BirthDay);
                    bw.Write((ushort)0x17); //??
                    bw.Write((uint)0x04); //??
                    bw.Write((uint)0x04); //??
                    bw.Seek(0x10, SeekOrigin.Current);
                    bw.Write(InitialTown);
                    bw.Write(InitialTown);
                }
              
                base64Info = Encoding.ASCII.GetBytes(Convert.ToBase64String(base64Info).Replace('+', '-').Replace('/', '_'));                
                Buffer.BlockCopy(base64Info, 0, characterData, 0x40, base64Info.Length);
            }

            return characterData;
        }
    }
}

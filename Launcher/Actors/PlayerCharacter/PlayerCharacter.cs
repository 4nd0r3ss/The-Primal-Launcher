using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Linq;

namespace PrimalLauncher
{
    [Serializable]
    public class PlayerCharacter : Actor
    {
        public CharaWork CharaWork { get; set; }

        #region Info              
        public byte WorldId { get; set; }
        public byte Slot { get; set; }
        public long TotalPlaytime { get; set; }
        public bool IsNew { get; set; }
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
        public byte CurrentClassId { get; set; }
        #endregion

        public uint Anima { get; set; }
        public Inventory Inventory { get; set; }
        public Journal Journal { get; set; }
        public Dictionary<byte, Job> Jobs { get; set; } = Job.LoadAll();
        public OrderedDictionary GeneralParameters { get; set; }

        public List<Group> Groups { get; set; } = new List<Group>();      
        
        public List<Quest> QuestsFinished { get; set; } = new List<Quest>();
        public Queue<byte[]> PacketQueue { get; set; }

        public string ChocoboName { get; set; }
        public ChocoboAppearance ChocoboAppearance { get; set; }
        public bool HasGobbue { get; set; }

        private uint SpawnDistance { get; set; }

        public void Setup(byte[] data)
        {
            //Character ID
            Id = NewId();
            IsNew = true;

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
            Appearance.Face = new Face
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
            CurrentClassId = data[0x2a];
            Jobs[CurrentClassId].Level = 1; //having a class level > 0 makes it active.
            Jobs[CurrentClassId].IsCurrent = true; //current class the player will start with.                        
            LoadInitialEquipment();

            Position = EntryPoints.GetStartPosition(InitialTown);
            Journal = new Journal(InitialTown);
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
            int equipmentSetNumber = (Tribe * 100) + CurrentClassId;
            uint underShirtId = (uint)8040000 + Tribe;
            uint underGarmentId = (uint)8060000 + Tribe;
            DataTable defaultSet = GameData.Instance.GetGameData("boot_skillequip");
            uint[] itemGraphicIds = defaultSet.Select("id = '" + equipmentSetNumber + "'")[0].ItemArray.Select(Convert.ToUInt32).ToArray();   
            
            Appearance.SetToSlots(itemGraphicIds, underShirtId, underGarmentId);
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
                Packet.Send(handler, ServerOpcode.PlayerCommand, data);
            }
        }

        public void SetUnendingJourney(Socket sender)
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

            Packet.Send(sender, ServerOpcode.UnendingJourney, data);
        }

        public void SetEntrustedItems(Socket sender)
        {
            byte[] data = new byte[0x30];

            for (int i = 0; i < 15; i++)
                data[i] = 0xFF;

            data[0xf] = 0x3F;

            Packet.Send(sender, ServerOpcode.EntrustedItems, data);

        }

        public void SelectTarget(Socket handler, byte[] srcData)
        {
            uint targetId = (uint)(srcData[0x13] << 24 | srcData[0x12] << 16 | srcData[0x11] << 8 | srcData[0x10]);
            uint unknown = (uint)(srcData[0x17] << 24 | srcData[0x16] << 16 | srcData[0x15] << 8 | srcData[0x014]);

            CurrentTarget = targetId; //store target id
            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes(CurrentTarget), 0, data, 0, sizeof(uint));
            Packet.Send(handler, ServerOpcode.SetTarget, data);
        }

        public void Spawn(Socket sender, ushort spawnType = 0x01, ushort isZoning = 0)
        {
            PacketQueue = null;
            State.Main = MainState.Passive;
            SpawnDistance = 30;

            //remove later
            if (CharaWork == null)
                CharaWork = new CharaWork();
                        
            CreateActor(sender, 0x08);
            CommandSequence(sender);
            SetSpeeds(sender);
            GetPosition(sender, spawnType, isZoning);
            SetAppearance(sender);
            SetName(sender, -1, Name); //-1 = it's a custom name.
            SendUnknown(sender);
            SetMainState(sender);
            SetSubState(sender);
            SetAllStatus(sender);
            SetIcon(sender);
            SetIsZoning(sender, false);

            SetGrandCompany(sender);
            SetTitle(sender);
            SendCurrentJob(sender);
            SpecialEventWork(sender);
            SetMounts(sender);
            AchievementPoints(sender);
            AchievementsLatest(sender);
            AchievementsCompleted(sender);
            LoadLuaParameters();
            SetLuaScript(sender);           
            Inventory.Send(sender);
            Work(sender);
            StartPlayTime();

           
        }

        public Zone GetCurrentZone() => World.Instance.Zones.Find(x => x.Id == Position.ZoneId);

        public void SetGrandCompany(Socket sender)
        {           
            Packet.Send(sender, ServerOpcode.SetGrandCompany, new byte[] { 0x02, 0x7F, 0x0B, 0x7F, 0x00, 0x00, 0x00, 0x00 });
        }

        public void SetTitle(Socket sender)
        {
            Packet.Send(sender, ServerOpcode.SetTitle, new byte[] { 0x8f, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
        }

        public void SetMounts(Socket sender)
        {
            ChocoboName = "Boko";
            HasGobbue = true;

            if (!string.IsNullOrEmpty(ChocoboName))
            {
                Packet.Send(sender, ServerOpcode.SetChocoboName, Encoding.ASCII.GetBytes(ChocoboName));
                Packet.Send(sender, ServerOpcode.SetHasChocobo, new byte[] { 0x1f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            }
            
            if(HasGobbue)
                Packet.Send(sender, ServerOpcode.SetHasGobbue, new byte[] { 0x1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
        }

        public static void Logout(Socket sender) => Packet.Send(sender, ServerOpcode.Logout, new byte[0x08]);

        public static void ExitGame(Socket sender) => Packet.Send(sender, ServerOpcode.ExitGame, new byte[0x08]);

        public void InitializeOpening(Socket sender)
        {
            if (TotalPlaytime == 0)
            {                
                World.Instance.Directors.Add(new OpeningDirector());
                World.Instance.Directors.Add(new QuestDirector());    
                //Journal.Quests[0].StartPhase(sender);
                OpeningDirector director = (OpeningDirector)World.Instance.GetDirector("Opening");
                director.Spawn(sender);
                director.StartEvent(sender);
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

        public void BindQuestDirector(Socket sender, string questName, bool startEvent = false)
        {
            QuestDirector questDirector = ((QuestDirector)World.Instance.GetDirector("Quest"));

            if(startEvent)
                questDirector.StartEvent(sender);

            questDirector.Spawn(sender, questName);
            User.Instance.Character.LuaParameters = null;
            User.Instance.Character.LoadLuaParameters(questDirector.Id);
        }

        #region Group Methods
        public void GetGroups(Socket sender)
        {

            if (Groups.Count == 0) //dirty...
            {
                Groups.Add(new PartyGroup());
                Groups.Add(new RetainerGroup());
            }

            foreach (var group in Groups)
            {
                if(!(group is DutyGroup))
                    group.SendPackets(sender);
            }                            
          
            ActiveLinkshell(sender);
        }

        public void GetGroupInitWork(Socket sender, byte[] data)
        {
            ulong groupId = 0;

            using (MemoryStream stream = new MemoryStream(data))
            using(BinaryReader br = new BinaryReader(stream))
            {
                br.ReadUInt64();
                br.ReadUInt64();
                groupId = br.ReadUInt64();
            }

            Groups.Find(x => x.Id == groupId).InitWork(sender);            
        }

        public void AddDutyGroup(Socket sender, List<uint> membersClassId, bool addQuestDirector = false)
        {
            DutyGroup dutyGroup = new DutyGroup();

            if(addQuestDirector)
                dutyGroup.MemberList.Add(((QuestDirector)World.Instance.GetDirector("Quest")).Id);

            dutyGroup.AddMembers(GetCurrentZone().GetActorsByClassId(membersClassId));
            dutyGroup.InitializeGroup(sender);
            dutyGroup.SendPackets(sender);
            Groups.RemoveAll(x => x.GetType().Name == "DutyGroup"); //remove later
            Groups.Add(dutyGroup);
        }
        #endregion

        public void ActiveLinkshell(Socket sender)
        {
            byte[] data = new byte[0x78];

            Buffer.BlockCopy(BitConverter.GetBytes(0x4e22), 0, data, 0x40, sizeof(ushort));           

            Packet.Send(sender, ServerOpcode.ActiveLinkshell, data);
        }

        private void SpecialEventWork(Socket handler)
        {
            byte[] data = new byte[0x18];
            data[0x02] = 0x12;
            Packet.Send(handler, ServerOpcode.SetSpecialEventWork, data);
        }

        #region Achievement Methods
        private void AchievementPoints(Socket handler)
        {
            byte[] data = new byte[0x08];
            Packet.Send(handler, ServerOpcode.AchievementPoints, data);
        }

        private void AchievementsLatest(Socket handler)
        {
            byte[] data = new byte[0x20];
            Packet.Send(handler, ServerOpcode.AchievementsLatest, data);
        }

        private void AchievementsCompleted(Socket handler)
        {
            byte[] data = new byte[0x80];
            Packet.Send(handler, ServerOpcode.AchievementsCompeted, data);
        }
        #endregion

        #region Work Methods
        public void Work(Socket sender)
        {
            WorkProperties property = new WorkProperties(sender, Id, @"/_init");
           

            property.Add("charaWork.eventSave.bazaarTax", (byte)5);
            property.Add("charaWork.battleSave.potencial", 6.6f);

            for (int i = 0; i < 32; i++)
                if (i < 5 && i != 3) property.Add(string.Format("charaWork.property[{0}]", i), (byte)1);
                       
            AddWorkClassParameters(ref property);
            AddStatusShownTime(ref property);
            AddGeneralParameters(ref property);

            property.Add("charaWork.battleTemp.castGauge_speed[0]", 1.0f);
            property.Add("charaWork.battleTemp.castGauge_speed[1]", 0.25f);
            property.Add("charaWork.commandBorder", CharaWork.CommandBorder);
            property.Add("charaWork.battleSave.negotiationFlag[0]", true);

            AddWorkCommand(ref property);               

            for (int i = 0; i < 64; i++)
                property.Add(string.Format("charaWork.commandCategory[{0}]", i), (byte)1);

            //for (int i = 0; i < 4096; i++)
                property.Add(string.Format("charaWork.commandAcquired[{0}]", 1150), true);

            //job abilities
            for (int i = 0; i < 36; i++)
                property.Add(string.Format("charaWork.additionalCommandAcquired[{0}]", i), true);

            for (int i = 0; i < 40; i++)
                property.Add(string.Format("charaWork.parameterSave.commandSlot_compatibility[{0}]", i), true);
                        
            AddWorkSystem(ref property);

            Journal.AddToWork(ref property);            
            AddWorkNpcLinkshell(ref property);
            property.Add("playerWork.restBonusExpRate", 0f);
            AddWorkCharacterBackground(ref property);
            property.FinishWriting();
        }

        
        private void AddStatusShownTime(ref WorkProperties property)
        {
            //status buff/ailment timer? database.cs ln 892
            //property.Add(string.Format("charaWork.statusShownTime[{0}]", i), );
        }

        private void AddGeneralParameters(ref WorkProperties property)
        {
            //Write character's parameters
            for (int i = 0; i < GeneralParameters.Count; i++)
            {
                if ((ushort)GeneralParameters[i] > 0)
                    property.Add(string.Format("charaWork.battleTemp.generalParameter[{0}]", i), GeneralParameters[i]);
            }
        }

        private void AddWorkCommand(ref WorkProperties property)
        {            
            for (int i = 0; i < CharaWork.CommandBorder; i++)            
                if (CharaWork.Command[i] != 0)               
                    property.Add(string.Format("charaWork.command[{0}]", i), 0xA0F00000 | CharaWork.Command[i]);

            //hotbar
           // for(int i = CharaWork.CommandBorder; i < (CharaWork.CommandBorder + Jobs[CurrentClassId].Hotbar.Length); i++)
            for(int i = 0; i < Jobs[CurrentClassId].Hotbar.Length; i++)
            {
                if (Jobs[CurrentClassId].Hotbar[i] != 0)
                    property.Add(string.Format("charaWork.command[{0}]", CharaWork.CommandBorder + i), 0xA0F00000 | Jobs[CurrentClassId].Hotbar[i]);
            }
            //add hotbar here
            //if (i >= commandBorder)
            //{
            //    property.Add(string.Format("charaWork.parameterTemp.maxCommandRecastTime[{0}]", i - commandBorder), (ushort)5);
            //    property.Add(string.Format("charaWork.parameterSave.commandSlot_recastTime[{0}]", i - commandBorder), (uint)(Server.GetTimeStamp() + 5));
            //}
        }

        private void AddWorkClassParameters(ref WorkProperties property)
        {
            Job currentClass = Jobs[CurrentClassId];
            property.Add("charaWork.parameterSave.hp[0]", currentClass.Hp); 
            property.Add("charaWork.parameterSave.hpMax[0]", currentClass.MaxHp);
            property.Add("charaWork.parameterSave.mp", currentClass.Mp);
            property.Add("charaWork.parameterSave.mpMax", currentClass.MaxMp);
            property.Add("charaWork.parameterTemp.tp", currentClass.Tp);
            property.Add("charaWork.parameterSave.state_mainSkill[0]", currentClass.Id);
            property.Add("charaWork.parameterSave.state_mainSkillLevel", currentClass.Level);
        }
               
        private void AddWorkSystem(ref WorkProperties property)
        {
            property.Add("charaWork.parameterTemp.forceControl_float_forClientSelf[0]", 1.0f);
            property.Add("charaWork.parameterTemp.forceControl_float_forClientSelf[1]", 1.0f);
            property.Add("charaWork.parameterTemp.forceControl_int16_forClientSelf[0]", (short)-1);
            property.Add("charaWork.parameterTemp.forceControl_int16_forClientSelf[1]", (short)-1);
            property.Add("charaWork.parameterTemp.otherClassAbilityCount[0]", (byte)4);
            property.Add("charaWork.parameterTemp.otherClassAbilityCount[1]", (byte)5);
            property.Add("charaWork.parameterTemp.giftCount[1]", (byte)5);
            property.Add("charaWork.depictionJudge", CharaWork.DepictionJudge);
        }      

        private void AddWorkNpcLinkshell(ref WorkProperties property)
        {
            //NPC linkshell
            //for(int i = 0; i < 64; i++)
            //{
            //    //if(npcLinkshellCalling[i])
            //    //    property.Add(string.Format("playerWork.npcLinkshellChatCalling[{0}]", i), true);

            //    //if (npcLinkshellExtra[i])
            //    //    property.Add(string.Format("playerWork.npcLinkshellChatExtra[{0}]", i), true);
            //}
        }

        private void AddWorkCharacterBackground(ref WorkProperties property)
        {
            //From PlayerCharacter obj
            property.Add("playerWork.tribe", Tribe);
            property.Add("playerWork.guardian", Guardian);
            property.Add("playerWork.birthdayMonth", BirthMonth);
            property.Add("playerWork.birthdayDay", BirthDay);
            property.Add("playerWork.initialTown", (byte)InitialTown);
        }
        #endregion Work Methods

        #region Exp & Level Methods
        public void UpdateExp(Socket sender)
        {
            WorkProperties prop = new WorkProperties(sender, Id, "charaWork/battleStateForSelf");
            prop.Add("charaWork.battleSave.skillPoint[" + (CurrentClassId - 1) + "]", (int)Jobs[CurrentClassId].TotalExp);
            prop.FinishWriting();
        }

        public byte[] ClassExp(Socket sender)
        {
            if (PacketQueue == null || PacketQueue.Count == 0)
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
                    jobLevelCap.Enqueue(job.LevelCap);
                }

                WorkProperties property = new WorkProperties(sender, Id, @"charaWork/exp");
                property.Add("charaWork.battleSave.skillLevel", jobLevel);
                property.Add("charaWork.battleSave.skillLevelCap", jobLevelCap, true);
                PacketQueue = property.PacketQueue;
            }

            return PacketQueue.Dequeue();
        }
        
        public void AddExp(Socket sender, int exp)
        {
            //we want to add exp only if level is below cap.
            if (Jobs[CurrentClassId].Level < Jobs[CurrentClassId].LevelCap)
            {
                //add exp bonus multiplier TODO:put multiplier definition somewhere else (add as an option in UI?)
                float expBonus = 1.2f;
                Jobs[CurrentClassId].TotalExp += Convert.ToInt64(exp * expBonus);

                //send add exp command result
                SendActionResult(sender, 0, new List<CommandResult> {
                    new CommandResult
                    {
                        TargetId = Id,
                        Amount = (ushort)(exp * expBonus),
                        TextId = 33934,
                        Param = (byte)(expBonus > 1 ? ((expBonus -1) * 100) : 0)
                    }
                });

                //calculate leveling
                long totalExp = Jobs[CurrentClassId].TotalExp;
                short currentLevel = Jobs[CurrentClassId].Level;
                short levelsToUp = 0;

                while (totalExp >= Job.ExpTable[currentLevel])
                {
                    totalExp -= Job.ExpTable[currentLevel];
                    levelsToUp++;
                }

                if (levelsToUp > 0)
                {
                    Jobs[CurrentClassId].TotalExp = (currentLevel + levelsToUp) >= Jobs[CurrentClassId].LevelCap ? 0 : totalExp;
                    LevelUp(sender, levelsToUp);
                }

                //refresh exp values in game client UI.
                UpdateExp(sender);
            }
        }

        private void LevelUp(Socket sender, short numLevels)
        {
            Jobs[CurrentClassId].Level += numLevels;

            SendActionResult(sender, 0, new List<CommandResult> {
                new CommandResult
                {
                    TargetId = Id,
                    Amount = (ushort)Jobs[CurrentClassId].Level,
                    TextId = 33909
                }
            });

            UpdateLevel(sender);
            World.Instance.SetMusic(sender, 0x52, MusicMode.Play);
        }

        public void LevelDown(Socket sender, short toLevel)
        {
            if (toLevel > 0)
            {
                Jobs[CurrentClassId].Level = toLevel;
                Jobs[CurrentClassId].TotalExp = 0;
                UpdateLevel(sender);
                UpdateExp(sender);
            }
        }

        private void UpdateLevel(Socket sender)
        {
            WorkProperties property = new WorkProperties(sender, Id, @"charaWork/stateForAll");
            property.Add("charaWork.battleSave.skillLevel[" + (CurrentClassId - 1) + "]", Jobs[CurrentClassId].Level);
            property.Add("charaWork.parameterSave.state_mainSkillLevel", Jobs[CurrentClassId].Level);
            property.FinishWriting();
        }
        #endregion

        private void UpdateClass(Socket sender)
        {
            WorkProperties property = new WorkProperties(sender, Id, @"charaWork/stateForAll");
            property.Add("charaWork.parameterSave.state_mainSkill[0]", CurrentClassId);
            property.Add("charaWork.parameterSave.state_mainSkillLevel", Jobs[CurrentClassId]);
            property.FinishWriting();
        }

        public void SendActionResult(Socket sender, Command command, List<CommandResult> resultList = null, uint animationId = 0, uint unknown = 0)
        {
            int numResults = resultList != null ? resultList.Count : 0;

            byte[] data = new byte[0x38];

            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(animationId), 0, data, 0x04, sizeof(uint));
            //Buffer.BlockCopy(BitConverter.GetBytes(unknown), 0, data, 0x1c, sizeof(uint));

            if (resultList != null)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(numResults), 0, data, 0x20, sizeof(int)); //#results
                Buffer.BlockCopy(BitConverter.GetBytes((short)command), 0, data, 0x24, sizeof(short));
                Buffer.BlockCopy(BitConverter.GetBytes((unknown > 0 ? unknown : 0x0810)), 0, data, 0x26, sizeof(ushort)); //unknown

                foreach (var result in resultList)
                {
                    byte[] resultBytes = result.ToBytes();
                    Buffer.BlockCopy(resultBytes, 0, data, 0x28, resultBytes.Length); //unknown
                }
            }

            Packet.Send(sender, ServerOpcode.CommandResult01, data);
        }   
        
        public void ToggleZoneActors(Socket sender)
        {
            foreach (Actor actor in GetCurrentZone().Actors)
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
                        actor.Spawn(sender);
                }
                else
                {
                    //if actor is NOT inside the player's square, set it do despawn.
                    if (actor.Spawned)
                        actor.Despawn(sender);
                }
            }
        }

        public void ToggleMount(Socket sender, Command command, bool isChocobo)
        {         
            //set player state
            byte[] data = new byte[0x08];
            byte[] mountTextSheet = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0xF8, 0x5F, 0x91, 0x65, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte bgm = 0x53;

            if (command == Command.Mount)
            {
                if (isChocobo)
                {
                    data[0x05] = (byte)ChocoboAppearance.Maelstrom4;     
                    Packet.Send(sender, ServerOpcode.SetChocoboMounted, data);
                }
                else
                {
                    data = new byte[0x28];
                    data[0] = 0x01;
                    Packet.Send(sender, ServerOpcode.SetGobbueMounted, data);
                    bgm = 0x98;
                }

                Speeds.SetMounted();
            }
            else
            {
                mountTextSheet[0x08] = 0x93;
                State.Main = MainState.Passive;
                SetMainState(sender);
                Speeds.SetUnmounted();
            }

            //change to chocobo bgm
            World.Instance.SetMusic(sender, (ushort)(command == Command.Mount ? bgm : World.Instance.Zones.Find(x => x.Id == Position.ZoneId).GetCurrentBGM()), MusicMode.FadeStart);

            //TODO: send command answer - this should probably be in event manager.
            data = new byte[0x28];
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(0x7c000062), 0, data, 0x4, 4);
            data[0x27] = 0x08; //8 animation slots to be played in sequence?
            Packet.Send(sender, ServerOpcode.CommandResult, data);      

            SetSpeeds(sender);
            SetSpeeds(sender);
            SetSpeeds(sender);

            World.Instance.SendTextSheetMessage(sender, ServerOpcode.TextSheetMessage30b, mountTextSheet);

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
            Packet.Send(sender, ServerOpcode.CommandResultX1, data);
        }

        public void ToggleStance(Socket sender, Command command)
        {
            if(command == Command.BattleStance)           
                State.Main = MainState.Active;            
            else           
                State.Main = MainState.Passive;

            CommandResult cr = new CommandResult
            {
                TargetId = User.Instance.Character.Id,
                EffectId = 1,
                Sequence = 1
            };

            SetMainState(sender);
            SendActionResult(sender, command, new List<CommandResult> { cr }, 0x72000062, 0x032A);           
        }

        public void EquipSoulStone(Socket sender, byte[] data)
        {
            if (CurrentJobId == 0)
            {
                byte jobIndex = 13;

                if (CurrentClassId == 7 || CurrentClassId == 8)
                    jobIndex = 11;
                else if (CurrentClassId == 22 || CurrentClassId == 23)
                    jobIndex = 4;

                CurrentJobId = (byte)(CurrentClassId + jobIndex);
                SendCurrentJob(sender);
                PlayAnimationEffect(sender, Job.AnimationEffectId(CurrentJobId));

                //send text sheet should be a method in the world class.
                byte[] text = new byte[]
                {
                0x41, 0x29, 0x9B, 0x02, 0x01, 0x00, 0xF8, 0x5F, 0x97, 0x75, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x02, 0x9B, 0x29, 0x41, 0x00, 0x00, 0x00, 0x00, 0x10,
                0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                };
                Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, text, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(Id).Reverse().ToArray(), 0, text, 0x17, 4);
                Packet.Send(sender, ServerOpcode.TextSheetMessage50b, text, sourceId: 0x5ff80001);

                //SetSubState(sender, 0x0c);
                SetSpeeds(sender);
                SetSpeeds(sender);
                SetSpeeds(sender);

                SendActionResult(sender, Command.EquipSoulStone, new List<CommandResult> {
                    new CommandResult
                    {
                        TargetId = Id,
                        EffectId = 1,
                        Sequence = 1
                    }
                }, 0x7c000062);
            }
            else
            {
                CurrentJobId = 0;
                SendCurrentJob(sender);
                PlayAnimationEffect(sender, AnimationEffect.ChangeClass);
            }

            UpdateLevel(sender);
            UpdateClass(sender);
            UpdateExp(sender);
        }

        private void SendCurrentJob(Socket sender)
        {
            byte[] data = new byte[0x08];
            data[0] = CurrentJobId;
            Packet.Send(sender, ServerOpcode.SetCurrentJob, data);
        }
               
        public void ChangeGear(Socket sender, byte[] data)
        {
            //we read the bytes in the index below to be able to differentiate equip/unequip packets. It's the fastest way I can think of.
            uint pattern = (uint)(data[0x53] << 24 | data[0x52] << 16 | data[0x51] << 8 | data[0x50]);
            bool isEquipping = pattern == 0x05050505 ? true : false;
            byte gearSlot = 0;
            uint itemUniqueId = 0;

            if (isEquipping)
            {
                gearSlot = (byte)(data[0x58] - 1);
                itemUniqueId = (uint)(data[0x5e] << 24 | data[0x5f] << 16 | data[0x60] << 8 | data[0x61]);

                //if a weapon is being equipped
                if (gearSlot == 0)
                {
                    Item weaponToEquip = Inventory.GetBagItemByUniqueId(itemUniqueId);

                    if (weaponToEquip == null)
                    {
                        Log.Instance.Error("Something went wrong... The requested item wasn't found in the inventory.");
                        return;
                    }

                    Item equippedWeapon = (Item)Inventory.Bag[Inventory.GearSlots[0]];
                    ushort equippedCategory = Convert.ToUInt16(equippedWeapon.Id.ToString().Substring(0, 3));
                    ushort toEquipCategory = Convert.ToUInt16(weaponToEquip.Id.ToString().Substring(0, 3));

                    if (equippedCategory != toEquipCategory)
                    {
                        byte jobToChangeTo = Job.Category[toEquipCategory];
                        CurrentClassId = jobToChangeTo;

                        //for now, if the job is not activated, activate it.
                        short level = Jobs[CurrentClassId].Level;
                        Jobs[CurrentClassId].Level = level == 0 ? (short)1 : level;

                        //if a soul stone is equipped, remove it.
                        if (CurrentJobId != 0)
                        {
                            CurrentJobId = 0;
                            SendCurrentJob(sender);
                        }

                        data = new byte[] { 0x41, 0x29, 0x9B, 0x02, 0x01, 0x00, 0xF8, 0x5F, 0x89, 0x77, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00,
                                            0x02, 0x00, 0x00, 0x6B, 0x1E, 0x4C, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
                                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0F,
                                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                        Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0, 4);
                        Packet.Send(sender, ServerOpcode.TextSheetMessage70b, data, sourceId: 0x5ff80001);

                        SendActionResult(sender, Command.ChangeEquipment, new List<CommandResult> {
                            new CommandResult
                            {
                                TargetId = Id,
                                EffectId = 1,
                                Sequence = 1
                            }
                        }, 0x7c000062, 0x40000000);

                        UpdateLevel(sender);
                        UpdateClass(sender);
                        UpdateExp(sender);
                        Inventory.ChangeGear(sender, gearSlot, itemUniqueId);
                    }

                    return;
                }
            }
            else
                gearSlot = (byte)(data[0x51] - 1);

            Inventory.ChangeGear(sender, gearSlot, itemUniqueId);
        }

        public void PlayAnimationEffect(Socket sender, AnimationEffect animation)
        {
            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)animation), 0, data, 0, sizeof(ushort));           
            data[0x03] = 0x04;
            Packet.Send(sender, ServerOpcode.PlayAnimationEffect, data);
        }
        
        public void ToggleUIControl(Socket sender, UIControl control, uint unknown = 0x02)
        {
            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes((uint)control), 0, data, 0, sizeof(uint)); // (0x02 & 0xff);           
            Buffer.BlockCopy(BitConverter.GetBytes(unknown), 0, data, 0x04, sizeof(uint)); // (0x02 & 0xff);           
            Packet.Send(sender, ServerOpcode.SetUIControl, data);
        }

        public void GetBlackList(Socket sender)
        {
            byte[] data = new byte[0x666];
            Buffer.BlockCopy(BitConverter.GetBytes(1), 0, data, 0x04, sizeof(uint));         
            Buffer.BlockCopy(Encoding.ASCII.GetBytes("Test2"), 0, data, 0x08, sizeof(uint));   
            Packet.Send(sender, ServerOpcode.SendBlackList, data);
        }

        public void GetFriendlist(Socket sender)
        {
            byte[] data = new byte[0x666];
            Buffer.BlockCopy(BitConverter.GetBytes(1), 0, data, 0x04, sizeof(uint));
            Buffer.BlockCopy(Encoding.ASCII.GetBytes("Test"), 0, data, 0x08, sizeof(uint));           
            Packet.Send(sender, ServerOpcode.SendFriendList, data);
        }

        #region Position Methods
        public void GoForward(Socket sender, float distance)
        {
            Position.X += (float)(distance * Math.Sin(Position.R));
            Position.Z += (float)(distance * Math.Cos(Position.R));
            SetPosition(sender);
        }

        public void TurnBack(Socket sender, float distance)
        {
            Position.R = (3.2f - Math.Abs(Position.R)) * (Position.R <= 0 ? 1 : -1);
            GoForward(sender, distance);
        }

        public void GetPosition(Socket sender, ushort spawnType = 0, ushort isZonning = 0)
        {
            base.SetPosition(sender, spawnType, isZonning, true);
        }

        public void SetPosition(Socket sender, uint zoneId, float x, float y, float z, float r, ushort spawnType)
        {
            Position position = new Position(zoneId, x, y, z, r, spawnType);
            Position = position;
            base.SetPosition(sender, spawnType, 0, true);
        }

        public void SetPosition(Socket sender, string offsetStr)
        {
            float[] offset = Array.ConvertAll(offsetStr.Split(new char[] { ',' }), float.Parse);
            World.Instance.MapUIChange(sender, 0x10);
            SetPosition(sender, (uint)offset[0], offset[1], offset[2], offset[3], offset[4], (ushort)offset[5]);
        }

        public void UpdatePosition(Socket sender, byte[] data)
        {
            //get player character
            PlayerCharacter pc = User.Instance.Character;

            //position from packet
            float x = BitConverter.ToSingle(new byte[] { data[0x18], data[0x19], data[0x1a], data[0x1b] }, 0);
            float y = BitConverter.ToSingle(new byte[] { data[0x1c], data[0x1d], data[0x1e], data[0x1f] }, 0);
            float z = BitConverter.ToSingle(new byte[] { data[0x20], data[0x21], data[0x22], data[0x23] }, 0);
            float r = BitConverter.ToSingle(new byte[] { data[0x24], data[0x25], data[0x26], data[0x27] }, 0);

            //execute toggle zone actors only if position changes
            if(x != pc.Position.X || y != pc.Position.Y || z != pc.Position.Z)
                ToggleZoneActors(sender);

            //get player position from packet
            pc.Position.X = x;
            pc.Position.Y = y;
            pc.Position.Z = z;
            pc.Position.R = r;

            //byte[] moveState = new byte[] { data[0x28], data[0x29] }; //unused so far. maybe part of mouse positioning?
            //byte[] mousePosition = byte[] { data[0x2a], data[0x2b] }; //2d mouse cursor hud position?
            //byte[] cameraRotation = new byte[] { data[0x2c], data[0x2d], data[0x2e], data[0x2f] }; //indicates the camera rotation (maybe it's 2 shorts?).

            //save player position 
            User.Instance.AccountList[0].CharacterList[pc.Slot] = pc;
            User.Instance.Save();
        }     
        #endregion

        public uint NewId()
        {
            Random rnd = new Random();   
            return (uint)rnd.Next(0xff, 0xffff);
        }

        public void Unknown0x02(Socket sender)
        {
            byte[] data = new byte[0x10];
            Buffer.BlockCopy(BitConverter.GetBytes(User.Instance.Character.Id), 0, data, 0x08, 0x04);
            Packet.Send(sender, ServerOpcode.Unknown0x02, data);           
        }       
    }
       
    public static class GeneralParameter
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
        /// This method creates a dictionary with all the attribute points alloted to a slot identified by name. It was modeled like that so the values can be edited.
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

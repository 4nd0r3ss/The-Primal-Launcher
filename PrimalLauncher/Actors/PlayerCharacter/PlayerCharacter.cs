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
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    [Serializable]
    public class PlayerCharacter : ActorBattle
    {
        public byte WorldId { get; set; }       
        private int CurrentTitle { get; set; }

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
        public NpcLinkshell Linkshell { get; set; }
        public List<GroupBase> Groups { get; set; } //= new List<GroupBase>();       
        public Achievements Achievements { get; set; }

        public override GroupBase BattleGroup { get => GetPartyGroup(); }

        public void Setup(byte[] data)
        {            
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
            CharaWork.CurrentClass.Level = 1; //having a class level > 0 makes it active.
            CharaWork.CurrentClass.IsCurrent = true; //current class the player will start with.                        
            LoadInitialEquipment();

            //work
            Journal = new Journal(InitialTown);
            Linkshell = new NpcLinkshell();     
            Achievements = new Achievements();
            
            //travel
            Anima = 100;
            Position = EntryPoints.GetStartPosition(InitialTown);
            uint homePoint = Aetheryte.GetStartHomePoint(InitialTown);
            AttunedAetherytes = new List<uint>{ homePoint };
            HomePoint = homePoint;

            //battle
            AutoAttackDelay = 4 * 1000; //4 seconds delay

            //add fixed groups
            Groups = new List<GroupBase>();
            Groups.Add(new PartyGroup());
            Groups.Add(new RetainerGroup());            
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

        //put this on job class
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
            SubState.Chant = 0;

            CharaWork.PacketQueue = null;
            State.Main = MainState.Passive;
            SpawnDistance = 40;
            //Icon = 0x00_02_00_00;
            TargetId = 0;
            CharaWork.CurrentClass.Tp = 0;
            CharaWork.CurrentClass.Hp = CharaWork.CurrentClass.MaxHp;
                   
            //in case the player quit the game while monted.
            Speeds.SetUnmounted();
            Journal.InitializeQuests();
            CreateActor(0x08);
            CharaWork.CommandSequence();
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
            SendTitle();
            SendCurrentJob();
            SendSpecialEventWork();
            SetMounts();
            Achievements.Send();
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

        public void SetTitle(byte[] data)
        {
            CurrentTitle = data.GetInt32(0x10);
            SendTitle();
        }

        public void SendTitle()
        {
            byte[] data = new byte[0x08];
            data.Write(0, CurrentTitle);
            Packet.Send(ServerOpcode.SetTitle, data);
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
            foreach (var group in Groups)
            {
                group.SendPackets();
            }     
        }

        public PartyGroup GetPartyGroup()
        {
            return (PartyGroup)Groups.SingleOrDefault(x => x is PartyGroup);
        }
        
        #endregion      

        private void SendSpecialEventWork()
        {
            byte[] data = new byte[0x18];
            data[0x02] = 0x12;
            Packet.Send(ServerOpcode.SetSpecialEventWork, data);
        }
       
        public void Work()
        {
            WorkProperties property = new WorkProperties(Id, @"/_init", true);            

            CharaWork.AddToWork(ref property);
            Journal.AddToWork(ref property);            
            Linkshell.AddToWork(ref property);            
            AddPlayerWork(ref property);
            property.FinishWritingAndSend();
        }

        private void AddPlayerWork(ref WorkProperties property)
        {
            property.Add("playerWork.restBonusExpRate", 0f);
            property.Add("playerWork.tribe", Tribe);
            property.Add("playerWork.guardian", Guardian);
            property.Add("playerWork.birthdayMonth", BirthMonth);
            property.Add("playerWork.birthdayDay", BirthDay);
            property.Add("playerWork.initialTown", (byte)InitialTown);
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
            data.Write(0, Id);
            data.Write(0x4, 0x7c000062);           
            data[0x27] = 0x08; //8 animation slots to be played in sequence?
            Packet.Send(ServerOpcode.CommandResult, data);      

            SetSpeeds();
            SetSpeeds();
            SetSpeeds();

            World.SendTextSheet(mountTextSheet, source: Id);

            //TODO: command result - this should probably be in event manager.
            data = new byte[0x38];
            data.Write(0, Id);
            data.Write(0x4, 0x7c000062);
            data.Write(0x20, (uint)1);
            data.Write(0x24, (ushort)command);
            data.Write(0x26, (ushort)0x810);
            data.Write(0x28, Id);
            data[0x30] = 1;
            data[0x36] = 1;
            Packet.Send(ServerOpcode.CommandResultX1, data);
        }
             
        public void AddLoot(Dictionary<uint, int> rewardList)
        {

        }

        public void EquipSoulStone(byte[] data)
        {
            if (!CharaWork.CurrentClass.IsSoulStoneEquippped)
            {
                CharaWork.CurrentClass.IsSoulStoneEquippped = true;                

                //SetSubState(0x0c);
                SetSpeeds();
                SetSpeeds();
                SetSpeeds();

                SendCommandResult(Command.EquipSoulStone, new List<CommandResult> {
                    new CommandResult
                    {
                        TargetId = Id,
                        EffectId = EffectId.Default,
                        HitSequence = 1
                    }
                }, 0x7c000062);               
            }
            else
            {
                CharaWork.CurrentClass.IsSoulStoneEquippped = false;                          
            }
            
            SendCurrentJob();
            PlayAnimationEffect(Job.AnimationEffectId(CharaWork.CurrentJob.Id));
           
            CharaWork.UpdateLevel();
            CharaWork.UpdateClass();
            CharaWork.UpdateExp();
        }

        private void SendCurrentJob()
        {
            byte[] data = new byte[0x08];

            if(CharaWork.CurrentJob.Id != CharaWork.CurrentClassId)
                data[0] =  CharaWork.CurrentJob.Id;

            Packet.Send(ServerOpcode.SetCurrentJob, data);
        }
               
        public void ChangeEquipment(byte[] data)
        {
            File.WriteAllBytes("equip.txt", data);
            var luaParams = PrimalLauncher.LuaParameters.ReadParameters(data, 0x41);


            bool isEquipping = luaParams[0] != null;
            uint itemUniqueId = (uint)((long)luaParams[luaParams.Count - 1] & 0xFFFFFFFF);
            byte gearSlot;

            if (isEquipping)
            {
                gearSlot = (byte)(data[0x58] - 1);
                Item itemToEquip = Inventory.GetBagItemByUniqueId(itemUniqueId);
                World.SendTextSheet(0x7789, new object[] {/*quality?*/ 1, (int)itemToEquip.Id, 1, 0, 0, 1, 0 }, Id);

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
                        if (CharaWork.CurrentClass.Level == 0)
                            CharaWork.CurrentClass.Level = 1;


                        //if a soul stone is equipped, remove it.
                        if (CharaWork.CurrentClass.IsSoulStoneEquippped)
                            CharaWork.CurrentClass.IsSoulStoneEquippped = false;

                        SendCurrentJob();

                        SendCommandResult(Command.ChangeEquipment, new List<CommandResult> {
                            new CommandResult
                            {
                                TargetId = Id,
                                EffectId = EffectId.Default,
                                HitSequence = 1
                            }
                        }, 0x7c000062, 0x40000000);

                        CharaWork.UpdateLevel();
                        CharaWork.UpdateClass();
                        CharaWork.UpdateExp();
                        CharaWork.UpdateHotbar();

                        Inventory.ChangeGear(gearSlot, itemUniqueId);

                        ////check if character has soul of the current class                       
                        //DataTable jobsTable = GameData.Instance.GetGameData("xtx/text_jobName");
                        //DataRow[] selected = jobsTable.Select("ID = '" + CharaWork.GetClassJobId() + "'");
                        //string jobName = (string)selected[0][1];
                        //jobName = jobName.Substring(0, jobName.Length - 1);

                        if (Inventory.HasKeyItem("soul of the " + CharaWork.CurrentJob.Name))
                            EquipSoulStone(null);
                        else
                            PlayAnimationEffect(AnimationEffect.ChangeClass);

                        World.SendTextSheet(0x7597, new object[] { 0, 0, Id, (int)CharaWork.CurrentJob.Id }, Id);
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
                World.SendTextSheet(0x778A, new object[] {/*quality?*/ 1, (int)itemToUnequip.Id, 1, 0, 0, 1, 0 }, Id);
                Inventory.ChangeGear(gearSlot, itemUniqueId);
            }
        }
        
        public void ToggleUIControl(UIControl control, uint unknown = 0x02)
        {
            byte[] data = new byte[0x08];
            data.Write(0, (uint)control);// (0x02 & 0xff);
            data.Write(0x04, unknown);// (0x02 & 0xff);                     
            Packet.Send(ServerOpcode.SetUIControl, data);
        }

        public void GetBlackList()
        {
            byte[] data = new byte[0x666];
            data.Write(0x04, 1);
            data.Write(0x08, "Test2");            
            Packet.Send(ServerOpcode.SendBlackList, data);
        }

        public void GetFriendlist()
        {
            byte[] data = new byte[0x666];
            data.Write(0x04, 1);
            data.Write(0x08, "Test");                     
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

        public void CancelAction()
        {
            //cancel action if player moves
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
            {
                ToggleZoneActors();

                if (SubState.Chant > 0)
                    CancelAction();
            }          
                

            //this might be useful someday...
            //byte[] moveState = new byte[] { data[0x28], data[0x29] }; //unused so far. maybe part of mouse coords?
            //byte[] mousePosition = byte[] { data[0x2a], data[0x2b] }; //2d mouse cursor hud position?
            //byte[] cameraRotation = new byte[] { data[0x2c], data[0x2d], data[0x2e], data[0x2f] }; //indicates the camera rotation (maybe it's 2 shorts?).

            //save player position 
            User.Instance.SavePlayerCharacter(this);
        }     
        #endregion
               
        public void Unknown0x02()
        {
            byte[] data = new byte[0x10];
            data.Write(0x08, Id);            
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

        public bool CurrentZoneisInstance()
        {
            return GetCurrentZone() is ZoneInstance;
        }

        #region Targeting
        public void TargetSelect(byte[] data)
        {
            uint targetId = data.GetUInt32(0x10);
            uint unknown = data.GetUInt32(0x14);

            TargetLookAtPlayer(false);//release previous target 
            TargetId = targetId != 0xC0000000 ? targetId : 0;
            TargetLookAtPlayer(true);

            byte[] response = new byte[0x08];
            response.Write(0,TargetId);           
            Packet.Send(ServerOpcode.SetTarget, data);
        }

        private void TargetLookAtPlayer(bool lookAtPlayer)
        {
            var previousTarget = GetCurrentZone().GetActorById(TargetId);

            if (previousTarget != null && previousTarget != this && previousTarget is Monster)
                previousTarget.ToggleHeadDirection(lookAtPlayer);
        }

        public void TargetLockOn(byte[] data)
        {
            uint targetId = data.GetUInt32(0x10);
            uint unknown = data.GetUInt32(0x14); //unused so far

            if (targetId != 0xC0000000 && State.Main == MainState.Active) //0xC0000000 = no target
            {
                LockOnTarget = true;
                AutoAttack();
            }
            else
            {
                LockOnTarget = false;
                IsEngaged = false;                
            }                            
        }

        public void TargetGetData()
        {
            if (TargetId > 0)
            {
                Actor actor = User.Instance.Character.GetCurrentZone().Actors.Find(x => x.Id == TargetId);

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
        public override void AutoAttack()
        {
            //check if there is a target selected and if it's in attack distance
            if (LockOnTarget && TargetId > 0 && GetTargetDistance() <= CharaWork.CurrentJob.AutoAttackMaxDistance)
            {
                ActorBattle target = ((ActorBattle)GetCurrentZone().GetActorById(TargetId));

                //chack if actor exists and it's not dead and if 
                if (target != null && !target.IsDead() && !IsDead())
                {
                    short damageDealt = 100; //to be calculated

                    if (!BattleManager.Instance.BattleEngaged)
                        BattleManager.Instance.StartBattle(this);

                    var (totalDamage, effectId) = target.CalculateDamage(this, damageDealt);

                    CommandResult cr = new CommandResult
                    {
                        TargetId = target.Id,
                        TotalPoints = totalDamage,
                        TextSheetId = 0x765D,
                        EffectId = (EffectId)0x08000604, //target hit animation
                        HitPosition = 1,
                        HitSequence = 1
                    };

                    SendCommandResult(Command.PlayerAutoAttack, new List<CommandResult> { cr }, 0x19001000);

                    Thread.Sleep(500);
                    AddTp(100);

                    target.TakeDamage(this, damageDealt);

                    //this should be in BM?
                    if (target.IsDead())
                    {
                        Monster monsterTarget = (Monster)target;

                        TargetId = 0;
                        LockOnTarget = false;

                        SendCommandResult(0, new List<CommandResult> { new CommandResult(Id, 1, 0x847F, 0, 50, 1) }, 0);
                        CharaWork.AddExp(monsterTarget.RewardExp);
                        Inventory.AddGil(monsterTarget.RewardGil);
                    }
                }

                Log.Instance.Info("Player auto-attack");

                if (!IsTutorialComplete) BattleTutorial.Instance.NextTutorial("tp");
            }
        }

        public override void Engage(uint attacker)
        {
            //TODO: find a better way to do this.
            IsEngaged = true;
            BattleManager.Instance.BattleEngaged = true;

            AutoAttack(); //first hit happens instantly.            
        }

        public override void Disengage()
        {
            IsEngaged = false;
            ToggleStance(Command.NormalStance);
        }
        #endregion

        public override void AddTp(ushort amount)
        {
            if(amount > 0)
            {
                base.AddTp(amount);
                if (!IsTutorialComplete && CharaWork.CurrentClass.Tp >= 1000) BattleTutorial.Instance.NextTutorial("weaponskill");
            }               
        }

        public override void Die()
        {
            Thread.Sleep(500);
            SetSubState();
            State.Main = MainState.Dead;
            State.Type = 0;
            SetMainState();
            SetEnmity(-1);

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
                    bw.Write(CharaWork.CurrentClass.Id);
                    bw.Write(CharaWork.CurrentClass.Level);
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

        public void TestRaiseCommand()
        {
            WorkProperties work = new WorkProperties(User.Instance.Character.Id, "playerWork/confirmRaiseCommand");
            

            work.Add("variableCommandConfirmRaise", 1);
            work.Add("variableCommandConfirmRaiseSender", Name.ToString());
            work.Add("variableCommandConfirmRaiseSenderByID", Id);
            work.Add("variableCommandConfirmRaiseSenderSex", 1);
            work.SendUpdate();
        }
    }
}

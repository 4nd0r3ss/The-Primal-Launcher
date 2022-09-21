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
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;

namespace PrimalLauncher
{
    [Serializable]
    public class Actor
    {
        public CharaWork CharaWork { get; set; }
        public uint CurrentTargetId { get; set; }
        public bool IsEngaged { get; set; }

        public uint Id { get; set; }
        public byte[] Name { get; set; } = new byte[0x20];
        public int NameId { get; set; }
        public uint ClassId { get; set; }
        public string ClassName { get; set; }
        public string ClassPath { get; set; }
        public uint ClassCode { get; set; }
        public int QuestIcon { get; set; }
        public bool Spawned { get; set; }
        public string Family { get; set; }
        public uint Icon { get; set; }
        public bool CanSpawn { get; set; } //spawn lock to be used by quest actors.
        public int AutoAttackDelay { get; set; }
        public Dictionary<uint, string> TalkFunctions { get; set; }

        #region States
        public State State { get; set; } = new State();
        public SubState SubState { get; set; } = new SubState();
        #endregion
       
        public Appearance Appearance { get; set; } = new Appearance();      
        public Position Position { get; set; } = new Position();
        public LuaParameters LuaParameters { get; set; }
        public List<Event> Events { get; set; } = new List<Event>();
        public Speeds Speeds { get; set; } = new Speeds();        
        

        public Actor()
        {
            CharaWork = new CharaWork();
            CharaWork.AddNpcJob();
        }

        public virtual void Spawn(ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {
            Prepare();
            CreateActor(0x08);
            SetEventConditions();
            SetSpeeds();
            SetPosition(spawnType, isZoning);
            SetAppearance();
            SetName();
            SetMainState();
            SetSubState();
            SetAllStatus();
            SetIcon();
            SetIsZoning(false);
            SetLuaScript();
            Init();
            Spawned = true;
        }

        public virtual void Despawn()
        {
            Spawned = false;            
            Packet.Send(ServerOpcode.RemoveActor, new byte[0x08], Id);
        }

        public virtual void Prepare()
        {          
            LuaParameters = new LuaParameters
            {
                ActorName = GenerateName(),
                ClassName = ClassName,
                ClassCode = ClassCode,
                Parameters = new object[] {ClassPath + ClassName, false, false, false, false, false, (int)ClassId, false, false, 0, 1}
            };         
        }

        public void SetEventConditions()
        {
            if (Events.Count > 0)   
                foreach (var e in Events)                
                    e.SetEventCondition(Id);
        }

        public void ToggleEvents(bool enable)
        {
            foreach (Event e in Events)
                e.Enabled = (byte)(enable ? 1 : 0);

            SetEventStatus();
        }

        public virtual void Init()
        {
            WorkProperties property = new WorkProperties(Id, @"/_init");
            property.Add("charaWork.battleSave.potencial", 0x3F800000);

            for(int i = 0; i < CharaWork.Property.Length; i++)
                if(CharaWork.Property[i])
                    property.Add("charaWork.property[0]", true);
            property.Add("charaWork.property[1]", true);
            property.Add("charaWork.property[2]", true);
            property.Add("charaWork.property[4]", true);


            property.Add("charaWork.parameterSave.hp[0]", CharaWork.CurrentJob.Hp);
            property.Add("charaWork.parameterSave.hpMax[0]", CharaWork.CurrentJob.MaxHp);
            property.Add("charaWork.parameterSave.mp", CharaWork.CurrentJob.Mp);
            property.Add("charaWork.parameterSave.mpMax", CharaWork.CurrentJob.MaxMp);
            property.Add("charaWork.parameterTemp.tp", CharaWork.CurrentJob.Tp);
            property.Add("charaWork.parameterSave.state_mainSkill[0]", CharaWork.CurrentJob.Id);
            property.Add("charaWork.parameterSave.state_mainSkill[2]", CharaWork.CurrentJob.Id);
            property.Add("charaWork.parameterSave.state_mainSkillLevel", CharaWork.CurrentJob.Level);
            property.Add("npcWork.hateType", (byte)0x01);
            property.FinishWritingAndSend(Id);
        }

        public virtual void SetLuaScript()
        {
            byte[] data = new byte[0x108];

            Buffer.BlockCopy(BitConverter.GetBytes(LuaParameters.ClassCode), 0, data, 0, sizeof(uint));
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(LuaParameters.ActorName), 0, data, 0x04, LuaParameters.ActorName.Length);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(LuaParameters.ClassName), 0, data, 0x24, LuaParameters.ClassName.Length);

            LuaParameters.WriteParameters(ref data, LuaParameters);
            LuaParameters = null;

            Packet.Send(ServerOpcode.LoadClassScript, data, Id);
        }

        public void SetIsZoning(bool isZoning = false)
        {
            byte[] data = new byte[0x08];
            data[0] = (byte)(isZoning ? 1 : 0);
            Packet.Send(ServerOpcode.SetIsZoning, data, Id);
        }

        public void SetIcon()
        {
            byte[] data = new byte[0x08];
            data.Write(0, Icon);            
            Packet.Send(ServerOpcode.SetIcon, data, Id);
        }

        public void SetAllStatus()
        {
            byte[] data = new byte[0x28];
            /* will be properly implemented later */
            Packet.Send(ServerOpcode.SetAllStatus, data, Id);
        }

        public void SetSubState() => Packet.Send(ServerOpcode.SetSubState, SubState.ToBytes(), Id);       

        public void SetMainState() => Packet.Send(ServerOpcode.SetMainState, State.ToBytes(), Id);        

        public void CreateActor(byte code = 0)
        {
            byte[] data = new byte[0x08];
            data[0] = code;
            Packet.Send(ServerOpcode.CreateActor, data, Id);
        }

        public void SendUnknown() => Packet.Send(ServerOpcode.Unknown, new byte[0x18], Id);

        public void SetName(int isCustom = 0, byte[] customName = null)
        {
            byte[] data = new byte[0x28];
            byte[] nameToPrint = customName ?? Name;
            int nameIdToPrint = isCustom != 0 ? isCustom : NameId;
            
            Buffer.BlockCopy(BitConverter.GetBytes(nameIdToPrint), 0, data, 0x00, sizeof(uint));
            Buffer.BlockCopy(nameToPrint, 0, data, 0x04, nameToPrint.Length);
            Packet.Send(ServerOpcode.SetName, data, Id);
        }

        public virtual void SetPosition(ushort spawnType = 0, ushort isZonning = 0, bool isPlayer = false)
        {
            byte[] data = new byte[0x28];
            int idToPrint = (int)Id;

            if (isPlayer)
                idToPrint = -1;

            Buffer.BlockCopy(BitConverter.GetBytes(idToPrint), 0, data, 0x04, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(Position.X), 0, data, 0x08, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(Position.Y), 0, data, 0x0c, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(Position.Z), 0, data, 0x10, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(Position.R), 0, data, 0x14, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(Position.FloatingHeight), 0, data, 0x1c, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(spawnType), 0, data, 0x24, sizeof(ushort));
            Buffer.BlockCopy(BitConverter.GetBytes(isZonning), 0, data, 0x26, sizeof(ushort));

            Packet.Send(ServerOpcode.SetPosition, data, Id);           
        }
        public void MoveToPosition(Position position, int moveState)
        {            
            byte[] data = new byte[0x30];   
            Buffer.BlockCopy(BitConverter.GetBytes(position.X), 0, data, 0x08, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(position.Y), 0, data, 0x0c, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(position.Z), 0, data, 0x10, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(position.R), 0, data, 0x14, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(moveState), 0, data, 0x18, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(position.FloatingHeight), 0, data, 0x24, sizeof(int));           

            Packet.Send(ServerOpcode.MoveToPosition, data, Id);
        }

        public void SetSpeeds(uint[] value = null) => Packet.Send(ServerOpcode.SetSpeed, Speeds.ToBytes(), Id);       

        public void SetAppearance() => Packet.Send(ServerOpcode.SetAppearance, Appearance.ToSlotBytes(), Id);

        public void SetQuestIcon()
        {
            if (QuestIcon >= 0)
                Packet.Send(ServerOpcode.SetQuestIcon, BitConverter.GetBytes((ulong)QuestIcon), Id);
        }

        public void DoEmote(byte emoteId)
        {
            byte[] data = new byte[0x10];            
            uint targetId = CurrentTargetId > 0 ? CurrentTargetId : Id;
            uint animation = 0;
            int textSheet;

            //emotes and textsheets can be calculated as we have a pattern, so I've created the formulas below.
            //had to split into 2 sections, as numbers follow the pattern only up to a certain point.
            if (emoteId <= 0x8F)
            {
                textSheet = 21001 + (10 * (emoteId - 101));
                animation = (uint)(0x05000000 | (0x1000 * (emoteId - 100)));
            }
            else
            {
                //from id 0x90 onward, main textsheet ids are increased by 2
                textSheet = 21421 + (2 * (emoteId - 143 - (emoteId > 0x95 ? 1 : 0)));

                //here we get animation ids that cannot be calculated as they are out of order for some reason.
                Dictionary<byte, uint> specialAnimations = new Dictionary<byte, uint>
                {
                    {0x90, 0x02C000}, //examine self
                    {0x91, 0x035000}, //pose
                    {0x92, 0x032000}, //storm salute
                    {0x93, 0x033000}, //serpent salute
                    {0x94, 0x034000}, //flame salute
                    {0x95, 0x02D000}, //blow kiss
                    {0x97, 0x02F000}, //grovel
                    {0x98, 0x030000}, //happy
                    {0x99, 0x031000}, //disappointed
                    {0x9A, 0x02E000}, //air quotes
                    {0x9B, 0x036000}, //pray
                    {0x9C, 0x037000}, //fire dance
                };

                animation = 0x05000000 | specialAnimations[emoteId];
            }

            if (CurrentTargetId == 0)
                textSheet++;

            Buffer.BlockCopy(BitConverter.GetBytes(animation), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(targetId), 0, data, 0x04, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(textSheet), 0, data, 0x08, 4);
            Packet.Send(ServerOpcode.DoEmote, data, Id, targetId);
        }

        public void ToggleHeadDirection(bool lookAtTarget = false)
        {
            //byte[] data = new byte[0x08];

            //if (lookAtTarget)
            //{
            //    Buffer.BlockCopy(BitConverter.GetBytes(User.Instance.Character.Id), 0, data, 0, 4);
            //    Packet.Send(ServerOpcode.SetHeadToTarget, data, Id);
            //}
            //else
            //{
            //    Packet.Send(ServerOpcode.ResetHead, data, Id);
            //}
        }

        #region Event virtual methods   
        public virtual void SetEventStatus()
        {      
            foreach (Event ec in Events)
            {
                byte[] data = new byte[0x28];
                Buffer.BlockCopy(BitConverter.GetBytes((uint)ec.Enabled), 0, data, 0, sizeof(uint));
                byte eventType = 1;

                switch (ec.Name)
                {
                    case "pushDefault":
                    case "exit":
                    case "caution":
                    case "pushCommandIn":
                    case "pushCommandOut":
                        eventType = 2;
                        break;
                    default:
                        eventType = 1;
                        break;
                }

                data[0x04] = eventType;
                Buffer.BlockCopy(Encoding.ASCII.GetBytes(ec.Name), 0, data, 0x05, ec.Name.Length);

                Packet.Send(ServerOpcode.SetEventStatus, data, Id);
            }
        }

        public virtual void noticeEvent() { }      

        public virtual void pushDefault() { }

        public virtual void talkDefault()
        {           
            SendTalk();            
        }

        public void SendTalk(string function = null)
        {
            KeyValuePair<uint, string> talkFunction;
            uint talkCode = 0;

            if (!string.IsNullOrEmpty(function))            
                talkFunction = new KeyValuePair<uint, string>(GetTalkCode(), function);           
            else      
                talkFunction = TalkFunctions.FirstOrDefault(x => x.Key > 0);//if there is a function for a specific quest, execute it. (used for opening mostly)        

            talkCode = talkFunction.Key;

            //if not, we try to get the default function
            if (talkCode == 0) //zero key means nothing returned.
            {                
                talkFunction = TalkFunctions.FirstOrDefault(x => x.Key == 0);

                //if the actor has the method, just call it.
                if (talkFunction.Value != null && GetType().GetMethod(talkFunction.Value) != null)
                {
                    InvokeMethod(talkFunction.Value, new object[] {});
                    return;
                }

                talkCode = GetTalkCode();
            }

            EventManager.Instance.CurrentEvent.DelegateEvent(talkCode, talkFunction.Value, null);
        }        

        public static uint GetTalkCode(string regionName = null)
        {
            regionName = string.IsNullOrEmpty(regionName) ? User.Instance.Character.GetCurrentZone().MapName.Substring(0, 3).ToLower() : regionName;

            switch (regionName)
            {
                case "sea":
                    return 0x01AFCC;                   
                case "fst":
                    return 0x01AFCD;                
                case "roc":
                    return 0x01AFCE;                   
                case "wil":
                    return 0x01AFCF;                 
                case "srt":
                    return 0x01AFD0;                   
                case "lak":
                    return 0x01AFD1;
                default:
                    return 0;
            }
        }
        #endregion

        #region Text dialogs
        public void ShowAttentionDialog(object[] parameters) => SendDialogData(new List<object> { "attention", Id, "" }, parameters);
        public void SendDialogData(List<object> toSend, object[] parameters)
        {
            if(parameters != null)
                toSend.AddRange(parameters);

            SendData(toSend.ToArray());
        }

        public void SendData(object[] toSend)
        {
            byte[] data = new byte[0xC0];
            LuaParameters parameters = new LuaParameters();

            foreach (object obj in toSend)
                parameters.Add(obj);

            LuaParameters.WriteParameters(ref data, parameters, 0);
            Packet.Send(ServerOpcode.GeneralData, data);
        }
        #endregion

        #region Misc methods
        /// <summary>
        /// Converts a number to a base 63 string. This function was taken from Ioncannon's code, all credit goes to him. 
        /// </summary>
        /// <param name="number">The number to be converted.</param>
        /// <returns></returns>
        public string ToStringBase63(int number)
        {
            var lookup = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var startIndex = (int)Math.Floor(number / (double)lookup.Length);
            var secondDigit = lookup.Substring(startIndex, 1);
            var firstDigit = lookup.Substring(number % lookup.Length, 1);

            return secondDigit + firstDigit;
        }

        public string GenerateName()
        {
            Zone zone = World.Instance.GetZone(Position.ZoneId);

            uint zoneId = zone.Id;
            uint privLevel = zone.PrivLevel;           
            string zoneName = MinifyMapName(zone.MapName, zone.PrivLevel);
            string className = MinifyClassName();

            zoneName = char.ToLowerInvariant(zoneName[0]) + zoneName.Substring(1);      
            className = char.ToLowerInvariant(className[0]) + className.Substring(1);

            if(className.Length > 6 && (className.Length + (zoneName.Length + 4)) > 25)
                try{ className = className.Substring(0, 21 - zoneName.Length); }
                catch (ArgumentOutOfRangeException e) { Log.Instance.Error(e.Message); throw e; }
                        
            return string.Format("{0}_{1}_{2}@{3:X3}{4:X2}", className, zoneName, ToStringBase63((int)(Id & 0xff)), zoneId, privLevel);
        }

        protected string MinifyMapName(string mapName, byte privLevel = 0)
        {
            string minifiedStr = mapName.Replace("Field", "Fld")
                .Replace("Dungeon", "Dgn")
                .Replace("Town", "Twn")
                .Replace("Battle", "Btl")
                .Replace("Test", "Tes")
                .Replace("Event", "Evt")
                .Replace("Ship", "Shp")
                .Replace("Office", "Ofc");

            if (privLevel > 0)
                minifiedStr = minifiedStr.Substring(0, minifiedStr.Length - 1) + "P";

            return minifiedStr;
        }

        protected string MinifyClassName()
        {
            return ClassName.Replace("Populace", "ppl")
                .Replace("Monster", "Mon")
                .Replace("RetainerFurniture", "rtnFurnitu")
                .Replace("MarketEntrance", "marketEntr")
                .Replace("Crowd", "Crd")
                .Replace("MapObj", "Map")
                .Replace("Object", "Obj")
                .Replace("Retainer", "Rtn")
                .Replace("Director", "Dire")
                .Replace("Standard", "Std")
                .Replace("Opening", "opening");
        }

        protected int GetEventCode(string eventName)
        {
            return (int)Enum.Parse(typeof(EventType), eventName);
        }

        public void GetBaseModel(byte id)
        {
            DataTable itemNames = GameData.Instance.GetGameData("tribe");
            DataRow[] selected = itemNames.Select("id = '" + id + "'");
            int model = (int)selected[0][1]; //had to do this as it was throwing cast error
            Appearance.BaseModel = (uint)model;
        }

        public void InvokeMethod(string methodName, object[] methodParams)
        {
            var method = GetType().GetMethod(methodName);

            if (method != null)
                method.Invoke(this, methodParams);
            else
            {
                string msg = "Actor.InvokeMethod: Type " + GetType().Name + " has no method " + methodName + ".";
                Log.Instance.Error(msg);
                ChatProcessor.SendMessage(MessageType.SystemError, msg);
                EventManager.Instance.CurrentEvent.Finish();
            }            
        }
        #endregion

        public void SendCommandResult(Command command, List<CommandResult> resultList, uint animationId = 0, uint unknown = 0, uint senderId = 0)
        {
            byte[] data;
            byte[] resultBytes;
            ServerOpcode opcode = ServerOpcode.CommandResultX1;            

            //when we have one single result, we just print it.
            if (resultList.Count == 1)
            {
                data = new byte[0x38];
                resultBytes = resultList[0].ToBytes();                
            }
            else //more than one result, it is printed in a different way. It ends up looking like a table.
            {
                opcode = ServerOpcode.CommandResultX10;
                data = new byte[0xD8];
                resultBytes = CommandResult.ToBytes(resultList);
            }

            Buffer.BlockCopy(BitConverter.GetBytes(senderId > 0 ? senderId : Id), 0, data, 0, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(animationId), 0, data, 0x04, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(resultList.Count), 0, data, 0x20, sizeof(int)); //#results
            Buffer.BlockCopy(BitConverter.GetBytes((short)command), 0, data, 0x24, sizeof(short));
            Buffer.BlockCopy(BitConverter.GetBytes((unknown > 0 ? unknown : 0x0810)), 0, data, 0x26, sizeof(ushort)); //unknown

            Buffer.BlockCopy(resultBytes, 0, data, 0x28, resultBytes.Length); //unknown
            Packet.Send(opcode, data, senderId > 0 ? senderId : Id);
        }

        public void ToggleStance(Command command)
        {
            if (command == Command.BattleStance)
                State.Main = MainState.Active;
            else
                State.Main = MainState.Passive;

            CommandResult cr = new CommandResult
            {
                TargetId = Id,
                EffectId = 1,
                HitSequence = 1
            };

            SetMainState();
            SendCommandResult(command, new List<CommandResult> { cr }, 0x7C000062, senderId: Id);
        }

        public void PlayAnimationEffect(AnimationEffect animation)
        {
            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)animation), 0, data, 0, sizeof(ushort));
            data[0x03] = 0x04;
            Thread.Sleep(500);
            Packet.Send(ServerOpcode.PlayAnimationEffect, data, Id);
        }   

        public virtual Zone GetCurrentZone() => World.Instance.GetZone(Position.ZoneId);

        public void StartEvent(string eventName, string functionName = null)
        {
            if (!string.IsNullOrEmpty(eventName))
            {
                Log.Instance.Warning("Actor.StartEvent");

                byte[] data = new byte[0x70];
                uint characterId = User.Instance.Character.Id;
                uint serverCode = 0x75dc1700 + (uint)GetEventCode(eventName);

                Buffer.BlockCopy(BitConverter.GetBytes(characterId), 0, data, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0x04, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(serverCode), 0, data, 0x08, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(ClassCode), 0, data, 0x0c, 4);
                LuaParameters parameters = new LuaParameters();
                parameters.Add(Encoding.ASCII.GetBytes(eventName));

                if (string.IsNullOrEmpty(functionName))
                    parameters.Add(true);
                else
                    parameters.Add(functionName);

                LuaParameters.WriteParameters(ref data, parameters, 0x10);
                Packet.Send(ServerOpcode.StartEvent, data, sourceId: characterId);
            }            
        }
    }
}

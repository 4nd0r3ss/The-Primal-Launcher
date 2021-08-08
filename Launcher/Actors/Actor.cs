using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;

namespace PrimalLauncher
{
    [Serializable]
    public class Actor
    {
        public uint TargetId { get; set; } //remove this
        public uint CurrentTarget { get; set; }

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

        #region CharaWork


        #endregion

        public virtual void Spawn(Socket sender, ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {
            Prepare();
            CreateActor(sender, 0x08);
            SetEventConditions(sender);
            SetSpeeds(sender);
            SetPosition(sender, spawnType, isZoning);
            SetAppearance(sender);
            SetName(sender);
            SetMainState(sender);
            SetSubState(sender);
            SetAllStatus(sender);
            SetIcon(sender);
            SetIsZoning(sender, false);
            SetLuaScript(sender);
            Init(sender);
            Spawned = true;
        }

        public virtual void Despawn(Socket sender)
        {
            Spawned = false;            
            Packet.Send(sender, ServerOpcode.RemoveActor, new byte[0x08], Id);
        }

        public virtual void Prepare()
        {          
            LuaParameters = new LuaParameters
            {
                ActorName = GenerateName(),
                ClassName = ClassName,
                ClassCode = ClassCode,
                Parameters = new object[] {ClassPath + ClassName, false, false, false, false, false, (int)ClassId, false, false, 0, 0}
            };         
        }

        public void SetEventConditions(Socket sender)
        {
            if (Events.Count > 0)   
                foreach (var e in Events)                
                    e.SetEventCondition(sender, Id);
        }

        public void ToggleEvents(Socket sender, bool enable)
        {
            foreach (Event e in Events)
                e.Enabled = (byte)(enable ? 1 : 0);

            SetEventStatus(sender);
        }

        public virtual void Init(Socket sender) { }

        public virtual void SetLuaScript(Socket sender)
        {
            byte[] data = new byte[0x108];

            Buffer.BlockCopy(BitConverter.GetBytes(LuaParameters.ClassCode), 0, data, 0, sizeof(uint));
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(LuaParameters.ActorName), 0, data, 0x04, LuaParameters.ActorName.Length);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(LuaParameters.ClassName), 0, data, 0x24, LuaParameters.ClassName.Length);

            LuaParameters.WriteParameters(ref data, LuaParameters);
            LuaParameters = null;

            Packet.Send(sender, ServerOpcode.LoadClassScript, data, Id);
        }

        public void SetIsZoning(Socket sender, bool isZoning = false)
        {
            byte[] data = new byte[0x08];
            data[0] = (byte)(isZoning ? 1 : 0);
            Packet.Send(sender, ServerOpcode.SetIsZoning, data, Id);
        }

        public void SetIcon(Socket sender)
        {
            byte[] data = new byte[0x08];
            /* will be properly implemented later */
            Packet.Send(sender, ServerOpcode.SetIcon, data, Id);
        }

        public void SetAllStatus(Socket sender)
        {
            byte[] data = new byte[0x28];
            /* will be properly implemented later */
            Packet.Send(sender, ServerOpcode.SetAllStatus, data, Id);
        }

        public void SetSubState(Socket sender) => Packet.Send(sender, ServerOpcode.SetSubState, SubState.ToBytes(), Id);       

        public void SetMainState(Socket sender) => Packet.Send(sender, ServerOpcode.SetMainState, State.ToBytes(), Id);        

        public void CreateActor(Socket sender, byte code = 0)
        {
            byte[] data = new byte[0x08];
            data[0] = code;
            Packet.Send(sender, ServerOpcode.CreateActor, data, Id);
        }

        public void SendUnknown(Socket sender) => Packet.Send(sender, ServerOpcode.Unknown, new byte[0x18], Id);

        public void SetName(Socket sender, int isCustom = 0, byte[] customName = null)
        {
            byte[] data = new byte[0x28];
            byte[] nameToPrint = customName ?? Name;
            int nameIdToPrint = isCustom != 0 ? isCustom : NameId;
            
            Buffer.BlockCopy(BitConverter.GetBytes(nameIdToPrint), 0, data, 0x00, sizeof(uint));
            Buffer.BlockCopy(nameToPrint, 0, data, 0x04, nameToPrint.Length);
            Packet.Send(sender, ServerOpcode.SetName, data, Id);
        }

        public virtual void SetPosition(Socket sender, ushort spawnType = 0, ushort isZonning = 0, bool isPlayer = false)
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

            Packet.Send(sender, ServerOpcode.SetPosition, data, Id);           
        }
        public void MoveToPosition(Socket sender, Position position)
        {            
            byte[] data = new byte[0x30];   
            Buffer.BlockCopy(BitConverter.GetBytes(position.X), 0, data, 0x08, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(position.Y), 0, data, 0x0c, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(position.Z), 0, data, 0x10, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(position.R), 0, data, 0x14, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(1), 0, data, 0x18, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(position.FloatingHeight), 0, data, 0x24, sizeof(int));           

            Packet.Send(sender, ServerOpcode.MoveToPosition, data, Id);
        }

        public void SetSpeeds(Socket sender, uint[] value = null) => Packet.Send(sender, ServerOpcode.SetSpeed, Speeds.ToBytes(), Id);       

        public void SetAppearance(Socket sender) => Packet.Send(sender, ServerOpcode.SetAppearance, Appearance.ToSlotBytes(), Id);       
               
        public void DoEmote(Socket sender)
        {
            byte[] data = new byte[] { 0x00, 0xB0, 0x00, 0x05, 0x41, 0x29, 0x9B, 0x02, 0x6E, 0x52, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0x04, 4);
            Packet.Send(sender, ServerOpcode.DoEmote, data, Id);
        }

        #region Event virtual methods   
        public virtual void SetEventStatus(Socket sender)
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
                        eventType = 2;
                        break;
                    default:
                        eventType = 1;
                        break;
                }

                data[0x04] = eventType;
                Buffer.BlockCopy(Encoding.ASCII.GetBytes(ec.Name), 0, data, 0x05, ec.Name.Length);

                Packet.Send(sender, ServerOpcode.SetEventStatus, data, Id);
            }
        }

        public virtual void noticeEvent(Socket sender) { }      

        public virtual void pushDefault(Socket sender) { }

        public virtual void talkDefault(Socket sender)
        {
            //if there is a function for a specific quest, execute it. (used for opening mostly)
            KeyValuePair<uint, string> talkFunction = TalkFunctions.FirstOrDefault(x => x.Key > 0);

            //if not, we try to get the default function
            if(talkFunction.Key == 0) //zero key means nothing returned.
            {
                string regionName = User.Instance.Character.GetCurrentZone().MapName.Substring(0, 3).ToLower();
                uint talkCode = 0;

                switch (regionName)
                {
                    case "sea":
                        talkCode = 0x01AFCC;
                        break;
                    case "fst":
                        talkCode = 0x01AFCD;
                        break;
                    case "roc":
                        talkCode = 0x01AFCE;
                        break;
                    case "wil":
                        talkCode = 0x01AFCF;
                        break;
                    case "srt":
                        talkCode = 0x01AFD0;
                        break;
                    case "lak":
                        talkCode = 0x01AFD1;
                        break;
                }

                talkFunction = TalkFunctions.FirstOrDefault(x => x.Key == 0);
                EventManager.Instance.CurrentEvent.DelegateEvent(sender, talkCode, talkFunction.Value, null);
            }
            else
            {
                EventManager.Instance.CurrentEvent.DelegateEvent(sender, talkFunction.Key, talkFunction.Value, null);
            } 
        }
        #endregion
        public void SetQuestIcon(Socket sender)
        {
            if(QuestIcon >= 0)
                Packet.Send(sender, ServerOpcode.SetQuestIcon, BitConverter.GetBytes((ulong)QuestIcon), Id);
        }

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
            Zone zone = World.Instance.Zones.Find(x => x.Id == Position.ZoneId);

            //if (zone == null)
            //    zone = ZoneRepository.GetPrivateArea((int)Position.ZoneId);

            uint zoneId = zone.Id;
            uint privLevel = zone.PrivLevel;           
            string zoneName = MinifyMapName(zone.MapName, zone.PrivLevel);
            string className = MinifyClassName();

            //if (zone.ZoneType == ZoneType.Inn)
            //{
            //    zoneName = zoneName.Remove(zoneName.Length - 1, 1) + "P";
            //    //privLevel = ((PrivateArea)zone).GetPrivateAreaType();
            //}

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
                .Replace("Crowd", "Crd")
                .Replace("MapObj", "Map")
                .Replace("Object", "Obj")
                .Replace("Retainer", "Rtn")
                .Replace("Director", "Dire")
                .Replace("Standard", "Std")
                .Replace("Opening", "opening");
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
                Log.Instance.Error("Actor.InvokeMethod: Type " + GetType().Name + " has no method " + methodName + ".");
        }
    }
}

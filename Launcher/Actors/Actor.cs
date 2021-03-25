using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;

namespace Launcher
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

        #region States
        public State State { get; set; } = new State();
        public SubState SubState { get; set; } = new SubState();
        #endregion

        #region Head       
        public ushort HairStyle { get; set; }
        public ushort HairColor { get; set; }
        public ushort HairHighlightColor { get; set; }
        public ushort HairVariation { get; set; }
        public ushort EyeColor { get; set; }
        public ushort SkinColor { get; set; }
        #endregion        

        public Face Face { get; set; }
        public Appearance Appearance { get; set; } = new Appearance();      
        public Position Position { get; set; } = new Position();
        public LuaParameters LuaParameters { get; set; }
        public List<EventCondition> EventConditions { get; set; } = new List<EventCondition>();
        public Speeds Speeds { get; set; } = new Speeds();

        public virtual void Spawn(Socket sender, ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0, ushort actorIndex = 0)
        {
            Prepare(actorIndex);
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
            LoadActorScript(sender);
            Init(sender);
        }

        public virtual void Prepare(ushort actorIndex = 0)
        {
            Zone zone = World.Instance.Zones.Find(x => x.Id == Position.ZoneId);
            Id = 4 << 28 | zone.Id << 19 | (uint)actorIndex; // 0x46700087;           

            LuaParameters = new LuaParameters
            {
                ActorName = GenerateActorName(actorIndex),
                ClassName = ClassName,
                ClassCode = ClassCode
            };

            LuaParameters.Add(ClassPath + ClassName);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(ClassId);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add((uint)0);
            LuaParameters.Add((uint)0);
            //LuaParameters.Add("TEST");
        }

        public void SetEventConditions(Socket sender)
        {
            if (EventConditions.Count > 0) //not all actors have event conditions
            {
                foreach (var e in EventConditions)
                {
                    byte[] data = new byte[0x28];
                    byte[] conditionName = Encoding.ASCII.GetBytes(e.EventName);
                    int conditionNameLength = e.EventName.Length;

                    switch (e.Opcode)
                    {
                        case ServerOpcode.EmoteEvent:
                            Buffer.BlockCopy(BitConverter.GetBytes(e.Priority), 0, data, 0, sizeof(byte));
                            Buffer.BlockCopy(BitConverter.GetBytes(e.IsDisabled), 0, data, 0x1, sizeof(byte));
                            Buffer.BlockCopy(BitConverter.GetBytes(e.EmoteId), 0, data, 0x2, sizeof(ushort));
                            Buffer.BlockCopy(conditionName, 0, data, 0x4, conditionNameLength);
                            break;
                        case ServerOpcode.PushEventCircle:
                            data = new byte[0x38];
                            Buffer.BlockCopy(BitConverter.GetBytes(e.Radius), 0, data, 0, sizeof(uint));
                            Buffer.BlockCopy(BitConverter.GetBytes(0x44533088), 0, data, 0x04, sizeof(uint));
                            Buffer.BlockCopy(BitConverter.GetBytes(100.0f), 0, data, 0x08, sizeof(uint));
                            Buffer.BlockCopy(BitConverter.GetBytes(0), 0, data, 0x0c, sizeof(uint));
                            Buffer.BlockCopy(BitConverter.GetBytes(e.Direction), 0, data, 0x10, sizeof(byte));
                            Buffer.BlockCopy(BitConverter.GetBytes(0), 0, data, 0x11, sizeof(byte));
                            Buffer.BlockCopy(BitConverter.GetBytes(e.IsSilent), 0, data, 0x12, sizeof(byte));
                            Buffer.BlockCopy(conditionName, 0, data, 0x13, conditionNameLength);
                            break;
                        case ServerOpcode.PushEvenFan:
                            data = new byte[0x40];
                            Buffer.BlockCopy(BitConverter.GetBytes(e.Radius), 0, data, 0, sizeof(uint));
                            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0x04, sizeof(uint));
                            Buffer.BlockCopy(BitConverter.GetBytes(e.Radius), 0, data, 0x08, sizeof(uint));
                            Buffer.BlockCopy(BitConverter.GetBytes(e.Direction), 0, data, 0x10, sizeof(byte));
                            Buffer.BlockCopy(BitConverter.GetBytes(e.Priority), 0, data, 0x11, sizeof(byte));
                            Buffer.BlockCopy(BitConverter.GetBytes(e.IsSilent), 0, data, 0x12, sizeof(byte));
                            Buffer.BlockCopy(conditionName, 0, data, 0x13, conditionNameLength);
                            break;
                        case ServerOpcode.PushEventTriggerBox:
                            data = new byte[0x40];
                            Buffer.BlockCopy(BitConverter.GetBytes(e.BgObjectId), 0, data, 0, sizeof(uint));
                            Buffer.BlockCopy(BitConverter.GetBytes(e.LayoutId), 0, data, 0x4, sizeof(uint));
                            Buffer.BlockCopy(BitConverter.GetBytes(e.ActorId), 0, data, 0x8, sizeof(byte));
                            Buffer.BlockCopy(BitConverter.GetBytes(e.Direction), 0, data, 0x14, sizeof(byte));
                            Buffer.BlockCopy(conditionName, 0, data, 0x17, conditionNameLength);
                            Buffer.BlockCopy(Encoding.ASCII.GetBytes(e.ReactionName), 0, data, 0x38, e.ReactionName.Length);
                            break;
                        case ServerOpcode.NoticeEvent:
                        case ServerOpcode.TalkEvent:
                        default:
                            Buffer.BlockCopy(BitConverter.GetBytes(e.Priority), 0, data, 0, sizeof(byte));
                            Buffer.BlockCopy(BitConverter.GetBytes(e.IsDisabled), 0, data, 0x1, sizeof(byte));
                            Buffer.BlockCopy(conditionName, 0, data, 0x2, conditionNameLength);
                            break;
                    }

                    SendPacket(sender, e.Opcode, data);
                }
            }
        }

        public virtual void Init(Socket sender)
        {
            //byte[] data = new byte[0x88];
            //SendPacket(sender, Opcode.ActorInit, data);
        }

        public virtual void LoadActorScript(Socket sender)
        {
            byte[] data = new byte[0x108];

            Buffer.BlockCopy(BitConverter.GetBytes(LuaParameters.ClassCode), 0, data, 0, sizeof(uint));
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(LuaParameters.ActorName), 0, data, 0x04, LuaParameters.ActorName.Length);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(LuaParameters.ClassName), 0, data, 0x24, LuaParameters.ClassName.Length);

            LuaParameters.WriteParameters(ref data, LuaParameters);

            SendPacket(sender, ServerOpcode.LoadClassScript, data);
        }

        public void SetIsZoning(Socket sender, bool isZoning = false)
        {
            byte[] data = new byte[0x08];
            data[0] = (byte)(isZoning ? 1 : 0);
            SendPacket(sender, ServerOpcode.SetIsZoning, data);
        }

        public void SetIcon(Socket sender)
        {
            byte[] data = new byte[0x08];
            /* will be properly implemented later */
            SendPacket(sender, ServerOpcode.SetIcon, data);
        }

        public void SetAllStatus(Socket sender)
        {
            byte[] data = new byte[0x28];
            /* will be properly implemented later */
            SendPacket(sender, ServerOpcode.SetAllStatus, data);
        }

        public void SetSubState(Socket sender) => SendPacket(sender, ServerOpcode.SetSubState, SubState.ToBytes());       

        public void SetMainState(Socket sender) => SendPacket(sender, ServerOpcode.SetMainState, State.ToBytes());        

        public void CreateActor(Socket sender, byte code = 0)
        {
            byte[] data = new byte[0x08];
            data[0] = code;
            SendPacket(sender, ServerOpcode.CreateActor, data);
        }

        public void SendUnknown(Socket sender) => SendPacket(sender, ServerOpcode.Unknown, new byte[0x18]);

        public void SetName(Socket sender, int isCustom = 0, byte[] customName = null)
        {
            byte[] data = new byte[0x28];

            if (customName != null)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(isCustom), 0, data, 0, sizeof(int));
                Buffer.BlockCopy(customName, 0, data, 0x04, customName.Length);                
            }
            else
            {
                Buffer.BlockCopy(BitConverter.GetBytes(NameId), 0, data, 0x00, sizeof(uint));
                Buffer.BlockCopy(Name, 0, data, 0x04, Name.Length);
            }

            SendPacket(sender, ServerOpcode.SetName, data);
        }

        public virtual void SetPosition(Socket sender, ushort spawnType = 0, ushort isZonning = 0, int changingZone = 0, bool isPlayer = false)
        {
            byte[] data = new byte[0x28];

            if (changingZone == 0)
                changingZone = (int)Id;

            uint idToPrint = Id;
            if (isPlayer)
                idToPrint = 0;

            Buffer.BlockCopy(BitConverter.GetBytes(idToPrint), 0, data, 0x04, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(Position.X), 0, data, 0x08, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(Position.Y), 0, data, 0x0c, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(Position.Z), 0, data, 0x10, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(Position.R), 0, data, 0x14, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(Position.FloatingHeight), 0, data, 0x1c, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(spawnType), 0, data, 0x24, sizeof(ushort));
            Buffer.BlockCopy(BitConverter.GetBytes(isZonning), 0, data, 0x26, sizeof(ushort));

            SendPacket(sender, ServerOpcode.SetPosition, data);
        }

        public void SetSpeeds(Socket sender, uint[] value = null) => SendPacket(sender, ServerOpcode.SetSpeed, Speeds.ToBytes());       

        public void SetAppearance(Socket sender)
        {
            byte[] data = new byte[0x108];

            Dictionary<uint, uint> AppearanceSlots = new Dictionary<uint, uint>
            {
                //slot number, value
                { 0x00, Appearance.BaseModel },
                { 0x01, Appearance.Size },
                { 0x02, (uint)(SkinColor | HairColor << 10 | EyeColor << 20) },
                { 0x03, BitField.PrimitiveConversion.ToUInt32(Face) },
                { 0x04, (uint)(HairHighlightColor | HairStyle << 10) },
                { 0x05, Appearance.Voice },
                { 0x06, Appearance.MainWeapon },
                { 0x07, Appearance.SecondaryWeapon },
                { 0x08, Appearance.SPMainWeapon },
                { 0x09, Appearance.SPSecondaryWeapon },
                { 0x0a, Appearance.Throwing },
                { 0x0b, Appearance.Pack },
                { 0x0c, Appearance.Pouch },
                { 0x0d, Appearance.Head },
                { 0x0e, Appearance.Body },
                { 0x0f, Appearance.Legs },
                { 0x10, Appearance.Hands },
                { 0x11, Appearance.Feet },
                { 0x12, Appearance.Waist },
                { 0x13, Appearance.Neck },
                { 0x14, Appearance.RightEar },
                { 0x15, Appearance.LeftEar },
                { 0x16, Appearance.Wrists },
                { 0x17, 0 },
                { 0x18, Appearance.LeftFinger },
                { 0x19, Appearance.RightFinger },
                { 0x1a, Appearance.RightIndex },
                { 0x1b, Appearance.LeftIndex }
            };

            using (MemoryStream stream = new MemoryStream(data))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    foreach (var slot in AppearanceSlots)
                    {
                        writer.Write(slot.Value);
                        writer.Write(slot.Key);
                    }
                }
            }

            data[0x100] = (byte)AppearanceSlots.Count;

            SendPacket(sender, ServerOpcode.SetAppearance, data);
        }

        public void SendPacket(Socket sender, ServerOpcode opcode, byte[] data, uint sourceId = 0, uint targetId = 0)
        {
            GamePacket gamePacket = new GamePacket
            {
                Opcode = (ushort)opcode,
                Data = data
            };

            if (sourceId == 0)
                sourceId = Id;

            //Packet packet = new Packet(new SubPacket(gamePacket) { SourceId = sourceId > 0 ? sourceId : Id, TargetId = targetId > 0 ? targetId : TargetId });
            Packet packet = new Packet(new SubPacket(gamePacket) { SourceId = Id, TargetId = TargetId });
            sender.Send(packet.ToBytes());
        }
               
        public void DoEmote(Socket sender)
        {
            byte[] data = new byte[] { 0x00, 0xB0, 0x00, 0x05, 0x41, 0x29, 0x9B, 0x02, 0x6E, 0x52, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0x04, 4);
            SendPacket(sender, ServerOpcode.DoEmote, data);
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

        public string GenerateActorName(int actorNumber)
        {
            Zone zone = World.Instance.Zones.Find(x => x.Id == Position.ZoneId);            
            uint zoneId = zone.Id;
            uint privLevel = 0;

            //get actor zone name
            string zoneName = zone.MapName
                .Replace("Field", "Fld")
                .Replace("Dungeon", "Dgn")
                .Replace("Town", "Twn")
                .Replace("Battle", "Btl")
                .Replace("Test", "Tes")
                .Replace("Event", "Evt")
                .Replace("Ship", "Shp")
                .Replace("Office", "Ofc");

            //if (zone.ZoneType == ZoneType.Inn)
            //{
            //    zoneName = zoneName.Remove(zoneName.Length - 1, 1) + "P";
            //    //privLevel = ((PrivateArea)zone).GetPrivateAreaType();
            //}

            zoneName = Char.ToLowerInvariant(zoneName[0]) + zoneName.Substring(1);

            //Format Class Name
            string className = ClassName.Replace("Populace", "ppl")
                                        .Replace("Monster", "Mon")
                                        .Replace("Crowd", "Crd")
                                        .Replace("MapObj", "Map")
                                        .Replace("Object", "Obj")
                                        .Replace("Retainer", "Rtn")
                                        .Replace("Standard", "Std");

            className = Char.ToLowerInvariant(className[0]) + className.Substring(1);

            if(className.Length > 6 && (className.Length + (zoneName.Length + 4)) > 25)
                try{ className = className.Substring(0, 21 - zoneName.Length); }
                catch (ArgumentOutOfRangeException e) { /*Log.Instance.Error(e.Message);*/ }
                        
            return string.Format("{0}_{1}_{2}@{3:X3}{4:X2}", className, zoneName, ToStringBase63(actorNumber), zoneId, privLevel);
        }

        public void GetBaseModel(byte id)
        {
            DataTable itemNames = GameData.Instance.GetGameData("tribe");
            DataRow[] selected = itemNames.Select("id = '" + id + "'");
            int model = (int)selected[0][1]; //had to do this as it was throwing cast error
            Appearance.BaseModel = (uint)model;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Launcher
{
    [Serializable]
    public class Actor
    {
        
        public uint TargetId { get; set; }

        public uint Id { get; set; }
        public uint NameId { get; set; }
        public uint DisplayNameId { get; set; }
        public string ClassPath { get; set; }
        public byte PropFlag { get; set; }
        public List<EventCondition> EventConditions { get; set; } = new List<EventCondition>();

        #region General
        public uint Size { get; set; }
        public uint Voice { get; set; }
        public ushort SkinColor { get; set; }
        #endregion

        #region Head       
        public ushort HairStyle { get; set; }
        public ushort HairColor { get; set; }
        public ushort HairHighlightColor { get; set; }
        public ushort HairVariation { get; set; }
        public ushort EyeColor { get; set; } //oddly not part of face bitfield values. Maybe it was added at a later time in development?
        #endregion        

        public Face Face { get; set; }

        public GearSet GearSet { get; set; }
        public Tribe Tribe { get; set; } = Tribe.GetTribe(0); //objects wont have a tribe, so we load zeros by default.

        public Position Position { get; set; } = new Position();
        public LuaParameters LuaParameters { get; set; }
        public uint[] Speeds { get; set; }   

        public enum Opcode
        {
            Unknown              = 0x0f,
            CreateActor          = 0xca,
            LoadActorScript      = 0xcc,
            SetPosition          = 0xce,
            SetSpeed             = 0xd0,            
            SetAppearance        = 0xd6,
            SetName              = 0x13d,
            SetMainState         = 0x134,
            SetSubState          = 0x144,
            SetAllStatus         = 0x179,
            SetIcon              = 0x145,
            SetIsZoning          = 0x17b,       
            AchievementPoints    = 0x19c,
            AchievementsLatest   = 0x19b,
            AchievementsCompeted = 0x19a,
            PlayerCommand        = 0x132,            
            ActorInit            = 0x137,

            //specific to player character
            SetGrandCompany      = 0x194,
            SetTitle             = 0x19d,
            SetCurrentJob        = 0x1a4,
            SetSpecialEventWork  = 0x196,
            SetChocoboName       = 0x198,
            SetHasChocobo        = 0x199,

            BattleActionResult01 = 0x139,
            EndClientOrderEvent  = 0x131,

            //event conditions
            SetTalkEventCondition                = 0x012e,
            SetNoticeEventcondition              = 0x016b,
            SetEmoteEventCondition               = 0x016c,
            SetPushEventConditionWithCircle      = 0x016f,
            SetPushEventConditionWithFan         = 0x0170,
            SetPushEventConditionWithTriggerBox  = 0x0175
        }

        public enum MainState
        {
            Passive      = 0x00,
            Dead         = 0x01,
            Active       = 0x02,
            Dead2        = 0x03,
            SitObject    = 0x0a,
            SitFloor     = 0x0e,
            Mounting     = 0x0f
        }

        public virtual void Spawn(Socket handler, ushort spawnType = 0)
        {
            CreateActor(handler, 0);
            SetEventCondition(handler);
            SetSpeeds(handler, Speeds);
            SetPosition(handler, Position, spawnType);
            SetName(handler);
            SetMainState(handler, MainState.Passive, 0);
            SetIsZoning(handler);
            LoadActorScript(handler,LuaParameters);
        }

        public void SetEventCondition(Socket handler)
        {
            if(EventConditions.Count > 0) //not all actors have event conditions
            {
                foreach(var e in EventConditions)
                {
                    byte[] data;

                    if(e.Opcode == Opcode.SetPushEventConditionWithCircle)
                    {
                        data = new byte[0x38];
                        Buffer.BlockCopy(BitConverter.GetBytes(e.Radius), 0, data, 0, sizeof(uint));
                        Buffer.BlockCopy(BitConverter.GetBytes(e.Option1), 0, data, 0x10, sizeof(byte));
                        Buffer.BlockCopy(BitConverter.GetBytes(e.Option2), 0, data, 0x12, sizeof(byte));
                        Buffer.BlockCopy(Encoding.ASCII.GetBytes(e.ConditionName), 0, data, 0x13, sizeof(byte));
                    }
                    else
                    {
                        data = new byte[0x28];
                        Buffer.BlockCopy(BitConverter.GetBytes(e.Option1), 0, data, 0, sizeof(byte));
                        Buffer.BlockCopy(BitConverter.GetBytes(e.Option2), 0, data, 0x1, sizeof(byte));
                        Buffer.BlockCopy(Encoding.ASCII.GetBytes(e.ConditionName), 0, data, 0x2, sizeof(byte));
                    }

                    SendPacket(handler, e.Opcode, data);
                }
            }
        }
        
        public virtual void ActorInit(Socket handler)
        {
            byte[] data = new byte[0x88];
            SendPacket(handler, Opcode.ActorInit, data);
        }

        public virtual void LoadActorScript(Socket handler, LuaParameters luaParameters)
        {
            byte[] data = new byte[0x108];           

            Buffer.BlockCopy(BitConverter.GetBytes(0x30400000), 0, data, 0, sizeof(uint));
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(luaParameters.ActorName), 0, data, 0x04, luaParameters.ActorName.Length);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(luaParameters.ClassName), 0, data, 0x24, luaParameters.ClassName.Length);

            LuaParameters.WriteParameters(ref data, luaParameters);          

            SendPacket(handler, Opcode.LoadActorScript, data);
        }

        public void SetIsZoning(Socket handler)
        {
            byte[] data = new byte[0x08];
            /* will be properly implemented later */
            SendPacket(handler, Opcode.SetIsZoning, data);
        }

        public void SetIcon(Socket handler)
        {
            byte[] data = new byte[0x08];
            /* will be properly implemented later */
            SendPacket(handler, Opcode.SetIcon, data);
        }

        public void SetAllStatus(Socket handler)
        {
            byte[] data = new byte[0x28];
            /* will be properly implemented later */
            SendPacket(handler, Opcode.SetAllStatus, data);
        }

        public void SetSubState(Socket handler)
        {
            byte[] data = new byte[0x08];
            /* will be properly implemented later */
            SendPacket(handler, Opcode.SetSubState, data);

        }

        public void SetMainState(Socket handler, MainState state, byte type)
        {
            byte[] data = new byte[0x08];
            data[0] = (byte)state;
            data[0x01] = type;
            SendPacket(handler, Opcode.SetMainState, data);
        }

        public void CreateActor(Socket handler, byte code)
        {
            byte[] data = new byte[0x08];
            data[0] = code;
            SendPacket(handler, Opcode.CreateActor, data);
        }

        public void SendUnknown(Socket handler) => SendPacket(handler, Opcode.Unknown, new byte[0x18]);

        public void SetName(Socket handler, int isCustom = 0, byte[] customName = null)
        {

            byte[] data = new byte[0x28];
            
            if(customName != null)
            {
                Buffer.BlockCopy(customName, 0, data, 0x04, customName.Length);
                Buffer.BlockCopy(BitConverter.GetBytes(isCustom), 0, data, 0, sizeof(int));
            }                
            else
                Buffer.BlockCopy(BitConverter.GetBytes(NameId), 0, data, 0x00, sizeof(uint));

            SendPacket(handler, Opcode.SetName, data);
        }               

        public void SetPosition(Socket handler, Position position, ushort spawnType = 0, ushort isZonning = 0)
        {
            byte[] data = new byte[0x28];

            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0x04, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(position.X), 0, data, 0x08, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(position.Y), 0, data, 0x0c, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(position.Z), 0, data, 0x10, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(position.R), 0, data, 0x14, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(position.FloatingHeight), 0, data, 0x1c, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(spawnType), 0, data, 0x24, sizeof(ushort));
            Buffer.BlockCopy(BitConverter.GetBytes(isZonning), 0, data, 0x26, sizeof(ushort));

            SendPacket(handler, Opcode.SetPosition, data);
        }
               
        public void SetSpeeds(Socket handler, uint[] value = null)
        {         
            byte[] data = new byte[0x88];

            if (value == null) //load defaults
            {
                value = new uint[0x04];
                value[0] = 0;           //Stopped
                value[1] = 0x40000000;  //Walking
                value[2] = 0x40d00000; // 0x40a00000;  //Running
                value[3] = 0x40a00000;  //Acive
            }

            byte index = 0;
            for(int i = 0; i < value.Length; i++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(value[i]), 0, data, index, sizeof(uint));
                index += 0x04;
                data[index] = (byte)i;
                index += 0x04;
            }            

            data[0x80] = 0x04; //only 4 stated discovered so far.

            SendPacket(handler, Opcode.SetSpeed, data);
        }

        public void SetAppearance(Socket handler)
        {
            byte[] data = new byte[0x108];

            Dictionary<uint, uint> AppearanceSlots = new Dictionary<uint, uint>
            {
                { 0x00, Tribe.Model }, //slot number, value
                { 0x01, 0x00 },
                { 0x02, (uint)(SkinColor | HairColor << 10 | EyeColor << 20) },
                { 0x03, BitField.PrimitiveConversion.ToUInt32(Face) },
                { 0x04, (uint)(HairHighlightColor | HairStyle << 10) },
                { 0x05, Voice },
                { 0x06, GearSet.MainWeapon },
                { 0x07, GearSet.SecondaryWeapon },
                { 0x08, GearSet.SPMainWeapon },
                { 0x09, GearSet.SPSecondaryWeapon },
                { 0x0a, GearSet.Throwing },
                { 0x0b, GearSet.Pack },
                { 0x0c, GearSet.Pouch },
                { 0x0d, GearSet.Head },
                { 0x0e, GearSet.Body },
                { 0x0f, GearSet.Legs },
                { 0x10, GearSet.Hands },
                { 0x11, GearSet.Feet },
                { 0x12, GearSet.Waist },
                { 0x13, GearSet.Neck },
                { 0x14, GearSet.RightEar },
                { 0x15, GearSet.LeftEar },
                { 0x16, GearSet.LeftIndex },
                { 0x17, GearSet.RightIndex },
                { 0x18, GearSet.RightFinger },
                { 0x19, GearSet.LeftFinger },
                { 0x1a, 0 },
                { 0x1b, 0 }
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

            SendPacket(handler, Opcode.SetAppearance, data);
        }

        public void SendPacket(Socket handler, Opcode opcode, byte[] data)
        {
            GamePacket gamePacket = new GamePacket
            {
                Opcode = (ushort)opcode,
                Data = data
            };

            Packet packet = new Packet(new SubPacket(gamePacket) { SourceId = Id, TargetId = TargetId });
            handler.Send(packet.ToBytes());
        }

        /// <summary>
        /// This struct is used to keep data about event conditions for actors. Actors have a list of event conditions populated by specialized class' constructor. 
        /// </summary>
        [Serializable]
        public struct EventCondition
        {
            public Opcode Opcode { get; set; }
            public float Radius { get; set; }
            public byte Option1 { get; set; }
            public byte Option2 { get; set; }
            public string ConditionName { get; set; }

            public EventCondition(Opcode opcode, string conditionName, float radius, byte opt1, byte opt2)
            {
                Opcode = opcode;
                Radius = radius;
                Option1 = opt1;
                Option2 = opt2;
                ConditionName = conditionName;
            }
        }

        /// <summary>
        /// Converts a number to a base 63 string. This function was taken from Ioncannon's code, all credit goes to him. 
        /// </summary>
        /// <param name="number">The number to be converted.</param>
        /// <returns></returns>
        public string ToStringBase63(int number)
        {
            var lookup = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var secondDigit = lookup.Substring((int)Math.Floor(number / (double)lookup.Length), 1);
            var firstDigit = lookup.Substring(number % lookup.Length, 1);

            return secondDigit + firstDigit;
        }

        public string GenerateActorName()
        {
            return "";
        }
    }

    /// <summary>
    /// This class store and process all actor's parameters being passed to the game client Lua engine.
    /// </summary>
    [Serializable]
    public class LuaParameters
    {
        public string ActorName { get; set; }
        public string ClassName { get; set; }
        public List<KeyValuePair<byte, object>> List { get; set; }  = new List<KeyValuePair<byte, object>>();

        /// <summary>
        /// Adds one single Lua parameter to the parameter list of the instanced obj.
        /// </summary>
        /// <param name="param">The parameter to be written.</param>
        public void Add(object param)
        {
            if (param is int)
                List.Add(new KeyValuePair<byte, object>(0, (int)param));
            else if (param is uint)
                List.Add(new KeyValuePair<byte, object>(0x01, (uint)param));
            else if (param is string)
                List.Add(new KeyValuePair<byte, object>(0x02, (string)param));
            else if (param is null)
                List.Add(new KeyValuePair<byte, object>(0x05, null));
            else if (param is byte)
                List.Add(new KeyValuePair<byte, object>(0xc, (byte)param));
            else if (param is bool)
            {
                if ((bool)param)
                    List.Add(new KeyValuePair<byte, object>(0x03, null));
                else
                    List.Add(new KeyValuePair<byte, object>(0x04, null));
            }
        }

        /// <summary>
        /// Writes all parameters in the instanced obj parameter list to the packet to be sent.
        /// </summary>
        /// <param name="data">A pointer to the packet buffer. </param>
        /// <param name="luaParameters">The list of parameters to be witten.</param>
        public static void WriteParameters(ref byte[] data, LuaParameters luaParameters)
        {
            //Write Params - using binary writer bc sizes, types, #items can vary.
            using (MemoryStream stream = new MemoryStream(data))
            {
                using(BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Seek(0x44, SeekOrigin.Begin); //points to the right position

                    foreach (var parameter in luaParameters.List)
                    {
                        if (parameter.Key == 0x01)
                            writer.Write((byte)0);
                        else
                            writer.Write(parameter.Key);

                        switch (parameter.Key)
                        {
                            case 0:
                                writer.Write(SwapEndian((int)parameter.Value));
                                break;
                            case 0x01:
                                writer.Write(SwapEndian((uint)parameter.Value));
                                break;
                            case 0x02:
                                string str = (string)parameter.Value;
                                writer.Write(Encoding.ASCII.GetBytes(str), 0, Encoding.ASCII.GetByteCount(str));
                                writer.Write((byte)0);
                                break;                           
                            case 0x05: //null
                                break;
                            case 0x06:
                                writer.Write(SwapEndian((uint)parameter.Value));
                                break;
                            case 0x07:
                                break;
                            case 0x09:
                                break;
                            case 0x0c:
                                writer.Write((byte)parameter.Value);
                                break;
                            case 0x1b:
                                break;
                            case 0x0f:
                                continue;
                        }
                    }

                    writer.Write((byte)0x0f);
                }               
            }            
        }

        #region Endian swappers
        public static ulong SwapEndian(ulong input)
        {
            return 0x00000000000000FF & (input >> 56) |
                   0x000000000000FF00 & (input >> 40) |
                   0x0000000000FF0000 & (input >> 24) |
                   0x00000000FF000000 & (input >> 8) |
                   0x000000FF00000000 & (input << 8) |
                   0x0000FF0000000000 & (input << 24) |
                   0x00FF000000000000 & (input << 40) |
                   0xFF00000000000000 & (input << 56);
        }

        public static uint SwapEndian(uint input)
        {
            return ((input >> 24) & 0xff) |
                   ((input << 8) & 0xff0000) |
                   ((input >> 8) & 0xff00) |
                   ((input << 24) & 0xff000000);
        }

        public static int SwapEndian(int input)
        {
            var inputAsUint = (uint)input;

            input = (int)
                (((inputAsUint >> 24) & 0xff) |
                 ((inputAsUint << 8) & 0xff0000) |
                 ((inputAsUint >> 8) & 0xff00) |
                 ((inputAsUint << 24) & 0xff000000));

            return input;
        }
        #endregion  
    }
}

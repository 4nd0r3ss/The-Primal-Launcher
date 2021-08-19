using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    public class Aetheryte : Actor
    {       
        public uint PrivilegeLevel { get; set; }   
        public uint TeleportMenuPageId { get; set; }
        public uint TeleportMenuId { get; set; }
        public uint AnimaCost { get; set; }
        private float PushEventRadius { get; set; }
        private AetheryteType AetheryteType { get; set; }
                
        public Aetheryte(uint classId, AetheryteType type, Position position, uint menuPageId = 0, uint menuId = 0, uint body = 1024)
        {
            ClassId = classId;
            Appearance.Size = 0x02;
            Position = position;
            AetheryteType = type;
            TeleportMenuPageId = menuPageId;
            TeleportMenuId = menuId;
            AnimaCost = 2; //calculated by distance from current location?
            Appearance = new Appearance { Body = body, Size = 0x02, SkinColor = 0x01, HairColor = 0x01, EyeColor = 0x01 };
            ClassPath = "/Chara/Npc/Object/Aetheryte/";
            ClassCode = 0x26000000;

            switch (AetheryteType)
            {
                case AetheryteType.Crystal:
                    NameId = 4010014;
                    ClassName = "AetheryteParent";
                    PushEventRadius = 10.0f;
                    break;
                case AetheryteType.Gate:
                    NameId = 4010015;
                    ClassName = "AetheryteChild"; 
                    PushEventRadius = 3.0f;
                    break;                
            }            
        }

        public override void Prepare()
        {            
            Zone zone = World.Instance.Zones.Find(x => x.Id == Position.ZoneId);
            Events = new List<Event>();
            Appearance.BaseModel = (uint)AetheryteType;

            Events.Add(new Event { Opcode = ServerOpcode.TalkEvent, Name = "talkDefault", Priority = 0x04, Enabled = 1 });
            Events.Add(new Event { Opcode = ServerOpcode.NoticeEvent, Name = "pushCommand", Priority = 0x04 });            
            Events.Add(new Event { Opcode = ServerOpcode.PushEventCircle, Enabled = 1, Name = "pushCommandIn", ServerCodes = Id, Radius = PushEventRadius, Direction = 0x01, Silent = 0x01 });
            Events.Add(new Event { Opcode = ServerOpcode.PushEventCircle, Enabled = 1, Name = "pushCommandOut", ServerCodes = Id, Radius = PushEventRadius, Direction = 0x11, Silent = 0x01 });
            Events.Add(new Event{ Opcode = ServerOpcode.NoticeEvent, Name = "noticeEvent", Silent = 1 });
            
            base.Prepare();     
        }

        public override void Spawn(Socket sender, ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {
            Prepare();
            CreateActor(sender, 0x08);
            SetEventConditions(sender);
            SetSpeeds(sender);
            SetPosition(sender);
            SetAppearance(sender);
            SetName(sender);
            SetMainState(sender);
            SetSubState(sender);
            SetAllStatus(sender);
            SetIsZoning(sender);
            SetLuaScript(sender);
            Init(sender);
            SetEventStatus(sender);
            Spawned = true;
        }

        public override void Init(Socket sender)
        {
            WorkProperties property = new WorkProperties(sender, Id, @"/_init");    
            property.Add("charaWork.property[0]", true);
            property.Add("charaWork.property[1]", true);            
            property.Add("npcWork.pushCommand", (short)0x2712);
            property.Add("npcWork.pushCommandPriority", (byte)0x08);
            property.FinishWriting(Id);
        }

        public override void talkDefault(Socket sender)
        {
            LuaParameters parameters = new LuaParameters()
            {
                Parameters = new object[]
                {
                    (sbyte)0x05,
                    Encoding.ASCII.GetBytes("eventAetheryteParentSelect"),
                    true,
                    100,
                    0,
                    0,
                    0,
                    0,
                    0
                }
            };

            byte[] data = new byte[0x298];
            Buffer.BlockCopy(BitConverter.GetBytes(User.Instance.Character.Id), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0x04, 4);
            LuaParameters.WriteParameters(ref data, parameters, 0x08);
            Packet.Send(sender, ServerOpcode.EventRequestResponse, data);
            File.WriteAllBytes("aeth.txt", data);
            EventManager.Instance.CurrentEvent.FunctionName = "talkDefault";
            EventManager.Instance.CurrentEvent.IsQuestion = true;
        }

    }

   

}

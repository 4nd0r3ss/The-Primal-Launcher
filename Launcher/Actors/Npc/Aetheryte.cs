using System;
using System.Collections.Generic;
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
            Appearance = new Appearance { Body = body }; 

            switch (AetheryteType)
            {
                case AetheryteType.Crystal:
                    NameId = 4010014;
                    ClassName = "AetheryteParent";
                    ClassPath = "/Chara/Npc/Object/Aetheryte/";                    
                    ClassCode = 0x26000000;
                    PushEventRadius = 10.0f;
                    break;
                case AetheryteType.Gate:
                    NameId = 4010015;
                    ClassName = "AetheryteChild";
                    ClassPath = "/Chara/Npc/Object/Aetheryte/";                    
                    ClassCode = 0x26000000;
                    PushEventRadius = 3.0f;
                    break;
                case AetheryteType.Shard:
                    ClassName = "PopulaceStandard";
                    ClassPath = "/Chara/Npc/Populace/";
                    ClassId = 0x001250A0;
                    ClassCode = 0x33800000;
                    break;
            }            
        }

        public override void Prepare()
        {
            float pushEventRadius = 3.0f;
            Zone zone = World.Instance.Zones.Find(x => x.Id == Position.ZoneId);
            Events = new List<Event>();

            Appearance.BaseModel = (uint)AetheryteType;

            if (AetheryteType != AetheryteType.Shard)
            {
                Events.Add(new Event { Opcode = ServerOpcode.TalkEvent, Name = "talkDefault", Priority = 0x04, Enabled=1 });
                Events.Add(new Event { Opcode = ServerOpcode.NoticeEvent, Name = "pushCommand", Priority = 0x04 });
                //push event
                Events.Add(new Event { Opcode = ServerOpcode.PushEventCircle, Enabled=1, Name = "pushCommandIn", ServerCodes = 0x44c00014, Radius = pushEventRadius, Direction = 0x01, Silent = 0x00, Action= "defaultTalkWithMiniaeth_arc_001" });
                Events.Add(new Event { Opcode = ServerOpcode.PushEventCircle, Enabled=1, Name = "pushCommandOut", ServerCodes = 0x44c00014, Radius = pushEventRadius, Direction = 0x11, Silent = 0x00 });
            }

            Events.Add(new Event{ Opcode = ServerOpcode.NoticeEvent, Name = "noticeEvent", Enabled = 0x01 });
            
            base.Prepare();     
        }

        public override void Init(Socket sender)
        {
            //charaWork.property[0]


            WorkProperties property = new WorkProperties(sender, Id, @"/_init");
            property.Add((uint)0xE14B0CA8, true);

            if (AetheryteType != AetheryteType.Shard)
            {
                //charaWork.property[1]
                //??
                //npcWork.pushCommandPriority
                property.Add((uint)0x2138FD71, true);
                property.Add((uint)0x03CF5B58, (short)0x2712);
                property.Add((uint)0x40E25304, (byte)0x08);
            }

            property.FinishWriting();
        }

        public override void talkDefault(Socket sender)
        {
            EventManager.Instance.CurrentEvent.DelegateEvent(sender, 0x01AFCD, "defaultTalkWithMiniaeth_arc_001", null);
        }

    }

   

}

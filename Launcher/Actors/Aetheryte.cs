using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public class Aetheryte : Actor
    {
        public string MapName { get; set; }
        public uint PrivilegeLevel { get; set; }
        public uint BaseModel { get; set; }
        private float PushEventRadius { get; set; }
        //Aetherytes have many attributes in common, so we'll initialize base class' fields with all common values 
        public Aetheryte(uint actorId, uint baseModel, Position position, uint body = 1024)
        {           
            Id = actorId;
            Size = 2;
            HairColor = 1;
            SkinColor = 1;
            EyeColor = 1;
            PropFlag = 3;
            Model = new Model { Type = baseModel };
            AppearanceCode = 0x02;
            Position = position;
            GearSet = new GearSet { Body = body }; //body is the only appearance slot that has info for aetherytes.

            BaseModel = baseModel;            

            if (baseModel == 20902)
            {
                NameId = 4010014;
                ClassPath = "AetheryteParent";
                PushEventRadius = 10.0f;
            }
            else
            {
                NameId = 4010015;
                ClassPath = "AetheryteChild";
                PushEventRadius = 3.0f;
            }            
        }

        public override void Prepare(ushort actorIndex)
        {
            float pushEventRadius = 3.0f;

            //(Opcode opcode, string conditionName, float radius = 0, byte priority = 0, byte isDisabld)

            //Event conditions
            EventConditions = new List<EventCondition>();
            EventConditions.Add(new EventCondition{ Opcode = ServerOpcode.TalkEvent, EventName = "talkDefault", Priority = 0x04 });
            EventConditions.Add(new EventCondition{ Opcode = ServerOpcode.NoticeEvent, EventName = "pushCommand", Priority = 0x04 });
            EventConditions.Add(new EventCondition{ Opcode = ServerOpcode.NoticeEvent, EventName = "noticeEvent", IsDisabled = 0x01 });             

            //push event
            EventConditions.Add(new EventCondition{ Opcode = ServerOpcode.PushEventCircle, EventName = "pushCommandIn", ServerCodes = 0x44c00014, Radius = pushEventRadius, Direction = 0x01, IsSilent =  0x00 });
            EventConditions.Add(new EventCondition { Opcode = ServerOpcode.PushEventCircle, EventName = "pushCommandOut", ServerCodes = 0x44c00014, Radius = pushEventRadius, Direction = 0x11, IsSilent = 0x00 });

            Zone zone = ActorRepository.Instance.Zones.Find(x => x.Id == Position.ZoneId);

            string zoneName = zone.MapName
                .Replace("Field", "Fld")
                .Replace("Dungeon", "Dgn")
                .Replace("Town", "Twn")
                .Replace("Battle", "Btl")
                .Replace("Test", "Tes")
                .Replace("Event", "Evt")
                .Replace("Ship", "Shp")
                .Replace("Office", "Ofc");

            //actor naming stuff
            //Actor number is the actor ordinal number in the area actor list. the position in the array.
            string name = char.ToLowerInvariant(ClassPath[0]) + ClassPath.Substring(1,10) + "_" + zoneName + "_" + ToStringBase63((int)actorIndex);

            LuaParameters = new LuaParameters
            {
                ActorName = name + "@" + Position.ZoneId.ToString("X3") + PrivilegeLevel.ToString("X2"),
                ClassName = ClassPath,
                ServerCodes = 0x26000000
            };           

            LuaParameters.Add("/Chara/Npc/Object/Aetheryte/" + ClassPath);            
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add((uint)0x00138840);
            LuaParameters.Add(false);
            LuaParameters.Add(false);           
            LuaParameters.Add((uint)0);
            LuaParameters.Add((uint)1);
            LuaParameters.Add("TEST");
        }

        public override void ActorInit(Socket handler)
        {
            byte[] data =
            {
                0x20, 0x01, 0xA8, 0x0C, 0x4B, 0xE1, 0x01, 0x01, 0x71, 0xFD, 0x38, 0x21, 0x01, 0x02, 0x58, 0x5B,
                0xCF, 0x03, 0x12, 0x27, 0x01, 0x04, 0x53, 0xE2, 0x40, 0x08, 0x88, 0x2F, 0x5F, 0x69, 0x6E, 0x69,
                0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };
            SendPacket(handler, ServerOpcode.ActorInit, data);
        }

    }

   

}

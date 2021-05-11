using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    class Monster : Actor
    {
        public Monster()
        {
            ClassPath = "/chara/npc/monster/";
            ClassCode = 0x30400000;            
        }

        public override void Prepare()
        {           
            string actorName = GenerateActorName();
            actorName = actorName.Substring(0, actorName.IndexOf("_") - 1) + actorName.Substring(actorName.IndexOf("_")); //dirty way of removing extra character...

            LuaParameters = new LuaParameters
            {
                ActorName = actorName,
                ClassName = ClassName,
                ClassCode = ClassCode
            };

            LuaParameters.Add(ClassPath + (!string.IsNullOrEmpty(Family) ? Family + "/" : "") + ClassName);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add((int)ClassId);
            LuaParameters.Add(true);
            LuaParameters.Add(true);
            LuaParameters.Add((int)0x0A);
            LuaParameters.Add((int)0);
            LuaParameters.Add((int)0x01);
            LuaParameters.Add((Family == "fighter" ? true : false));
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add((int)0);
        }

        public override void Spawn(Socket sender, ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
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
            SetEventStatus(sender);
            Spawned = true;
        }

        public override void Init(Socket sender)
        {
            byte[] data =
            {
                0x64,
                0x04, 0xF7, 0x6D, 0xF6, 0xC0, 0x00, 0x00, 0x80, 0x3F,
                0x01, 0xA8, 0x0C, 0x4B, 0xE1, 0x01,
                0x01, 0x71, 0xFD, 0x38, 0x21, 0x01,
                0x01, 0x13, 0x53, 0x67, 0x7B, 0x01,
                0x01, 0xB1, 0xCF, 0xFB, 0xFB, 0x01,
                0x02, 0xAA, 0xBC, 0x32, 0x42, 0xF4, 0x01,
                0x02, 0x69, 0xFB, 0xCD, 0x7B, 0xF4, 0x01,
                0x02, 0x10, 0x97, 0xF8, 0x13, 0x00, 0x00,
                0x02, 0xC5, 0xD5, 0x95, 0x3C, 0x00, 0x00,
                0x02, 0x76, 0xC8, 0x99, 0x0B, 0x00, 0x00,
                0x01, 0x24, 0xCE, 0x32, 0x75, 0x03,
                0x01, 0xE3, 0xCF, 0x4F, 0x58, 0x03,
                0x02, 0x88, 0x35, 0x06, 0x96, 0x02, 0x00,   //unit # or level?
                0x01, 0xBE, 0x05, 0x9C, 0x8E, 0x01,             
                0x88, 0x2F, 0x5F, 0x69, 0x6E, 0x69, 0x74,

                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00
            };

            Packet.Send(sender, ServerOpcode.ActorInit, data, Id);
        }
    }
}

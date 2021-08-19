using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PrimalLauncher
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
            string actorName = GenerateName();
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
            WorkProperties property = new WorkProperties(sender, Id, @"/_init");
            property.Add("charaWork.battleSave.potencial", 0x3F800000);
            property.Add("charaWork.property[0]", true);
            property.Add("charaWork.property[1]", true);
            property.Add("charaWork.property[2]", true);
            property.Add("charaWork.property[4]", true);
            property.Add("charaWork.parameterSave.hp[0]", (short)0x01F4);
            property.Add("charaWork.parameterSave.hpMax[0]", (short)0x01F4);
            property.Add("charaWork.parameterSave.mp", (short)0);
            property.Add("charaWork.parameterSave.mpMax", (short)0);
            property.Add("charaWork.parameterTemp.tp", (short)0);
            property.Add("charaWork.parameterSave.state_mainSkill[0]", (byte)0x03);
            property.Add("charaWork.parameterSave.state_mainSkill[2]", (byte)0x03);
            property.Add("charaWork.parameterSave.state_mainSkillLevel", (short)0x02);
            property.Add("npcWork.hateType", (byte)0x01);
            property.FinishWriting(Id);
        }
    }
}

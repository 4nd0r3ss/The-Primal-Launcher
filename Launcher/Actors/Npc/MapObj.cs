using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    class MapObj : Actor
    {
        public int ObjectId { get; set; }
        public int LayoutId { get; set; }

        public MapObj()
        {
            ClassPath = "/chara/npc/mapobj/";            
        }

        public override void Prepare()
        {
            LuaParameters = new LuaParameters
            {
                ActorName = GenerateName(),
                ClassName = ClassName,
                ClassCode = 0x30400000
            };

            LuaParameters.Add(ClassPath + ClassName);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add((int)ClassId);
            LuaParameters.Add(true);
            LuaParameters.Add(true);            
            LuaParameters.Add((int)0);
            LuaParameters.Add((int)0);
            LuaParameters.Add(LayoutId);
            LuaParameters.Add(ObjectId);        
        }

        public override void Spawn(Socket sender, ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {
            Prepare();
            CreateActor(sender, 0x08);
            BindToMapObj(sender);
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

        private void BindToMapObj(Socket sender)
        {
            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes(ObjectId), 0, data, 0, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(LayoutId), 0, data, 0x04, sizeof(int));
            Packet.Send(sender, ServerOpcode.BindToMapObj, data, Id);
        }

        public override void Init(Socket sender)
        {
            WorkProperties property = new WorkProperties(sender, Id, @"/_init");
            property.Add("charaWork.battleSave.potencial", 0x3F800000);
            property.Add("charaWork.property[0]", true);           
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

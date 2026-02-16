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
using System.Text;

namespace PrimalLauncher
{
    public class MapObj : Actor
    {
        public int ObjectId { get; set; }
        public int LayoutId { get; set; }
        public string SpawnAnimation { get; set; }
        public string DespawnAnimation { get; set; }

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

        public override void Spawn(ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {
            Prepare();
            CreateActor(0x08);
            BindToMapObj();
            SetEventConditions();
            SetSpeeds();
            SetPosition();
            SetAppearance();
            SetName();
            SetMainState();
            SetSubState();
            SetAllStatus();
            SetIsZoning();
            SetLuaScript();
            Init();
            SetEventStatus();

            if (!string.IsNullOrEmpty(SpawnAnimation))
                PlayAnimation(SpawnAnimation);

            Spawned = true;
        }

        public override void Despawn()
        {
            if (!string.IsNullOrEmpty(DespawnAnimation))
                PlayAnimation(DespawnAnimation);

            base.Despawn();
        }

        private void BindToMapObj()
        {
            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes(ObjectId), 0, data, 0, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(LayoutId), 0, data, 0x04, sizeof(int));
            Packet.Send(ServerOpcode.ObjBindToMap, data, Id);
        }

        public override void Init()
        {
            WorkProperties property = new WorkProperties(Id, @"/_init");
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
            property.FinishWritingAndSend(Id);
        }

        public void PlayAnimation(string animationName)
        {
            byte[] data = new byte[0x10];
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(animationName), 0, data, 0, animationName.Length);
            Packet.Send(ServerOpcode.ObjPlayAnimation, data, Id);
        }
    }
}

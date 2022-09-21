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
using System.Linq;

namespace PrimalLauncher
{
    [Serializable]
    public class Populace : Actor
    {
        public Populace()
        {
            ClassPath = "/chara/npc/populace/";           
            ClassCode = 0x30400000;           
        }

        public override void Spawn(ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {
            Prepare();
            CreateActor(0x08);
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
            SetQuestIcon();
            Spawned = true;
        }               

        public override void Init()
        {
            WorkProperties property = new WorkProperties(Id, @"/_init");
            property.Add("charaWork.battleSave.potencial", 0x3F800000);
            property.Add("charaWork.property[0]", true);
            property.Add("charaWork.property[1]", true);
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
            property.FinishWritingAndSend(Id);
        }

        //TODO: remove this from here.
        public void defaultTalkWithInn_ExitDoor()
        {
            if (EventManager.Instance.CurrentEvent.IsQuestion)
            {
                if (EventManager.Instance.CurrentEvent.Selection[0] == 1)
                {
                    World.Instance.TeleportPlayer(EntryPoints.GetInnExit(User.Instance.Character.InitialTown));
                }

                EventManager.Instance.CurrentEvent.Finish();
            }
            else
            {
                uint innCode = User.Instance.Character.InitialTown;
                string regionName = "sea";

                if (innCode == 2)
                    regionName = "fst";
                else if (innCode == 3)
                    regionName = "wil";

                EventManager.Instance.CurrentEvent.IsQuestion = true;
                EventManager.Instance.CurrentEvent.Callback = "defaultTalkWithInn_ExitDoor";
                EventManager.Instance.CurrentEvent.DelegateEvent(GetTalkCode(regionName), TalkFunctions.FirstOrDefault(x => x.Key == 0).Value, null);
            }
        }
    }
}

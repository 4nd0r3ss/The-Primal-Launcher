using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    public class Populace : Actor
    {
        public Populace()
        {
            ClassPath = "/chara/npc/populace/";           
            ClassCode = 0x30400000;
            //TalkFunction = "processEvent000_9"; // random speech for all npcs, need to change this.
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
            SetQuestIcon(sender);
            Spawned = true;
        }               

        public override void Init(Socket sender)
        {
            WorkProperties property = new WorkProperties(sender, Id, @"/_init");
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
            property.FinishWriting(Id);
        }

        public void defaultTalkWithInn_ExitDoor(Socket sender)
        {
            if (EventManager.Instance.CurrentEvent.IsQuestion)
            {
                if (EventManager.Instance.CurrentEvent.Selection == 1)
                {
                    World.Instance.TeleportPlayer(sender, EntryPoints.GetInnExit(User.Instance.Character.InitialTown));
                }

                EventManager.Instance.CurrentEvent.Finish(sender);
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
                EventManager.Instance.CurrentEvent.FunctionName = "defaultTalkWithInn_ExitDoor";
                EventManager.Instance.CurrentEvent.DelegateEvent(sender, GetTalkCode(regionName), TalkFunctions.FirstOrDefault(x => x.Key == 0).Value, null);
            }
        }
    }
}

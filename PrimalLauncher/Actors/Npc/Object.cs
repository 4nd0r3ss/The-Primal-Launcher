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
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace PrimalLauncher
{
    public class Object : Actor
    {
        public Object()
        {
            ClassPath = "/chara/npc/object/";
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
            ToggleEvents(true);
            Spawned = true;
        }

        public override void Init()
        {
            WorkProperties property = new WorkProperties(Id, @"/_init");

            
            property.Add("charaWork.battleSave.potencial", 0x3F800000);
            property.Add("charaWork.property[0]", true);  
            
            if(ClassName == "ObjectBed" || ClassName == "ObjectItemStorage")
            {
                property.Add("charaWork.property[1]", true);
                property.Add("charaWork.property[4]", true);
            }

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

        #region Opening stopper methods
        public void caution()
        {
            Event caution = Events.Find(x => x.Name == "caution");

            if (caution != null && !string.IsNullOrEmpty(caution.Action))
            {
                string action = caution.Action;
                List<object> parameters = new List<object>();               

                //if the action has parameters
                if (action.IndexOf(":") > 0)
                {
                    string[] split = action.Split(new char[] { ':' }); //separate function name from parameter string
                    action = split[0]; //function name 
                    parameters.Add(split[1]); //parameter string
                }

                World.SendTextSheet(0x20853D);
            }
        } 
        
        public void exit()
        {
            Event exit = Events.Find(x => x.Name == "exit");

            if (exit != null && !string.IsNullOrEmpty(exit.Action))
            {
                string action = exit.Action;

                if (action.IndexOf(":") > 0)
                {
                    string[] split = action.Split(new char[] { ':' });

                    switch (split[0])
                    {
                        case "TurnBack":                            
                            User.Instance.Character.TurnBack(Convert.ToSingle(split[1]));
                            break;
                        case "ExitInstance":
                            World.Instance.ZoneInstance.Exit();
                            break;
                        case "ExitTo":

                            break;
                    }
                }
            }
        }
        #endregion

        public void AskLogout()
        {
            if (EventManager.Instance.CurrentEvent.IsQuestion)
            {
                switch (EventManager.Instance.CurrentEvent.Selection[0])
                {
                    case 2:
                        PlayerCharacter.ExitGame();
                        break;
                    case 3:
                        PlayerCharacter.Logout();
                        break;
                    case 4:
                        Log.Instance.Success("Check bed selected.");
                        break;
                }

                EventManager.Instance.CurrentEvent.Finish();
            }
            else
            {
                EventManager.Instance.CurrentEvent.IsQuestion = true;
                EventManager.Instance.CurrentEvent.Callback = "AskLogout";
                EventManager.Instance.CurrentEvent.RequestParameters.Add(Encoding.ASCII.GetBytes("askLogout"));
                EventManager.Instance.CurrentEvent.RequestParameters.Add(User.Instance.Character.Id);
                EventManager.Instance.CurrentEvent.Response();
            }
        }
    }
}

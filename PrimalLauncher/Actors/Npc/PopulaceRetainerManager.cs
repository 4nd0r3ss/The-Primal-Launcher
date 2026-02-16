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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrimalLauncher
{
    public class PopulaceRetainerManager : PopulaceStandard
    {
        private int MenuNav { get; set; } = 0;
        private Retainer NewRetainer { get; set; }

        public string Cutscene { get; set; }
        public PopulaceRetainerManager()
        {
            ClassName = GetType().Name;
            Cutscene = "rtn0g010";
        }

        public override void Spawn(ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {
            MenuNav = 0;
            NewRetainer = new Retainer();
            base.Spawn(spawnType, isZoning, changingZone);
        }

        public override void talkDefault()
        {
            if (!EventManager.Instance.CurrentEvent.IsQuestion)
            {
                SendTalk("newEventTalkStep1", 11);
            }
            else
            {
                object selection = null;
                
                if(EventManager.Instance.CurrentEvent.Selection != null & EventManager.Instance.CurrentEvent.Selection.Length > 0)
                    selection = EventManager.Instance.CurrentEvent.Selection[0];

                switch (MenuNav)
                {
                    case 0:
                        return;
                    case 11:
                        if((uint)selection == 0xffffffff)
                        {
                            EndTalk();
                            return;
                        }
                        else
                        {
                            SendTalk("eventTalkStep11", 2);                           
                        }
                        break;                    
                    case 2:
                        SendTalk("eventTalkStep2", 4);//choose by yourself [race selection]                      
                        break;                    
                    case 4:
                        SendTalk("eventTalkStep4", 6);                                              
                        break;
                    case 5:
                        SendTalk("eventTalkStepFinalAnswer", 7);                        
                        NewRetainer.Name = (string)selection;                       
                        break;
                    case 6:
                        SendTalk("eventTalkStepFinish", 0);
                        EndTalk();
                        break;
                    case 7:
                        SendTalk("eventTaklSelectCutSeane", 3, new List<object> { Cutscene });
                        break;
                    default:
                        EndTalk();
                        break;
                }
            }
        }

        private void SendTalk(string functionName, int menuNav, List<object> parameters = null)
        {
            if (parameters == null)            
                parameters = new List<object>();

            Log.Instance.Info("function: " + functionName + ", nav:" + MenuNav);
            EventManager.Instance.CurrentEvent.SendTalkResponse(functionName, parameters, true);
            MenuNav = menuNav;
        }

        private void EndTalk()
        {
            EventManager.Instance.CurrentEvent.Finish();
            MenuNav = 0;
        }

        /// <summary>
        /// Cutscene seems buggy and incomplete, need to re-check lua function parameters.
        /// </summary>
        /// <returns></returns>
        private string GetCutscene()
        {
            if (ClassId == 1001184)
                return "rtn0g010";
            else if(ClassId == 1000166)
                return "rtn0u010";
            else
                return "rtn0l010";
        }
    }
}

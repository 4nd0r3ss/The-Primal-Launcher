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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    public class InnDesk : PopulaceStandard
    {
        public InnDesk()
        {
            ClassName = "PopulaceStandard";
        }

        public override void talkDefault()
        {
            if (User.Instance.Character.Journal.HasFinishedQuest(GetInnQuestId()))
            {
                if (!EventManager.Instance.CurrentEvent.IsQuestion)
                {
                    EventManager.Instance.CurrentEvent.DelegateEvent(GetTalkCode(), "defaultTalkWithInn_Desk");
                    EventManager.Instance.CurrentEvent.IsQuestion = true;
                    EventManager.Instance.CurrentEvent.Callback = "talkDefault";
                }
                else
                {
                    int selection = Convert.ToInt32(EventManager.Instance.CurrentEvent.Selection[0]);

                    if(selection == 1)
                        World.Instance.TeleportPlayer(EntryPoints.GetInnEntry(User.Instance.Character.InitialTown));

                    EndTalk();
                }
            }
            else
            {
                EventManager.Instance.CurrentEvent.DelegateEvent(GetTalkCode(), TalkFunctions.FirstOrDefault(x => x.TalkCode == 0).FunctionName);                              
            }
        }

        private uint GetInnQuestId()
        {
            if (ClassId == 1000458)
                return 110828;
            else if(ClassId == 0)
                return 110838;
            else
                return 110848;
        }

        private void SendTalk(string functionName, int menuNav, List<object> parameters = null)
        {
            if (parameters == null)
                parameters = new List<object>();

            //Log.Instance.Info("function: " + functionName + ", nav:" + MenuNav);
            EventManager.Instance.CurrentEvent.SendTalkResponse(functionName, parameters, true);
            //MenuNav = menuNav;
        }

        private void EndTalk()
        {
            EventManager.Instance.CurrentEvent.Finish();
            //MenuNav = 0;
        }
    }
}

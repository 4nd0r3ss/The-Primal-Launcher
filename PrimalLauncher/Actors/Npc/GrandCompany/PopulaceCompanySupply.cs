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

namespace PrimalLauncher
{
    public class PopulaceCompanySupply : PopulaceCompany
    {
        public PopulaceCompanySupply()
        {
            ClassName = GetType().Name;
        }

        public override void talkDefault()
        {
            //eventTalkPreJoin
            //eventTalkExclusive
            //eventTalkJoined
            //eventTalkStepBreak
            //eventQuestItemMenuOpen
            //eventQuestItemMenuSelect
            //eventQuestItemMenuClose

            if (!EventManager.Instance.CurrentEvent.IsQuestion)
            {
                SendTalk("eventTalkExclusive", new List<object> {  });
            }
            else
            {
                uint? selection = (uint?)EventManager.Instance.CurrentEvent.Selection[0];
                ChatProcessor.SendMessage(MessageType.System, ClassName + " not implemented. ");
                EndTalk();
            }
        }

        private void EndTalk()
        {
            EventManager.Instance.CurrentEvent.SendTalkResponse("eventTalkStepBreak", new List<object> { }, false);
            EventManager.Instance.CurrentEvent.Finish();
        }

        private void SendTalk(string function, List<object> parameters)
        {
            EventManager.Instance.CurrentEvent.SendTalkResponse(function, parameters, true);
        }

    }
}

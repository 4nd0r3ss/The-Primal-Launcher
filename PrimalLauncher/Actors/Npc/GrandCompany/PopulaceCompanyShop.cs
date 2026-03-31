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
    public class PopulaceCompanyShop : PopulaceCompany
    {
        public PopulaceCompanyShop()
        {
            ClassName = GetType().Name;
        }

        public override void talkDefault()
        {
            //eventTalkStepCantUse
            //eventTalkPreJoin
            //eventTalkPreJoinQuest
            //eventTalkJoined
            //eventTalkFestival
            //eventTalkFestival2
            //eventTalkMainMenu - info and open store option
            //eventShopMenuOpen - no params, nothing happens. maybe player or world need to have some value set? check Ask/GrandCompanyShopWidget
            //eventShopMenuAsk - nothing happens
            //eventShopMenuClose
            //eventGuideChocoboWhistle - get chocobo directions -- quest phase?
            //eventGuideTownTransport - city travel points aetherte shards, needs an item to use.
            //eventAskChocoboCustomize - buy something chocobo related for 2 seals
            //eventChocoboCustomize - points player to stables to customize chocobo

            if (!EventManager.Instance.CurrentEvent.IsQuestion)
            {
                SendTalk("eventTalkStepCantUse", new List<object> {  });
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

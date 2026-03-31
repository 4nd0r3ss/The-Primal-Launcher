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
    public class PopulaceBlackMarketeer : PopulaceStandard
    {
        private int MenuNav { get; set; }
        
        public PopulaceBlackMarketeer()
        {
            ClassName = GetType().Name;
        }

        public void talkDefault()
        {
            if (!EventManager.Instance.CurrentEvent.IsQuestion)
            {
                StartTalk();
            }
            else
            {
                uint? selection = (uint?)EventManager.Instance.CurrentEvent.Selection[0];

                ChatProcessor.SendMessage(MessageType.System, "PopulaceBlackMarketeer not implemented. ");

                EndTalk();

                //switch (MenuNav)
                //{
                //    case 1:
                //        SendTalk("eventAskMainMenu", new List<object> { User.Instance.Character.Id });
                //        MenuNav = 2;
                //        break;
                //    case 2:
                //        string function = "eventGilShopMenuOpen";

                //        if (selection == 2)
                //            function = "eventSealShopMenuOpen";

                //        SendTalk(function, new List<object> { User.Instance.Character.Id });
                //        break;

                //}


                //Log.Instance.Info("PopulaceGuildlevePublisher: " + selection + ", nav: " + MenuNav);
            }
        }

        private void StartTalk()
        {
            //Play around with the parameter list value to see what happens.
            //EventManager.Instance.CurrentEvent.SendTalkResponse("eventTalkWelcome", new List<object> { User.Instance.Character.Id }, true);
            EventManager.Instance.CurrentEvent.SendTalkResponse("eventSealShopMenuOpen", new List<object> {  }, true);

            MenuNav = 1;
        }

        private void EndTalk()
        {
            EventManager.Instance.CurrentEvent.Finish();           
        }

        private void SendTalk(string function, List<object> parameters)
        {
            EventManager.Instance.CurrentEvent.SendTalkResponse(function, parameters, true);
        }
    }
}

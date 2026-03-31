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
    public class PopulaceCompanyGLPublisher : PopulaceCompany
    {
        private int MenuNav { get; set; }
        public PopulaceCompanyGLPublisher()
        {
            ClassName = GetType().Name;
        }

        public override void talkDefault()
        {
            //talkOutsider
            //talkOfferWelcome
            //askCompanyLeve - noparams show box with choose leve and briefing on leves
            //askLeveDetail - seems to be like the leve details function from normal GL publisher
            //eventGLDifficulty - shows dialig with difficulty options
            //eventGLStart - has 3 unknown params
            //talkAfterOffer
            //talkOfferLimit
            //eventGLPlay - this seems to be the main leve selection window
            //eventGLShinpu
            //eventGLThanks
            //eventGLReward

            if (!EventManager.Instance.CurrentEvent.IsQuestion)
            {
                int companyId = User.Instance.Character.CompanyId;
                int companytRank = User.Instance.Character.CompanyRank;

                if (companyId == 0) //not enlisted
                {
                    SendTalk("eventGLDifficulty", new List<object> { User.Instance.Character.Id });
                }
                else if (companyId == CompanyId && companytRank == 0) //not ranked
                {
                    SendTalk("eventTalkProvisional", new List<object> { User.Instance.Character.Id });
                }
                else if (companyId == CompanyId && companytRank > 0) //has rank
                {
                    SendTalk("talkOfferWelcome", new List<object> { true });
                    MenuNav = 1;
                }
                else //not your company
                {
                    SendTalk("eventTalkExclusive", new List<object> { User.Instance.Character.Id });
                }
            }
            else
            {
                uint? selection = (uint?)EventManager.Instance.CurrentEvent.Selection[0];

                switch (MenuNav)
                {
                    case 0:
                        EndTalk();
                        break;
                    case 1:
                        SendTalk("askCompanyLeve", new List<object> { User.Instance.Character.Id });
                        MenuNav = 2;
                        break;

                    
                }

                
                //ChatProcessor.SendMessage(MessageType.System, ClassName + " not implemented. ");
                
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

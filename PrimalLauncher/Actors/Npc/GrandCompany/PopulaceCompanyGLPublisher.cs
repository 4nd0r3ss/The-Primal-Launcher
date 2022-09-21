// Copyright (C) 2022 Andreus Faria
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    internal class PopulaceCompanyGLPublisher : PopulaceCompany
    {
        public override void talkDefault()
        {

            if (EventManager.Instance.CurrentEvent.IsQuestion)
            {
                EventManager.Instance.CurrentEvent.GetQuestionSelection();
                uint? selection = EventManager.Instance.CurrentEvent.Selection[0];

                if (selection.HasValue)
                {
                    if (selection == 0)
                        EndTalkEvent();
                    else
                        SendTalkResponse(selection);
                }
            }
            else
            {
                SendTalkResponse();
                EventManager.Instance.CurrentEvent.Callback = "talkDefault";
                EventManager.Instance.CurrentEvent.IsQuestion = true;
            }
        }     

        private void SendTalkResponse(uint? selection = null)
        {
            int companyId = 0;
            int companytRank = 11;

            if (companyId == 0) //not enlisted
            {
                SendResponse("talkOfferWelcome");
            }
            else if (companyId == CompanyId && companytRank == 0) //not ranked
            {
                SendResponse("eventTalkProvisional");
            }
            else if (companyId == CompanyId && companytRank > 0) //has rank
            {
                //CompanyMemberOptions(selection);
            }
            else //not your company
            {
                SendResponse("talkOutsider", new List<object> { CompanyId });
            }
        }
    }
}

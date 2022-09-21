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
    public class PopulaceFlyingShip : Populace
    {
        public bool IsDepartureAttandant { get; set; } //if it's departure side or arrivals side attendant.
        
        public PopulaceFlyingShip()
        {
            ClassName = "PopulaceFlyingShip";
            IsDepartureAttandant = ClassId == 1500003 || ClassId == 1500055 || ClassId == 1500208;
        }

        //public override void talkDefault()
        //{
        //    if (IsDepartureAttandant)
        //    {

        //    }
        //    else
        //    {
        //        //if player is inside airship landing
        //        //if(User.Instance.Character.Position.ZoneId == 99)
        //        //{
        //        //    if (EventManager.Instance.CurrentEvent.IsQuestion)
        //        //    {

        //        //    }
        //        //    else
        //        //    {
        //        int hasTicket = 30010;
        //        //EventManager.Instance.CurrentEvent.Response(new object[]
        //        //{
        //        //        (sbyte)1,
        //        //        Encoding.ASCII.GetBytes("talkDefault"),
        //        //        Encoding.ASCII.GetBytes("eventOut")
        //        //});
        //        EventManager.Instance.CurrentEvent.DelegateEvent(GetTalkCode(), "eventNG");
        //        //EventManager.Instance.CurrentEvent.Callback = "talkDefault";
        //        EventManager.Instance.CurrentEvent.IsQuestion = true;
        //        //    }
        //        //}
        //        //else
        //        //{
        //        //EventManager.Instance.CurrentEvent.DelegateEvent(GetTalkCode(), "talkDefault", new object[] { "eventNG" });
        //        ////EventManager.Instance.CurrentEvent.Response(new object[]
        //        ////{
        //        ////        (sbyte)1,
        //        ////        Encoding.ASCII.GetBytes("talkDefault"),
        //        ////        Encoding.ASCII.GetBytes("eventNG")
        //        ////});
        //        //EventManager.Instance.CurrentEvent.Finish();
        //        //}
        //    }
        //}
    }
}

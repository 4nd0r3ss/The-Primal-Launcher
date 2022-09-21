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
    public class PopulaceCompany : Populace
    {
        protected int CompanyId { get; set; }
        protected int StepCount { get; set; }
        protected string StartTalkFunction { get; set; }

        public override void Prepare()
        {
            ClassName = GetType().Name;

            if (Position.ZoneId == 233)
                CompanyId = 3;
            else if (Position.ZoneId == 230)
                CompanyId = 1;
            else
                CompanyId = 2;

            base.Prepare();
        }

        protected void SendResponse(string functionName, List<object> parameters = null)
        {
            List<object> toSend = new List<object>
            {
                (sbyte)1,
                Encoding.ASCII.GetBytes("talkDefault"),
                Encoding.ASCII.GetBytes(functionName)
            };

            if (parameters != null)
                toSend.AddRange(parameters);

            EventManager.Instance.CurrentEvent.Response(toSend.ToArray());

        }

        /// <summary>
        /// Finishes talk event for this actor. Most GC actors have an extra Lua function that must be called to finish the talk event, and  
        /// it returns 0 as some other functions do. Because of that we need to keep track of what function should be caled next.
        /// </summary>
        protected void EndTalkEvent()
        {
            if (StepCount == 0)
            {
                SendResponse("eventTalkStepBreak");
                StepCount++;
            }
            else
            {
                EventManager.Instance.CurrentEvent.Finish();
                StepCount = 0;
            }
        }
    }
}

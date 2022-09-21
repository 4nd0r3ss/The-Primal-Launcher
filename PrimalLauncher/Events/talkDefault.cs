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
using System.Text;

namespace PrimalLauncher
{
    public class talkDefault : EventRequest
    {
        public talkDefault(byte[] data) : base(data)
        {
            EventType eventType = (EventType)Enum.Parse(typeof(EventType), GetType().Name);
            RequestParameters.Add((sbyte)0x05);
            RequestParameters.Add(Encoding.ASCII.GetBytes(GetType().Name));
        }

        public override void ProcessEventResult(byte[] data)
        {
            Data = data;

            if (!string.IsNullOrEmpty(Callback) && GetType().GetMethod(Callback) != null && !IsQuestion)
            {
                InvokeMethod(Callback, new object[] { data });
            }
            else
            {
                if (IsQuestion)
                {                    
                    GetQuestionSelection();

                    if (ReturnToOwner) //if return to owner, the actor class has a method to answer the question.
                    {
                        GetActor().InvokeMethod(Callback, new object[] {});
                        return;
                    }
                    else
                    {
                        InvokeMethod(Callback, new object[] {});
                    }
                }

                Finish();
            }

        }   
    }
}

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
using System.Threading;

namespace PrimalLauncher
{
    public class noticeEvent : EventRequest
    {
        public noticeEvent(byte[] data) : base(data)
        {
            EventType eventType = (EventType)Enum.Parse(typeof(EventType), GetType().Name);
            RequestParameters.Add((sbyte)eventType);
            RequestParameters.Add(Encoding.ASCII.GetBytes(GetType().Name));
        }

        //public override void Finish()
        //{
        //    base.Finish();

        //    if (!string.IsNullOrEmpty(Callback))
        //        InvokeMethod(Callback, new object[] {});          
        //}
    }
}

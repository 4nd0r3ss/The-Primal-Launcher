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

namespace PrimalLauncher
{
    public class pushDefault : EventRequest
    {
        public pushDefault(byte[] data) : base(data)
        {
            InitLuaParameters();
        }

        public override void ProcessEventResult(byte[] data)
        {
            Data = data;

            if (!string.IsNullOrEmpty(Callback))
            {
                if (ReturnToOwner)
                {
                    InvokeActorEvent(GetActor());
                }
                else
                {
                    if (GetType().GetMethod(Callback) != null)
                        InvokeMethod(Callback, new object[] { data });
                    else
                        Finish();
                }                
            }
            else
            {
                Finish();
            }
        }      
    }
}

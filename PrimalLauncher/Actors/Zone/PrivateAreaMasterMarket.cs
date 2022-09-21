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
    public class PrivateAreaMasterMarket : Zone
    {
        public int PlaceNameId { get; set; }

        public override void Prepare()
        {
            ClassName = "PrivateAreaMasterMarket";

            Events = new List<Event>
            {
               
                new Event { Opcode = ServerOpcode.NoticeEvent, Name = "noticeEvent", Enabled = 1, Priority = 4 }
            };

            LuaParameters = new LuaParameters
            {
                ActorName = "_areaMaster" + "@0" + LuaParameters.SwapEndian(Id).ToString("X").Substring(0, 4),
                ClassName = ClassName,
                ClassCode = 0x30400000
            };

            LuaParameters.Add("/Area/privatearea/" + ClassName);
            LuaParameters.Add(false);
            LuaParameters.Add(true);
            LuaParameters.Add(MapName);
            LuaParameters.Add((!string.IsNullOrEmpty(ContentFunction) ? ContentFunction : ""));
            LuaParameters.Add((!string.IsNullOrEmpty(ContentFunction) ? 1 : -1));
            LuaParameters.Add(Convert.ToByte(MountAllowed));

            for (int i = 7; i > -1; i--)
                LuaParameters.Add(((byte)Type & (1 << i)) != 0);
        }

        public override void Spawn(ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {
            base.Spawn(spawnType, isZoning, changingZone);

            //StartEvent("noticeEvent");
        }

        public override void noticeEvent()
        {
            SendResponse("cueAttentionOnClient");
        }

        private void SendResponse(string functionName, object[] parameters = null)
        {
            List<object> toAdd = new List<object>
            {
                (sbyte)1,
                Encoding.ASCII.GetBytes("noticeEvent"),
                Encoding.ASCII.GetBytes(functionName)
            };

            if (parameters != null)
                toAdd.AddRange(parameters);

            EventManager.Instance.CurrentEvent.Response(toAdd.ToArray());
        }
    }
}

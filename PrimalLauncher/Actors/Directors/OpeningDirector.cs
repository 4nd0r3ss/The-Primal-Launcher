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

using System.Collections.Generic;

namespace PrimalLauncher
{
    public class OpeningDirector : Director
    {
        public override void Prepare()
        {
            Zone zone = World.Instance.GetZone(User.Instance.Character.Position.ZoneId);
            string zoneName = MinifyMapName(zone.MapName);

            LuaParameters = new LuaParameters
            {
                ActorName = MinifyClassName() + "_" + zoneName + "_" + (Id & 0xff).ToString("X2") + "@0" + LuaParameters.SwapEndian(User.Instance.Character.Position.ZoneId).ToString("X").Substring(0, 4),
                ClassName = ClassName,
                ClassCode = ClassCode
            };

            LuaParameters.Add("/Director/" + ClassName);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);

            Events = new List<Event>();
            Events.Add(new Event { Opcode = ServerOpcode.NoticeEvent, Name = "noticeEvent", Priority = 0x0e });
            Events.Add(new Event { Opcode = ServerOpcode.NoticeEvent, Name = "noticeRequest", Enabled = 0x01, Silent = 1 });
            Events.Add(new Event { Opcode = ServerOpcode.NoticeEvent, Name = "reqForChild", Enabled = 0x01, Silent = 1 });
        }

        public override void Spawn(ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {
            base.Spawn(spawnType, isZoning, changingZone);
            StartEvent("noticeEvent");
        }
    }
}

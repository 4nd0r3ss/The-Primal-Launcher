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
using System.Text;

namespace PrimalLauncher
{
    class QuestDirector : Director
    {
        public string QuestName { get; set; }

        public QuestDirector(string questName)
        {
            QuestName = questName;
        }

        public override void Prepare()
        {
            Zone zone = User.Instance.Character.GetCurrentZone();
            string zoneName = MinifyMapName(zone.MapName);   

            LuaParameters = new LuaParameters
            {
                ActorName = "questDirect" + "_"+ zoneName + "_" + (Id & 0xff).ToString("X2") + "@0" + LuaParameters.SwapEndian(User.Instance.Character.Position.ZoneId).ToString("X").Substring(0, 4),
                ClassName = ClassName + QuestName,
                ClassCode = ClassCode
            };

            LuaParameters.Add("/Director/Quest/" + ClassName + QuestName);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);

            Events = new List<Event>();
            Events.Add(new Event { Opcode = ServerOpcode.NoticeEvent, Name = "noticeEvent", Priority = 0x0e, Enabled = 1 });
            Events.Add(new Event { Opcode = ServerOpcode.NoticeEvent, Name = "noticeRequest", Silent = 0x01 });
            Events.Add(new Event { Opcode = ServerOpcode.NoticeEvent, Name = "reqForChild", Silent = 0x01 });
        }

        public override void noticeEvent()
        {

            EventManager.Instance.CurrentEvent.DelegateEvent(110006, "tellByNpcLinkshellChat", new object[] { 1300018, (uint)(0xA0F00000 | 110006), 266 });
        }
    }
}

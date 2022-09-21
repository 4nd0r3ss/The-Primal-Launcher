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
using System.Threading;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    public class ElevatorStandard : Object
    {
        public int Floor { get; set; }
        public Dictionary<int, string> Destinations { get; set; } = new Dictionary<int, string>();

        private short PlaceDrivenCommand { get; set; }

        public ElevatorStandard()
        {
            Events = new List<Event>
            {
                new Event { Opcode = ServerOpcode.TalkEvent, Name = "talkDefault", Priority = 0x04, Enabled = 1 },
                new Event { Opcode = ServerOpcode.NoticeEvent, Name = "pushCommand", Priority = 0x04 },
                new Event { Opcode = ServerOpcode.PushEventCircle, Enabled = 1, Name = "pushCommandIn", ServerCodes = Id, Radius = 4.0f, Direction = 0x01, Silent = 0x01 },
                new Event { Opcode = ServerOpcode.PushEventCircle, Enabled = 1, Name = "pushCommandOut", ServerCodes = Id, Radius = 4.0f, Direction = 0x11, Silent = 0x01 },
                new Event { Opcode = ServerOpcode.NoticeEvent, Name = "noticeEvent", Silent = 1 }
            };
        }

        public override void Prepare()
        {
            if (Position.ZoneId == 133)
                PlaceDrivenCommand = 0x2716;
            else
                PlaceDrivenCommand = 0x271D;

            ClassName = "Object";

            LuaParameters = new LuaParameters
            {
                ActorName = GenerateName(),
                ClassName = "ElevatorStandard",
                ClassCode = ClassCode,
                Parameters = new object[] { ClassPath + "ElevatorStandard", false, false, false, false, false, (int)ClassId, false, false, 0, 1 }
            };
        }

        public override void Init()
        {
            WorkProperties property = new WorkProperties(Id, @"/_init");
            property.Add("charaWork.property[0]", true);
            property.Add("charaWork.property[1]", true);
            property.Add("npcWork.pushCommand", PlaceDrivenCommand);
            property.Add("npcWork.pushCommandPriority", (byte)0x08);
            property.FinishWritingAndSend(Id);
        }

        public void PlaceDriven()
        {
            StartEvent("talkDefault");
        }

        public override void talkDefault()
        {
            if (EventManager.Instance.CurrentEvent.IsQuestion)
            {
                EventManager.Instance.CurrentEvent.GetQuestionSelection();
                uint? selection = EventManager.Instance.CurrentEvent.Selection[0];

                if (selection.HasValue && selection != 3)
                {
                    SendResponse((int)selection);                   
                    EventManager.Instance.CurrentEvent.AddCutsceneTask("finished", "SetPlayerPosition", Destinations[(int)selection]);
                }
                else
                {
                    EventManager.Instance.CurrentEvent.Finish();
                }     
            }
            else
            {
                SendResponse();
                EventManager.Instance.CurrentEvent.Callback = "talkDefault";
                EventManager.Instance.CurrentEvent.IsQuestion = true;
            }
        }

        private void SendResponse(int selection = 0)
        {
            string functionName = "elevatorAsk";

            if (Position.ZoneId == 133)
                functionName += "Limsa";
            else
                functionName += "Uldah";

            functionName += Floor.ToString("D3");

            EventManager.Instance.CurrentEvent.Response(new object[]
                {
                        (sbyte)1,
                        Encoding.ASCII.GetBytes("talkDefault"),
                        Encoding.ASCII.GetBytes(functionName),
                        selection
                });
        }
    }
}

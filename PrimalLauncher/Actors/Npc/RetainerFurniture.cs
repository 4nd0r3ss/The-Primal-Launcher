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
    public class RetainerFurniture : Object
    {
        public RetainerFurniture()
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
            ClassName = GetType().Name;
            base.Prepare();
        }

        public override void Init()
        {
            WorkProperties property = new WorkProperties(Id, @"/_init");
            property.Add("charaWork.property[0]", true);
            property.Add("charaWork.property[1]", true);
            property.Add("npcWork.pushCommand", (short)0x2718);
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

                switch (selection)
                {
                    case 0:
                        SendResponse("eventRingBell");
                        break;
                    case null:
                        EventManager.Instance.CurrentEvent.Finish();
                        break;
                }
            }
            else
            {
                SendResponse("eventPushStepOpenRetainerMenu");   
                EventManager.Instance.CurrentEvent.Callback = "talkDefault";
                EventManager.Instance.CurrentEvent.IsQuestion = true;
            }
        }

        private void SendResponse(string functionName)
        {   
            EventManager.Instance.CurrentEvent.Response(new object[]
                {
                        (sbyte)1,
                        Encoding.ASCII.GetBytes("talkDefault"),
                        Encoding.ASCII.GetBytes(functionName)
                });
        }
    }
}

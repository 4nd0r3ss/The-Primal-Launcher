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
    internal class MarketEntrance : Object
    {      
        private int ExtraPlace { get; set; }      
        private bool ShowItemSearch { get; set; } = true;
        private int ItemId { get; set; }
        private int Page { get; set; }

        public override void Prepare()
        {
            Events = new List<Event>
            {
                new Event { Opcode = ServerOpcode.PushEventCircle, Name = "pushDefault", Enabled = 1, Silent = 0, Direction = 1, Radius = 4.0f },
                new Event { Opcode = ServerOpcode.NoticeEvent, Name = "noticeEvent", Enabled = 0, Priority = 0 }
            };

            ClassName = "MarketEntrance";

            LuaParameters = new LuaParameters
            {
                ActorName = GenerateName(),
                ClassName = "MarketEntrance",
                ClassCode = ClassCode,
                Parameters = new object[] { ClassPath + "MarketEntrance", false, false, false, false, false, (int)ClassId, false, false, 0, 1 }
            };
        }

        public override void Init()
        {
            WorkProperties property = new WorkProperties(Id, @"/_init");
            property.Add("charaWork.property[0]", true);
            property.FinishWritingAndSend(Id);
        }

        public override void pushDefault()
        {         
            if (EventManager.Instance.CurrentEvent.IsQuestion)
            {
                EventManager.Instance.CurrentEvent.GetQuestionSelection();
                uint? selection = EventManager.Instance.CurrentEvent.Selection[0];

                if (selection.HasValue)
                {                    
                    switch (selection)
                    {
                        case 0xFFFFFFFD:                        
                            if (Page == 0)
                                EventManager.Instance.CurrentEvent.Finish();
                            else
                                SendMarketMenu();
                            break;
                        case 0xFFFFFFFF:
                        case 0:
                        case null:
                            EventManager.Instance.CurrentEvent.Finish();
                            break;
                        case 0xFFFFFFFE: //stop searching for item
                            SendMarketMenu();
                            break;
                        case 0x05E8: //Maelstrom Command                           
                        case 0x09DE: //Adders Nest                           
                        case 0x0DBA: //Hall of Flames
                            SendPlayerToGCHQ((uint)selection);                            
                            break;
                        case 0x0C1A: //market wards
                            SendWardNames();
                            break;
                        case 0x043F:
                        case 0x082B:
                        case 0x0C13:
                            SendPlayerToCity();
                            break;
                        default:
                            SendPlayerToWard((int)selection);
                            break;
                    }
                }
                else
                {
                    EventManager.Instance.CurrentEvent.Finish();
                }
            }
            else
            {
                SendMarketMenu();                
                EventManager.Instance.CurrentEvent.Callback = "pushDefault";
                EventManager.Instance.CurrentEvent.IsQuestion = true;
            }
        }

        
        private void SendPlayerToWard(int selection)
        {
            Position entryPoint = null;

            if (selection >= 1261 && selection <= 1280)
                entryPoint = new Position(134, 160.1f, 0f, 204.71f, 3.13f, 0x0F);
            else if (selection >= 2261 && selection <= 2280)
                entryPoint = new Position(160, 159.98f, 0f, 199.91f, 3.07f, 0x0F);
            else if (selection >= 3261 && selection <= 3280)
                entryPoint = new Position(180, 160f, 4.0f, 274.0f, -3.1f, 0x0F);

            if (entryPoint != null)
            {
                World.Instance.TeleportPlayer(entryPoint);
            }
            else
            {
                Log.Instance.Error("Ward id out of bounds.");
            }                
        }

        /// <summary>
        /// Sends player to the specified Grand Company HQ.
        /// All HQs have the same entry point values, changing zoneId only.
        /// </summary>
        /// <param name="zoneId"></param>
        private void SendPlayerToGCHQ(uint selection)
        {
            uint zoneId;

            if (selection == 0x05E8)
                zoneId = 232;
            else if (selection == 0x09DE)
                zoneId = 234;
            else
                zoneId = 233;

            World.Instance.TeleportPlayer(new Position(zoneId, 160f, 0, -148.3f, 3.1f, 0x0F));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="zoneId"></param>
        private void SendPlayerToCity()
        {
            Position exit = null;

            if (Position.ZoneId == 232 || Position.ZoneId == 134)
                exit = new Position(230, -411.6f, 41.0f, 450.0f, -2.45f, 0x0F);
            else if (Position.ZoneId == 234 || Position.ZoneId == 160)
                exit = new Position(206, -193.81f, 23.63f, -1407.99f, 1.56f, 0x0F);
            else if(Position.ZoneId == 233 || Position.ZoneId == 180)
                exit = new Position(175, -230.77f, 190.0f, 45.28f, 2.4f, 0x0F);

            if (exit != null)
                World.Instance.TeleportPlayer(exit);
            else
                Log.Instance.Error("Could not find an exit position for this area.");
        }

        /// <summary>
        /// Shows the market entrance menu widget.
        /// </summary>
        private void SendMarketMenu()
        {
            Page = 0;
            int exitToAreaNameId = 0;
            int gchq = 0;

            if(Position.ZoneId == 232 || Position.ZoneId == 134)
                exitToAreaNameId = 1087;
            else if(Position.ZoneId == 234 || Position.ZoneId == 160)
                exitToAreaNameId = 2091;
            else if(Position.ZoneId == 233 || Position.ZoneId == 180)
                exitToAreaNameId = 3091;

            if(Position.ZoneId == 230)
                gchq = 1512;
            else if(Position.ZoneId == 206)
                gchq = 2526;
            else if (Position.ZoneId == 175)
                gchq = 3514;

            SendResponse("eventPushChoiceAreaOrQuest", new object[] { exitToAreaNameId, 3098, gchq, ExtraPlace, ShowItemSearch, ItemId });
        }        

        /// <summary>
        /// Shows a selection widget containing the names of the marked wards according to current zone.
        /// The numOfWards variable determines how many options will be displayed by the widget, to a max of 24. The
        /// way it works is it gets the starting id in startWardId and increments this id by 1 to numOfWards.
        /// </summary>
        private void SendWardNames()
        {
            int startWardId = 0;
            int numOfWards = 19;

            Page++;

            if(Position.ZoneId == 230 || Position.ZoneId == 232 || Position.ZoneId == 134)
                startWardId = 1261;
            else if (Position.ZoneId == 206 || Position.ZoneId == 234 || Position.ZoneId == 160)
                startWardId = 2261;
            else if (Position.ZoneId == 175 || Position.ZoneId == 233 || Position.ZoneId == 180)
                startWardId = 3261;

            if(startWardId > 0)
                SendResponse("eventPushStepPrvMarket", new object[] { startWardId, numOfWards });
            else
                SendMarketMenu();
        }

        /// <summary>
        /// Adds the shop npc to the specified market ward according to ward type.
        /// </summary>
        private void AddWardShopNpc()
        {

        }

        /// <summary>
        /// Sends a response to the client.
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="parameters"></param>
        private void SendResponse(string functionName, object[] parameters = null)
        {
            List<object> toAdd = new List<object>
            {
                (sbyte)1,
                Encoding.ASCII.GetBytes("pushDefault"),
                Encoding.ASCII.GetBytes(functionName)
            };

            if (parameters != null)
                toAdd.AddRange(parameters);

            EventManager.Instance.CurrentEvent.Response(toAdd.ToArray());
        }
    }
}

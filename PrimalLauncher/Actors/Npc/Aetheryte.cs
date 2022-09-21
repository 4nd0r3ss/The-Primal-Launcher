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
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;

namespace PrimalLauncher
{
    public class Aetheryte : Actor
    {       
        public uint PrivilegeLevel { get; set; }   
        public uint TeleportMenuPageId { get; set; }
        public uint TeleportMenuId { get; set; }
        public uint AnimaCost { get; set; }
        private float PushEventRadius { get; set; }
        public AetheryteType Type { get; set; }
        private List<int> Children { get; set; }
        private bool ShowTeleportMenu { get; set; }
        private short PlaceDrivenCommand { get; set; }
        private int Parent { get; set; }
                
        public Aetheryte(XmlNode node)
        {           
            var teleportMenuNode = node.SelectSingleNode("teleportMenu");            

            if (teleportMenuNode != null)
            {
                TeleportMenuPageId = Convert.ToUInt32(teleportMenuNode.Attributes["page"].Value);
                TeleportMenuId = Convert.ToUInt32(teleportMenuNode.Attributes["id"].Value);
            }

            ClassId = Convert.ToUInt32(node.Attributes["classId"].Value);
            Appearance.Size = 0x02;
            Position = new Position(node.SelectSingleNode("position"));
            Type = (AetheryteType)Enum.Parse(typeof(AetheryteType), node.Attributes["type"].Value);            
            AnimaCost = 2; //calculated by distance from current location?
            Appearance = new Appearance { Body = 1024, Size = 0x02, SkinColor = 0x01, HairColor = 0x01, EyeColor = 0x01 };
            ClassPath = "/Chara/Npc/Object/Aetheryte/";
            ClassCode = 0x26000000;
            Appearance.BaseModel = (uint)Type;

            switch (Type)
            {
                case AetheryteType.Crystal:
                    NameId = 4010014;
                    ClassName = "AetheryteParent";
                    PushEventRadius = 10.0f;
                    Children = GetChildren();                    
                    ShowTeleportMenu = Children.Any(x => x > 0);
                    PlaceDrivenCommand = 0x2712;
                    break;
                case AetheryteType.Gate:
                    NameId = 4010015;
                    ClassName = "AetheryteChild"; 
                    PushEventRadius = 3.0f;
                    Parent = GetParent();
                    ShowTeleportMenu = true;
                    PlaceDrivenCommand = 0x2713;
                    break;                
            }

            Events = new List<Event>
            {
                new Event { Opcode = ServerOpcode.TalkEvent, Name = "talkDefault", Priority = 0x04, Enabled = 1 },
                new Event { Opcode = ServerOpcode.NoticeEvent, Name = "pushCommand", Priority = 0x04 },
                //new Event { Opcode = ServerOpcode.PushEventCircle, Enabled = 0, Name = "pushDefault", Radius = PushEventRadius, Direction = 0x01 },
                new Event { Opcode = ServerOpcode.PushEventCircle, Enabled = 1, Name = "pushCommandIn", ServerCodes = Id, Radius = PushEventRadius, Direction = 0x01, Silent = 0x01 },
                new Event { Opcode = ServerOpcode.PushEventCircle, Enabled = 1, Name = "pushCommandOut", ServerCodes = Id, Radius = PushEventRadius, Direction = 0x11, Silent = 0x01 },
                new Event { Opcode = ServerOpcode.NoticeEvent, Name = "noticeEvent", Silent = 1 }
            };
        }

        private List<int> GetChildren()
        {
            DataRow dataRow = GetAetheryteDataRow(ClassId);       

            int child1 = Convert.ToInt32(dataRow.ItemArray[5]);
            int child2 = Convert.ToInt32(dataRow.ItemArray[6]);
            int child3 = Convert.ToInt32(dataRow.ItemArray[7]);
            int child4 = Convert.ToInt32(dataRow.ItemArray[8]);
            int child5 = Convert.ToInt32(dataRow.ItemArray[9]);

            return new List<int> {
                ValidateChildId(child1),
                ValidateChildId(child2),
                ValidateChildId(child3),
                ValidateChildId(child4),
                ValidateChildId(child5),
            };
        }

        private int ValidateChildId(int childClassId)
        {
            if(childClassId > 0)
            {
                DataRow dataRow = GetAetheryteDataRow((uint)childClassId);

                if (Convert.ToInt32(dataRow[1]) != 1065)
                    return childClassId;
                else
                    return 0;
            }
            else
            {
                return 0;
            }           
        }

        private int GetParent()
        {
            DataRow dataRow = GetAetheryteDataRow(ClassId);
            return Convert.ToInt32(dataRow[4]);
        }

        private DataRow GetAetheryteDataRow(uint classId)
        {
            DataTable dataTable = GameData.Instance.GetGameData("aetheryte");
            DataRow[] dataRows = dataTable.Select("id = '" + classId + "'");
            return dataRows != null && dataRows.Length > 0 ? dataRows[0] : null;
        }

        public override void Prepare()
        {         
            base.Prepare();     
        }

        public override void Spawn(ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {
            Prepare();
            CreateActor(0x08);
            SetEventConditions();
            SetSpeeds();
            SetPosition();
            SetAppearance();
            SetName();
            SetMainState();
            SetSubState();
            SetAllStatus();
            SetIsZoning();
            SetLuaScript();
            Init();
            SetEventStatus();
            SetQuestIcon();
            Spawned = true;
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
            //if is not attuned
            if(User.Instance.Character.AttunedAetherytes.FirstOrDefault(x => x == ClassId) == 0)
            {
                ShowAttentionDialog(new object[] { 0x02, (int)ClassId });
                User.Instance.Character.AttunedAetherytes.Add(ClassId);
            }

            if (EventManager.Instance.CurrentEvent.IsQuestion)
            {
                EventManager.Instance.CurrentEvent.GetQuestionSelection();
                uint? selection = EventManager.Instance.CurrentEvent.Selection[0];

                if (selection.HasValue)
                {
                    switch (selection)
                    {
                        case 0xFFFFFFFD:
                            FactionStanding();
                            break;
                        case 0xFFFFFFFE:
                            SetPlayerHomePoint();
                            break;
                        case 0xFFFFFFFF:
                            InitiateLeve();
                            break;
                        default:
                            TeleportToNode((uint)selection);
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
                List<object> parameters = new List<object>
                {
                    (sbyte)1,
                    Encoding.ASCII.GetBytes("talkDefault"),
                    Encoding.ASCII.GetBytes("event" + ClassName + "Select"),
                    ShowTeleportMenu
                };

                if (Type == AetheryteType.Crystal)
                {
                    parameters.Add((int)User.Instance.Character.Anima);
                    parameters.Add(Children[0]);
                    parameters.Add(Children[1]);
                    parameters.Add(Children[2]);
                    parameters.Add(Children[3]);
                    parameters.Add(Children[4]);
                }
                else
                {
                    parameters.Add(Parent);
                    parameters.Add((int)User.Instance.Character.Anima); //anima
                    parameters.Add(1);
                }       

                EventManager.Instance.CurrentEvent.RequestParameters = new LuaParameters(){ Parameters = parameters.ToArray() };
                EventManager.Instance.CurrentEvent.Response();
                EventManager.Instance.CurrentEvent.Callback = "talkDefault";
                EventManager.Instance.CurrentEvent.IsQuestion = true;
            }

        }      

        private void SetPlayerHomePoint()
        {
            User.Instance.Character.HomePoint = ClassId;

            EventManager.Instance.CurrentEvent.RequestParameters = new LuaParameters()
            {
                Parameters = new object[]
                    {
                        (sbyte)1,
                        Encoding.ASCII.GetBytes("talkDefault"),
                        Encoding.ASCII.GetBytes("event" + ClassName + "Desion"),
                        (int)ClassId
                    }
            };

            EventManager.Instance.CurrentEvent.Response();
        }

        private void InitiateLeve()
        {
            
        }

        private void FactionStanding()
        {
            World.SendTextSheet(0x007C, customOwnerId: Id);
            World.SendTextSheet(0x007D, new object[] { 1, 15 }, customOwnerId: Id);
            World.SendTextSheet(0x007E, new object[] { 2, 10 }, customOwnerId: Id);
            World.SendTextSheet(0x007F, new object[] { 3, 5 }, customOwnerId: Id);

            EventManager.Instance.CurrentEvent.Finish();
        }

        private void TeleportToNode(uint selection)
        {
            if(selection != 0xFF && selection > 0)
            {
                uint toNode = 0;

                if (Type == AetheryteType.Crystal)
                    toNode = (uint)Children[(int)selection - 1];
                else
                    toNode = (uint)Parent;

                User.Instance.Character.Anima--; //pay 1 anima
                Aetheryte node = World.Instance.Aetherytes.Find(x => x.ClassId == toNode);
                World.Instance.TeleportPlayerToAetheryte(node);
                return;
            }           

            EventManager.Instance.CurrentEvent.Finish();
        }

        public static uint GetStartHomePoint(uint startTown)
        {
            uint[] startHomePoints = { 0, 1280001, 1280061, 1280031 };
            return startHomePoints[(int)startTown];
        }

             
    }  
}

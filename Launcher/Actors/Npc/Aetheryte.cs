using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    public class Aetheryte : Actor
    {       
        public uint PrivilegeLevel { get; set; }   
        public uint TeleportMenuPageId { get; set; }
        public uint TeleportMenuId { get; set; }
        public uint AnimaCost { get; set; }
        private float PushEventRadius { get; set; }
        private AetheryteType Type { get; set; }
        private List<int> Children { get; set; }
        private bool ShowTeleportMenu { get; set; }
        private short InteractionMenuCommand { get; set; }
        private int Parent { get; set; }

                
        public Aetheryte(uint classId, AetheryteType type, Position position, uint menuPageId = 0, uint menuId = 0, uint body = 1024)
        {
            ClassId = classId;
            Appearance.Size = 0x02;
            Position = position;
            Type = type;
            TeleportMenuPageId = menuPageId;
            TeleportMenuId = menuId;
            AnimaCost = 2; //calculated by distance from current location?
            Appearance = new Appearance { Body = body, Size = 0x02, SkinColor = 0x01, HairColor = 0x01, EyeColor = 0x01 };
            ClassPath = "/Chara/Npc/Object/Aetheryte/";
            ClassCode = 0x26000000;

            switch (Type)
            {
                case AetheryteType.Crystal:
                    NameId = 4010014;
                    ClassName = "AetheryteParent";
                    PushEventRadius = 10.0f;
                    Children = GetChildren();                    
                    ShowTeleportMenu = Children.Any(x => x > 0);
                    InteractionMenuCommand = 0x2712;
                    break;
                case AetheryteType.Gate:
                    NameId = 4010015;
                    ClassName = "AetheryteChild"; 
                    PushEventRadius = 3.0f;
                    Parent = GetParent();
                    ShowTeleportMenu = true;
                    InteractionMenuCommand = 0x2713;
                    break;                
            }
        }

        private List<int> GetChildren()
        {
            DataRow dataRow = GetAetheryteDataRow();       

            return new List<int> {
                Convert.ToInt32(dataRow.ItemArray[5]),
                Convert.ToInt32(dataRow.ItemArray[6]),
                Convert.ToInt32(dataRow.ItemArray[7]),
                Convert.ToInt32(dataRow.ItemArray[8]),
                Convert.ToInt32(dataRow.ItemArray[9]),
            };
        }

        private int GetParent()
        {
            DataRow dataRow = GetAetheryteDataRow();
            return Convert.ToInt32(dataRow[4]);
        }

        private DataRow GetAetheryteDataRow()
        {
            DataTable dataTable = GameData.Instance.GetGameData("aetheryte");
            DataRow[] dataRows = dataTable.Select("id = '" + ClassId + "'");
            return dataRows != null && dataRows.Length > 0 ? dataRows[0] : null;
        }

        public override void Prepare()
        {            
            Zone zone = World.Instance.Zones.Find(x => x.Id == Position.ZoneId);
            Events = new List<Event>();
            Appearance.BaseModel = (uint)Type;

            Events.Add(new Event { Opcode = ServerOpcode.TalkEvent, Name = "talkDefault", Priority = 0x04, Enabled = 1 });
            Events.Add(new Event { Opcode = ServerOpcode.NoticeEvent, Name = "pushCommand", Priority = 0x04 });            
            Events.Add(new Event { Opcode = ServerOpcode.PushEventCircle, Enabled = 1, Name = "pushCommandIn", ServerCodes = Id, Radius = PushEventRadius, Direction = 0x01, Silent = 0x01 });
            Events.Add(new Event { Opcode = ServerOpcode.PushEventCircle, Enabled = 1, Name = "pushCommandOut", ServerCodes = Id, Radius = PushEventRadius, Direction = 0x11, Silent = 0x01 });
            Events.Add(new Event{ Opcode = ServerOpcode.NoticeEvent, Name = "noticeEvent", Silent = 1 });

            base.Prepare();     
        }

        public override void Spawn(Socket sender, ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {
            Prepare();
            CreateActor(sender, 0x08);
            SetEventConditions(sender);
            SetSpeeds(sender);
            SetPosition(sender);
            SetAppearance(sender);
            SetName(sender);
            SetMainState(sender);
            SetSubState(sender);
            SetAllStatus(sender);
            SetIsZoning(sender);
            SetLuaScript(sender);
            Init(sender);
            SetEventStatus(sender);
            Spawned = true;
        }

        public override void Init(Socket sender)
        {
            WorkProperties property = new WorkProperties(sender, Id, @"/_init");    
            property.Add("charaWork.property[0]", true);
            property.Add("charaWork.property[1]", true);            
            property.Add("npcWork.pushCommand", InteractionMenuCommand);
            property.Add("npcWork.pushCommandPriority", (byte)0x08);
            property.FinishWriting(Id);
        }

        public void PlaceDriven(Socket sender)
        {
            EventManager.Instance.CurrentEvent.RequestParameters = new LuaParameters()
            {
                Parameters = new object[]
                         {
                            (sbyte)1,
                            Encoding.ASCII.GetBytes("commandRequest"),
                            Encoding.ASCII.GetBytes("eventAetheryteParentSelect"),
                            ShowTeleportMenu,
                            100,
                            Children[4],
                            Children[3],
                            Children[2],
                            Children[1],
                            Children[0]
                         }
            };

            EventManager.Instance.CurrentEvent.Response(sender);
            EventManager.Instance.CurrentEvent.FunctionName = "talkDefault";
            EventManager.Instance.CurrentEvent.IsQuestion = true;
        }

        public override void talkDefault(Socket sender)
        {
            if (EventManager.Instance.CurrentEvent.IsQuestion)
            {
                uint selection = EventManager.Instance.CurrentEvent.Selection;

                switch (selection)
                {
                    case 0xFFFFFFFD:
                        FactionStanding(sender);
                        break;
                    case 0xFFFFFFFE:
                        SetPlayerHomePoint(sender);
                        break;
                    case 0xFFFFFFFF:
                        InitiateLeve(sender);
                        break;
                    default:
                        TeleportToNode(sender, selection);
                        break;
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
                EventManager.Instance.CurrentEvent.Response(sender);
                EventManager.Instance.CurrentEvent.FunctionName = "talkDefault";
                EventManager.Instance.CurrentEvent.IsQuestion = true;
            }

        }      

        private void SetPlayerHomePoint(Socket sender)
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

            EventManager.Instance.CurrentEvent.Response(sender);
        }

        private void InitiateLeve(Socket sender)
        {
            
        }

        private void FactionStanding(Socket sender)
        {

        }

        private void TeleportToNode(Socket sender, uint selection)
        {
            if(selection != 0xFF && selection > 0)
            {
                uint toNode = 0;

                if (Type == AetheryteType.Crystal)
                    toNode = (uint)Children[(int)selection - 1];
                else
                    toNode = (uint)Parent;

                User.Instance.Character.Anima--; //cost 1 anima
                Aetheryte node = ActorRepository.Instance.Aetherytes.Find(x => x.ClassId == toNode);
                World.Instance.TeleportPlayerToAetheryte(sender, node);
                return;
            }           

            EventManager.Instance.CurrentEvent.Finish(sender);
        }
    }  
}

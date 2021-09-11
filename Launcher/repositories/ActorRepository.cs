using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PrimalLauncher
{
    public class ActorRepository
    {
        private static ActorRepository _instance = null;   
        private List<Aetheryte> _aetherytes;

        //get game data tables with actors data
        private readonly DataTable ActorsGraphics = GameData.Instance.GetGameData("actorclass_graphic");
        private readonly DataTable ActorsNameIds = GameData.Instance.GetGameData("actorclass");
        private readonly DataTable ActorsNames = GameData.Instance.GetGameData("xtx/displayName");
        
        public List<Aetheryte> Aetherytes
        {
            get
            {
                if(_aetherytes == null)                
                    _aetherytes = GetAetherytes();                

                return _aetherytes;
            }
        }
        public static ActorRepository Instance
        {
            get
            {              
              if (_instance == null)
                  _instance = new ActorRepository();

              return _instance;   
            }
        }

        public List<Aetheryte> GetAetherytes()
        {
            XmlDocument aetheryteFile = new XmlDocument();
            string file = GetXmlResource("AetheryteList.xml");
            List<Aetheryte> result = new List<Aetheryte>();

            try
            {
                //prepare xml nodes
                aetheryteFile.LoadXml(file);
                XmlElement root = aetheryteFile.DocumentElement;
                XmlNode list = root;

                //each npc node in xml 
                foreach (XmlNode node in list.ChildNodes)
                {
                    uint clasId = Convert.ToUInt32(node.Attributes["classId"].Value);
                    uint pageId = 0;
                    uint menuId = 0;
                    AetheryteType type = (AetheryteType)Enum.Parse(typeof(AetheryteType), node.Attributes["type"].Value);

                    var teleportMenuNode = node.SelectSingleNode("teleportMenu");
                    var positionNode = node.SelectSingleNode("position");

                    if(teleportMenuNode != null)
                    {
                        pageId = Convert.ToUInt32(teleportMenuNode.Attributes["page"].Value);
                        menuId = Convert.ToUInt32(teleportMenuNode.Attributes["id"].Value);
                    }

                    Position posisition = new Position
                    {
                        ZoneId = Convert.ToUInt32(positionNode.Attributes["zoneId"].Value),
                        X = Convert.ToSingle(positionNode.Attributes["x"].Value),
                        Y = Convert.ToSingle(positionNode.Attributes["y"].Value),
                        Z = Convert.ToSingle(positionNode.Attributes["z"].Value),
                        R = Convert.ToSingle(positionNode.Attributes["r"].Value),
                    };

                    result.Add(new Aetheryte(clasId, type, posisition, pageId, menuId));
                }
            }
            catch (Exception e)
            {
                Log.Instance.Warning(e.Message);
            }

            return result;
        } 

        public List<Actor> GetZoneNpcs(uint zoneId, string fileName = "npc.xml")
        {
            if (string.IsNullOrEmpty(fileName))
                fileName = "npc.xml";

            string zoneDir = @"" + zoneId.ToString("X") ;       
            string fileNamePath = zoneDir + "." + fileName;
            XmlDocument npcFile = new XmlDocument();
            string file = GetXmlResource("zones.x" + fileNamePath);
            List<Actor> zoneNpcs = new List<Actor>();           

            if (file != "")
            {
                try
                {           
                    //prepare xml nodes
                    npcFile.LoadXml(file);
                    XmlElement root = npcFile.DocumentElement;
                    XmlNode chara = root.FirstChild;                    

                    //each npc node in xml 
                    foreach (XmlNode node in chara.ChildNodes)
                    {
                        Actor actor = LoadActor(node, zoneId);

                        if(actor != null)
                            zoneNpcs.Add(actor);
                    }                                                    
                }
                catch (Exception e)
                {
                    Log.Instance.Warning(e.Message);
                }
            }

            return zoneNpcs;
        }

        public List<Actor> GetCompanyWarp(uint zoneId)
        {            
            XmlDocument npcFile = new XmlDocument();
            string file = GetXmlResource("CompanyWarp.xml");
            List<Actor> npcs = new List<Actor>();

            if (file != "")
            {
                try
                {
                    //prepare xml nodes
                    npcFile.LoadXml(file);
                    XmlElement root = npcFile.DocumentElement;
                    XmlNode chara = root.FirstChild;

                    //each npc node in xml 
                    foreach (XmlNode node in chara.ChildNodes)
                    {
                        if (Convert.ToUInt32(node.Attributes["zone"].Value) == zoneId)
                        {
                            CompanyWarp companyWarp = (CompanyWarp)LoadActor(node, zoneId);
                            companyWarp.Region = Convert.ToUInt32(node.Attributes["region"].Value);
                            companyWarp.Zone = Convert.ToUInt32(node.Attributes["zone"].Value);
                            companyWarp.WarpId = Convert.ToInt32(node.Attributes["id"].Value);
                            npcs.Add(companyWarp);
                        }
                    }                        
                }
                catch (Exception e)
                {
                    Log.Instance.Warning(e.Message);
                }
            }

            return npcs;
        }

        public Position GetCompanyWarpPositionById(uint region, uint id)
        {
            XmlDocument npcFile = new XmlDocument();
            string file = GetXmlResource("CompanyWarp.xml");

            try
            {
                //prepare xml nodes
                npcFile.LoadXml(file);
                XmlElement root = npcFile.DocumentElement;
                XmlNode chara = root.FirstChild;

                //each npc node in xml 
                foreach (XmlNode node in chara.ChildNodes)
                {
                    if (Convert.ToUInt32(node.Attributes["region"].Value) == region && Convert.ToUInt32(node.Attributes["id"].Value) == id)
                        return SetPosition(Convert.ToUInt32(node.Attributes["zone"].Value), node.SelectSingleNode("position"));
                }
            }
            catch (Exception e)
            {
                Log.Instance.Warning(e.Message);
            }

            return null;
        }

        private Actor LoadActor(XmlNode node, uint zoneId)
        {
            Type type = Type.GetType("PrimalLauncher." + node.Name);

            string className = node.Attributes["className"] != null ? node.Attributes["className"].Value : "";

            if (type != null
                && (
                className == "" ||
                className == "PopulaceCutScenePlayer" ||
                className == "PopulaceStandard" ||
                className == "PopulaceCompanyWarp" ||
                className == "PopulaceFlyingShip" ||
                className == "PopulaceLinkshellManager" ||
                className == "PopulacePassiveGLPublisher" ||
                className == "RetainerFurniture" ||
                className == "PopulaceGuildlevePublisher" ||
                className == "PopulaceRetainerManager" ||
                className == "MapObjShipPort" ||
                className == "PopulaceSpecialEventCryer" ||                
                type.Name == "Monster" ||               
                type.Name == "Object"
                )
            )
            {

                //if(node.Attributes["className"].Value == "RetainerFurniture")
                //{
                //    int i = 1;
                //}


                //XmlNode node = objNode.SelectSingleNode("PopulaceStandard");
                uint classId = Convert.ToUInt32(node.SelectSingleNode("classId").InnerText);
                uint state = node.SelectSingleNode("state") != null ? Convert.ToUInt32(node.SelectSingleNode("state").InnerText) : 0; //TODO: fix this as it is 2 bytes. so far it's alaways 0 so it's ok.
                ushort animation = node.SelectSingleNode("animation") != null ? Convert.ToUInt16(node.SelectSingleNode("animation").InnerText) : (ushort)0;
                int questIcon = node.SelectSingleNode("questIcon") != null ? Convert.ToInt32(node.SelectSingleNode("questIcon").InnerText) : -1;

                //get table lines with npc data
                //had to separate the selects as it throws an exception when the actor have no appearance and/or nameid data.
                DataRow[] actorsGraphicsSelect = ActorsGraphics.Select("id = '" + classId + "'");
                DataRow[] actorsNameIdsSelect = ActorsNameIds.Select("id = '" + classId + "'");
                DataRow actorGraphics = actorsGraphicsSelect != null && actorsGraphicsSelect.Length > 0 ? actorsGraphicsSelect[0] : null;
                DataRow actorNameId = actorsNameIdsSelect != null && actorsNameIdsSelect.Length > 0 ? actorsNameIdsSelect[0] : null;

                Actor actor = (Actor)Activator.CreateInstance(type);
                actor.Family = node.Attributes["family"] != null ? node.Attributes["family"].Value : "";
                actor.ClassId = classId;
                actor.ClassName = node.Attributes["className"] != null ? node.Attributes["className"].Value : actor.ClassName;
                actor.NameId = actorNameId != null ? Convert.ToInt32(actorNameId.ItemArray[1]) : 0;
                actor.Appearance = SetAppearance(actorGraphics);
                actor.Position = SetPosition(zoneId, node.SelectSingleNode("position"));
                actor.QuestIcon = questIcon;
                actor.SubState = new SubState { MotionPack = animation };
                actor.Events.AddRange(SetEvents(node.SelectSingleNode("events")));
                actor.TalkFunctions = GetTalkFunctions(classId, actorNameId, node.SelectSingleNode("talkFunctions"));

                return actor;
            }
            else
            {
                return null;
            }
        }

        private KeyValuePair<uint, string> GenerateDefaultTalkFunction(uint classId, DataRow actorNameId)
        {                      
            if (classId < 3000000 && actorNameId != null && (int)actorNameId.ItemArray[1] > 0) //< 3000000 is NPC. > is monster.
            {
                DataRow displayNameRow = ActorsNames.Select("id = '" + actorNameId.ItemArray[1] + "'")[0];
                string displayName = (displayNameRow.ItemArray[1] + ""); //just to parse to string.
                displayName = displayName
                    .Replace(" ", "")
                    .Replace("`", "")
                    .Replace("\0", "")
                    .Replace("'", "");
                displayName = char.ToUpper(displayName[0]) + displayName.Substring(1, displayName.Length - 1).ToLower();

                return new KeyValuePair<uint, string>(
                    0,
                    "defaultTalkWith" + displayName + "_001"
                );
            }
            else
            {
                return new KeyValuePair<uint, string>();
            }
        }

        private Dictionary<uint, string> GetTalkFunctions(uint classId, DataRow actorNameId, XmlNode talkFunctionsNode)
        {
            Dictionary<uint, string> functions = new Dictionary<uint, string>();

            //try to generate a default talk function for the actor. If successful, add to the list of talk functions.
            KeyValuePair<uint, string> defaultFunction = GenerateDefaultTalkFunction(classId, actorNameId);

            if (!string.IsNullOrEmpty(defaultFunction.Value))
                functions.Add(defaultFunction.Key, defaultFunction.Value);

            //get additional talk functions from the xml file.
            if (talkFunctionsNode != null && talkFunctionsNode.ChildNodes != null && talkFunctionsNode.ChildNodes.Count > 0)
            {
                foreach (XmlNode node in talkFunctionsNode.ChildNodes)
                {                    
                    uint questId = node.Attributes["questId"] != null ? Convert.ToUInt32(node.Attributes["questId"].Value) : 0;
                    string function = node.Attributes["name"] != null ? node.Attributes["name"].Value : "";

                    //if there is already a default talk function, replace the one generated above, if any.
                    if (functions.ContainsKey(0) && questId == 0)
                    {
                        functions[0] = function;
                        continue;
                    }

                    functions.Add(questId, function);                        
                }
            }            

            return functions;
        }

        private List<Event> SetEvents(XmlNode eventNode)
        {
            List<Event> eventList = new List<Event>();

            if (eventNode != null && eventNode.ChildNodes != null && eventNode.ChildNodes.Count > 0)
            {              
                foreach (XmlNode node in eventNode.ChildNodes)
                {
                    switch (node.Name)
                    {                       
                        case "talkDefault":
                            eventList.Add(new Event
                            {
                                Opcode = ServerOpcode.TalkEvent,
                                Name = "talkDefault",
                                Priority = Convert.ToByte(node.Attributes["priority"].Value),
                                Enabled = Convert.ToByte(node.Attributes["enabled"].Value)
                            });
                            break;

                        case "noticeEvent":
                            eventList.Add(new Event
                            {
                                Opcode = ServerOpcode.NoticeEvent,
                                Name = "noticeEvent",
                                Priority = Convert.ToByte(node.Attributes["priority"].Value),
                                Enabled = Convert.ToByte(node.Attributes["enabled"].Value),
                                Silent = node.Attributes["silent"] != null ? Convert.ToByte(node.Attributes["silent"].Value) : (byte)0
                            });
                            break;
                        
                        case "exit":
                        case "caution":
                        case "pushCommandIn":
                        case "pushCommandOut":
                        case "pushDefault":
                            eventList.Add(new Event
                            {
                                Opcode = ServerOpcode.PushEventCircle,
                                Name = node.Name,
                                Radius = node.Attributes["radius"] != null ? float.Parse(node.Attributes["radius"].Value) : 0,
                                Silent = node.Attributes["silent"] != null ? Convert.ToByte(node.Attributes["silent"].Value == "false" ? 0 : 1) : (byte)0,
                                Enabled = node.Attributes["enabled"] != null ? Convert.ToByte(node.Attributes["enabled"].Value) : (byte)0,
                                Direction = node.Attributes["outwards"] != null ? Convert.ToByte(node.Attributes["outwards"].Value == "false" ? 1 : 0x11) : (byte)0,
                                Action = node.Attributes["action"] != null ? node.Attributes["action"].Value : ""
                            });
                            break;

                        case "pushCommand":

                            break;
                        case "in":
                            //pushWithBoxEventConditions
                            break;
                        default:
                            Log.Instance.Warning("ActorRepository.SetEvents: Unhandled event '" + node.Name + "' received.");
                            break;
                    }
                }
            }            

            return eventList;
        }

        private Appearance SetAppearance(DataRow actorGraphics)
        {
            if(actorGraphics != null)
            {
                return new Appearance
                {
                    BaseModel = Convert.ToUInt32(actorGraphics.ItemArray[1]),
                    Size = Convert.ToUInt32(actorGraphics.ItemArray[2]),
                    MainWeapon = Convert.ToUInt32(actorGraphics.ItemArray[20]),
                    SecondaryWeapon = Convert.ToUInt32(actorGraphics.ItemArray[21]),
                    SPMainWeapon = Convert.ToUInt32(actorGraphics.ItemArray[22]),
                    SPSecondaryWeapon = Convert.ToUInt32(actorGraphics.ItemArray[23]),
                    Throwing = Convert.ToUInt32(actorGraphics.ItemArray[24]),
                    Pack = Convert.ToUInt32(actorGraphics.ItemArray[25]),
                    Pouch = Convert.ToUInt32(actorGraphics.ItemArray[26]),
                    Head = Convert.ToUInt32(actorGraphics.ItemArray[27]),
                    Body = Convert.ToUInt32(actorGraphics.ItemArray[28]),
                    Legs = Convert.ToUInt32(actorGraphics.ItemArray[29]),
                    Hands = Convert.ToUInt32(actorGraphics.ItemArray[30]),
                    Feet = Convert.ToUInt32(actorGraphics.ItemArray[31]),
                    Waist = Convert.ToUInt32(actorGraphics.ItemArray[32]),
                    Neck = Convert.ToUInt32(actorGraphics.ItemArray[33]),
                    RightEar = Convert.ToUInt32(actorGraphics.ItemArray[34]),
                    LeftEar = Convert.ToUInt32(actorGraphics.ItemArray[35]),
                    RightIndex = Convert.ToUInt32(actorGraphics.ItemArray[36]),
                    LeftIndex = Convert.ToUInt32(actorGraphics.ItemArray[37]),
                    RightFinger = Convert.ToUInt32(actorGraphics.ItemArray[38]),
                    LeftFinger = Convert.ToUInt32(actorGraphics.ItemArray[39]),
                    Voice = Convert.ToUInt32(actorGraphics.ItemArray[18]),
                    HairStyle = Convert.ToUInt16(actorGraphics.ItemArray[3]),
                    HairHighlightColor = Convert.ToUInt16(actorGraphics.ItemArray[4]),
                    HairColor = Convert.ToUInt16(actorGraphics.ItemArray[16]),
                    SkinColor = Convert.ToUInt16(actorGraphics.ItemArray[17]),
                    EyeColor = Convert.ToUInt16(actorGraphics.ItemArray[18]),
                    Face = SetFace(actorGraphics)
                };
            }
            else
            {
                //this is necessary for actors who have no appearance data.
                return new Appearance
                {
                    Face = new Face()
                };
            }            
        }

        private Face SetFace(DataRow actorGraphics)
        {
            return new Face
            {
                Characteristics = Convert.ToByte(actorGraphics.ItemArray[7]),
                CharacteristicsColor = Convert.ToByte(actorGraphics.ItemArray[8]),
                Type = Convert.ToByte(actorGraphics.ItemArray[6]),
                Ears = Convert.ToByte(actorGraphics.ItemArray[15]),
                Mouth = Convert.ToByte(actorGraphics.ItemArray[14]),
                Features = Convert.ToByte(actorGraphics.ItemArray[13]),
                Nose = Convert.ToByte(actorGraphics.ItemArray[12]),
                EyeShape = Convert.ToByte(actorGraphics.ItemArray[11]),
                IrisSize = Convert.ToByte(actorGraphics.ItemArray[10]),
                EyeBrows = Convert.ToByte(actorGraphics.ItemArray[9])
            };
        }

        private Position SetPosition(uint zoneId, XmlNode positionNode)
        {
            return new Position
            {
                ZoneId = zoneId,
                X = Convert.ToSingle(positionNode.Attributes["x"].Value),
                Y = Convert.ToSingle(positionNode.Attributes["y"].Value),
                Z = Convert.ToSingle(positionNode.Attributes["z"].Value),
                R = Convert.ToSingle(positionNode.Attributes["r"].Value)
            };
        }

        /// <summary>
        /// Get the contents of a XML file configured as a resource.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string GetXmlResource(string fileName)
        {
            //From https://social.msdn.microsoft.com/Forums/vstudio/en-US/6990068d-ddee-41e9-86fc-01527dcd99b5/how-to-embed-xml-file-in-project-resources?forum=csharpgeneral
            string result = string.Empty;
            Stream stream = typeof(ActorRepository).Assembly.GetManifestResourceStream("Launcher.Resources.xml." + fileName);
            if(stream != null)
                using (stream)            
                    using (StreamReader sr = new StreamReader(stream))                
                        result = sr.ReadToEnd();               
           
            return result;
        }

        public Actor CreateTestActor(uint classId)
        {
            Position pos = User.Instance.Character.Position;
            string file = @"<?xml version='1.0' encoding='utf-8' ?>
               <chara>                   
                       <Populace className='PopulaceStandard'>" +
                           "<classId>"+ classId + "</classId>" +
                           "<position x='"+ pos.X +"' y='"+ pos.Y + "' z='"+ pos.Z +"' r='"+ pos.R +"' />" +   
                           @"<animation>0</animation>       
                           <events>       
                               <talkDefault priority='4' enabled='1' />          
                               <noticeEvent priority='0' enabled='0' />             
                           </events>             
                       </Populace>                   
               </chara>
            ";
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(file);
            XmlElement root = doc.DocumentElement;
            XmlNode node = root.FirstChild;

            Actor actor = LoadActor(node, pos.ZoneId);

            return actor;
        }
    }
}
